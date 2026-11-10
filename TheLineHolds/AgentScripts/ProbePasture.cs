using UnityEngine;
using UnityEditor;
using System.IO;
public static class ProbePasture
{
 public static string All()
 {
  var a=GameObject.Find("Continuous rolling meadow and road").GetComponent<Renderer>().sharedMaterial;var b=GameObject.Find("North pasture").GetComponent<Renderer>().sharedMaterial;
  var ta=new Texture2D(2,2);ta.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(a.mainTexture)));var tb=new Texture2D(2,2);tb.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(b.mainTexture)));
  var core=ta.GetPixelBilinear(.5f,1);var outer=tb.GetPixelBilinear(.5f,97f/180);
  if(Vector3.Distance(new Vector3(core.r,core.g,core.b),new Vector3(outer.r,outer.g,outer.b))>.09f)throw new System.Exception($"Pasture seam: {core} versus {outer}");
  return $"PASS core/outer pasture pigment seam: {core} versus {outer}";
 }
}
