// Run against the three authored layouts using the same campaign assets, purchase costs, role order and seed.
using System;
using System.Linq;
using System.Text;
using UnityEngine;
using TheLineHolds;

public static class CheckWinterCampaign
{
    static readonly int[] Order={3,4,6,7,5,1,8,2,9,0};
    static readonly int[] Roles={0,0,2,1,2,0,1,2,1,0};
    public static string Compare()
    {
        var game=UnityEngine.Object.FindAnyObjectByType<BattleController>();
        var report=new StringBuilder();
        foreach (string name in new[]{"Countryside","Winter Forest","Saint-Aubin"})
        {
            bool winter=name=="Winter Forest", town=name=="Saint-Aubin";
            var route=town?TownLayout.Route(false):winter?WinterLayout.Route():BattleRoute.Countryside();
            var pads=town?TownLayout.Pads:winter?WinterLayout.Pads:BattleRoute.DefaultPads;
            var other=town?TownLayout.Route(true):null;
            var order=town?new[]{4,8,5,9,3,6,7,0,1,2}:Order;
            float minClear=float.MaxValue,minCover=float.MaxValue;
            foreach(var pad in pads)
            {
                float covered=0;
                foreach(var entrance in other==null?new[]{route}:new[]{route,other})
                {
                    float laneCoverage=0;
                    for(float d=0;d<entrance.Length;d+=.05f)
                    {
                        var offset=entrance.Position(d)-pad;offset.y=0;
                        minClear=Mathf.Min(minClear,offset.magnitude);
                        if(offset.magnitude<=game.defenses[0].range)laneCoverage+=.05f;
                    }
                    covered=Mathf.Max(laneCoverage,covered);
                }
                minCover=Mathf.Min(covered,minCover);
            }
            report.AppendLine($"{name} {route.Length:F1} m, minimum clearance {minClear:F2} m, minimum MG coverage {minCover:F1} m");
            if(minClear<2.45f || minCover<4.5f)throw new Exception("Lost roadside clearance or base MG coverage");
            foreach(int seed in new[]{42,1944,2026}) foreach(bool mixed in new[]{true,false})
            {
                var m=new MatchState(game.waves,route,pads,seed,other,town?new BattleRoute(TownLayout.Entrance(false)).Length:0);
                float finalDepth=0;
                for(int tick=0;!m.Ended && tick<120000;tick++)
                {
                    if(tick%30==0)
                    {
                        for(int i=0;i<order.Length;i++)
                            if(m.Towers[order[i]]==null && !m.Build(order[i],game.defenses[mixed?Roles[i]:0]))break;
                        foreach(int pad in order)
                        {
                            var tower=m.Towers[pad];
                            if(tower==null)continue;
                            m.Upgrade(pad,tower.Definition.upgrades.Length==2 && tower.Definition.role==DefenseRole.Mortar?1:0);
                        }
                        if(m.Phase==MatchPhase.Preparation)m.StartWave();
                    }
                    m.Step(1f/60);
                    if(m.Wave==10)foreach(var enemy in m.Enemies)finalDepth=Mathf.Max(finalDepth,enemy.Distance/route.Length);
                }
                if(mixed && (m.Phase!=MatchPhase.Victory || finalDepth<.75f || m.Lives<15))
                    throw new Exception($"{name} seed {seed} mixed defense target: {m.Lives} lives, {finalDepth:P0} penetration");
                if(!mixed && m.Phase!=MatchPhase.Defeat)throw new Exception($"{name} MG-only campaign should lose");
                report.AppendLine($"  {seed} {(mixed?"mixed":"MG only")}: {m.Phase} wave {m.Wave}, {m.Lives} lives, final penetration {finalDepth:P0}");
            }
        }
        return report.ToString();
    }
}
