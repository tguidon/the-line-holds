using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TheLineHolds;
public static class CheckCampaign
{
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 public static string Rules()
 {
  var game=UnityEngine.Object.FindAnyObjectByType<BattleController>();
  Check(game.waves.Length==10,"Ten waves required");
  var enemies=game.waves.SelectMany(w=>w.spawns).Select(e=>e.enemy).Distinct().ToArray();Check(enemies.Length==7,"Seven enemy archetypes required");
  Check(enemies.Select(e=>e.speed).Distinct().Count()>=6,"Speed roles missing");
  foreach(var wave in game.waves)
  {
   var a=WavePlanner.Create(wave,1944);var b=WavePlanner.Create(wave,1944);var c=WavePlanner.Create(wave,42);
   Check(a.Sum(e=>e.UnitCount)==wave.UnitCount,"Planner changed announced total");
   foreach(var enemy in enemies)Check(a.Where(e=>e.enemy==enemy).Sum(e=>e.UnitCount)==wave.spawns.Where(e=>e.enemy==enemy).Sum(e=>e.UnitCount),"Planner changed composition");
   Check(a.Select(e=>$"{e.enemy.name}:{e.count}:{e.delay}").SequenceEqual(b.Select(e=>$"{e.enemy.name}:{e.count}:{e.delay}")),"Seed replay is unstable");
   Check(!a.Select(e=>e.delay).SequenceEqual(c.Select(e=>e.delay)),"No formation variation");
  }
  Check(game.waves[4].spawns.All(e=>e.enemy.spawnSeparation>0) && game.waves[8].spawns.All(e=>e.enemy.spawnSeparation>0),"Vehicle-only attacks lost");
  var match=new MatchState(game.waves,new BattleRoute(BattleRoute.DefaultPoints),BattleRoute.DefaultPads);
  Check(match.Build(0,game.defenses[0]),"Build failed");Check(match.Upgrade(0,0),"Veteran upgrade failed");
  Check(match.Funds==135 && match.Towers[0].Invested==165 && match.Towers[0].Definition.tier==2,"Upgrade accounting wrong");
  Check(!match.Upgrade(0,9) && !match.Upgrade(0,0),"Invalid/unaffordable upgrade accepted");
  match.Paused=true;Check(!match.Upgrade(0,0),"Paused unaffordable upgrade accepted");match.Paused=false;
  Check(match.Sell(0) && match.Funds==217,"Invested-cost sale refund wrong");
  Check(game.defenses.All(d=>d.upgrades.Length==1 && d.upgrades[0].defense.upgrades.Length==2),"Upgrade branches missing");
  var tankWave=ScriptableObject.CreateInstance<WaveDefinition>();tankWave.spawns=new[]{new SpawnEntry(enemies.First(e=>e.name=="ArmoredCar"),0,6,.01f),new SpawnEntry(enemies.First(e=>e.name=="HeavyTank"),0,6,.01f)};
  var spacing=new MatchState(new[]{tankWave},match.Route,match.Pads);spacing.StartWave();float minGap=float.MaxValue;
  for(int i=0;i<10000 && !spacing.Ended && spacing.Phase==MatchPhase.Combat;i++)
  {spacing.Step(1f/60);var sorted=spacing.Enemies.OrderByDescending(e=>e.Distance).ToArray();for(int j=1;j<sorted.Length;j++)minGap=Mathf.Min(minGap,Vector3.Distance(sorted[j-1].Position,sorted[j].Position));}
  UnityEngine.Object.DestroyImmediate(tankWave);Check(minGap>2.6f,"Mixed-speed vehicles overlap: "+minGap);
  string result=$"PASS ten waves/seven archetypes, announced composition, seeded variety, upgrade branches/accounting/guards, mixed-speed vehicle spacing {minGap:0.00}m";Debug.Log("[Campaign] "+result);return result;
 }
 public static string Balance()
 {
  var game=UnityEngine.Object.FindAnyObjectByType<BattleController>();var output=new System.Text.StringBuilder();
  for(int strategy=0;strategy<6;strategy++)foreach(int seed in new[]{1944,42,2026})
  {
   var match=new MatchState(game.waves,new BattleRoute(BattleRoute.DefaultPoints),BattleRoute.DefaultPads,seed);
   int[] pads=strategy==5?new[]{0,2,3,4,1,5,6,7,8,9}:new[]{3,4,6,7,5,1,8,2,9,0};int[] types=strategy==2?new[]{0,0,0,0,0,0,0,0,0,0}:new[]{0,0,2,1,2,0,1,2,1,0};
   float elapsed=0,depth=0;int upgrades=0,rear=0;
   match.Cue+=(cue,p)=>{if(cue=="fire" && match.Wave==10 && (p.x>4))rear++;};
   for(int tick=0;tick<120000 && !match.Ended;tick++)
   {
    if(tick%30==0)
    {
     for(int i=0;i<pads.Length;i++)if(match.Towers[pads[i]]==null){if(strategy==3 && i>=2)break;if(!match.Build(pads[i],game.defenses[types[i]]))break;}
     if(strategy!=1 && strategy!=3)
     {
      // Broad coverage first, then veterans, then specialize by role.
      foreach(int pad in pads)
      {var tower=match.Towers[pad];if(tower==null)continue;int choice=strategy==4?(tower.Definition.role==DefenseRole.MachineGun?1:0):(tower.Definition.role==DefenseRole.Mortar?1:0);if(tower.Definition.upgrades.Length==1)choice=0;
       if(match.Upgrade(pad,choice))upgrades++;}
     }
     if(match.Phase==MatchPhase.Preparation)match.StartWave();
    }
    match.Step(1f/60);elapsed+=1f/60;
    if(match.Wave==10)foreach(var e in match.Enemies)depth=Mathf.Max(depth,e.Distance/match.Route.Length);
   }
   if(strategy==0 || strategy==4)Check(match.Phase==MatchPhase.Victory && depth>=.75f && rear>=20,"Upgraded mixed defenses must win and engage the rear");
   if(strategy==1 || strategy==2 || strategy==3)Check(match.Phase==MatchPhase.Defeat,"Static, single-role or neglected defenses must fail");
   output.AppendLine($"strategy={strategy} seed={seed}: {match.Phase} wave={match.Wave} lives={match.Lives} kills={match.Killed} escapes={match.Escaped} funds={match.Funds} upgrades={upgrades} seconds={elapsed:0} finalDepth={depth:P0} rear={rear}");
  }
  Debug.Log("[Campaign balance]\n"+output);return output.ToString();
 }
}
