using System;
using System.Linq;
using UnityEngine;
using TheLineHolds;
public static class CheckOverview
{
 public static string All()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<BattleController>();g.NewMatch(42);var c=g.battleCamera;float aspect=c.aspect;
  try
  {
   foreach(float ratio in new[]{16f/9,4f/3,3032f/2648,1f,.8f})
   {
    c.aspect=ratio;g.CameraControls.ResetView();
    var points=BattleRoute.DefaultPoints.Concat(new[]{BattleRoute.OnGround(-12.25f,-6.6f),BattleRoute.OnGround(-12.25f,-6.6f)+Vector3.up*1.6f,BattleRoute.OnGround(12.25f,5.8f)+Vector3.up*1.6f});
    foreach(var p in points){var v=c.WorldToViewportPoint(p);if(v.z<=0 || v.x<.04f || v.x>.96f || v.y<.13f || v.y>.89f)throw new Exception($"Overview clips at aspect {ratio:0.00}: world={p}, viewport={v}");}
   }
   return "PASS full route, entry and exit fit at five aspect ratios";
  }
  finally{c.ResetAspect();g.CameraControls.ResetView();g.MainMenu();}
 }
}
