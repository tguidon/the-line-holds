using System.Collections.Generic;
using UnityEngine;

namespace TheLineHolds.Editor
{
    static class BattlefieldFarm
    {
        public static readonly CountrysideLayout.Building[] Buildings = {
            new CountrysideLayout.Building("Barn", -6.55f, 9.3f, .78f, 134, 2.05f),
            new CountrysideLayout.Building("ToolShed", 1.65f, 10.4f, .85f, 150, 1.3f),
            new CountrysideLayout.Building("StoneCottage", -6.3f, -9.5f, .85f, -24, 1.45f)
        };
        static readonly Vector2 field = new Vector2(-2.7f, 10.6f);
        static readonly Color soil = DioramaSurface.Hex("886F50");
        static readonly Color packed = DioramaSurface.Hex("AB9B77");

        static float FieldEdge(Vector2 p) => Mathf.Max(Mathf.Abs(p.x-field.x)-2.55f, Mathf.Abs(p.y-field.y)-1.78f);

        // Leave a headland between the standing grain and the wagon track to the lane.
        static float TrackDistance(Vector2 p)
        {
            var a=new Vector2(-5.9f,7.85f);
            var b=new Vector2(2.6f,8.35f);
            var c=new Vector2(8.1f,5.35f);
            float Segment(Vector2 from,Vector2 to)
            {
                var d=to-from;
                return (p-from-d*Mathf.Clamp01(Vector2.Dot(p-from,d)/d.sqrMagnitude)).magnitude;
            }
            return Mathf.Min(Segment(a,b),Segment(b,c));
        }
        public static bool Occupied(Vector2 p, float margin)
        {
            if(FieldEdge(p)<margin || TrackDistance(p)<.46f+margin)return true;
            foreach(var b in Buildings)if((p-b.Position).sqrMagnitude<Mathf.Pow(b.Radius+margin,2))return true;
            return false;
        }
        public static Color Paint(float x,float z,Color grass)
        {
            if(x < -10 || x>9 || (z<5 && z> -7) || z>14 || z< -13)return grass;
            var p=new Vector2(x,z);
            float variation=Mathf.PerlinNoise(x*2.3f+19,z*2.3f+31);
            float edge=FieldEdge(p)+(variation-.5f)*.13f;
            float bed=1-FieldPigment.Ramp(-.06f,.14f,edge);
            float furrow=.88f+.12f*Mathf.Cos((z-field.y)*Mathf.PI*2/.405f);
            Color ground=Color.Lerp(grass,soil*(furrow+.13f*variation),bed);
            float wear=0;
            foreach(var b in Buildings)
                wear=Mathf.Max(wear,1-FieldPigment.Ramp(b.Radius-.4f,b.Radius+.72f,Vector2.Distance(p,b.Position)));
            float d=TrackDistance(p);
            float rut=1-FieldPigment.Ramp(.04f,.13f,Mathf.Abs(d-.24f));
            float track=(1-FieldPigment.Ramp(.29f,.57f,d))*(.30f+rut*.45f);
            return Color.Lerp(ground,packed*(.9f+variation*.15f),Mathf.Max(wear*.64f,track));
        }

        public static void Plant(Transform parent)
        {
            var vertices=new List<Vector3>();
            var triangles=new[]{new List<int>(),new List<int>(),new List<int>()};
            var rng=new System.Random(194406);
            float Sample()=>(float)rng.NextDouble();
            void Face(int material,Vector3 a,Vector3 b,Vector3 c)
            {
                int i=vertices.Count;
                vertices.Add(a);vertices.Add(b);vertices.Add(c);
                triangles[material].Add(i);triangles[material].Add(i+1);triangles[material].Add(i+2);
            }
            for(int row=-4;row<=4;row++)for(int plant=0;plant<38;plant++)
            {
                float x=field.x-2.38f+plant*.128f;
                float z=field.y+row*.405f+Mathf.Sin(x*1.4f+row*.41f)*.026f;
                // Two shorter rows leave a little stubble at the barn end.
                bool cut=row<=-3 && plant<(row==-4?14:9);
                for(int stalk=0;stalk<4;stalk++)
                {
                    var basePoint=BattleRoute.OnGround(x+(Sample()-.5f)*.14f,z+(Sample()-.5f)*.16f)-Vector3.up*.044f;
                    float height=cut?.10f+Sample()*.05f:.29f+Sample()*.13f;
                    var tip=basePoint+new Vector3(.035f+Sample()*.04f,height,(Sample()-.5f)*.05f);
                    var side=new Vector3(.008f,0,.014f);
                    Face(0,basePoint-side,tip,basePoint+side);
                    if(cut)continue;
                    var leaf=Vector3.Lerp(basePoint,tip,.56f);
                    Face(0,leaf-side,leaf+new Vector3(-.074f,.083f,.03f),leaf+side);
                    var shoulder=tip+new Vector3(.018f,.048f,.004f);
                    var crown=tip+new Vector3(.028f,.14f,.006f);
                    int shade=Sample()<.35f?1:2;
                    foreach(var axis in new[]{Vector3.right*.024f,Vector3.forward*.024f})
                    {
                        Face(shade,tip,shoulder-axis,shoulder+axis);
                        Face(shade,shoulder-axis,crown,shoulder+axis);
                    }
                }
            }
            var mesh=new Mesh{name="Farm wheat rows"};
            mesh.SetVertices(vertices);mesh.subMeshCount=triangles.Length;
            for(int i=0;i<triangles.Length;i++)mesh.SetTriangles(triangles[i],i);
            mesh.RecalculateNormals();mesh.RecalculateBounds();
            var go=new GameObject(mesh.name,typeof(MeshFilter),typeof(MeshRenderer));
            go.transform.SetParent(parent,false);
            go.GetComponent<MeshFilter>().sharedMesh=DioramaBuilder.SaveMesh(mesh,mesh.name);
            var renderer=go.GetComponent<MeshRenderer>();
            renderer.sharedMaterials=new[]{DioramaBuilder.Material("Wheat straw","9F9253"),DioramaBuilder.Material("Wheat ochre","C4A458"),DioramaBuilder.Material("Ripe wheat","DBC483")};
            foreach(var material in renderer.sharedMaterials)material.SetFloat("_Cull",0);
            renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }
}
