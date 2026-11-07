using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheLineHolds.Editor
{
    // Bake the road into the same continuous surface as the meadow: no coplanar ribbons or seams.
    public static class DioramaSurface
    {
        public const string Root = "Assets/TheLineHolds/Art/Diorama";
        public static Mesh Terrain()
        {
            const int nx=144, nz=84;
            var v=new Vector3[(nx+1)*(nz+1)];var uv=new Vector2[v.Length];var tris=new List<int>();
            for(int z=0;z<=nz;z++)for(int x=0;x<=nx;x++)
            {
                int i=z*(nx+1)+x;float px=x*24f/nx-12,pz=z*14f/nz-7;
                v[i]=new Vector3(px,BattleRoute.Ground(px,pz),pz);uv[i]=new Vector2(x/(float)nx,z/(float)nz);
                if(x==nx || z==nz)continue;
                tris.AddRange(new[]{i,i+nx+1,i+1,i+1,i+nx+1,i+nx+2});
            }
            var m=new Mesh{name="Rolling meadow"};m.vertices=v;m.uv=uv;m.SetTriangles(tris,0);m.RecalculateNormals();m.RecalculateBounds();return m;
        }
        public static Texture2D Paint()
        {
            const int width=1536,height=896;
            float[] distance=new float[width*height], side=new float[distance.Length];
            Array.Fill(distance,100f);
            var points=BattleRoute.DefaultPoints;
            // Rasterize only a narrow band around each segment; deterministic and fast to regenerate.
            for(int k=1;k<points.Length;k++)
            {
                var a=new Vector2(points[k-1].x,points[k-1].z);var b=new Vector2(points[k].x,points[k].z);var ab=b-a;
                int x0=Mathf.Clamp((int)((Mathf.Min(a.x,b.x)+10.8f)/24*width),0,width-1);
                int x1=Mathf.Clamp((int)((Mathf.Max(a.x,b.x)+13.2f)/24*width),0,width-1);
                int z0=Mathf.Clamp((int)((Mathf.Min(a.y,b.y)+5.8f)/14*height),0,height-1);
                int z1=Mathf.Clamp((int)((Mathf.Max(a.y,b.y)+8.2f)/14*height),0,height-1);
                for(int z=z0;z<=z1;z++)for(int x=x0;x<=x1;x++)
                {
                    var p=new Vector2(x*24f/(width-1)-12,z*14f/(height-1)-7);
                    var delta=p-a-ab*Mathf.Clamp01(Vector2.Dot(p-a,ab)/ab.sqrMagnitude);float d=delta.magnitude;int i=z*width+x;
                    if(d>=distance[i])continue;distance[i]=d;side[i]=(ab.x*delta.y-ab.y*delta.x)/ab.magnitude;
                }
            }
            var colors=new Color32[width*height];var rng=new System.Random(1944);
            Color moss=Hex("607044"), grass=Hex("85905B"), straw=Hex("A4A273"), dirt=Hex("AF9D76"), rut=Hex("8F805F");
            for(int z=0;z<height;z++)for(int x=0;x<width;x++)
            {
                int i=z*width+x;float px=x*24f/(width-1)-12,pz=z*14f/(height-1)-7;
                float n=Mathf.PerlinNoise(px*.28f+43,pz*.28f+17), fine=Mathf.PerlinNoise(px*8+17,pz*8+9);
                Color c=Color.Lerp(moss,grass,Mathf.SmoothStep(.1f,.9f,n));
                c=Color.Lerp(c,straw,Mathf.SmoothStep(.54f,.83f,Mathf.PerlinNoise(px*.8f+5,pz*.8f+32))*.35f);
                float edge=.64f+(Mathf.PerlinNoise(px*3+80,pz*3+80)-.5f)*.17f;
                float blend=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(edge-.09f,edge+.16f,distance[i]));
                Color road=Color.Lerp(dirt,rut,Mathf.Exp(-Mathf.Pow((Mathf.Abs(side[i])-.32f)/.10f,2))*.35f);
                road*=.95f+fine*.09f;
                c=Color.Lerp(c,road,blend);
                // Fine pigment grain avoids uniform plastic surfaces; it is filtered by mipmaps.
                c*=.965f+(float)rng.NextDouble()*.07f+(fine-.5f)*.08f;
                colors[i]=c;
            }
            Directory.CreateDirectory(Root);
            var texture=new Texture2D(width,height,TextureFormat.RGB24,true);texture.SetPixels32(colors);texture.Apply();
            string path=Root+"/MeadowRoad.png";File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.wrapMode=TextureWrapMode.Clamp;importer.maxTextureSize=2048;importer.anisoLevel=8;importer.mipmapEnabled=true;importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        public static Color Hex(string hex){ColorUtility.TryParseHtmlString("#"+hex,out var c);return c;}
        public static Mesh Earth()
        {
            var v=new List<Vector3>();var uv=new List<Vector2>();var t=new List<int>();
            for(int edge=0;edge<4;edge++)for(int j=0;j<144;j++)
            {
                Vector2 P(float a)=>edge==0?new Vector2(Mathf.Lerp(-12,12,a),-7):edge==1?new Vector2(12,Mathf.Lerp(-7,7,a)):edge==2?new Vector2(Mathf.Lerp(12,-12,a),7):new Vector2(-12,Mathf.Lerp(7,-7,a));
                var a=P(j/144f);var b=P((j+1)/144f);int i=v.Count;
                v.Add(new Vector3(a.x,-1.32f,a.y));v.Add(new Vector3(b.x,-1.32f,b.y));
                v.Add(new Vector3(a.x,BattleRoute.Ground(a.x,a.y)-.008f,a.y));v.Add(new Vector3(b.x,BattleRoute.Ground(b.x,b.y)-.008f,b.y));
                uv.Add(new Vector2(j/12f,0));uv.Add(new Vector2((j+1)/12f,0));uv.Add(new Vector2(j/12f,1));uv.Add(new Vector2((j+1)/12f,1));
                t.AddRange(new[]{i,i+2,i+1,i+1,i+2,i+3});
            }
            var m=new Mesh{name="Continuous earth cut"};m.SetVertices(v);m.SetUVs(0,uv);m.SetTriangles(t,0);m.RecalculateNormals();return m;
        }
        public static Texture2D EarthPaint()
        {
            var texture=new Texture2D(256,256,TextureFormat.RGB24,true);var c=new Color[256*256];
            for(int y=0;y<256;y++)for(int x=0;x<256;x++)
            {
                float n=Mathf.PerlinNoise(x*.018f,y*.025f);float layer=.5f+.5f*Mathf.Sin(y*.032f+n*1.2f);
                c[y*256+x]=Color.Lerp(Hex("514537"),Hex("786549"),layer*.5f+y/512f)*(n*.16f+.92f);
            }
            texture.SetPixels(c);texture.Apply();string path=Root+"/EarthLayers.png";File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(path);return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}
