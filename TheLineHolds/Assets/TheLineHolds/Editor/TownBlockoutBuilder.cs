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
    // Approval blockout. Only the footprint, height and condition of each block are represented.
    public static class TownBlockoutBuilder
    {
        internal const string Art="Assets/TheLineHolds/Art/SaintAubin";
        static Transform root;
        static readonly Dictionary<string,Material> finishes=new();
        static Vector3[] pads, road;
        static readonly List<Rect> footprints=new();
        [MenuItem("The Line Holds/Visuals/Build Saint-Aubin blockout")]
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
            var earth=Mat("Cleared earth","B29A72");var trim=Mat("Cut stone","D3C5A6");
            for(int i=0;i<pads.Length;i++)
            {
                float x=pads[i].x,z=pads[i].z;
                var site=new GameObject("Gun site "+(i+1)).transform;site.SetParent(root);site.position=pads[i];
                game.padViews[i]=site;
                Disc("Stone lip",x,.028f,z,1.03f,.055f,trim,site);
                Disc("Level bed",x,.058f,z,.91f,.024f,earth,site);
                foreach(var dir in new[]{Vector2.left,Vector2.right,Vector2.up,Vector2.down})
                    Box("Survey peg",x+.94f*dir.x,.105f,z+.94f*dir.y,.095f,.16f,.095f,Mat("Oak","735B40"),site);
            }
            // Town hall faces the arrival apron in the square. Keep its wings out of both streets.
            House(5.55f,1.45f,2.75f,1.35f,1.55f,0,true);
            Box("Town hall steps",5.55f,.085f,.65f,2.05f,.17f,.55f,trim);
            Box("Town hall doorway",5.55f,.5f,.75f,.48f,.86f,.08f,Mat("Oak","735B40"));
            Box("Belfry",5.55f,2.25f,1.45f,.68f,1.05f,.65f,trim);
            Roof(5.55f,2.76f,1.45f,.9f,.86f,.45f,Mat("Slate","52636B"));
            Box("Civic flagstaff",6.87f,1.16f,.5f,.055f,2.32f,.055f,Mat("Oak","735B40"));
            Box("Town standard",7.12f,2.05f,.5f,.49f,.34f,.04f,Mat("Garrison blue","486680"));
            // Large front-row roofs would hide the road in the normal oblique view.
            // Reserve the traveled envelope plus room around every emplacement before adding a block.
            var random=new System.Random(2904);
            int placed=0;
            for(int row=-4;row<=4;row++)
                for(int col=0;col<9;col++)
                {
                    float x=-8+col*3.45f+(row%2)*.48f,z=row*3.4f+.55f;
                    float w=2.0f+(float)random.NextDouble()*.65f,d=1.38f+(float)random.NextDouble()*.65f;
                    if(!Clear(x,z,w,d))continue;
                    float h=(z<0?.78f:1.02f)+(float)random.NextDouble()*.45f;
                    House(x,z,w,d,h,placed++%5,false);
                }
            var sun=Object.FindAnyObjectByType<Light>();
            sun.shadowStrength=.65f;
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
        static void House(float x,float z,float w,float d,float h,int kind,bool hall)
        {
            footprints.Add(new Rect(x-w/2,z-d/2,w,d));
            var wall=Mat(kind%2==0?"Limewash":"Warm limestone",kind%2==0?"C9C1A9":"B4AA95");
            var roof=kind==2?Mat("Clay tile","9E624F"):Mat("Slate","52636B");
            if(kind==1 || kind==4)
            {
                Box("Roofless plot",x,.07f,z,w,.14f,d,Mat("Exposed floor","8E8374"));
                Box("Surviving party wall",x-w/2+.1f,h*.41f,z,.2f,h*.82f,d,wall);
                Box("Rear wall fragment",x-w*.2f,h*.33f,z+d/2-.1f,w*.6f,h*.66f,.2f,wall);
                Box("Collapsed frontage",x+w*.13f,.16f,z-d/2+.09f,w*.74f,.32f,.18f,wall);
            }
            else
            {
                Box(hall?"Town hall":"Intact house",x,h*.5f,z,w,h,d,wall);
                Roof(x,h,z,w+.12f,d+.14f,.48f,roof);
                if(!hall)Box("Chimney",x-w*.27f,h+.43f,z+.16f,.24f,.65f,.25f,wall);
            }
        }
        static void Roof(float x,float y,float z,float w,float d,float rise,Material finish)
        {
            float angle=Mathf.Atan2(rise,d/2)*Mathf.Rad2Deg;
            foreach(int sign in new[]{-1,1})
            {
                var slope=Box("Roof pitch",x,y+rise/2,z+sign*d/4,w,.095f,Mathf.Sqrt(d*d/4+rise*rise),finish);
                slope.localRotation=Quaternion.Euler(sign*angle,0,0);
            }
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
