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
            Color lawn=Hex("829259"),yard=Hex("A79D85"),street=Hex("858780"),edge=Hex("BEB7A3"),lane=Hex("B7A181"),square=Hex("B8AF9B");
            Color moss=Hex("77835A"),chalk=Hex("C5B69A"),dirt=Hex("8D795D");
            var pixels=new Color32[field.Length];
            var random=new System.Random(371);
            for(int j=0;j<H;j++)for(int i=0;i<W;i++)
            {
                float x=(i+.5f)/Scale-40,z=(j+.5f)/Scale-30,d=field[j*W+i];
                float patch=Mathf.PerlinNoise(x*.32f+41,z*.38f+20);
                float fleck=Mathf.PerlinNoise(x*2.23f+17,z*2.53f+81);
                float town=Mathf.SmoothStep(0,1,Mathf.InverseLerp(-10.1f,-7.3f,x+patch*1.3f));
                var ground=Color.Lerp(lawn,yard,town);
                float grain=(float)random.NextDouble()*.064f-.032f+(patch-.5f)*.12f+(fleck-.5f)*.08f;
                bool plaza=x>3.95f && x<7.78f && z>-2.83f && z<4.12f;
                if(plaza)ground=square;
                else if(town>.5f)
                {
                    // Courtyard paving, garden strips, and a dirt lip against each row of façades.
                    float band=Mathf.Repeat(z-.55f+1.70f,3.4f)-1.70f;
                    if(band>1.03f)ground=Color.Lerp(yard,moss,Mathf.Clamp01((patch-.40f)*2.8f)*town);
                    else if(band<-.97f)ground=Color.Lerp(ground,chalk,.23f*town);
                }
                float curb=1.39f+(fleck-.5f)*.09f;
                if(d<curb)
                {
                    var paving=Color.Lerp(lane,street,town);
                    float wheel=Mathf.Clamp01(1-Mathf.Abs(d-.62f)*5f);
                    paving=Color.Lerp(paving,dirt,(1-town)*wheel*.39f);
                    grain+=town*(wheel*.029f-.015f);
                    ground=Color.Lerp(paving,Color.Lerp(lane,edge,town),Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.15f,1.26f,d)));
                }
                if(town>.45f && (d<curb || plaza))
                {
                    int row=j/5, col=(i+(row%2)*5)/10;
                    int hash=unchecked(row*73856093 ^ col*19349663);
                    grain+=((hash&255)/255f-.5f)*.076f;
                    if(j%5==0 || (i+row%2*5)%10==0)grain-=.069f;
                }
                else if(d<1.67f && d>=curb)
                    ground=Color.Lerp(ground,dirt,.25f);
                pixels[j*W+i]=new Color(ground.r+grain,ground.g+grain,ground.b+grain);
            }
            var tex=new Texture2D(W,H,TextureFormat.RGB24,false);
            tex.SetPixels32(pixels);tex.Apply();
            string path=TownBuilder.Art+"/Paving.png";
            File.WriteAllBytes(path,tex.EncodeToPNG());Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.npotScale=TextureImporterNPOTScale.None;importer.wrapMode=TextureWrapMode.Clamp;
            importer.SaveAndReimport();
            var mat=TownBuilder.Mat("Street plan","FFFFFF");
            mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            var go=new GameObject("Street plan",typeof(MeshRenderer),typeof(MeshFilter));go.transform.SetParent(root);
            string meshPath=TownBuilder.Art+"/Street surface.asset";
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if(!mesh){mesh=new Mesh();AssetDatabase.CreateAsset(mesh,meshPath);}
            mesh.vertices=new[]{new Vector3(-40,0,-30),new Vector3(-40,0,30),new Vector3(40,0,30),new Vector3(40,0,-30)};
            mesh.uv=new[]{Vector2.zero,Vector2.up,Vector2.one,Vector2.right};mesh.triangles=new[]{0,1,2,0,2,3};
            mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
            go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=mat;
        }
    }
}
