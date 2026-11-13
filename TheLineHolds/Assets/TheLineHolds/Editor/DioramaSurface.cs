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
            var m=new Mesh{name="Rolling meadow"};m.vertices=v;m.uv=uv;m.SetTriangles(tris,0);m.RecalculateNormals();m.RecalculateTangents();m.RecalculateBounds();return m;
        }
        public static Texture2D Paint()
        {
            const int width=1536,height=896;
            float[] distance=new float[width*height], along=new float[distance.Length];
            Array.Fill(distance,100f);
            var points=new List<Vector3>(BattleRoute.DefaultPoints);
            points.Insert(0,BattleRoute.OnGround(-12.15f,-5));points.Add(BattleRoute.OnGround(12.15f,5));
            // Rasterize only a narrow band around each segment; deterministic and fast to regenerate.
            float travelled=0;
            for(int k=1;k<points.Count;k++)
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
                    if(d>=distance[i])continue;distance[i]=d;along[i]=travelled+Mathf.Clamp(Vector2.Dot(p-a,ab)/ab.magnitude,0,ab.magnitude);
                }
                travelled+=ab.magnitude;
            }
            var pads=BattleRoute.DefaultPads;
            var approaches=Array.ConvertAll(pads,p=>BattlefieldDetails.NearestRoad(new Vector2(p.x,p.z)));
            var colors=new Color32[width*height];var rng=new System.Random(1944);
            var pathSoil=Hex("9E9475");var gardenEarth=Hex("796448");
            for(int z=0;z<height;z++)for(int x=0;x<width;x++)
            {
                int i=z*width+x;float px=x*24f/(width-1)-12,pz=z*14f/(height-1)-7;
                float fine=Mathf.PerlinNoise(px*8+17,pz*8+9);
                Color c=BattlefieldFarm.Paint(px,pz,FieldPigment.Grass(px,pz));
                float edge=.91f+(Mathf.PerlinNoise(px*3+80,pz*3+80)-.5f)*.32f;
                float blend=1-FieldPigment.Ramp(edge-.09f,edge+.16f,distance[i]);
                if(blend>0)
                {
                    var road=FieldPigment.Road(px,pz,distance[i],along[i]);
                    c=Color.Lerp(c,FieldPigment.GateSetts(px,pz,road),blend);
                }
                float clearing=0,approach=0;
                var siteOffset=Vector2.zero;
                for(int site=0;site<pads.Length;site++)
                {
                    var pad=pads[site];var a=new Vector2(pad.x,pad.z);var ab=approaches[site]-a;var q=new Vector2(px,pz)-a;
                    float trail=(q-ab*Mathf.Clamp01(Vector2.Dot(q,ab)/ab.sqrMagnitude)).magnitude;
                    approach=Mathf.Max(approach,1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.08f,.22f,trail)));
                    float d=new Vector2(px-pad.x,pz-pad.z).magnitude;
                    float radius=.88f+(Mathf.PerlinNoise(px*5+20,pz*5+31)-.5f)*.16f;
                    float weight=1-FieldPigment.Ramp(radius-.09f,radius+.15f,d);
                    if(weight>clearing){clearing=weight;siteOffset=q;}
                }
                c=Color.Lerp(c,pathSoil,approach*.48f*(1-blend));
                if(clearing>0)c=Color.Lerp(c,FieldPigment.GunSite(px,pz,siteOffset,fine),clearing);
                // Authored garden beds sit clear of the road and gun sites.
                float garden=BattlefieldDetails.GardenWeight(px,pz);
                c=Color.Lerp(c,gardenEarth*(.94f+fine*.12f),garden);
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
            var v=new List<Vector3>();var uv=new List<Vector2>();var indices=new[]{new List<int>(),new List<int>(),new List<int>()};
            float perimeter=0;
            for(int edge=0;edge<4;edge++)
            {
                float length=edge%2==0?24:14;int steps=Mathf.CeilToInt(length/.16f);
                Vector2 P(float a)=>edge==0?new Vector2(Mathf.Lerp(-12,12,a),-7):edge==1?new Vector2(12,Mathf.Lerp(-7,7,a)):edge==2?new Vector2(Mathf.Lerp(12,-12,a),7):new Vector2(-12,Mathf.Lerp(7,-7,a));
                for(int j=0;j<steps;j++)
                {
                    var a=P(j/(float)steps);var b=P((j+1)/(float)steps);
                    float ga=BattleRoute.Ground(a.x,a.y),gb=BattleRoute.Ground(b.x,b.y);
                    float wa=(Mathf.PerlinNoise(a.x*.45f+17,a.y*.45f+5)-.5f)*.10f,wb=(Mathf.PerlinNoise(b.x*.45f+17,b.y*.45f+5)-.5f)*.10f;
                    float[] ya={-1.32f,-.67f+wa,ga-.20f+wa*.45f,ga-.008f},yb={-1.32f,-.67f+wb,gb-.20f+wb*.45f,gb-.008f};
                    for(int layer=0;layer<3;layer++)
                    {
                        int start=v.Count;
                        v.Add(new Vector3(a.x,ya[layer],a.y));v.Add(new Vector3(b.x,yb[layer],b.y));
                        v.Add(new Vector3(a.x,ya[layer+1],a.y));v.Add(new Vector3(b.x,yb[layer+1],b.y));
                        // World-sized texels, rather than stretching one blurry tile over the cut height.
                        float u0=(perimeter+length*j/steps)*.6f,u1=(perimeter+length*(j+1)/steps)*.6f;
                        uv.Add(new Vector2(u0,layer==2?0:ya[layer]));uv.Add(new Vector2(u1,layer==2?0:yb[layer]));uv.Add(new Vector2(u0,layer==2?1:ya[layer+1]));uv.Add(new Vector2(u1,layer==2?1:yb[layer+1]));
                        indices[layer].AddRange(new[]{start,start+2,start+1,start+1,start+2,start+3});
                    }
                }
                perimeter+=length;
            }
            var m=new Mesh{name="Stratified earth cut"};m.SetVertices(v);m.SetUVs(0,uv);m.subMeshCount=3;
            for(int i=0;i<3;i++)m.SetTriangles(indices[i],i);
            m.RecalculateNormals();m.RecalculateTangents();m.RecalculateBounds();return m;
        }
        public static Texture2D RootPaint()
        {
            const int width=512,height=128;var pixels=new Color[width*height];var random=new System.Random(694);
            for(int y=0;y<height;y++)for(int x=0;x<width;x++)pixels[y*width+x]=Hex(y>122?"55633A":"484A35")*(.91f+(float)random.NextDouble()*.12f);
            void Line(int x0,int y0,int x1,int y1)
            {
                int steps=Mathf.Max(Mathf.Abs(x1-x0),Mathf.Abs(y1-y0));
                for(int i=0;i<=steps;i++)
                {
                    int x=(Mathf.RoundToInt(Mathf.Lerp(x0,x1,i/(float)Mathf.Max(1,steps)))+width)%width,y=Mathf.RoundToInt(Mathf.Lerp(y0,y1,i/(float)Mathf.Max(1,steps)));
                    if(y>=0 && y<height){pixels[y*width+x]=Hex("776C4D");pixels[y*width+(x+1)%width]=Hex("696247");}
                }
            }
            for(int root=0;root<22;root++)
            {
                int x=random.Next(width),y=127;
                for(int segment=0;segment<4;segment++)
                {
                    int nx=x+random.Next(-10,11),ny=y-random.Next(9,23);Line(x,y,nx,ny);
                    if(segment==1)Line(nx,ny,nx+random.Next(-20,21),ny-random.Next(4,17));
                    x=nx;y=ny;
                }
            }
            var texture=new Texture2D(width,height,TextureFormat.RGB24,true);texture.SetPixels(pixels);texture.Apply();
            string path=Root+"/SodRoots.png";File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.maxTextureSize=512;importer.wrapMode=TextureWrapMode.Repeat;importer.anisoLevel=8;importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        public static Texture2D EarthPaint()
        {
            const int size=512;var colors=new Color[size*size];var rng=new System.Random(1844);
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float grain=(float)rng.NextDouble();
                float broad=Mathf.PerlinNoise(x*.018f+7,y*.015f+17);
                float line=Mathf.Sin(y*.10f+broad*3)*.012f;
                float value=.78f+broad*.14f+grain*.08f+line;
                colors[y*size+x]=new Color(value,value*.99f,value*.94f);
            }
            // Crisp embedded grains and small stones, with restrained sediment lines.
            for(int stone=0;stone<370;stone++)
            {
                int cx=rng.Next(size),cy=rng.Next(size),rx=rng.Next(1,7),ry=rng.Next(1,4);
                float shade=.74f+(float)rng.NextDouble()*.32f;
                for(int dy=-ry;dy<=ry;dy++)for(int dx=-rx;dx<=rx;dx++)
                    if(dx*dx/(float)(rx*rx)+dy*dy/(float)(ry*ry)<1)
                        colors[((cy+dy+size)%size)*size+(cx+dx+size)%size]=new Color(shade,shade*.98f,shade*.90f);
            }
            var texture=new Texture2D(size,size,TextureFormat.RGB24,true);texture.SetPixels(colors);texture.Apply();
            string path=Root+"/EarthLayers.png";File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.maxTextureSize=1024;importer.wrapMode=TextureWrapMode.Repeat;importer.anisoLevel=8;importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.mipmapEnabled=true;importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}
