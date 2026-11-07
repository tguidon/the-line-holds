using System.IO;
using UnityEditor;
using UnityEngine;
namespace TheLineHolds.Editor
{
    // Original, shared leaf motif. UVs belong to individual leaves, not to a smooth canopy ball.
    public static class FoliageMaterials
    {
        static Texture2D Bake(string name,int size,System.Func<float,float,Color> pixel)
        {
            var image=new Texture2D(size,size,TextureFormat.RGB24,true);var colors=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)colors[y*size+x]=pixel(x/(float)(size-1),y/(float)(size-1));
            image.SetPixels(colors);image.Apply();string path=DioramaSurface.Root+"/"+name+".png";
            File.WriteAllBytes(path,image.EncodeToPNG());Object.DestroyImmediate(image);AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.mipmapEnabled=true;importer.anisoLevel=4;importer.maxTextureSize=512;importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        public static void Configure()
        {
            var leaf=Bake("Broadleaf veins",256,(u,v)=>
            {
                float halfWidth=.5f*Mathf.Pow(Mathf.Sin(v*Mathf.PI),.8f);
                float edge=Mathf.Clamp01(Mathf.Abs(u-.5f)/Mathf.Max(.03f,halfWidth));
                float grain=Mathf.PerlinNoise(u*45+7,v*45+13);
                var c=Color.Lerp(DioramaSurface.Hex("B0B790"),DioramaSurface.Hex("E0D1A3"),v*.35f+grain*.12f);
                c*=Mathf.Lerp(1.04f,.72f,Mathf.Pow(edge,4));
                float rib=Mathf.Exp(-Mathf.Pow((u-.5f)/.016f,2));
                float lateral=Mathf.Abs(Mathf.Sin((v-Mathf.Abs(u-.5f)*.62f)*Mathf.PI*9));
                float veins=Mathf.Max(rib,Mathf.Exp(-lateral*lateral/.009f)*.48f);
                return Color.Lerp(c,DioramaSurface.Hex("DFD5A6"),veins*.48f);
            });
            foreach(var name in new[]{"Leaf","LeafLight","LeafDark"})
            {
                string path=name=="Leaf"?"Assets/TheLineHolds/Art/Materials/Leaf.mat":DioramaSurface.Root+"/"+name+".mat";
                var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
                mat.color=DioramaSurface.Hex(name=="Leaf"?"637B40":name=="LeafLight"?"7F914C":"4F673A");
                mat.mainTexture=leaf;mat.SetFloat("_Cull",0);mat.SetFloat("_Smoothness",.18f);EditorUtility.SetDirty(mat);
            }
            var bags=DioramaBuilder.Material("BagCloth","A18C60");
            bags.mainTexture=Bake("Sandbag weave",128,(u,v)=>
            {
                float thread=(Mathf.Sin(u*Mathf.PI*64)+Mathf.Sin(v*Mathf.PI*64))*.018f;
                float grain=Mathf.PerlinNoise(u*24,v*24)*.06f;
                return Color.white*(.91f+thread+grain);
            });
            bags.SetFloat("_Smoothness",.05f);EditorUtility.SetDirty(bags);
            AssetDatabase.SaveAssets();
        }
    }
}
