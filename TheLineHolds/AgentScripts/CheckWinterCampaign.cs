// Run against the two authored layouts using the same campaign assets, purchase order and seed.
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
        foreach (bool winter in new[]{false,true})
        {
            var route=winter?WinterLayout.Route():BattleRoute.Countryside();
            var pads=winter?WinterLayout.Pads:BattleRoute.DefaultPads;
            float minClear=float.MaxValue,minCover=float.MaxValue;
            foreach(var pad in pads)
            {
                float covered=0;
                for(float d=0;d<route.Length;d+=.05f)
                {
                    var offset=route.Position(d)-pad;offset.y=0;
                    minClear=Mathf.Min(minClear,offset.magnitude);
                    if(offset.magnitude<=game.defenses[0].range)covered+=.05f;
                }
                minCover=Mathf.Min(covered,minCover);
            }
            report.AppendLine($"{(winter?"Winter Forest":"Countryside")} {route.Length:F1} m, minimum clearance {minClear:F2} m, minimum MG coverage {minCover:F1} m");
            if(minClear<2.45f || minCover<4.5f)throw new Exception("Lost roadside clearance or base MG coverage");
            foreach(int seed in new[]{42,1944,2026}) foreach(bool mixed in new[]{true,false})
            {
                var m=new MatchState(game.waves,route,pads,seed);
                float finalDepth=0;
                for(int tick=0;!m.Ended && tick<120000;tick++)
                {
                    if(tick%30==0)
                    {
                        for(int i=0;i<Order.Length;i++)
                            if(m.Towers[Order[i]]==null && !m.Build(Order[i],game.defenses[mixed?Roles[i]:0]))break;
                        foreach(int pad in Order)
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
                    throw new Exception("Mixed defenses lost the campaign balance target");
                if(!mixed && m.Phase!=MatchPhase.Defeat)throw new Exception("MG-only campaign should lose");
                report.AppendLine($"  {seed} {(mixed?"mixed":"MG only")}: {m.Phase} wave {m.Wave}, {m.Lives} lives, final penetration {finalDepth:P0}");
            }
        }
        return report.ToString();
    }
}
