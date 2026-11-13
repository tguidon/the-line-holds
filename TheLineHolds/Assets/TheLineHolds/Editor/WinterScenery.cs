using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheLineHolds.Editor
{
    internal sealed class WinterScenery
    {
        const string Root=WinterForestBuilder.Art;
        readonly Transform parent;
        readonly System.Random random=new(1944);
        readonly Vector2[] trail=WinterSurface.Trail;
        readonly Vector3[] sites=WinterLayout.Pads;
        readonly List<Vector2> trunks=new();
        readonly Dictionary<string,GameObject> models=new();
        readonly Dictionary<string,Material> finishes=new();
        readonly Dictionary<(int x,int z),Dictionary<Material,List<CombineInstance>>> chunks=new();
        static readonly Dictionary<string,string> Palette=new() {
            {"Powder","E5EEF3"},{"BlueIce","A7C0D2"},{"Needles","24433F"},{"SpruceTips","45665C"},
            {"FirBark","5F5048"},{"SawnWood","B48B63"},{"CampTimber","846144"},{"PineBoards","AA8057"},
            {"RoofIron","46535A"},{"GlassAmber","D6AE67"},{"SignalRed","A64B36"},{"Canvas","86816C"},
            {"DeepEarth","473F39"},{"Granite","687780"},{"Birch","D6D2C5"}
        };

        internal WinterScenery(Transform parent) { this.parent=parent; }
        float Between(float a,float b)=>Mathf.Lerp(a,b,(float)random.NextDouble());
        float RoadClearance(Vector2 p)
        {
            float best=float.MaxValue;
            for(int i=1;i<trail.Length;i++)
            {
                var d=trail[i]-trail[i-1];var q=p-trail[i-1];
                best=Mathf.Min(best,(q-d*Mathf.Clamp01(Vector2.Dot(q,d)/d.sqrMagnitude)).sqrMagnitude);
            }
            return Mathf.Sqrt(best);
        }
        void Place(string name,float x,float z,float size=1,float yaw=0,float lift=0)
        {
            if(!models.TryGetValue(name,out var model))
            {
                model=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Models/"+name+".fbx");
                if(!model)model=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TheLineHolds/Prefabs/"+name+".prefab");
                if(!model)throw new InvalidOperationException("Missing winter kit piece "+name);
                models.Add(name,model);
            }
            var at=(Mathf.FloorToInt(x/12),Mathf.FloorToInt(z/12));
            if(!chunks.TryGetValue(at,out var batch))chunks.Add(at,batch=new());
            var placement=Matrix4x4.TRS(new Vector3(x,WinterLayout.Ground(x,z)+lift,z),Quaternion.Euler(0,yaw,0),Vector3.one*size);
            foreach(var filter in model.GetComponentsInChildren<MeshFilter>())
            {
                var mesh=filter.sharedMesh;
                var materials=filter.GetComponent<Renderer>().sharedMaterials;
                for(int sub=0;sub<mesh.subMeshCount;sub++)
                {
                    var mat=materials[sub];
                    if(Palette.TryGetValue(mat.name,out var hex))
                    {
                        if(!finishes.TryGetValue(mat.name,out var finish))
                        {
                            finish=WinterForestBuilder.Material(mat.name,DioramaSurface.Hex(hex));
                            finishes.Add(mat.name,finish);
                        }
                        mat=finish;
                    }
                    if(!batch.TryGetValue(mat,out var parts))batch.Add(mat,parts=new());
                    parts.Add(new CombineInstance {mesh=mesh,subMeshIndex=sub,transform=placement*filter.transform.localToWorldMatrix});
                }
            }
        }

        internal static readonly Vector2[] Pits={new(-10.35f,-.05f),new(1.8f,9.0f),new(9.7f,-6.1f)};
        static readonly Vector2 Hut=new(12.6f,-2.85f);
        internal void Build()
        {
            Place("SignalHut",Hut.x,Hut.y,1,135);
            Place("WoodSledge",11,-5.2f,.88f,65);
            Place("Crate",11.75f,-4.65f,.58f,12);
            Place("Crate",12.3f,-4.8f,.53f,-6);
            foreach(var p in Pits)Place("Foxhole",p.x,p.y);
            Place("WoodSledge",-10.7f,6.1f,.78f,60);
            Place("TrailMarker",-11.9f,2.8f,1.16f,134);
            Place("SnowyOutcrop",3.05f,2.9f,1.12f,24);
            Place("SnowyOutcrop",4.15f,2.5f,.70f,62);
            Place("SnowyOutcrop",-6,-3.25f,.52f,15);
            foreach(var p in sites)
                for(int k=0;k<4;k++)Place("BuildStake",p.x+(k%2==0?-.77f:.77f),p.z+(k<2?-.72f:.72f),.9f,k*37);
            // Dense uneven groves around a quieter clearing. Keep foreground crowns out of the firing lanes.
            for(int i=0;i<1600;i++)
            {
                var p=new Vector2(Between(-30,30),Between(-27,27));
                float grove=Mathf.PerlinNoise(p.x*.13f+18,p.y*.14f+33);
                if(grove<.37f || trunks.Any(q=>(p-q).sqrMagnitude<2.6f))continue;
                bool interior=Mathf.Abs(p.x)<14 && p.y>-9 && p.y<9;
                if(interior && grove<.59f)continue;
                if(RoadClearance(p)<(interior?2.2f:1.85f) || (p-Hut).sqrMagnitude<7.5f
                    || sites.Any(s=>new Vector2(s.x-p.x,s.z-p.y).sqrMagnitude<4.8f)
                    || Pits.Any(s=>(s-p).sqrMagnitude<3.4f))continue;
                float size=Between(.47f,1.12f);
                if(interior || (p.y < -6 && p.y > -12 && Mathf.Abs(p.x)<14))size*=.7f;
                // From the reset view the crowns project northwest of their trunks.
                var crown=p+new Vector2(-.57f,.82f)*(size*3.8f);
                if(sites.Any(s=>new Vector2(s.x-crown.x,s.z-crown.y).sqrMagnitude<2.7f)
                    || (p-new Vector2(11.9f,-5.2f)).sqrMagnitude<5)continue;
                Place(random.Next(7)==0?"SilverBirch":"SnowSpruce",p.x,p.y,size,Between(0,360));
                trunks.Add(p);
            }
            // Scatter wind-bared stones and saplings at the margins, avoiding traffic and gun sites.
            for(int i=0;i<230;i++)
            {
                var p=new Vector2(Between(-23,23),Between(-18,18));
                if(RoadClearance(p)<1.25f || sites.Any(s=>new Vector2(s.x-p.x,s.z-p.y).sqrMagnitude<1.6f)
                    || Pits.Any(s=>(s-p).sqrMagnitude<1.8f) || (p-Hut).sqrMagnitude<4)continue;
                Place(random.Next(5)==0?"SnowSpruce":"SnowyOutcrop",p.x,p.y,Between(.11f,.34f),Between(0,360));
            }
            BakeChunks();
        }

        void BakeChunks()
        {
            foreach(var (cell,batches) in chunks)
            {
                var meshes=new List<CombineInstance>();var materials=new List<Material>();
                foreach(var (material,parts) in batches)
                {
                    var group=new Mesh{indexFormat=IndexFormat.UInt32};
                    group.CombineMeshes(parts.ToArray());
                    meshes.Add(new CombineInstance{mesh=group});materials.Add(material);
                }
                string name=$"Forest {cell.x} {cell.z}";
                var mesh=new Mesh{name=name,indexFormat=IndexFormat.UInt32};
                mesh.CombineMeshes(meshes.ToArray(),false,false);
                foreach(var part in meshes)UnityEngine.Object.DestroyImmediate(part.mesh);
                string path=Root+"/"+name+".asset";
                var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if(saved){EditorUtility.CopySerialized(mesh,saved);UnityEngine.Object.DestroyImmediate(mesh);}
                else{saved=mesh;AssetDatabase.CreateAsset(saved,path);}
                var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));
                go.transform.SetParent(parent,false);go.GetComponent<MeshFilter>().sharedMesh=saved;
                go.GetComponent<MeshRenderer>().sharedMaterials=materials.ToArray();
            }
        }
    }
}
