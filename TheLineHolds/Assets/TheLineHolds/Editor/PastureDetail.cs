using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace TheLineHolds.Editor
{
    public static class PastureDetail
    {
        // Core-sized grass tufts, spatially batched so the camera need not draw the entire countryside.
        public static void Build(Transform parent)
        {
            var rng=new System.Random(194411);float R()=>(float)rng.NextDouble();var batches=new Dictionary<Vector2Int,(List<Vector3> vertices,List<int> triangles)>();
            for(int attempt=0;attempt<32000;attempt++)
            {
                float x=R()*88-44,z=R()*70-35;var p=new Vector2(x,z);
                if(CountrysideLayout.Reserved(p) || Mathf.PerlinNoise(x*.65f+37,z*.65f+52)<.38f)continue;
                var tile=new Vector2Int(Mathf.FloorToInt(x/10),Mathf.FloorToInt(z/10));if(!batches.TryGetValue(tile,out var data))data=(new List<Vector3>(),new List<int>());
                var ground=BattleRoute.OnGround(x,z)-Vector3.up*.045f;
                bool cleared=BattlefieldFarm.Occupied(p,.05f);
                for(int blade=0;blade<5;blade++)
                {
                    float angle=R()*Mathf.PI*2;var side=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));var root=ground+new Vector3((R()-.5f)*.18f,0,(R()-.5f)*.18f);float h=.10f+R()*.18f,w=.012f+R()*.013f;if(cleared)continue;int i=data.vertices.Count;
                    data.vertices.Add(root-side*w);data.vertices.Add(root+side*w);data.vertices.Add(root+Vector3.up*h+side*.055f);data.triangles.AddRange(new[]{i,i+2,i+1});
                }
                batches[tile]=data;
            }
            var material=DioramaBuilder.Material("Grass blades","829564");material.SetFloat("_Cull",0);EditorUtility.SetDirty(material);
            foreach(var pair in batches)
            {
                string name=$"Pasture grass {pair.Key.x} {pair.Key.y}";var mesh=new Mesh{name=name,indexFormat=IndexFormat.UInt32};mesh.SetVertices(pair.Value.vertices);mesh.SetTriangles(pair.Value.triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
                var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent,false);go.GetComponent<MeshFilter>().sharedMesh=DioramaBuilder.SaveMesh(mesh,name);go.GetComponent<MeshRenderer>().sharedMaterial=material;go.GetComponent<MeshRenderer>().shadowCastingMode=ShadowCastingMode.Off;
            }
        }
    }
}
