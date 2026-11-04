using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;
namespace TheLineHolds.Editor
{
    public static class PrototypeBuilder
    {
        const string Root="Assets/TheLineHolds";
        static TMP_FontAsset fieldFont;
        static readonly Dictionary<string,Material> materials=new Dictionary<string,Material>();
        [MenuItem("The Line Holds/Rebuild Prototype")]
        public static void Build()
        {
            if(EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before rebuilding.");
            foreach(var dir in new[]{"Data/Enemies","Data/Defenses","Data/Waves","Art/Materials","Prefabs","Scenes","Audio","UI"}) Directory.CreateDirectory(Root+"/"+dir);
            AssetDatabase.Refresh();
            SetupMaterials(); fieldFont=BuildFont();
            var infantry=Enemy("Infantry",40,1.05f,10); var tank=Enemy("LightTank",200,.63f,30);
            var mg=Defense("MachineGun","Machine-gun nest","Rapid fire / short range",75,3.7f,.23f,6,0,.12f);
            var at=Defense("AntiTank","Anti-tank gun","Heavy shot / long range",125,5.8f,2.3f,64,0,.35f);
            var mortar=Defense("Mortar","Mortar","Explosive / area damage",150,4.8f,3.1f,32,1.65f,.95f);
            var waves=new WaveDefinition[5]; int[] counts={8,12,8,16,20}, tanks={0,0,2,3,5};
            float[] spacing={2.8f,2.5f,2.5f,1.65f,1.2f};
            string[] briefings={"Eight infantry. Establish road coverage.","Twelve infantry. Hold a sustained advance.","Armor sighted. Prepare heavy guns.","Mixed column. Cover the bends.","Final assault. Hold every approach."};
            for(int w=0;w<5;w++)
            {
                var wave=Asset<WaveDefinition>($"Data/Waves/Wave{w+1}"); wave.briefing=briefings[w];
                var list=new List<SpawnEntry>(); int total=counts[w]+tanks[w], added=0;
                for(int i=0;i<total;i++) { bool armor=tanks[w]>0 && added<tanks[w] && i%4==3; if(total-i<=tanks[w]-added) armor=true; if(armor) added++; list.Add(new SpawnEntry(armor?tank:infantry,i==0?1:spacing[w]+(i%(w<3?4:5)==0?12:0))); }
                wave.spawns=list.ToArray(); EditorUtility.SetDirty(wave); waves[w]=wave;
            }
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var terrain=new GameObject("Layered countryside diorama").transform;
            BuildTerrain(terrain); BuildRoad(terrain);
            var controller=new GameObject("Battle director").AddComponent<BattleController>(); controller.waves=waves; controller.defenses=new[]{mg,at,mortar};
            controller.dynamicRoot=new GameObject("Match objects").transform;
            controller.padViews=BattleRoute.DefaultPads.Select((p,i)=>Pad(p,i,terrain)).ToArray();
            BuildProps(terrain);
            var cam=new GameObject("Battle Camera",typeof(Camera),typeof(AudioListener),typeof(UniversalAdditionalCameraData)).GetComponent<Camera>();
            cam.tag="MainCamera"; cam.orthographic=true; cam.orthographicSize=13.5f; cam.transform.position=new Vector3(21,27,-30); cam.transform.LookAt(new Vector3(0,0,0)); cam.transform.position-=cam.transform.up*.9f;
            cam.clearFlags=CameraClearFlags.SolidColor; cam.backgroundColor=new Color(.29f,.31f,.29f); cam.nearClipPlane=.1f; cam.farClipPlane=150; cam.allowHDR=true;
            var cameraData=cam.GetComponent<UniversalAdditionalCameraData>(); cameraData.renderPostProcessing=true; cameraData.volumeLayerMask=1; cameraData.antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            controller.battleCamera=cam;
            var light=new GameObject("Soft afternoon light").AddComponent<Light>(); light.type=LightType.Directional; light.intensity=2.3f; light.color=new Color(1,.91f,.75f); light.shadows=LightShadows.Soft; light.transform.rotation=Quaternion.Euler(48,-35,0);
            RenderSettings.ambientMode=AmbientMode.Flat; RenderSettings.ambientLight=new Color(.53f,.59f,.62f); RenderSettings.ambientIntensity=1;
            var urp=GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset; if(!urp) throw new Exception("URP must be active."); urp.supportsHDR=true;
            var profile=Asset<VolumeProfile>("Art/Materials/FieldGrade"); profile.components.Clear();
            profile.Add<Tonemapping>(true).mode.value=TonemappingMode.ACES;
            var bloom=profile.Add<Bloom>(true); bloom.threshold.value=.9f; bloom.intensity.value=.25f;
            var vignette=profile.Add<Vignette>(true); vignette.intensity.value=.18f;
            var volume=new GameObject("Field color grade").AddComponent<Volume>(); volume.isGlobal=true; volume.sharedProfile=profile;
            QualitySettings.SetQualityLevel(QualitySettings.names.Length-1,true); QualitySettings.shadowDistance=100;
            controller.tracerMaterial=materials["Flash"]; controller.rangeMaterial=materials["Range"]; controller.healthMaterial=materials["Health"]; controller.damageMaterial=materials["Damage"]; controller.smokeMaterial=materials["Smoke"];
            controller.audioPlayer=controller.gameObject.AddComponent<BattleAudio>(); BuildAudio(controller.audioPlayer);
            var font=BuildFont(); var hud=new GameObject("Command UI").AddComponent<GameHud>(); hud.font=font; controller.hud=hud;
            PlayerSettings.companyName="The Line Holds"; PlayerSettings.productName="The Line Holds"; PlayerSettings.bundleVersion="0.1.0";
            PlayerSettings.defaultScreenWidth=1600; PlayerSettings.defaultScreenHeight=900; PlayerSettings.fullScreenMode=FullScreenMode.Windowed; PlayerSettings.resizableWindow=true;
            PlayerSettings.runInBackground=true; PlayerSettings.macOS.buildNumber="1";
            EditorUtility.SetDirty(profile); EditorUtility.SetDirty(urp);
            EditorSceneManager.SaveScene(scene,Root+"/Scenes/Countryside.unity");
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Root+"/Scenes/Countryside.unity",true)};
            AssetDatabase.SaveAssets(); Debug.Log("[Builder] Countryside saved: 10 pads, 5 waves, 3 defenses, 2 enemies; URP HDR and camera postprocessing enabled.");
        }
        static T Asset<T>(string path) where T:ScriptableObject
        {
            string p=Root+"/"+path+".asset"; var a=AssetDatabase.LoadAssetAtPath<T>(p); if(a) return a;
            a=ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(a,p); return a;
        }
        static EnemyDefinition Enemy(string name,float hp,float speed,int reward)
        {
            var d=Asset<EnemyDefinition>("Data/Enemies/"+name); d.displayName=name=="LightTank"?"Light tank":"Infantry"; d.health=hp;d.speed=speed;d.reward=reward;d.prefab=Prefab(name);EditorUtility.SetDirty(d);return d;
        }
        static DefenseDefinition Defense(string name,string title,string description,int cost,float range,float interval,float damage,float splash,float flight)
        {
            var d=Asset<DefenseDefinition>("Data/Defenses/"+name);d.displayName=title;d.description=description;d.cost=cost;d.range=range;d.interval=interval;d.damage=damage;d.splashRadius=splash;d.flightTime=flight;d.prefab=Prefab(name);EditorUtility.SetDirty(d);return d;
        }
        static GameObject Prefab(string name)
        {
            var model=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Art/Models/"+name+".fbx"); if(!model)throw new Exception("Missing Blender model: "+name);
            var go=UnityEngine.Object.Instantiate(model);go.name=name;
            foreach(var r in go.GetComponentsInChildren<Renderer>())
            {
                r.sharedMaterials=r.sharedMaterials.Select(m=>materials.TryGetValue(m.name.Replace(" (Instance)",""),out var material)?material:materials["Olive"]).ToArray();
            }
            var result=PrefabUtility.SaveAsPrefabAsset(go,Root+"/Prefabs/"+name+".prefab");UnityEngine.Object.DestroyImmediate(go);return result;
        }
        static void SetupMaterials()
        {
            var colors=new Dictionary<string,Color> {
                {"Olive",new Color(.25f,.29f,.16f)},{"Dark",new Color(.075f,.085f,.065f)},{"Steel",new Color(.19f,.22f,.19f)},
                {"Cloth",new Color(.36f,.39f,.25f)},{"Skin",new Color(.59f,.43f,.3f)},{"Sand",new Color(.51f,.44f,.29f)},
                {"Wood",new Color(.25f,.17f,.1f)},{"Leaf",new Color(.25f,.32f,.15f)},{"Plaster",new Color(.62f,.57f,.43f)},{"Roof",new Color(.27f,.24f,.2f)},
                {"Grass",new Color(.36f,.40f,.22f)},{"GrassLight",new Color(.41f,.43f,.26f)},{"Soil",new Color(.23f,.18f,.115f)},
                {"Road",new Color(.51f,.46f,.33f)},{"Pad",new Color(.56f,.53f,.4f)},{"Base",new Color(.10f,.105f,.09f)},
                {"Smoke",new Color(.38f,.37f,.32f)},{"Flash",new Color(1.5f,1.0f,.25f)},{"Range",new Color(.9f,.77f,.34f)},{"Health",new Color(.65f,.8f,.35f)},{"Damage",new Color(1,.25f,.13f)} };
            foreach(var pair in colors)
            {
                string p=Root+"/Art/Materials/"+pair.Key+".mat"; var m=AssetDatabase.LoadAssetAtPath<Material>(p);
                var shader=Shader.Find(pair.Key=="Flash" || pair.Key=="Range" || pair.Key=="Health" || pair.Key=="Damage"?"Universal Render Pipeline/Unlit":"Universal Render Pipeline/Lit"); if(!shader)throw new Exception("URP shader missing");
                if(!m){m=new Material(shader);AssetDatabase.CreateAsset(m,p);} m.color=pair.Value; if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.13f); materials[pair.Key]=m;EditorUtility.SetDirty(m);
            }
        }
        static GameObject Primitive(string name,PrimitiveType type,Vector3 position,Vector3 scale,string material,Transform parent)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent);go.transform.position=position;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=materials[material];UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());return go;
        }
        static void BuildTerrain(Transform parent)
        {
            Primitive("Dark miniature plinth",PrimitiveType.Cube,new Vector3(0,-1.55f,0),new Vector3(24.4f,.45f,14.4f),"Base",parent);
            float[] xs={-12,-4,-2,4,6,12};
            for(int i=0;i<xs.Length-1;i++)
            {
                float a=xs[i],b=xs[i+1],ya=BattleRoute.Ground(a),yb=BattleRoute.Ground(b);
                var mesh=new Mesh(); mesh.vertices=new[]{new Vector3(a,ya,-7),new Vector3(a,ya,7),new Vector3(b,yb,7),new Vector3(b,yb,-7),new Vector3(a,-1.32f,-7),new Vector3(a,-1.32f,7),new Vector3(b,-1.32f,7),new Vector3(b,-1.32f,-7)};
                mesh.subMeshCount=2;mesh.SetTriangles(new[]{0,1,2,0,2,3},0);mesh.SetTriangles(new[]{4,0,3,4,3,7,1,5,6,1,6,2,4,5,1,4,1,0,3,2,6,3,6,7},1);mesh.RecalculateNormals();
                mesh=SaveMesh(mesh,"Terrain"+i);var go=new GameObject("Terrace "+(i+1),typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterials=new[]{materials[i%2==0?"Grass":"GrassLight"],materials["Soil"]};
            }
            // Thin exposed strata on the front edge emphasize the self-contained terrain slab.
            for(int i=0;i<30;i++) { float x=-11.7f+i*.8f; Primitive("Earth stratum",PrimitiveType.Cube,new Vector3(x,-.75f,-7.015f),new Vector3(.75f,.07f,.035f),i%3==0?"Wood":"Sand",parent); }
        }
        static Mesh SaveMesh(Mesh mesh,string name)
        {
            mesh.name=name; string p=Root+"/Art/Materials/"+name+".asset"; var existing=AssetDatabase.LoadAssetAtPath<Mesh>(p);if(existing){existing.Clear(); existing.vertices=mesh.vertices; existing.subMeshCount=mesh.subMeshCount; for(int i=0;i<mesh.subMeshCount;i++)existing.SetTriangles(mesh.GetTriangles(i),i); existing.normals=mesh.normals; existing.RecalculateBounds(); existing.UploadMeshData(false); EditorUtility.SetDirty(existing); UnityEngine.Object.DestroyImmediate(mesh);}else AssetDatabase.CreateAsset(mesh,p); return existing ? existing : mesh;
        }
        static void BuildRoad(Transform parent)
        {
            var route=new BattleRoute(BattleRoute.DefaultPoints); int n=Mathf.CeilToInt(route.Length/.2f);var vertices=new Vector3[(n+1)*2];var triangles=new int[n*6];
            for(int i=0;i<=n;i++)
            {
                float d=route.Length*i/n;var p=route.Position(d);var direction=route.Position(Mathf.Min(route.Length,d+.1f))-route.Position(Mathf.Max(0,d-.1f));var side=Vector3.Cross(Vector3.up,direction).normalized*.62f;
                for(int j=0;j<2;j++){var v=p+(j==0?side:-side);v.y=BattleRoute.Ground(v.x)+.045f;vertices[i*2+j]=v;}
                if(i<n){int a=i*2,k=i*6;triangles[k]=a;triangles[k+1]=a+1;triangles[k+2]=a+2;triangles[k+3]=a+1;triangles[k+4]=a+3;triangles[k+5]=a+2;}
            }
            var mesh=new Mesh{vertices=vertices,triangles=triangles};mesh.RecalculateNormals();SaveMesh(mesh,"RoadMesh");
            var go=new GameObject("Authored winding road",typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent);go.GetComponent<MeshFilter>().sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(Root+"/Art/Materials/RoadMesh.asset");go.GetComponent<MeshRenderer>().sharedMaterial=materials["Road"];go.GetComponent<MeshRenderer>().shadowCastingMode=ShadowCastingMode.Off;
            Marker("ENEMY ENTRY",route.Points[0],parent,new Color(.8f,.35f,.18f));Marker("DEFENDED EXIT",route.Points[route.Points.Length-1],parent,new Color(.74f,.8f,.52f));
        }
        static Transform Pad(Vector3 p,int index,Transform parent)
        {
            var go=Primitive("Emplacement "+(index+1),PrimitiveType.Cylinder,p,new Vector3(1.5f,.1f,1.5f),"Pad",parent);
            var text=new GameObject("Pad number").AddComponent<TextMeshPro>();text.transform.SetParent(parent,false);text.transform.position=p+new Vector3(0,.13f,-.53f);text.transform.rotation=Quaternion.Euler(65,0,0);text.font=fieldFont;text.text=(index+1).ToString("00");text.fontSize=4;text.color=new Color(.14f,.17f,.12f);text.alignment=TextAlignmentOptions.Center;text.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;text.rectTransform.sizeDelta=new Vector2(1,.6f); return go.transform;
        }
        static void Marker(string text,Vector3 p,Transform parent,Color color)
        {
            Primitive(text+" post",PrimitiveType.Cube,p+Vector3.up*.45f,new Vector3(.08f,.9f,.08f),"Wood",parent);
            var t=new GameObject(text).AddComponent<TextMeshPro>();t.transform.SetParent(parent);t.transform.position=p+new Vector3(0,1.7f,0);t.transform.rotation=Quaternion.Euler(38,-25,0);t.font=fieldFont;t.text=text;t.fontSize=2.1f;t.color=color;t.alignment=TextAlignmentOptions.Center;t.rectTransform.sizeDelta=new Vector2(4,.65f);
        }
        static void BuildProps(Transform parent)
        {
            var pine=Prefab("Pine");var house=Prefab("Farmhouse");var crate=Prefab("Crate");var bags=Prefab("Sandbags");
            foreach(var p in new[]{new Vector2(-10,5.5f),new Vector2(-10,2),new Vector2(-11,-1),new Vector2(3,6),new Vector2(10,-5),new Vector2(11,-3),new Vector2(-1,6)})Prop(pine,p.x,p.y,1,parent);
            Prop(house,-10,-.2f,.7f,parent);Prop(house,10,-.3f,.75f,parent);
            Prop(crate,-3,6,.7f,parent);Prop(crate,10,4,.7f,parent);Prop(bags,9,5.9f,1,parent);
            var random=new System.Random(1944);var route=new BattleRoute(BattleRoute.DefaultPoints);
            for(int i=0;i<165;i++)
            {
                float x=(float)random.NextDouble()*23-11.5f,z=(float)random.NextDouble()*13-6.5f;var p=BattleRoute.OnGround(x,z);
                bool clear=true;for(float d=0;d<route.Length;d+=.5f)if(Vector3.Distance(p,route.Position(d))<1.0f){clear=false;break;}
                if(!clear || BattleRoute.DefaultPads.Any(pad=>Vector3.Distance(p,pad)<1.1f))continue;
                var tuft=Primitive("Grass tuft",PrimitiveType.Cube,p+Vector3.up*.08f,new Vector3(.06f,.18f,.21f),i%3==0?"Sand":"Leaf",parent);tuft.transform.rotation=Quaternion.Euler(0,(float)random.NextDouble()*180,12);
            }
            // Recessed trench bed with timber revetment along the near edge.
            Primitive("Trench",PrimitiveType.Cube,new Vector3(0,BattleRoute.Ground(0)+.01f,-6),new Vector3(6,.03f,.65f),"Dark",parent);
            for(int i=0;i<18;i++) Primitive("Trench timber",PrimitiveType.Cube,new Vector3(-2.8f+i*.32f,BattleRoute.Ground(0)+.1f,-6.35f),new Vector3(.22f,.25f,.1f),"Wood",parent);
        }
        static void Prop(GameObject prefab,float x,float z,float scale,Transform parent){var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);go.transform.SetParent(parent);go.transform.position=BattleRoute.OnGround(x,z);go.transform.localScale=Vector3.one*scale;}
        static TMP_FontAsset BuildFont()
        {
            string p=Root+"/UI/FieldFont.asset";var existing=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(p);if(existing)return existing;
            if(!AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/LiberationSans.ttf")) TMP_PackageResourceImporter.ImportResources(true,false,false);
            var source=AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/LiberationSans.ttf");
            if(!source)throw new Exception("TMP essentials font import did not complete. Run Build again after import.");
            var font=TMP_FontAsset.CreateFontAsset(source);font.name="FieldFont";font.atlasPopulationMode=AtlasPopulationMode.Dynamic;
            AssetDatabase.CreateAsset(font,p);AssetDatabase.AddObjectToAsset(font.material,font);foreach(var texture in font.atlasTextures)AssetDatabase.AddObjectToAsset(texture,font);
            return font;
        }
        static void BuildAudio(BattleAudio audio)
        {
            audio.placement=Tone("Placement",660,.14f,0);audio.firing=Tone("Firing",100,.09f,.7f);audio.impact=Tone("Impact",65,.18f,.8f);audio.escape=Tone("Escape",230,.45f,.1f);audio.wave=Tone("Wave",520,.4f,0);audio.victory=Tone("Victory",880,1,0);audio.defeat=Tone("Defeat",145,.9f,.15f);
        }
        static AudioClip Tone(string name,float hz,float duration,float noise)
        {
            string p=Root+"/Audio/"+name+".wav";int count=(int)(22050*duration);var random=new System.Random(name.Length);
            using(var writer=new BinaryWriter(File.Create(p)))
            {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+count*2);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)1);writer.Write(22050);writer.Write(44100);writer.Write((short)2);writer.Write((short)16);writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(count*2);
                for(int i=0;i<count;i++){float t=i/22050f;float envelope=Mathf.Pow(1-i/(float)count,2);float value=(Mathf.Sin(2*Mathf.PI*hz*t)*(1-noise)+((float)random.NextDouble()*2-1)*noise)*envelope*.45f;writer.Write((short)(value*short.MaxValue));}
            }
            AssetDatabase.ImportAsset(p);return AssetDatabase.LoadAssetAtPath<AudioClip>(p);
        }
        [MenuItem("The Line Holds/Build/macOS")]
        public static void BuildMac() { BuildPlayer(BuildTarget.StandaloneOSX,"Builds/macOS/The Line Holds.app"); }
        [MenuItem("The Line Holds/Build/Windows")]
        public static void BuildWindows() { BuildPlayer(BuildTarget.StandaloneWindows64,"Builds/Windows/The Line Holds.exe"); }
        static void BuildPlayer(BuildTarget target,string output)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Root+"/Scenes/Countryside.unity"},locationPathName=output,target=target,options=BuildOptions.None});
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Build failed: "+report.summary.result);
            Debug.Log("[Build] "+output+" succeeded");
        }
    }
}
