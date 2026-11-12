// Full native campaign using earned funds and the frame-driven game. Time acceleration is for verification.
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TheLineHolds;
using Unity.Pipeline.Scripting;
var g=UnityEngine.Object.FindAnyObjectByType<BattleController>();g.NewMatch(42);
void Check(bool v,string m){if(!v)throw new Exception(m);}
var seen=new HashSet<string>();int upgrades=0,peakEnemies=0,peakCorpses=0,peakVoices=0;float depth=0,audioPeak=0;int rear=0;var samples=new float[1024];
int[] pads={3,4,6,7,5,1,8,2,9,0},types={0,0,2,1,2,0,1,2,1,0};
int startedFrame=Time.frameCount;float started=Time.realtimeSinceStartup;int projectileObjects=g.combatEffects.ProjectileObjectsCreated;
bool capturedFinal=false,captured=false,inspection=false,normalSample=false;float normalUntil=0,sampleStarted=0,sampleFPS=0;int sampleFrame=0;
g.Match.Journal+=m=>Debug.Log("[Campaign playthrough] "+m);
g.Match.Cue+=(cue,p)=>{if(cue=="fire" && g.Match.Wave==10 && p.x>4)rear++;};
Time.timeScale=4;
try
{
 while(!g.Match.Ended)
 {
  foreach(int i in Enumerable.Range(0,pads.Length))if(g.Match.Towers[pads[i]]==null && !g.Match.Build(pads[i],g.defenses[types[i]]))break;
  foreach(int pad in pads)
  {
   var t=g.Match.Towers[pad];if(t==null)continue;
   int option=t.Definition.role==DefenseRole.Mortar?1:0;if(t.Definition.upgrades.Length==1)option=0;
   if(g.Match.Upgrade(pad,option))upgrades++;
  }
  if(!inspection && g.Match.Wave>=3)
  {
   int pad=Array.FindIndex(g.Match.Towers,t=>t!=null && t.Definition.tier==2);
   if(pad>=0){g.Cancel();g.ClickPad(pad);g.hud.Refresh();Canvas.ForceUpdateCanvases();ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(System.IO.Path.GetTempPath(),"campaign-native-upgrades.png"));await System.Threading.Tasks.Task.Delay(150);g.Cancel();inspection=true;}
  }
  if(g.Match.Phase==MatchPhase.Preparation)g.StartNextWave();
  await Wait.Until(()=>
  {
   foreach(var e in g.Match.Enemies){seen.Add(e.Definition.name);if(g.Match.Wave==10)depth=Mathf.Max(depth,e.Distance/g.Match.Route.Length);}
   peakEnemies=Math.Max(peakEnemies,g.Match.Enemies.Count);peakCorpses=Math.Max(peakCorpses,g.EnemyViews.RetiringCount);peakVoices=Math.Max(peakVoices,g.audioPlayer.ActiveWorldVoices);
   AudioListener.GetOutputData(samples,0);foreach(float v in samples)audioPeak=Mathf.Max(audioPeak,Mathf.Abs(v));
   if(!normalSample && g.Match.Wave>=8 && g.Match.Enemies.Count>=12){normalSample=true;Time.timeScale=1;sampleStarted=Time.realtimeSinceStartup;sampleFrame=Time.frameCount;normalUntil=sampleStarted+3;}
   if(normalUntil>0 && Time.realtimeSinceStartup>=normalUntil){sampleFPS=(Time.frameCount-sampleFrame)/(Time.realtimeSinceStartup-sampleStarted);Time.timeScale=4;normalUntil=0;}
   if(!captured && g.Match.Wave>=8 && g.Match.Enemies.Count>=12 && g.EnemyViews.RetiringCount>0){ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(System.IO.Path.GetTempPath(),"campaign-native-combat.png"));captured=true;}
   if(!capturedFinal && g.Match.Wave==10 && g.Match.Enemies.Count>=18 && g.combatEffects.ActiveProjectiles>=3)
   {
    ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(System.IO.Path.GetTempPath(),"campaign-final-crossfire.png"));
    capturedFinal=true;
   }
   bool money=g.Match.Towers.Where(t=>t!=null).Any(t=>{int option=t.Definition.role==DefenseRole.Mortar?1:0;if(t.Definition.upgrades.Length==1)option=0;return option<t.Definition.upgrades.Length && t.Definition.upgrades[option].price<=g.Match.Funds;});
   int next=Array.FindIndex(pads,p=>g.Match.Towers[p]==null);if(next>=0 && g.Match.Funds>=g.defenses[types[next]].cost)money=true;
   return g.Match.Ended || g.Match.Phase==MatchPhase.Preparation || money;
  },timeoutMs:240000,because:"campaign wave should resolve");
 }
 Time.timeScale=1;await System.Threading.Tasks.Task.Delay(1600);
 Check(g.Match.Phase==MatchPhase.Victory && g.Match.CompletedWaves==10,"Ten-wave victory required");
 Check(g.Match.Killed+g.Match.Escaped==g.waves.Sum(w=>w.UnitCount),"Unit accounting");Check(seen.Count==7 && upgrades>=15,"Roster/upgrades not exercised");
 Check(depth>=.75f && rear>=20,"Final assault failed to engage the rear");Check(peakCorpses<=24 && g.EnemyViews.RetiringCount==0,"Corpse budget/cleanup");
 Check(g.combatEffects.ProjectileObjectsCreated==projectileObjects,"Projectile pool growth");Check(peakVoices<=24 && audioPeak>.0001f && audioPeak<.95f,"Audio budget/output");
 Check(sampleFPS>=35,"Native busy-scene frame rate too low: "+sampleFPS);
 ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(System.IO.Path.GetTempPath(),"campaign-native-victory.png"));await System.Threading.Tasks.Task.Delay(150);
 string result=$"Victory; waves=10; lives={g.Match.Lives}; kills={g.Match.Killed}; escapes={g.Match.Escaped}; upgrades={upgrades}; species={seen.Count}; finalDepth={depth:0.000}; rear={rear}; peakEnemies={peakEnemies}; peakCorpses={peakCorpses}; actorsCreated={g.EnemyViews.Created}; peakVoices={peakVoices}; audioPeak={audioPeak:0.0000}; normalSampleFPS={sampleFPS:0.0}; seconds={Time.realtimeSinceStartup-started:0}; frames={Time.frameCount-startedFrame}";
 Debug.Log("[Campaign result] "+result);return result;
}
finally{Time.timeScale=1;g.CameraControls.enabled=true;g.CameraControls.ResetView();}
