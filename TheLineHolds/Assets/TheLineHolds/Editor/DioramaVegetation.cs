using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheLineHolds.Editor
{
    public static class DioramaVegetation
    {
        public static void Build(Transform parent)
        {
            var route=new BattleRoute(BattleRoute.DefaultPoints);var random=new System.Random(1944);
            var grassV=new List<Vector3>();var grassT=new List<int>();var grassColors=new List<Color>();
            var shrubs=new Dictionary<Material,List<CombineInstance>>();var stones=new List<CombineInstance>();
            var bush=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TheLineHolds/Prefabs/Bush.prefab");
            var sphere=GameObject.CreatePrimitive(PrimitiveType.Sphere);var shape=sphere.GetComponent<MeshFilter>().sharedMesh;
            UnityEngine.Object.DestroyImmediate(sphere);
            float R()=> (float)random.NextDouble();
            float RoadDistance(Vector3 p)
            {
                float min=100;
                for(int i=1;i<route.Points.Length;i++)
                {
                    var a=route.Points[i-1];var b=route.Points[i];a.y=b.y=p.y;var ab=b-a;
                    min=Mathf.Min(min,(p-a-ab*Mathf.Clamp01(Vector3.Dot(p-a,ab)/ab.sqrMagnitude)).magnitude);
                }
                return min;
            }
            for(int i=0;i<2100;i++)
            {
                float x=R()*23.4f-11.7f,z=R()*13.4f-6.7f;var p=BattleRoute.OnGround(x,z);
                if(RoadDistance(p)<.83f || BattleRoute.DefaultPads.Any(pad=>new Vector2(pad.x-x,pad.z-z).magnitude<1.0f))continue;
                float patch=Mathf.PerlinNoise(x*.65f+37,z*.65f+52);
                if(patch<.44f)continue;
                for(int blade=0;blade<5;blade++)
                {
                    float angle=R()*Mathf.PI*2;var side=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
                    var root=p+new Vector3((R()-.5f)*.2f,-.03f,(R()-.5f)*.2f);
                    float h=.09f+R()*.18f,w=.014f+R()*.012f;int index=grassV.Count;
                    grassV.Add(root-side*w);grassV.Add(root+side*w);grassV.Add(root+Vector3.up*h+side*.06f);
                    grassT.AddRange(new[]{index,index+2,index+1});
                    var color=Color.Lerp(DioramaSurface.Hex("61754A"),DioramaSurface.Hex("B4B47C"),R());grassColors.Add(color);grassColors.Add(color);grassColors.Add(color);
                }
                if(i%15==0 && RoadDistance(p)>1.45f && !BattleRoute.DefaultPads.Any(pad=>new Vector2(pad.x-x,pad.z-z).magnitude<1.55f))
                {
                    float s=.22f+R()*.3f;
                    var placement=Matrix4x4.TRS(p,Quaternion.Euler(0,R()*360,0),Vector3.one*(.75f+s));
                    foreach(var filter in bush.GetComponentsInChildren<MeshFilter>())
                    {
                        var renderer=filter.GetComponent<MeshRenderer>();
                        for(int sub=0;sub<filter.sharedMesh.subMeshCount;sub++)
                        {
                            var material=renderer.sharedMaterials[sub];
                            if(!shrubs.TryGetValue(material,out var list)){list=new List<CombineInstance>();shrubs[material]=list;}
                            list.Add(new CombineInstance{mesh=filter.sharedMesh,subMeshIndex=sub,transform=placement*bush.transform.worldToLocalMatrix*filter.transform.localToWorldMatrix});
                        }
                    }
                }
                if(i%23==0)stones.Add(new CombineInstance{mesh=shape,transform=Matrix4x4.TRS(p,Quaternion.Euler(R()*40,R()*360,R()*25),new Vector3(.14f+R()*.17f,.09f+R()*.08f,.13f+R()*.2f))});
            }
            // Combine small details into a handful of draw calls instead of hundreds of objects.
            void Display(string name,Mesh mesh,Material mat)
            {
                var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent,false);
                go.GetComponent<MeshFilter>().sharedMesh=DioramaBuilder.SaveMesh(mesh,name);go.GetComponent<MeshRenderer>().sharedMaterial=mat;
            }
            var grass=new Mesh{name="Meadow grass clusters"};grass.SetVertices(grassV);grass.SetTriangles(grassT,0);grass.SetColors(grassColors);grass.RecalculateNormals();
            var blades=DioramaBuilder.Material("Grass blades","829564");blades.SetFloat("_Cull",0);UnityEditor.EditorUtility.SetDirty(blades);
            Display("Meadow grass clusters",grass,blades);
            foreach(var group in shrubs)
            {
                var leaves=new Mesh{name="Leafy shrubs "+group.Key.name,indexFormat=IndexFormat.UInt32};leaves.CombineMeshes(group.Value.ToArray());
                Display(leaves.name,leaves,group.Key);
            }
            var rocks=new Mesh{name="Weathered field stones"};rocks.CombineMeshes(stones.ToArray());
            Display("Weathered field stones",rocks,DioramaBuilder.Material("Warm limestone","9B9C87"));
        }
    }
}
