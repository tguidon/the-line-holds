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
    public static string All() => Run(true);
    public static string Survey() => Run(false);
    static string Run(bool assert)
    {
        var report=new StringBuilder(OpeningGun()+"\n");
        for(int layout=0;layout<6;layout++)
        {
            var m=Play(layout,out float seconds,out int peak,out float[] depth,out int rearShots);
            bool shouldWin=layout<2 || layout==5;
            if(assert && (m.Phase==MatchPhase.Victory)!=shouldWin)throw new Exception("Unexpected outcome for layout "+layout+": "+m.Phase);
            if(assert && shouldWin && (m.Killed+m.Escaped!=Waves.Sum(w=>w.UnitCount) || m.CompletedWaves!=5))throw new Exception("Incomplete resolution totals");
            if(assert && shouldWin && (depth[4]<.75f || rearShots<5))throw new Exception("Full mixed loadout still clears too early: depth="+depth[4]+", rear shots="+rearShots);
            report.AppendLine($"Layout {layout}: {m.Phase}, wave={m.Wave}, lives={m.Lives}, killed={m.Killed}, escaped={m.Escaped}, towers={m.Towers.Count(t=>t!=null)}, peak={peak}, {seconds:0}s; depth={string.Join(",",depth.Select(d=>(d*100).ToString("0")))}%; rear shots={rearShots}");
        }
        Debug.Log("[Balance verification]\n"+report);return report.ToString();
    }
    // 0/1: different mixed layouts; 2: MG-only; 3: lone gun; 4: no reinvestment; 5: user's five MG/three AT/two mortar layout.
    static MatchState Play(int layout,out float seconds,out int peak,out float[] depth,out int rearShots)
    {
        var m=New();var d=Defenses;seconds=0;peak=0;depth=new float[5];int rear=0;
        m.Cue+=(cue,p)=> { if(cue=="fire" && m.Wave==5 && (Vector3.Distance(p,m.Pads[8]+Vector3.up*.65f)<.01f || Vector3.Distance(p,m.Pads[9]+Vector3.up*.65f)<.01f))rear++; };
        int[][] pads={new[]{3,4,6,7,5,1,8,2,9,0},new[]{0,3,4,6,7,5,8,2,1,9},new[]{3,4,6,7,5,8,1,2,9,0},new[]{0},new[]{3,4,6},new[]{0,3,4,2,6,5,1,7,8,9}};
        int[][] types={new[]{0,0,2,1,2,0,1,2,0,0},new[]{0,2,0,1,2,1,0,2,0,0},new[]{0,0,0,0,0,0,0,0,0,0},new[]{0},new[]{0,0,2},new[]{0,0,0,1,1,0,0,2,2,1}};
        while(!m.Ended && seconds<1000)
        {
            // Strong strategies use earned money during combat, as a human player can.
            if(m.Phase==MatchPhase.Preparation || layout<2 || layout==5)
                for(int i=0;i<pads[layout].Length;i++)
                {
                    if(layout==4 && m.Wave>0)break;
                    int pad=pads[layout][i];
                    if(m.Towers[pad]==null && !m.Build(pad,d[types[layout][i]]))break;
                }
            if(m.Phase==MatchPhase.Preparation)
            {
                if(m.Wave==4 && (layout<2 || layout==5) && m.Towers.Any(t=>t==null))throw new Exception("Mixed strategy must fill all pads before final wave");
                m.StartWave();
            }
            m.Step(1f/60);seconds+=1f/60;peak=Math.Max(peak,m.Enemies.Count);
            foreach(var enemy in m.Enemies)depth[m.Wave-1]=Mathf.Max(depth[m.Wave-1],enemy.Distance/m.Route.Length);
        }
        rearShots=rear;return m;
    }
}
