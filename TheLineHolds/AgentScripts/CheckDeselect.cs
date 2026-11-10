using System;
using UnityEngine;
using TheLineHolds;
public static class CheckDeselect
{
 public static string All()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<BattleController>();g.NewMatch(42);g.Choose(0);g.ClickPad(0);g.Cancel();g.ClickPad(0);g.ClickPad(-1);
  if(g.SelectedPad!=-1)throw new Exception("Ground click leaves tower selected");
  g.ClickPad(0);g.ClickPad(0);if(g.SelectedPad!=-1)throw new Exception("Repeated tower click leaves tower selected");
  g.MainMenu();return "PASS ground and repeated tower click deselect";
 }
}
