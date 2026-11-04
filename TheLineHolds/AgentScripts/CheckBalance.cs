using System;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using TheLineHolds;
public static class CheckBalance
{
    static WaveDefinition[] Waves => Enumerable.Range(1,5).Select(i=>AssetDatabase.LoadAssetAtPath<WaveDefinition>($"Assets/TheLineHolds/Data/Waves/Wave{i}.asset")).ToArray();
    static DefenseDefinition[] Defenses => new[]{"MachineGun","AntiTank","Mortar"}.Select(n=>AssetDatabase.LoadAssetAtPath<DefenseDefinition>($"Assets/TheLineHolds/Data/Defenses/{n}.asset")).ToArray();
    static MatchState New() => new MatchState(Waves,new BattleRoute(BattleRoute.DefaultPoints),BattleRoute.DefaultPads);
    public static string OpeningGun()
    {
        var m=New();m.Build(0,Defenses[0]);m.StartWave();float furthest=0;int peak=0;
        for(int i=0;i<30000 && m.Phase==MatchPhase.Combat;i++)
        {
            m.Step(1f/60);peak=Math.Max(peak,m.Enemies.Count);
            foreach(var e in m.Enemies)furthest=Mathf.Max(furthest,e.Distance);
        }
        string result=$"Opening MG: killed={m.Killed}, escaped={m.Escaped}, peak concurrent={peak}, furthest={furthest:0.0}m / {m.Route.Length:0.0}m";
        Debug.Log("[Balance] "+result);
        if(m.Escaped==0)throw new Exception("Opening MG clears wave alone. "+result);
        return result;
    }
    public static string All()
    {
        var report=new StringBuilder(OpeningGun()+"\n");
        for(int layout=0;layout<5;layout++)
        {
            var m=Play(layout,out float seconds,out int peak);
            bool shouldWin=layout<2;
            if((m.Phase==MatchPhase.Victory)!=shouldWin)throw new Exception("Unexpected outcome for layout "+layout+": "+m.Phase);
            if(shouldWin && (m.Killed+m.Escaped!=Waves.Sum(w=>w.UnitCount) || m.CompletedWaves!=5))throw new Exception("Incomplete resolution totals");
            report.AppendLine($"Layout {layout}: {m.Phase}, wave={m.Wave}, lives={m.Lives}, killed={m.Killed}, escaped={m.Escaped}, funds={m.Funds}, towers={m.Towers.Count(t=>t!=null)}, peak={peak}, {seconds:0}s");
        }
        Debug.Log("[Balance verification]\n"+report);return report.ToString();
    }
    // 0/1: distinct mixed layouts; 2: MG-only; 3: lone opening gun; 4: never reinvest.
    static MatchState Play(int layout,out float seconds,out int peak)
    {
        var m=New();var d=Defenses;seconds=0;peak=0;
        int[][] pads={new[]{3,4,6,7,5,1,8,2,9,0},new[]{0,3,4,6,7,5,8,2,1,9},new[]{3,4,6,7,5,8,1,2,9,0},new[]{0},new[]{3,4,6}};
        int[][] types={new[]{0,0,2,1,2,0,1,2,0,0},new[]{0,2,0,1,2,1,0,2,0,0},new[]{0,0,0,0,0,0,0,0,0,0},new[]{0},new[]{0,0,2}};
        while(!m.Ended && seconds<1000)
        {
            if(m.Phase==MatchPhase.Preparation)
            {
                for(int i=0;i<pads[layout].Length;i++)
                {
                    if(layout==4 && m.Wave>0)break;
                    int pad=pads[layout][i];
                    if(m.Towers[pad]==null && !m.Build(pad,d[types[layout][i]]))break;
                }
                m.StartWave();
            }
            m.Step(1f/60);seconds+=1f/60;peak=Math.Max(peak,m.Enemies.Count);
        }
        return m;
    }
}
