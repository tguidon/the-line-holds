using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace TheLineHolds.Editor
{
    // The approved layout owns all road, gun site and building clearances.
    public static class TownBuilder
    {
        internal const string Art="Assets/TheLineHolds/Art/SaintAubin";
        static Transform root;
        static TownScenery scenery;
        static readonly Dictionary<string,Material> finishes=new();
        static Vector3[] pads, road;
        static readonly List<Rect> footprints=new();
        [MenuItem("The Line Holds/Visuals/Build Saint-Aubin")]
        public static void Build()
        {
            if(EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Save the active scene in Edit Mode before building Saint-Aubin.");
            var scene=EditorSceneManager.OpenScene(PrototypeBuilder.CountrysideScene);
            var game=Object.FindAnyObjectByType<BattleController>();
            Object.DestroyImmediate(GameObject.Find("Layered countryside diorama"));
            Object.DestroyImmediate(GameObject.Find("Diorama landscape"));
            Directory.CreateDirectory(Art);AssetDatabase.Refresh();finishes.Clear();footprints.Clear();
            root=new GameObject("Saint-Aubin • market quarter").transform;
            game.saintAubin=true;
            scenery=new TownScenery(root);
            // A higher, near-frontal view keeps the belfry below the central pad and the two
            // approach lanes readable without changing the existing orbit/zoom controls.
            game.battleCamera.transform.rotation=Quaternion.Euler(49,-17,0);
            game.battleCamera.transform.position=-game.battleCamera.transform.forward*48;
            var a=TownLayout.Route(false);var b=TownLayout.Route(true);
            var samples=new List<Vector3>();
            foreach(var route in new[]{a,b})
                for(float d=-44;d<=route.Length+.15f;d+=.15f)samples.Add(route.Position(d));
            road=samples.ToArray();pads=TownLayout.Pads;
            TownPaving.Build(root,a,b);
            var green=Mat("Spring pasture","879469");
            Box("Surrounding terrain",0,-.26f,0,360,.22f,360,green);
            foreach(var route in new[]{a,b})
            {
                var gate=route.Points[0];
                foreach(int side in new[]{-1,1})
                {
                    float z=gate.z+side*1.68f;
                    Box("Outskirts milestone",gate.x,.27f,z,.29f,.54f,.31f,Mat("Cut stone","D3C5A6"));
                    Box("Milestone cap",gate.x,.55f,z,.32f,.075f,.34f,Mat("Slate","52636B"));
                }
            }
            game.padViews=new Transform[pads.Length];
            var earth=Mat("Cleared earth","B09C7A");
            earth.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/TheLineHolds/Art/Diorama/Lime and patina.png");
            for(int i=0;i<pads.Length;i++)
            {
                float x=pads[i].x,z=pads[i].z;
                var site=new GameObject("Gun site "+(i+1)).transform;site.SetParent(root);site.position=pads[i];
                game.padViews[i]=site;
                Disc("Level bed",x,.038f,z,.96f,.072f,earth,site);
                scenery.Place("GunSite",x,z,Vector3.one,22*i);
            }
            footprints.Add(new Rect(4.12f,.64f,2.86f,1.62f));
            scenery.Place("Mairie",5.55f,1.45f,Vector3.one);
            // Large front-row roofs would hide the road in the normal oblique view.
            // Reserve the traveled envelope plus room around every emplacement before adding a block.
            var random=new System.Random(2904);
            int placed=0;
            for(int row=-4;row<=4;row++)
                for(int col=0;col<9;col++)
                {
                    float x=-8+col*3.45f+(row%2)*.48f,z=row*3.4f+.55f;
                    x+=.26f*Mathf.Sin(col*6.1f+row*2.7f);
                    z+=.29f*Mathf.Sin(col*1.7f-row*1.4f);
                    float w=2.0f+(float)random.NextDouble()*.65f,d=1.38f+(float)random.NextDouble()*.65f;
                    if(!Clear(x,z,w,d))continue;
                    float h=(z<0?.78f:1.02f)+(float)random.NextDouble()*.45f;
                    var kind=placed++%5;
                    string model=kind switch {0=>"Mercerie",1=>"BurnedHouse",2=>"Boulangerie",3=>"Maison",_=>"ShellHouse"};
                    footprints.Add(new Rect(x-w/2,z-d/2,w,d));
                    scenery.Place(model,x,z,new Vector3(w/2.5f,h/1.4f,d/1.75f),(row%3==0?180:0)+5f*Mathf.Sin(row*2.2f+col*3f));
                }
            scenery.Dress((x,z,r)=>Clear(x,z,r*2,r*2));
            var sun=Object.FindAnyObjectByType<Light>();
            sun.transform.rotation=Quaternion.Euler(43,31,0);
            sun.color=DioramaSurface.Hex("FFF1DE");
            sun.shadowStrength=.76f;
            game.audioPlayer.ambienceGain=.51f;
            EditorSceneManager.SaveScene(scene,PrototypeBuilder.TownScene);
            EditorBuildSettings.scenes=PrototypeBuilder.BuildScenes.Select(p=>new EditorBuildSettingsScene(p,true)).ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log($"Saint-Aubin: {placed} residential footprints; {pads.Length} sites; road lengths {a.Length:F1}/{b.Length:F1} m.");
        }
        static bool Clear(float x,float z,float w,float d)
        {
            var r=new Rect(x-w/2,z-d/2,w,d);
            bool Near(Vector3 p,float gap)
            {
                float dx=Mathf.Max(r.xMin-p.x,0,p.x-r.xMax),dz=Mathf.Max(r.yMin-p.z,0,p.z-r.yMax);
                return dx*dx+dz*dz<gap*gap;
            }
            return !road.Any(p=>Near(p,2.02f)) && !pads.Any(p=>Near(p,1.62f))
                && !footprints.Any(existing=>existing.Overlaps(new Rect(r.xMin-.52f,r.yMin-.52f,w+1.04f,d+1.04f)));
        }
        internal static Material Mat(string name,string hex)
        {
            if(finishes.TryGetValue(name,out var result))return result;
            string path=Art+"/"+name+".mat";
            var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!mat){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}
            mat.color=DioramaSurface.Hex(hex);mat.SetFloat("_Smoothness",.08f);EditorUtility.SetDirty(mat);
            return finishes[name]=mat;
        }
        static Transform Box(string n,float x,float y,float z,float w,float h,float d,Material mat,Transform parent=null)
            => Shape(PrimitiveType.Cube,n,new Vector3(x,y,z),new Vector3(w,h,d),mat,parent);
        static Transform Disc(string n,float x,float y,float z,float r,float h,Material mat,Transform parent)
            => Shape(PrimitiveType.Cylinder,n,new Vector3(x,y,z),new Vector3(2*r,h/2,2*r),mat,parent);
        static Transform Shape(PrimitiveType type,string n,Vector3 pos,Vector3 size,Material mat,Transform parent)
        {
            var go=GameObject.CreatePrimitive(type);Object.DestroyImmediate(go.GetComponent<Collider>());
            go.name=n;go.transform.SetParent(parent?parent:root);go.transform.position=pos;go.transform.localScale=size;
            go.GetComponent<Renderer>().sharedMaterial=mat;go.isStatic=true;return go.transform;
        }
    }
}
