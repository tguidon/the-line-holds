using System.Collections.Generic;
using UnityEngine;
namespace TheLineHolds.Editor
{
    // Clip at every terrain slope boundary so no road triangle bridges two different planes.
    public static class RoadSurfaceBuilder
    {
        public const float Clearance = .045f;
        static readonly float[] Bands = { float.NegativeInfinity, -4, -2, 4, 6, float.PositiveInfinity };
        public static Mesh Create(BattleRoute route)
        {
            var vertices=new List<Vector3>();var indices=new List<int>();
            int sections=Mathf.CeilToInt(route.Length/.2f);
            Vector3[] edge=new Vector3[(sections+1)*2];
            for(int i=0;i<=sections;i++)
            {
                float d=route.Length*i/sections;
                var p=route.Position(d);
                var direction=route.Position(Mathf.Min(route.Length,d+.1f))-route.Position(Mathf.Max(0,d-.1f));
                var side=Vector3.Cross(Vector3.up,direction).normalized*.62f;
                edge[2*i]=p+side;edge[2*i+1]=p-side;
            }
            for(int i=0;i<sections;i++)
            {
                int a=2*i;
                Append(edge[a],edge[a+1],edge[a+2],vertices,indices);
                Append(edge[a+1],edge[a+3],edge[a+2],vertices,indices);
            }
            var mesh=new Mesh();mesh.SetVertices(vertices);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            return mesh;
        }
        static void Append(Vector3 a,Vector3 b,Vector3 c,List<Vector3> vertices,List<int> indices)
        {
            for(int band=0;band<Bands.Length-1;band++)
            {
                var polygon=new List<Vector3>{a,b,c};
                if(!float.IsNegativeInfinity(Bands[band]))polygon=Clip(polygon,Bands[band],true);
                if(!float.IsPositiveInfinity(Bands[band+1]))polygon=Clip(polygon,Bands[band+1],false);
                if(polygon.Count<3)continue;
                for(int i=0;i<polygon.Count;i++)
                {
                    var p=polygon[i];p.y=BattleRoute.Ground(p.x)+Clearance;polygon[i]=p;
                }
                for(int i=1;i<polygon.Count-1;i++)
                {
                    if(Vector3.Cross(polygon[i]-polygon[0],polygon[i+1]-polygon[0]).sqrMagnitude<1e-12f)continue;
                    indices.Add(vertices.Count);vertices.Add(polygon[0]);
                    indices.Add(vertices.Count);vertices.Add(polygon[i]);
                    indices.Add(vertices.Count);vertices.Add(polygon[i+1]);
                }
            }
        }
        static List<Vector3> Clip(List<Vector3> polygon,float x,bool keepRight)
        {
            var result=new List<Vector3>();if(polygon.Count==0)return result;
            var previous=polygon[polygon.Count-1];bool wasInside=keepRight?previous.x>=x:previous.x<=x;
            foreach(var current in polygon)
            {
                bool inside=keepRight?current.x>=x:current.x<=x;
                if(inside!=wasInside)result.Add(Vector3.Lerp(previous,current,(x-previous.x)/(current.x-previous.x)));
                if(inside)result.Add(current);
                previous=current;wasInside=inside;
            }
            return result;
        }
    }
}
