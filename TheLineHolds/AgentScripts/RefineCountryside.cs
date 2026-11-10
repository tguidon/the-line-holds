using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TheLineHolds.Editor;
public static class RefineCountryside
{
 public static string All()
 {
  if(EditorApplication.isPlaying)throw new System.Exception("Stop agent Play Mode first");
  DioramaBuilder.RefreshModels(new[]{"StoneCottage","VillageInn","Dairy","ToolShed","VillageWell"});
  var parent=GameObject.Find("Diorama landscape").transform;var old=GameObject.Find("Surrounding countryside");if(old)Object.DestroyImmediate(old);
  CountrysideSurroundings.Build(parent);EditorSceneManager.MarkSceneDirty(parent.gameObject.scene);EditorSceneManager.SaveScene(parent.gameObject.scene);AssetDatabase.SaveAssets();
  return "Saved connected country lanes, five new architectural types, farm plots/hedges/orchard and spatial grass detail; core battlefield preserved";
 }
}
