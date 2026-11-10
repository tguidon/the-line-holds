using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using TheLineHolds;
public static class ReviewPolish
{
 public static string Overview(){var g=UnityEngine.Object.FindAnyObjectByType<BattleController>();g.NewMatch(42);g.hud.Refresh();Canvas.ForceUpdateCanvases();return "Live overview ready";}
 public static string Upgrades()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<BattleController>();g.NewMatch(42);((Economy)typeof(MatchState).GetField("economy",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(g.Match)).Credit(10000);
  int[] types={0,0,0,1,1,1,2,2,2};
  for(int i=0;i<9;i++){g.Match.Build(i,g.defenses[types[i]]);if(i%3>0)g.Match.Upgrade(i,0);if(i%3==2)g.Match.Upgrade(i,0);}
  g.ClickPad(1);g.hud.Refresh();return "Temporary nine-tower tier showcase";
 }
 public static async Task<string> Check()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<BattleController>();g.NewMatch(42);g.Choose(0);g.ClickPad(0);g.Cancel();g.ClickPad(0);g.ClickPad(-1);if(g.SelectedPad!=-1)throw new Exception("Ground deselect");
  g.ClickPad(0);g.ClickPad(0);if(g.SelectedPad!=-1)throw new Exception("Toggle deselect");
  g.ClickPad(0);g.Pause();if(g.SelectedPad!=-1 || g.Match.Paused)throw new Exception("Escape selection priority");
  g.ClickPad(0);g.hud.Refresh();Canvas.ForceUpdateCanvases();var buttons=g.hud.GetComponentsInChildren<UnityEngine.UI.Button>();
  buttons.Single(b=>b.name=="Upgrade 0").onClick.Invoke();if(g.Match.Towers[0].Definition.tier!=2 || g.Match.Funds!=135)throw new Exception("Upgrade callback/cost");
  await Task.Delay(100);g.hud.Refresh();buttons=g.hud.GetComponentsInChildren<UnityEngine.UI.Button>();
  if(buttons.Count(b=>b.name.StartsWith("Upgrade "))!=2)throw new Exception("Branches missing");
  buttons.Single(b=>b.name=="Close inspection").onClick.Invoke();if(g.SelectedPad!=-1)throw new Exception("Close inspection");
  var ambient=g.audioPlayer.GetComponents<AudioSource>().Single(a=>a.loop);await Task.Delay(150);
  if(!ambient.isPlaying || !ambient.clip || ambient.clip.length<40)throw new Exception("Ambient loop missing");
  g.audioPlayer.ToggleAmbience();await Task.Delay(1800);if(ambient.volume>.01f)throw new Exception("Ambient mute");g.audioPlayer.ToggleAmbience();
  if(GameObject.Find("Exposed earth layers") || GameObject.Find("Miniature display plinth"))throw new Exception("Floating land remains");
  if(!GameObject.Find("Surrounding countryside"))throw new Exception("Surroundings missing");
  g.MainMenu();return "PASS deselection routes, actual upgrade/branch callbacks and costs, original ambience/mute, continuous countryside";
 }
}
