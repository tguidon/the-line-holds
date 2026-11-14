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
        static float StoneTint(int row,int col)=>((unchecked(row*73856093 ^ col*19349663)&255)/255f)-.5f;
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
            Color lawn=Hex("7B875F"), sidewalk=Hex("B0B2AB"), street=Hex("787A77"), curb=Hex("C9C9BA");
            Color basalt=Hex("636A6B"), plazaStone=Hex("C4C1B4"), verge=Hex("A5967C");
            float DistanceToLane(Vector2 point,Vector2 start,Vector2 end)
            {
                var vector=end-start;
                return (point-start-vector*Mathf.Clamp01(Vector2.Dot(point-start,vector)/vector.sqrMagnitude)).magnitude;
            }
            var sideStreets=new (Vector2 From,Vector2 To)[] {
                (new(-28,8),new(-11.7f,7.9f)), (new(-11.7f,7.9f),new(3.3f,7.2f)),
                (new(2.8f,7.4f),new(3.9f,18)), (new(3.9f,8.65f),new(22,8.65f)),
                (new(-25,-7.0f),new(-4.7f,-7.03f)), (new(-4.7f,-7.03f),new(.65f,-7.56f)),
                (new(-15.1f,-15),new(-14.8f,-4.08f)), (new(15.25f,-15),new(15.27f,8.64f)),
                (new(12.2f,-9.03f),new(22,-9.03f)), (new(-13,13.26f),new(24,13.26f))
            };
            var pixels=new Color32[field.Length];
            var random=new System.Random(371);
            for(int j=0;j<H;j++)for(int i=0;i<W;i++)
            {
                float x=(i+.5f)/Scale-40,z=(j+.5f)/Scale-30,d=field[j*W+i];
                float town=Mathf.SmoothStep(0,1,Mathf.InverseLerp(-15.8f,-13.4f,x));
                float patch=Mathf.PerlinNoise(x*.68f+41,z*.53f+20)-.5f;
                bool plaza=x>=4.55f && x<=11.76f && z>=1.12f+(10.4f-x)*.28f && z<6.85f;
                var ground=Color.Lerp(lawn,plaza?plazaStone:sidewalk,town);
                float grain=(float)(random.NextDouble()-.5)*.032f+patch*.042f;
                if(d<1.19f)
                {
                    ground=Color.Lerp(verge,street,town);
                    int row=j/4,col=(i+(row%2)*4)/8;
                    grain+=StoneTint(row,col)*.060f;
                    if(j%4==0 || (i+(row%2)*4)%8==0)grain-=.068f*town;
                }
                else if(d<1.38f)
                {
                    ground=Color.Lerp(verge,curb,town);
                    if((i+j)%16==0)grain-=.085f;
                }
                else
                {
                    // Large sawn slabs on the footways. The square has nested bands of dark setts.
                    int row=j/12,col=(i+(row%2)*11)/22;
                    grain+=StoneTint(row,col)*.048f*town;
                    if(j%12==0 || (i+(row%2)*11)%22==0)grain-=.068f*town;
                    if(plaza)
                    {
                        float ring=Vector2.Distance(new Vector2(x,z),new Vector2(9.81f,4.44f));
                        if(Mathf.Abs(ring-1.34f)<.095f || Mathf.Abs(ring-1.57f)<.055f)ground=basalt;
                    }
                    bool court=(x>-5.8f && x<-2.5f && z>9.35f && z<10.6f)
                        || (x>4.6f && x<10.7f && z>8.0f && z<9.4f)
                        || (x>-8.4f && x<-5.8f && z>-6.8f && z<-5.0f);
                    if(court)ground=lawn;
                    if(town>.5f && !plaza && !court)
                    {
                        var point=new Vector2(x,z);
                        foreach(var lane in sideStreets)
                        {
                            float sideDistance=DistanceToLane(point,lane.From,lane.To);
                            if(sideDistance>.54f)continue;
                            ground=sideDistance>.45f?curb:Color.Lerp(sidewalk,basalt,.52f);
                            if(j%4==0 || (i+j/4%2*4)%8==0)grain-=.047f;
                            break;
                        }
                    }
                }
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
