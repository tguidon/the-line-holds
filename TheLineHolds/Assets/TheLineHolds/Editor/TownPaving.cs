using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheLineHolds.Editor
{
    // Rasterize the movement centerlines once. Taking the nearest distance makes one continuous
    // paved surface at the Y junction, without overlapping road meshes or a seam in the wheelway.
    internal static class TownPaving
    {
        const int Scale=24,W=80*Scale,H=60*Scale;
        public static void Build(Transform root,BattleRoute a,BattleRoute b)
        {
            var field=new float[W*H];
            System.Array.Fill(field,9f);
            void Segment(Vector3 p,Vector3 q)
            {
                var start=new Vector2(p.x,p.z);var v=new Vector2(q.x-p.x,q.z-p.z);
                int X(float x)=>Mathf.Clamp(Mathf.FloorToInt((x+40)*Scale),0,W-1);
                int Z(float z)=>Mathf.Clamp(Mathf.FloorToInt((z+30)*Scale),0,H-1);
                for(int j=Z(Mathf.Min(p.z,q.z)-1.7f);j<=Z(Mathf.Max(p.z,q.z)+1.7f);j++)
                    for(int i=X(Mathf.Min(p.x,q.x)-1.7f);i<=X(Mathf.Max(p.x,q.x)+1.7f);i++)
                    {
                        var d=new Vector2((i+.5f)/Scale-40,(j+.5f)/Scale-30)-start;
                        float t=Mathf.Clamp01(Vector2.Dot(d,v)/v.sqrMagnitude);
                        int k=j*W+i;field[k]=Mathf.Min(field[k],(d-v*t).magnitude);
                    }
            }
            foreach(var route in new[]{a,b})
            {
                Segment(route.Position(-42),route.Points[0]);
                for(int i=1;i<route.Points.Length;i++)Segment(route.Points[i-1],route.Points[i]);
            }
            Color Hex(string h)=>DioramaSurface.Hex(h);
            Color lawn=Hex("85966B"),yard=Hex("A59C87"),street=Hex("777A75"),edge=Hex("CCC4B1"),lane=Hex("BAA889"),square=Hex("BCB29B");
            var pixels=new Color32[field.Length];
            var random=new System.Random(371);
            for(int j=0;j<H;j++)for(int i=0;i<W;i++)
            {
                float x=(i+.5f)/Scale-40,z=(j+.5f)/Scale-30,d=field[j*W+i];
                float town=Mathf.SmoothStep(0,1,Mathf.InverseLerp(-9.4f,-8.5f,x));
                var ground=Color.Lerp(lawn,yard,town);
                if(x>4.1f && x<7.65f && z>-2.8f && z<2.75f)ground=square;
                float grain=(float)random.NextDouble()*.038f-.019f;
                if(d<1.4f)
                {
                    var paving=Color.Lerp(lane,street,town);
                    // A restrained sett pattern distinguishes the street from the building plots.
                    if(town>.5f && (j%5==0 || (i+(j/5%2)*5)%11==0))grain-=.039f;
                    ground=Color.Lerp(paving,Color.Lerp(lane,edge,town),Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.14f,1.24f,d)));
                }
                else grain+=(Mathf.PerlinNoise(x*.52f+91,z*.47f+65)-.5f)*.085f;
                pixels[j*W+i]=new Color(ground.r+grain,ground.g+grain,ground.b+grain);
            }
            var tex=new Texture2D(W,H,TextureFormat.RGB24,false);
            tex.SetPixels32(pixels);tex.Apply();
            string path=TownBlockoutBuilder.Art+"/Paving.png";
            File.WriteAllBytes(path,tex.EncodeToPNG());Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.npotScale=TextureImporterNPOTScale.None;importer.wrapMode=TextureWrapMode.Clamp;
            importer.SaveAndReimport();
            var mat=TownBlockoutBuilder.Mat("Street plan","FFFFFF");
            mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            var go=new GameObject("Street plan",typeof(MeshRenderer),typeof(MeshFilter));go.transform.SetParent(root);
            string meshPath=TownBlockoutBuilder.Art+"/Street surface.asset";
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if(!mesh){mesh=new Mesh();AssetDatabase.CreateAsset(mesh,meshPath);}
            mesh.vertices=new[]{new Vector3(-40,0,-30),new Vector3(-40,0,30),new Vector3(40,0,30),new Vector3(40,0,-30)};
            mesh.uv=new[]{Vector2.zero,Vector2.up,Vector2.one,Vector2.right};mesh.triangles=new[]{0,1,2,0,2,3};
            mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
            go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=mat;
        }
    }
}
