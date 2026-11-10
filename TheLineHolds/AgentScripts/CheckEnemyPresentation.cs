using System;
using System.Linq;
using UnityEngine;
using TheLineHolds;
public static class CheckEnemyPresentation
{
 public static string All()
 {
  var game=UnityEngine.Object.FindAnyObjectByType<BattleController>();game.NewMatch();
  var state=new EnemyState{Id=900,Definition=game.waves[0].spawns[0].enemy,Position=game.Match.Pads[3],Health=50,Distance=1};
  var view=UnityEngine.Object.Instantiate(state.Definition.prefab,state.Position,Quaternion.identity);var v=view.AddComponent<EnemyVisual>();v.Initialize(false);
  try
  {
   var root=view.transform.position;var body=view.transform.Find("Miniature");v.Tick(state,.05f);var first=body.localPosition;state.Distance+=.13f;v.Tick(state,.05f);
   if(first==body.localPosition || view.transform.position!=root)throw new Exception("Gait missing or changed gameplay root");
   v.Hit(.25f);v.Tick(state,.03f);var pose=body.localRotation;v.Tick(state,0);if(pose!=body.localRotation)throw new Exception("Paused pose drift");
   v.Die();v.Tick(state,.15f);var fall=body.localRotation;v.Tick(state,.2f);if(fall==body.localRotation)throw new Exception("Fall not animated");
   v.Tick(state,1);v.Initialize(false);if(body.localScale==Vector3.zero || v.Dying)throw new Exception("Pooled actor failed to reset");
   for(int i=0;i<20;i++){game.EnemyViews.Hit(state);game.EnemyViews.Kill(state);game.EnemyViews.Sync(game.Match,2);}
   if(game.EnemyViews.Created>1 || game.EnemyViews.RetiringCount!=0)throw new Exception("Actor pool or corpse cleanup failed");
   game.MainMenu();Debug.Log("[Enemy presentation] PASS gait/root isolation, hit/pause, timed fall, reset and 20 pooled defeat cycles");return "PASS gait/root isolation, hit/pause, timed fall, reset and 20 pooled defeat cycles";
  }finally{UnityEngine.Object.Destroy(view);game.MainMenu();}
 }
}
