using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TheLineHolds;
public static class CheckOrganic
{
 public static string All()
 {
  var g=UnityEngine.Object.FindAnyObjectByType<BattleController>();var wave=g.waves[7];var results=new List<string>();
  foreach(int seed in new[]{1944,42,2026})
  {
   var m=new MatchState(new[]{wave},new BattleRoute(BattleRoute.DefaultPoints),BattleRoute.DefaultPads,seed);m.StartWave();var ids=new HashSet<int>();var bins=new HashSet<int>();
   for(int i=0;i<800 && !m.Ended;i++){m.Step(.05f);foreach(var e in m.Enemies)if(ids.Add(e.Id))bins.Add(Mathf.RoundToInt(e.LateralOffset*10));}
   if(bins.Count<12)throw new Exception("Infantry still occupy repeated lanes: "+bins.Count);results.Add(seed+"="+bins.Count);
  }
  return "PASS continuous infantry spread across 0.1m road bins: "+string.Join(", ",results);
 }
}
