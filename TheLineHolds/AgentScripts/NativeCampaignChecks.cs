using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TheLineHolds;
var g=UnityEngine.Object.FindAnyObjectByType<BattleController>();g.NewMatch(42);
void Check(bool v,string m){if(!v)throw new Exception(m);}
Check(g.waves.Length==10,"Campaign wave count");var roster=g.waves.SelectMany(w=>w.spawns).Select(e=>e.enemy).Distinct().ToArray();Check(roster.Length==7,"Roster incomplete");
var review=new GameObject("Temporary native roster review");var originalSize=new Vector2Int(Screen.width,Screen.height);var authored=g.waves;
try
{
 for(int i=0;i<roster.Length;i++)
 {
  var e=roster[i];var root=UnityEngine.Object.Instantiate(e.prefab,BattleRoute.OnGround(-7+i%4*4,2-i/4*4),Quaternion.Euler(0,25,0),review.transform);
  var visual=root.AddComponent<EnemyVisual>();visual.Initialize(e.spawnSeparation>0);var state=new EnemyState{Definition=e,Distance=1,Id=i,Health=e.health,MaxHealth=e.health};
  var position=root.transform.position;visual.Tick(state,.05f);state.Distance+=.2f;visual.Tick(state,.05f);Check(root.transform.position==position,"Visual moved simulation root");
  Check(root.GetComponentsInChildren<Renderer>().All(r=>r.sharedMaterials.All(m=>m && m.shader.isSupported)),"Unsupported actor shader");
  visual.Hit(.2f);visual.Tick(state,.05f);visual.Die();visual.Tick(state,.18f);visual.Tick(state,2);visual.Initialize(e.spawnSeparation>0);Check(!visual.Dying,"Pooled actor reset failed");
 }
 g.CameraControls.Zoom(-4);g.hud.Refresh();Canvas.ForceUpdateCanvases();await Task.Delay(250);
 ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(System.IO.Path.GetTempPath(),"campaign-native-roster.png"));await Task.Delay(150);
 UnityEngine.Object.Destroy(review);
 // Worst-case wave intel and both upgrade choices at four native sizes.
 g.waves=new[]{authored.Last()};
 foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1200,1000),new Vector2Int(1024,768),new Vector2Int(1920,1080)})
 {
  Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);await Task.Delay(400);g.NewMatch(42);g.Choose(0);g.ClickPad(0);g.Cancel();g.ClickPad(0);g.UpgradeSelected(0);g.hud.Refresh();Canvas.ForceUpdateCanvases();await Task.Delay(80);
  var texts=g.hud.GetComponentsInChildren<Component>().Where(c=>c.GetType().Name=="TextMeshProUGUI").ToArray();
  string Text(Component c)=>(string)c.GetType().GetProperty("text").GetValue(c);
  bool Truncated(Component c)=>(bool)c.GetType().GetProperty("isTextTruncated").GetValue(c);
  var status=texts.Single(t=>Text(t).StartsWith("LIVES"));var intel=texts.Single(t=>Text(t).StartsWith("NEXT"));
  Check(!Truncated(status) && !Truncated(intel),"Truncated native counters/intel at "+size);
  var upgrades=g.hud.GetComponentsInChildren<Button>().Where(b=>b.name=="UPGRADE").ToArray();Check(upgrades.Length==2 && upgrades.All(b=>!b.interactable),"Native branch UI/affordability");
  ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(System.IO.Path.GetTempPath(),$"campaign-native-ui-{size.x}x{size.y}.png"));await Task.Delay(120);
 }
 g.waves=authored;g.NewMatch(42);g.audioPlayer.Fire(g.defenses[1],g.Match.Pads[0]);await Task.Delay(60);Check(g.audioPlayer.ActiveWorldVoices>0,"Combat audio unavailable");
 g.Pause();Check(g.Match.Paused && g.audioPlayer.ActiveWorldVoices==0,"Native pause failed");g.Pause();
 string result="PASS native seven actors/shaders/root isolation/damage/defeat/reset; ten waves; four resized worst-case intel/branch views; prices, audio and pause";Debug.Log("[Native campaign] "+result);return result;
}
finally{g.waves=authored;UnityEngine.Object.Destroy(review);Screen.SetResolution(originalSize.x,originalSize.y,FullScreenMode.Windowed);g.MainMenu();}
