using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TheLineHolds.Editor
{
    internal static class WinterSurface
    {
        const float Extent=40;
        const string Root=WinterForestBuilder.Art;
        internal static Vector2[] Trail => WinterLayout.Approach.Skip(1).Reverse()
            .Concat(WinterLayout.Route().Points.Select(p=>new Vector2(p.x,p.z))).ToArray();

        internal static void Build(Transform root)
        {
            const int cells=260;
            var vertices=new Vector3[(cells+1)*(cells+1)];
            var uv=new Vector2[vertices.Length];
            var triangles=new int[cells*cells*6];
            for (int z=0; z<=cells; z++) for (int x=0; x<=cells; x++)
            {
                int i=z*(cells+1)+x;
                float px=Extent*(2f*x/cells-1), pz=Extent*(2f*z/cells-1);
                float ground=WinterLayout.Ground(px,pz);
                foreach(var pit in WinterScenery.Pits)
                {
                    float dx=(px-pit.x)/.99f,dz=(pz-pit.y)/.64f;
                    ground-=.46f*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.38f,1,dx*dx+dz*dz)));
                }
                vertices[i]=new Vector3(px,ground,pz);
                uv[i]=new Vector2(x/(float)cells,z/(float)cells);
                if (x==cells || z==cells) continue;
                int t=(z*cells+x)*6;
                triangles[t]=i; triangles[t+1]=i+cells+1; triangles[t+2]=i+1;
                triangles[t+3]=i+1; triangles[t+4]=i+cells+1; triangles[t+5]=i+cells+2;
            }
            var mesh=new Mesh {name="Snowfield", indexFormat=UnityEngine.Rendering.IndexFormat.UInt32, vertices=vertices, uv=uv, triangles=triangles};
            mesh.RecalculateNormals();
            string path=Root+"/Snowfield.asset";
            var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (saved) { EditorUtility.CopySerialized(mesh,saved); UnityEngine.Object.DestroyImmediate(mesh); }
            else { saved=mesh; AssetDatabase.CreateAsset(saved,path); }
            var go=new GameObject("Snowfield",typeof(MeshFilter),typeof(MeshRenderer));
            go.transform.SetParent(root,false);
            go.GetComponent<MeshFilter>().sharedMesh=saved;
            var material=WinterForestBuilder.Material("Snow and packed track",Color.white);
            material.mainTexture=Paint();
            go.GetComponent<MeshRenderer>().sharedMaterial=material;
        }

        static Texture2D Paint()
        {
            const int width=2048;
            float scale=(width-1)/(2*Extent);
            var distance=new float[width*width];
            var along=new float[distance.Length];
            float travelled=0;
            Array.Fill(distance,100f);
            var path=Trail;
            for(int j=1;j<path.Length;j++)
            {
                var a=path[j-1];var b=path[j];var ab=b-a;
                int Pixel(float coordinate)=>Mathf.Clamp(Mathf.RoundToInt((coordinate+Extent)*scale),0,width-1);
                for(int z=Pixel(Mathf.Min(a.y,b.y)-1.5f); z<=Pixel(Mathf.Max(a.y,b.y)+1.5f);z++)
                for(int x=Pixel(Mathf.Min(a.x,b.x)-1.5f); x<=Pixel(Mathf.Max(a.x,b.x)+1.5f);x++)
                {
                    var q=new Vector2(x/scale-Extent,z/scale-Extent)-a;
                    float d=(q-ab*Mathf.Clamp01(Vector2.Dot(q,ab)/ab.sqrMagnitude)).magnitude;
                    int index=z*width+x;
                    if(d>=distance[index])continue;
                    distance[index]=d;along[index]=travelled+Mathf.Clamp(Vector2.Dot(q,ab)/ab.magnitude,0,ab.magnitude);
                }
                travelled+=ab.magnitude;
            }
            var snow=DioramaSurface.Hex("DBE5EC");var track=DioramaSurface.Hex("ABBAC1");
            var cleared=DioramaSurface.Hex("A79D8D");var rut=DioramaSurface.Hex("827F7A");
            var lee=DioramaSurface.Hex("ADBFCE");var slush=DioramaSurface.Hex("9DA8AB");
            var grain=new System.Random(1294);
            var pixels=new Color32[distance.Length];
            var sites=WinterLayout.Pads;
            var earth=DioramaSurface.Hex("584D43");
            for(int z=0;z<width;z++) for(int x=0;x<width;x++)
            {
                int i=z*width+x;
                float px=x/scale-Extent,pz=z/scale-Extent;
                float broad=Mathf.PerlinNoise(px*.19f+51,pz*.17f+17);
                float streak=Mathf.PerlinNoise(px*.62f+17,pz*2.9f+px*.40f+48);
                float fine=Mathf.PerlinNoise(px*5.6f+44,pz*5.6f+22);
                float grainValue=(float)grain.NextDouble();
                var color=Color.Lerp(lee,snow,.29f+.65f*broad+.26f*streak);
                float d=distance[i],edge=.99f+.22f*(fine-.5f)+.14f*(streak-.5f);
                if(d<1.4f)
                {
                    float wheel=Mathf.Abs(d-.47f-.061f*(streak-.5f));
                    float worn=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.044f,.16f,wheel));
                    var road=Color.Lerp(slush,track,broad*.75f+fine*.38f);
                    road=Color.Lerp(road,rut,worn*(.53f+.34f*fine+.1f*Mathf.Sin(along[i]*35)));
                    color=Color.Lerp(road,color,Mathf.SmoothStep(0,1,Mathf.InverseLerp(edge-.075f,edge+.13f,d)));
                    color=Color.Lerp(color,snow,.40f*Mathf.Clamp01(1-Mathf.Abs(d-edge-.13f)/.13f));
                }
                // Paired sled runners end beside the depot, with boot prints alongside the load.
                float haul=px-8.4f, side=pz+6.1f-haul*.38f;
                if(haul>0 && haul<3.2f)
                {
                    float marks=Mathf.Clamp01(1-Mathf.Abs(Mathf.Abs(side)-.24f)/.055f);
                    float boot=Mathf.Repeat(haul+.18f*Mathf.Sign(side-.66f),.43f)-.215f;
                    float print=Mathf.Clamp01(1-boot*boot/.009f-Mathf.Pow((Mathf.Abs(side-.66f)-.075f)/.055f,2));
                    color=Color.Lerp(color,slush,(marks*.52f+print*.66f)*Mathf.Clamp01(haul*3));
                }
                foreach(var site in sites)
                {
                    float dx=px-site.x,dz=pz-site.z;
                    float r2=dx*dx+dz*dz;
                    if(r2>1.46f)continue;
                    float r=Mathf.Sqrt(r2);
                    float rim=.94f+.08f*(fine-.5f);
                    float cut=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(rim-.04f,rim+.08f,r));
                    var pad=Color.Lerp(cleared,slush,.25f+.32f*fine);
                    float boards=Mathf.Abs(Mathf.Repeat(dz*4.8f+dx*.7f,1)-.5f);
                    pad*=.94f+.07f*fine+.055f*Mathf.Clamp01(boards*9);
                    color=Color.Lerp(color,pad,cut);
                    color=Color.Lerp(color,snow,.67f*Mathf.Clamp01(1-Mathf.Abs(r-rim-.12f)/.09f));
                }
                foreach(var pit in WinterScenery.Pits)
                {
                    float dx=(px-pit.x)/1.03f,dz=(pz-pit.y)/.68f;
                    float radius=dx*dx+dz*dz;
                    if(radius<1.8f)color=Color.Lerp(earth*(.86f+.35f*fine),color,
                        Mathf.SmoothStep(0,1,Mathf.InverseLerp(.63f,1.45f+.2f*fine,radius)));
                }
                pixels[i]=color*(.955f+.04f*grainValue+.055f*fine);
            }
            var texture=new Texture2D(width,width,TextureFormat.RGB24,false);
            texture.SetPixels32(pixels);texture.Apply();
            string file=Root+"/ForestFloor.png";
            File.WriteAllBytes(file,texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(file);
            var importer=(TextureImporter)AssetImporter.GetAtPath(file);
            importer.wrapMode=TextureWrapMode.Clamp;importer.anisoLevel=8;importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(file);
        }
    }
}
