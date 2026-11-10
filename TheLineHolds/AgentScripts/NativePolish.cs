using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using TheLineHolds;
var g=UnityEngine.Object.FindAnyObjectByType<BattleController>();g.NewMatch(42);
void Check(bool v,string m){if(!v)throw new Exception(m);}
var defs=g.defenses.SelectMany(d=>d.upgrades.SelectMany(u=>u.defense.upgrades.Select(v=>v.defense)).Concat(d.upgrades.Select(u=>u.defense))).ToArray();
Check(defs.Length==9 && defs.Select(d=>d.prefab).Distinct().Count()==9,"Upgrade models are not distinct");
Check(!GameObject.Find("Exposed earth layers") && !GameObject.Find("Miniature display plinth") && GameObject.Find("Surrounding countryside"),"Floating map remains");
var ambient=g.audioPlayer.GetComponents<AudioSource>().Single(a=>a.loop);await Task.Delay(200);Check(ambient.isPlaying && ambient.clip.length>40,"Ambient loop not playing");g.audioPlayer.ToggleAmbience();await Task.Delay(1800);Check(ambient.volume<.01f,"Ambient mute failed");g.audioPlayer.ToggleAmbience();
// Temporary funded tier showcase; full campaign purchases are verified separately.
((Economy)typeof(MatchState).GetField("economy",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(g.Match)).Credit(10000);
for(int i=0;i<9;i++){g.Match.Build(i,g.defenses[i/3]);if(i%3>0)g.Match.Upgrade(i,0);if(i%3==2)g.Match.Upgrade(i,0);}
g.ClickPad(1);g.hud.Refresh();Canvas.ForceUpdateCanvases();await Task.Delay(200);ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(System.IO.Path.GetTempPath(),"polish-native-upgrades.png"));await Task.Delay(120);g.NewMatch(42);
g.hud.Refresh();Canvas.ForceUpdateCanvases();await Task.Delay(200);ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(System.IO.Path.GetTempPath(),"polish-native-map.png"));await Task.Delay(120);
var vehicle=g.waves.SelectMany(w=>w.spawns).Select(s=>s.enemy).First(e=>e.name=="LightTank");var p=BattleRoute.OnGround(-4,2);var root=UnityEngine.Object.Instantiate(vehicle.prefab,p,Quaternion.Euler(0,25,0));var visual=root.AddComponent<EnemyVisual>();visual.Initialize(true);
var surface=root.GetComponentsInChildren<Renderer>().First();var block=new MaterialPropertyBlock();surface.GetPropertyBlock(block);var before=block.GetColor("_BaseColor");visual.Hit(1);surface.GetPropertyBlock(block);Check(before==block.GetColor("_BaseColor"),"Whole vehicle still flashes on hit");
var camera=g.battleCamera;g.CameraControls.enabled=false;camera.transform.position=p+new Vector3(8,12,-10);camera.transform.LookAt(p+Vector3.up*.4f);camera.orthographicSize=3;
try
{
 var enemy=new EnemyState{Definition=vehicle,Position=p,Health=vehicle.health,MaxHealth=vehicle.health};var shot=new ShotState{Definition=g.defenses[1],Target=enemy,Origin=p+new Vector3(3,.6f,-2),Destination=p+Vector3.up*.4f};
 int emitters=g.combatEffects.GetComponentsInChildren<ParticleSystem>().Length;g.combatEffects.Impact(shot);await Task.Delay(70);Check(g.combatEffects.ParticleCount>0,"No local impact feedback");ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(System.IO.Path.GetTempPath(),"polish-native-impact.png"));await Task.Delay(150);
 g.combatEffects.Destruction(p,true);visual.Die();visual.Tick(enemy,.1f);await Task.Delay(100);Check(g.combatEffects.ParticleCount>20,"Explosion absent");ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(System.IO.Path.GetTempPath(),"polish-native-explosion.png"));await Task.Delay(120);
 for(int i=0;i<10;i++)g.combatEffects.Destruction(p,true);Check(g.combatEffects.GetComponentsInChildren<ParticleSystem>().Length==emitters,"Explosion emitter leak");
}
finally{UnityEngine.Object.Destroy(root);g.CameraControls.enabled=true;g.CameraControls.ResetView();g.MainMenu();}
return "PASS nine distinct upgrades, native ambient loop/mute, continuous map, no whole-vehicle flash, local impact and bounded animated explosion";
