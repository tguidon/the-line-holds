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
            const int cells=200;
            var vertices=new Vector3[(cells+1)*(cells+1)];
            var uv=new Vector2[vertices.Length];
            var triangles=new int[cells*cells*6];
            for (int z=0; z<=cells; z++) for (int x=0; x<=cells; x++)
            {
                int i=z*(cells+1)+x;
                float px=Extent*(2f*x/cells-1), pz=Extent*(2f*z/cells-1);
                vertices[i]=new Vector3(px,WinterLayout.Ground(px,pz),pz);
                uv[i]=new Vector2(x/(float)cells,z/(float)cells);
                if (x==cells || z==cells) continue;
                int t=(z*cells+x)*6;
                triangles[t]=i; triangles[t+1]=i+cells+1; triangles[t+2]=i+1;
                triangles[t+3]=i+1; triangles[t+4]=i+cells+1; triangles[t+5]=i+cells+2;
            }
            var mesh=new Mesh {name="Snowfield", vertices=vertices, uv=uv, triangles=triangles};
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
                    distance[index]=Mathf.Min(d,distance[index]);
                }
            }
            var snow=DioramaSurface.Hex("D2E1EB");var track=DioramaSurface.Hex("96A9AF");
            var cleared=DioramaSurface.Hex("958D83");
            var pixels=new Color32[distance.Length];
            var sites=WinterLayout.Pads;
            for(int z=0;z<width;z++) for(int x=0;x<width;x++)
            {
                int i=z*width+x;
                float px=x/scale-Extent,pz=z/scale-Extent;
                var color=Color.Lerp(track,snow,Mathf.InverseLerp(.9f,1.05f,distance[i]));
                foreach(var site in sites)
                {
                    float dx=px-site.x,dz=pz-site.z;
                    if(dx*dx+dz*dz<.96f)color=cleared;
                }
                pixels[i]=color;
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
