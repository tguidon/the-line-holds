using System.Collections.Generic;
using System.Linq;
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
            var shrubs=new List<CombineInstance>();var stones=new List<CombineInstance>();
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
                    grassT.AddRange(new[]{index,index+2,index+1,index+1,index+2,index});
                    var color=Color.Lerp(DioramaSurface.Hex("61754A"),DioramaSurface.Hex("B4B47C"),R());grassColors.Add(color);grassColors.Add(color);grassColors.Add(color);
                }
                if(i%15==0 && RoadDistance(p)>1.35f)
                {
                    float s=.22f+R()*.3f;
                    for(int k=0;k<3;k++)shrubs.Add(new CombineInstance{mesh=shape,transform=Matrix4x4.TRS(p+new Vector3((R()-.5f)*.5f,s*.25f,(R()-.5f)*.5f),Quaternion.Euler(R()*25,R()*360,0),new Vector3(s*1.8f,s,s*1.4f))});
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
            Display("Meadow grass clusters",grass,DioramaBuilder.Material("Grass blades","829564"));
            var bushes=new Mesh{name="Hedgerow undergrowth",indexFormat=IndexFormat.UInt32};bushes.CombineMeshes(shrubs.ToArray());
            Display("Hedgerow undergrowth",bushes,DioramaBuilder.Material("Hedge foliage","61754A"));
            var rocks=new Mesh{name="Weathered field stones"};rocks.CombineMeshes(stones.ToArray());
            Display("Weathered field stones",rocks,DioramaBuilder.Material("Warm limestone","9B9C87"));
        }
    }
}
