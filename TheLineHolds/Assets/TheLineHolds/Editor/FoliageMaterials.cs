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
        // Shared, mipmapped finishes. The large-scale tonal variation survives the overview camera.
        internal static void FinishSurfaces()
        {
            var oak=Bake("Seasoned oak",256,(u,v)=>
            {
                float knot=Mathf.PerlinNoise(u*4+3,v*3+7);
                float grain=Mathf.PerlinNoise(u*7+19,v*52+knot*4);
                float ring=Mathf.Sin((v*19+knot*.8f)*Mathf.PI*2);
                return Color.white*(.69f+grain*.24f+ring*.045f);
            });
            var distemper=Bake("Lime and patina",256,(u,v)=>
            {
                float mottling=Mathf.PerlinNoise(u*7+21,v*6+16);
                float chips=FieldPigment.Ramp(.63f,.82f,Mathf.PerlinNoise(u*43+6,v*38+11));
                return Color.white*(.81f+mottling*.17f-chips*.16f);
            });
            void Coat(string name,string color,float smoothness,Texture2D map)
            {
                var mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/TheLineHolds/Art/Materials/"+name+".mat");
                if(!mat)mat=AssetDatabase.LoadAssetAtPath<Material>(DioramaSurface.Root+"/"+name+".mat");
                mat.color=DioramaSurface.Hex(color);mat.mainTexture=map;
                mat.SetFloat("_Smoothness",smoothness);EditorUtility.SetDirty(mat);
            }
            Coat("Wood","927456",.15f,oak);
            Coat("ShedPaint","607E80",.19f,oak);
            Coat("RoofClay","A45741",.13f,distemper);
            Coat("RoofClayLight","BD7654",.14f,distemper);
            Coat("RoofSlate","526775",.22f,distemper);
            Coat("RoofSlateLight","728493",.22f,distemper);
            Coat("Masonry","B0AA94",.13f,distemper);
            Coat("Plaster","CCBD97",.10f,distemper);
            Coat("Steel","59656B",.38f,distemper);
            Coat("Olive","5D6953",.24f,distemper);
            Coat("Cloth","747C70",.08f,distemper);
            Coat("BagCloth","9D9279",.07f,AssetDatabase.LoadAssetAtPath<Texture2D>(DioramaSurface.Root+"/Sandbag weave.png"));
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
                mat.color=DioramaSurface.Hex(name=="Leaf"?"5E784D":name=="LeafLight"?"849A5E":"4B6646");
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
            FinishSurfaces();
            AssetDatabase.SaveAssets();
        }
    }
}
