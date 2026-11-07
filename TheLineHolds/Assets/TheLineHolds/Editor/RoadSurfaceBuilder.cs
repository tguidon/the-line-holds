using System.Collections.Generic;
using UnityEngine;
namespace TheLineHolds.Editor
{
    // Diagnostic ribbon follows the shared continuous terrain height.
    public static class RoadSurfaceBuilder
    {
        public const float Clearance = .045f;
        public static Mesh Create(BattleRoute route)
        {
            var vertices=new List<Vector3>();var indices=new List<int>();
            int sections=Mathf.CeilToInt(route.Length/.1f);
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
            // Fine samples follow the continuous terrain; the visible road is painted into the turf.
            foreach(var p in new[]{a,b,c})
            {
                var q=p;q.y=BattleRoute.Ground(q.x,q.z)+Clearance;
                indices.Add(vertices.Count);vertices.Add(q);
            }
        }
    }
}
