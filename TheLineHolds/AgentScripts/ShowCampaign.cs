using System.Linq;
using UnityEngine;
using UnityEditor;
using TheLineHolds;
public static class ShowCampaign
{
 public static string Stage()
 {
  var game=Object.FindAnyObjectByType<BattleController>();game.NewMatch(42);
  var old=GameObject.Find("Campaign roster review");if(old)Object.DestroyImmediate(old);var root=new GameObject("Campaign roster review");
  string[] names={"Infantry","Scout","AssaultInfantry","Truck","ArmoredCar","LightTank","HeavyTank"};
  for(int i=0;i<names.Length;i++)
  {
   var definition=AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/TheLineHolds/Data/Enemies/"+names[i]+".asset");
   var p=BattleRoute.OnGround(-7+(i%4)*4,2-(i/4)*4);var view=Object.Instantiate(definition.prefab,p,Quaternion.Euler(0,25,0),root.transform);
   var visual=view.AddComponent<EnemyVisual>();visual.Initialize(definition.spawnSeparation>0);visual.Tick(new EnemyState{Definition=definition,Id=i,Distance=i*.13f},.05f);
  }
  game.CameraControls.Zoom(-4);game.hud.Refresh();Canvas.ForceUpdateCanvases();
  Debug.Log("[Roster review] Seven distinct enemy archetypes; temporary showcase, not a purchased match");return "Seven-role temporary review; stop Play Mode to remove";
 }
}
