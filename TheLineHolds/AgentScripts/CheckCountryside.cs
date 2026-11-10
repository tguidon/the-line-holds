using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using TheLineHolds;
using TheLineHolds.Editor;
public static class CheckCountryside
{
 static void Check(bool b,string m){if(!b)throw new Exception(m);}
 static float SegmentDistance(Vector2 p,Vector2 a,Vector2 b){var d=b-a;return (p-a-d*Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude)).magnitude;}
 public static string All()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<BattleController>();Check(g.padViews.Select(t=>t.position).SequenceEqual(BattleRoute.DefaultPads),"Core tower anchors changed");
  Check(CountrysideLayout.Buildings.Select(b=>b.Key).Distinct().Count()>=6,"Architecture still repeats two models");
  foreach(var b in CountrysideLayout.Buildings){Check(!CountrysideLayout.InCore(b.Position,b.Radius),"Building reaches core: "+b.Key);var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TheLineHolds/Prefabs/"+b.Key+".prefab");Check(prefab && prefab.GetComponentsInChildren<MeshFilter>().All(f=>f.sharedMesh.vertexCount>=3 && f.sharedMesh.vertices.All(v=>!float.IsNaN(v.x) && !float.IsNaN(v.y) && !float.IsNaN(v.z))),"Imported architecture invalid");}
  for(int r=0;r<CountrysideLayout.Roads.Length;r++)foreach(var p in new[]{CountrysideLayout.Roads[r].First(),CountrysideLayout.Roads[r].Last()})
  {
   bool linked=Mathf.Abs(p.x)>=89 || Mathf.Abs(p.y)>=89 || Vector2.Distance(p,new Vector2(-12,-5))<.1f || Vector2.Distance(p,new Vector2(12,5))<.1f || CountrysideLayout.Buildings.Any(b=>Vector2.Distance(p,b.Position)<b.Radius+1.3f);
   for(int other=0;other<CountrysideLayout.Roads.Length && !linked;other++)if(other!=r)for(int i=1;i<CountrysideLayout.Roads[other].Length;i++)if(SegmentDistance(p,CountrysideLayout.Roads[other][i-1],CountrysideLayout.Roads[other][i])<.08f){linked=true;break;}
   Check(linked,"Road ends in empty terrain: "+p);
  }
  var root=GameObject.Find("Surrounding countryside");var grass=root.GetComponentsInChildren<MeshFilter>().Where(f=>f.name.StartsWith("Pasture grass ")).ToArray();int vertices=grass.Sum(f=>f.sharedMesh.vertexCount);
  Check(grass.Length>=30 && vertices>90000,"Outer grass detail missing");
  Check(root.GetComponentsInChildren<Renderer>().All(r=>r.sharedMaterials.All(m=>m && m.name!="Default-Material" && m.shader.isSupported)),"Unstyled/unsupported scenery material");
  return $"PASS core sites unchanged; {CountrysideLayout.Buildings.Length} varied structures / {CountrysideLayout.Buildings.Select(b=>b.Key).Distinct().Count()} types; all {CountrysideLayout.Roads.Length} road chains connected; {grass.Length} grass tiles / {vertices} vertices; styled materials";
 }
}
