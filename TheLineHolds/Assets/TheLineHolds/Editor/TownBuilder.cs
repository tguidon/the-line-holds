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
    // Bake the route, build sites and authored market quarter into a dedicated scene.
    public static class TownBuilder
    {
        internal const string Art="Assets/TheLineHolds/Art/SaintAubin";
        static Transform root;
        static TownScenery scenery;
        static readonly Dictionary<string,Material> finishes=new();

        [MenuItem("The Line Holds/Visuals/Build Saint-Aubin")]
        public static void Build()
        {
            if(EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Save the active scene in Edit Mode before building Saint-Aubin.");
            var scene=EditorSceneManager.OpenScene(PrototypeBuilder.CountrysideScene);
            var game=Object.FindAnyObjectByType<BattleController>();
            Object.DestroyImmediate(GameObject.Find("Layered countryside diorama"));
            Object.DestroyImmediate(GameObject.Find("Diorama landscape"));
            Directory.CreateDirectory(Art);AssetDatabase.Refresh();finishes.Clear();
            root=new GameObject("Saint-Aubin • market quarter").transform;
            game.saintAubin=true;
            scenery=new TownScenery(root);
            // A higher, near-frontal view keeps the belfry below the central pad and the two
            // approach lanes readable without changing the existing orbit/zoom controls.
            game.battleCamera.transform.rotation=Quaternion.Euler(50,-9,0);
            game.battleCamera.transform.position=-game.battleCamera.transform.forward*48;
            var a=TownLayout.Route(false);var b=TownLayout.Route(true);
            var pads=TownLayout.Pads;
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
            new TownBlocks(scenery).Build();
            scenery.Dress();
            var sun=Object.FindAnyObjectByType<Light>();
            sun.transform.rotation=Quaternion.Euler(43,31,0);
            sun.color=DioramaSurface.Hex("FFF1DE");
            sun.shadowStrength=.76f;
            game.audioPlayer.ambienceGain=.51f;
            EditorSceneManager.SaveScene(scene,PrototypeBuilder.TownScene);
            EditorBuildSettings.scenes=PrototypeBuilder.BuildScenes.Select(p=>new EditorBuildSettingsScene(p,true)).ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log($"Saint-Aubin: {pads.Length} sites; road lengths {a.Length:F1}/{b.Length:F1} m.");
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
