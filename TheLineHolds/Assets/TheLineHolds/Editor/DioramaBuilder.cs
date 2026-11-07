using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TheLineHolds.Editor
{
    public static class DioramaBuilder
    {
        const string Root="Assets/TheLineHolds";
        static Transform environment;
        static readonly Dictionary<string,Material> materials=new Dictionary<string,Material>();
        public static Material Material(string name,string hex,float smoothness=.18f)
        {
            if(materials.TryGetValue(name,out var cached))return cached;
            string path=DioramaSurface.Root+"/"+name+".mat";
            var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            m.color=DioramaSurface.Hex(hex);m.SetFloat("_Smoothness",smoothness);EditorUtility.SetDirty(m);materials[name]=m;return m;
        }
        public static Mesh SaveMesh(Mesh mesh,string name)
        {
            string path=DioramaSurface.Root+"/"+name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(old)
            {
                // Assign channels explicitly so the renderer invalidates its cached vertex buffer.
                old.Clear();old.indexFormat=mesh.indexFormat;old.vertices=mesh.vertices;old.normals=mesh.normals;old.uv=mesh.uv;old.colors=mesh.colors;old.tangents=mesh.tangents;
                old.subMeshCount=mesh.subMeshCount;
                for(int i=0;i<mesh.subMeshCount;i++)old.SetTriangles(mesh.GetTriangles(i),i);
                old.RecalculateBounds();old.UploadMeshData(false);
                UnityEngine.Object.DestroyImmediate(mesh);EditorUtility.SetDirty(old);return old;
            }
            AssetDatabase.CreateAsset(mesh,path);return mesh;
        }
        static GameObject Surface(string name,Mesh mesh,Material mat)
        {
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(environment,false);
            go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=mat;return go;
        }
        static GameObject Model(string name,Vector3 p,float scale,float yaw)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/"+name+".prefab");
            var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);go.transform.SetParent(environment);go.transform.position=p;go.transform.rotation=Quaternion.Euler(0,yaw,0);go.transform.localScale=Vector3.one*scale;return go;
        }
        [MenuItem("The Line Holds/Visuals/Build Diorama Map")]
        public static string Map()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode before authoring.");
            System.IO.Directory.CreateDirectory(DioramaSurface.Root);AssetDatabase.Refresh();materials.Clear();
            var scene=EditorSceneManager.GetActiveScene();
            var previous=GameObject.Find("Diorama landscape");if(previous)UnityEngine.Object.DestroyImmediate(previous);
            var old=GameObject.Find("Layered countryside diorama");
            if(!old)throw new Exception("Open Countryside first.");
            // Keep the authored pad and marker objects, including their controller references.
            foreach(Transform t in old.transform.Cast<Transform>().ToArray())
            {
                bool keep=t.name.StartsWith("Emplacement ") || t.name=="Pad number" || t.name.Contains("ENTRY") || t.name.Contains("EXIT");
                if(!keep)UnityEngine.Object.DestroyImmediate(t.gameObject);
            }
            var game=UnityEngine.Object.FindAnyObjectByType<BattleController>();
            for(int i=0;i<game.padViews.Length;i++)
            {
                var pad=game.padViews[i];pad.position=BattleRoute.DefaultPads[i];pad.localScale=new Vector3(1.42f,.035f,1.42f);
                pad.GetComponent<Renderer>().sharedMaterial=Material("Emplacement limestone","B5AB83");
            }
            var numbers=old.GetComponentsInChildren<TMPro.TextMeshPro>().Where(t=>t.name=="Pad number").ToArray();
            for(int i=0;i<numbers.Length;i++){numbers[i].transform.position=BattleRoute.DefaultPads[i]+new Vector3(0,.075f,-.45f);numbers[i].transform.rotation=Quaternion.Euler(90,0,0);}
            foreach(var t in old.GetComponentsInChildren<TMPro.TextMeshPro>().Where(t=>t.name!="Pad number"))
            {var p=t.transform.position;p.y=BattleRoute.Ground(p.x,p.z)+1.7f;t.transform.position=p;}
            environment=new GameObject("Diorama landscape").transform;
            var turf=Material("Meadow and worn road","FFFFFF");turf.mainTexture=DioramaSurface.Paint();EditorUtility.SetDirty(turf);
            Surface("Continuous rolling meadow and road",SaveMesh(DioramaSurface.Terrain(),"Meadow"),turf);
            var earth=Material("Earth strata","FFFFFF");earth.mainTexture=DioramaSurface.EarthPaint();EditorUtility.SetDirty(earth);
            Surface("Exposed earth layers",SaveMesh(DioramaSurface.Earth(),"Earth"),earth);
            var plinth=GameObject.CreatePrimitive(PrimitiveType.Cube);plinth.name="Miniature display plinth";plinth.transform.SetParent(environment);plinth.transform.position=new Vector3(0,-1.47f,0);plinth.transform.localScale=new Vector3(24.3f,.3f,14.3f);plinth.GetComponent<Renderer>().sharedMaterial=Material("Display charcoal","303B38");UnityEngine.Object.DestroyImmediate(plinth.GetComponent<Collider>());
            AssetDatabase.DeleteAsset(DioramaSurface.Root+"/RoadDiagnostic.asset");
            RefreshModels(new[]{"Pine","Bush","Farmhouse","Crate","Sandbags"});
            var trees=new[]{new Vector2(-10.8f,5.6f),new Vector2(-11,2.5f),new Vector2(-10.9f,-.5f),new Vector2(-8.6f,6),new Vector2(-1.2f,6.2f),new Vector2(2.8f,6.1f),new Vector2(10.5f,-5.7f),new Vector2(11,-3),new Vector2(11,6.1f)};
            for(int i=0;i<trees.Length;i++)Model("Pine",BattleRoute.OnGround(trees[i].x,trees[i].y),.8f+(i%3)*.13f,i*57);
            Model("Farmhouse",BattleRoute.OnGround(-10.4f,.1f),.65f,15);Model("Farmhouse",BattleRoute.OnGround(10.4f,-.4f),.65f,-20);
            Model("Crate",BattleRoute.OnGround(-3,6),.7f,20);Model("Crate",BattleRoute.OnGround(9.8f,4),.7f,-15);
            FoliageMaterials.Configure();
            DioramaVegetation.Build(environment);
            AssetDatabase.DeleteAsset(DioramaSurface.Root+"/Hedgerow undergrowth.asset");
            AssetDatabase.DeleteAsset(DioramaSurface.Root+"/Hedge foliage.mat");
            Lighting(game.battleCamera);
            PlayerSettings.defaultScreenWidth=1920;PlayerSettings.defaultScreenHeight=1080;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;
            // Existing players may have saved smaller Unity window preferences. Apply the new default once.
            EditorUtility.SetDirty(game);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            string result=$"[Diorama] Smooth terrain, painted road, clustered foliage; {environment.GetComponentsInChildren<Renderer>().Length} landscape renderers; default window 1920x1080.";Debug.Log(result);return result;
        }
        public static string RefreshModels(string[] names)
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode before authoring prefabs.");
            foreach(var name in names)
            {
                AssetDatabase.ImportAsset(Root+"/Art/Models/"+name+".fbx",ImportAssetOptions.ForceUpdate|ImportAssetOptions.ForceSynchronousImport);
                var model=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Art/Models/"+name+".fbx");if(!model)throw new Exception("Missing model "+name);
                var go=UnityEngine.Object.Instantiate(model);go.name=name;
                if(name=="Infantry" || name=="LightTank")
                {
                    // Blender +Y exports as Unity -Z. Keep gameplay roots +Z-forward.
                    var miniature=go;go=new GameObject(name);miniature.name="Miniature";
                    miniature.transform.SetParent(go.transform,false);miniature.transform.localRotation=Quaternion.Euler(0,180,0)*miniature.transform.localRotation;
                }
                foreach(var r in go.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>
                {
                    var authored=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Art/Materials/"+m.name.Replace(" (Instance)","")+".mat");
                    return authored?authored:Material(m.name,ExtraColor(m.name));
                }).ToArray();
                if(name=="Infantry")go.AddComponent<InfantryVisual>();
                if(name=="MachineGun" || name=="AntiTank" || name=="Mortar")go.AddComponent<TowerVisual>();
                PrefabUtility.SaveAsPrefabAsset(go,Root+"/Prefabs/"+name+".prefab");UnityEngine.Object.DestroyImmediate(go);
            }
            AssetDatabase.SaveAssets();return "Updated "+string.Join(", ",names);
        }
        static string ExtraColor(string name)=>name.Contains("BagCloth")?"A18C60":name.Contains("LeafLight")?"738451":name.Contains("LeafDark")?"4C633E":name.Contains("Bark")?"71604A":name.Contains("Leather")?"604B36":name.Contains("Edge")?"6F7859":name.Contains("Badge")?"D0C4A0":"747C60";
        static void Lighting(Camera camera)
        {
            var light=UnityEngine.Object.FindObjectsByType<Light>().First(l=>l.type==LightType.Directional);
            light.color=DioramaSurface.Hex("FFF0D9");light.intensity=2f;light.transform.rotation=Quaternion.Euler(48,-38,0);light.shadows=LightShadows.Soft;light.shadowBias=.035f;light.shadowNormalBias=.12f;
            RenderSettings.skybox=null;RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=DioramaSurface.Hex("73868D");RenderSettings.ambientEquatorColor=DioramaSurface.Hex("68725F");RenderSettings.ambientGroundColor=DioramaSurface.Hex("4D4030");RenderSettings.ambientIntensity=1;
            camera.backgroundColor=DioramaSurface.Hex("61736D");camera.allowHDR=true;
            var data=camera.GetComponent<UniversalAdditionalCameraData>();data.renderPostProcessing=true;data.antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            var profile=UnityEngine.Object.FindAnyObjectByType<Volume>().sharedProfile;
            profile.components.RemoveAll(c=>!c);
            if(!profile.TryGet<Tonemapping>(out var tone))tone=profile.Add<Tonemapping>(true);
            tone.mode.Override(TonemappingMode.ACES);
            if(!profile.TryGet<Bloom>(out var existingBloom))profile.Add<Bloom>(true);
            if(!profile.TryGet<Vignette>(out var existingVignette))profile.Add<Vignette>(true);
            if(!profile.TryGet<ColorAdjustments>(out var grade))grade=profile.Add<ColorAdjustments>(true);
            grade.saturation.Override(4);grade.contrast.Override(4);grade.postExposure.Override(.1f);
            if(profile.TryGet<Bloom>(out var bloom)){bloom.threshold.Override(1.15f);bloom.intensity.Override(.12f);}
            if(profile.TryGet<Vignette>(out var vignette))vignette.intensity.Override(.15f);
            var urp=(UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;urp.shadowDistance=100;urp.shadowCascadeCount=2;urp.mainLightShadowmapResolution=4096;urp.msaaSampleCount=4;urp.supportsHDR=true;
            foreach(var component in profile.components)
            {
                // VolumeProfile.Add creates transient objects; attach overrides so reloads/builds retain them.
                if(!AssetDatabase.Contains(component))AssetDatabase.AddObjectToAsset(component,profile);
                EditorUtility.SetDirty(component);
            }
            EditorUtility.SetDirty(profile);EditorUtility.SetDirty(urp);
        }
    }
}
