using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ProbePasture
{
    static Texture2D ReadPigment(string surface)
    {
        var source=GameObject.Find(surface).GetComponent<Renderer>().sharedMaterial.mainTexture;
        var copy=new Texture2D(2,2);
        copy.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(source)));
        copy.wrapMode=source.wrapMode;
        return copy;
    }

    public static string All()
    {
        var core=ReadPigment("Continuous rolling meadow and road");
        var apron=ReadPigment("North pasture");
        try
        {
            foreach(float z in new[]{-7f,7f})
            {
                var a=core.GetPixelBilinear(.5f,(z+7)/14);
                var b=apron.GetPixelBilinear(.5f,(z+90)/180);
                if(Vector3.Distance(new Vector3(a.r,a.g,a.b),new Vector3(b.r,b.g,b.b))>.09f)
                    throw new Exception($"Pasture seam at z={z}: {a} versus {b}");
            }
            return "PASS north and south pasture pigments meet at the core boundary";
        }
        finally { UnityEngine.Object.DestroyImmediate(core); UnityEngine.Object.DestroyImmediate(apron); }
    }
}
