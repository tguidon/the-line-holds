using System.Collections.Generic;
using UnityEngine;

namespace TheLineHolds.Editor
{
    static class VergeFlowers
    {
        public static void Build(Transform parent)
        {
            // Loose groups against the garden walls, gate and field edges. The road crown stays clear.
            var centres=new[]{new Vector2(-11.55f,-3.43f),new Vector2(-10.2f,.96f),
                new Vector2(-9.65f,2.67f),new Vector2(-2.15f,-6.54f),new Vector2(3.2f,-6.16f),
                new Vector2(10.54f,-2.63f),new Vector2(10.72f,6.56f)};
            var random=new System.Random(1927);
            float Sample()=>(float)random.NextDouble();
            var vertices=new List<Vector3>();
            var indices=new[]{new List<int>(),new List<int>(),new List<int>()};
            void Triangle(int surface,Vector3 a,Vector3 b,Vector3 c)
            {
                int i=vertices.Count;
                vertices.Add(a);vertices.Add(b);vertices.Add(c);
                indices[surface].Add(i);indices[surface].Add(i+1);indices[surface].Add(i+2);
            }
            for(int group=0;group<centres.Length;group++)
            {
                for(int plant=0;plant<26;plant++)
                {
                    var c=centres[group]+new Vector2((Sample()+Sample()-1)*.88f,(Sample()+Sample()-1)*.51f);
                    var root=BattleRoute.OnGround(c.x,c.y)-Vector3.up*.035f;
                    var tip=root+new Vector3(Sample()*.04f,.12f+Sample()*.13f,Sample()*.035f);
                    Triangle(0,root-Vector3.right*.009f,tip,root+Vector3.right*.009f);
                    Triangle(0,root,root+new Vector3(-.064f,.091f,.024f),root+Vector3.up*.068f);
                    float angle=Sample()*Mathf.PI*2;
                    float size=.028f+Sample()*.031f;
                    for(int petal=0;petal<5;petal++)
                    {
                        float a=angle+petal*Mathf.PI*.4f;
                        var ray=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
                        var side=new Vector3(-ray.z,0,ray.x)*size*.50f;
                        var rim=tip+ray*size+Vector3.up*.012f;
                        Triangle(group%3==0?1:2,tip,rim+side,rim-side);
                    }
                }
            }
            var mesh=new Mesh{name="Flowers at field boundaries",subMeshCount=3};
            mesh.SetVertices(vertices);
            for(int i=0;i<3;i++)mesh.SetTriangles(indices[i],i);
            mesh.RecalculateNormals();mesh.RecalculateBounds();
            var go=new GameObject(mesh.name,typeof(MeshFilter),typeof(MeshRenderer));
            go.transform.SetParent(parent,false);
            go.GetComponent<MeshFilter>().sharedMesh=DioramaBuilder.SaveMesh(mesh,mesh.name);
            var red=DioramaBuilder.Material("Field poppy","B66450");
            var cream=DioramaBuilder.Material("Cow parsley","D4CC9E");
            red.SetFloat("_Cull",0);cream.SetFloat("_Cull",0);
            var renderer=go.GetComponent<MeshRenderer>();
            renderer.sharedMaterials=new[]{DioramaBuilder.Material("Grass blades","829564"),red,cream};
            renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }
}
