using System;
using System.Linq;
using UnityEngine;
using TMPro;
using TheLineHolds;
public static class CheckTacticalPause
{
 static void Check(bool b,string m){if(!b)throw new Exception(m);}
 public static string Orders()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<BattleController>();var m=new MatchState(g.waves,new BattleRoute(BattleRoute.DefaultPoints),BattleRoute.DefaultPads,42);m.Paused=true;
  Check(m.Build(0,g.defenses[0]),"Paused build rejected");Check(m.Upgrade(0,0),"Paused upgrade rejected");Check(m.Funds==135,"Upgrade accounting");
  Check(!m.Upgrade(0,0) && m.Funds==135,"Paused unaffordable upgrade spent funds");Check(m.Sell(0) && m.Funds==217,"Paused sale accounting");Check(!m.StartWave() && m.Wave==0,"Paused wave advanced");
  m.Paused=false;m.Build(0,g.defenses[0]);m.StartWave();for(int i=0;i<600 && m.Shots.Count==0;i++)m.Step(1f/60);Check(m.Enemies.Count>0 && m.Shots.Count>0,"Freeze fixture requires live enemies and an in-flight shot");m.Paused=true;
  var enemies=m.Enemies.Select(e=>(e.Id,e.Position,e.Health)).ToArray();var shots=m.Shots.Select(s=>(s.Id,s.Age)).ToArray();float cooldown=m.Towers[0].Cooldown;int remaining=m.Remaining;
  m.Step(100);Check(enemies.SequenceEqual(m.Enemies.Select(e=>(e.Id,e.Position,e.Health))) && shots.SequenceEqual(m.Shots.Select(s=>(s.Id,s.Age))) && remaining==m.Remaining && cooldown==m.Towers[0].Cooldown,"Combat advanced while paused");
  return "PASS paused build/upgrade/sale, prices and guard; spawning/movement/health/shots/reloads remain frozen";
 }
 public static string Disabled()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<BattleController>();g.NewMatch(42);g.Choose(1);g.ClickPad(0);g.Cancel();g.ClickPad(0);g.UpgradeSelected(0);g.hud.Refresh();Canvas.ForceUpdateCanvases();
  var b=g.hud.GetComponentsInChildren<UnityEngine.UI.Button>().Single(b=>b.name=="Upgrade 0");
  Check(!b.interactable,"Unaffordable upgrade enabled");Check(b.GetComponentInChildren<TextMeshProUGUI>().text.Contains("Need $"),"Disabled upgrade has no visible shortfall state");g.MainMenu();return "PASS unaffordable upgrade visibly locked with shortfall";
 }
 public static async System.Threading.Tasks.Task<string> Play()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<BattleController>();g.NewMatch(42);g.Choose(0);g.ClickPad(0);g.Cancel();g.StartNextWave();await System.Threading.Tasks.Task.Delay(2200);g.ClickPad(0);g.Pause();
  Check(g.Match.Paused && g.SelectedPad==0,"Pause button lost tower inspection");g.hud.Refresh();Canvas.ForceUpdateCanvases();
  Check(!g.hud.transform.Find("Overlay").gameObject.activeSelf,"Pause still blocks map with a modal");
  var before=g.Match.Enemies.Select(e=>(e.Id,e.Distance,e.Health)).ToArray();var shots=g.Match.Shots.Select(s=>(s.Id,s.Age)).ToArray();
  var upgrade=g.hud.GetComponentsInChildren<UnityEngine.UI.Button>().Single(b=>b.name=="Upgrade 0");upgrade.onClick.Invoke();Check(g.Match.Towers[0].Definition.tier==2,"Paused upgrade callback failed");
  g.Choose(0);g.ClickPad(1);Check(g.Match.Towers[1]!=null,"Paused build selection/world click failed");g.Cancel();g.ClickPad(1);g.SellSelected();Check(g.Match.Towers[1]==null,"Paused sell callback failed");
  g.ClickPad(0);g.hud.Refresh();Canvas.ForceUpdateCanvases();var locked=g.hud.GetComponentsInChildren<UnityEngine.UI.Button>().Single(b=>b.name=="Upgrade 0");int funds=g.Match.Funds;
  var click=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){button=UnityEngine.EventSystems.PointerEventData.InputButton.Left};
  UnityEngine.EventSystems.ExecuteEvents.Execute(locked.gameObject,click,UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);Check(g.Match.Funds==funds,"Disabled upgrade accepted click");
  Check(locked.GetComponentInChildren<TextMeshProUGUI>().text.Contains("Need $") && locked.GetComponentInChildren<TextMeshProUGUI>().color.r<.6f,"Disabled label not muted/explicit");
  var rotation=g.battleCamera.transform.rotation;g.CameraControls.Rotate(25);Check(Quaternion.Angle(rotation,g.battleCamera.transform.rotation)>20,"Paused camera blocked");await System.Threading.Tasks.Task.Delay(600);
  Check(before.SequenceEqual(g.Match.Enemies.Select(e=>(e.Id,e.Distance,e.Health))) && shots.SequenceEqual(g.Match.Shots.Select(s=>(s.Id,s.Age))),"World advanced during management");
  g.Pause();await System.Threading.Tasks.Task.Delay(120);Check(!g.Match.Paused && (!before.SequenceEqual(g.Match.Enemies.Select(e=>(e.Id,e.Distance,e.Health))) || g.Match.Enemies.Count==0),"Resume did not restart combat");g.MainMenu();
  return "PASS live pause without modal; inspect/build/upgrade/sell and camera; disabled upgrade rejects click; world frozen then resumes";
 }
 public static string Stage()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<BattleController>();g.NewMatch(42);g.Match.Build(4,g.defenses[1]);g.Match.Upgrade(4,0);
  ((Economy)typeof(MatchState).GetField("economy",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(g.Match)).Credit(191);
  g.ClickPad(4);g.Pause();g.hud.Refresh();Canvas.ForceUpdateCanvases();return "Temporary paused inspection: 226 supplies, two unaffordable branches";
 }

}
