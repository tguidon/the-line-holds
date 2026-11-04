using System;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using TheLineHolds;
// Run through Pipeline script run; assertions exercise actual authored data and simulation.
public static class VerifyPrototype
{
    static WaveDefinition[] Waves => Enumerable.Range(1,5).Select(i=>AssetDatabase.LoadAssetAtPath<WaveDefinition>($"Assets/TheLineHolds/Data/Waves/Wave{i}.asset")).ToArray();
    static DefenseDefinition[] Defenses => new[]{"MachineGun","AntiTank","Mortar"}.Select(n=>AssetDatabase.LoadAssetAtPath<DefenseDefinition>($"Assets/TheLineHolds/Data/Defenses/{n}.asset")).ToArray();
    static MatchState New() => new MatchState(Waves,new BattleRoute(BattleRoute.DefaultPoints),BattleRoute.DefaultPads);
    static void Check(bool condition,string message) { if(!condition)throw new Exception("FAILED: "+message); }
    public static string All()
    {
        var report=new StringBuilder(); var d=Defenses;
        Check(Waves.All(w=>w!=null),"all five waves exist");
        Check(Waves.Select(w=>w.spawns.Length).SequenceEqual(new[]{8,12,10,19,25}),"authored wave counts");
        Check(Waves.Select(w=>w.spawns.Count(s=>s.enemy.health==200)).SequenceEqual(new[]{0,0,2,3,5}),"authored armor composition");
        var m=New(); Check(m.Funds==300 && m.Lives==20 && m.Wave==0 && m.Enemies.Count==0,"fresh state");
        Check(m.Build(0,d[0]),"purchase");Check(m.Funds==225,"cost exactly once");Check(!m.Build(0,d[1]) && m.Funds==225,"occupied pad rejects purchase");
        Check(m.Build(1,d[2]),"mortar purchase");Check(!m.Build(2,d[1]) && m.Funds==75,"unaffordable purchase rejects");
        Check(m.Sell(0) && m.Funds==112,"floor 50 percent sale");Check(!m.Sell(0) && m.Funds==112,"duplicate sale rejects");
        m.Paused=true;Check(!m.StartWave() && !m.Build(0,d[0]) && !m.Sell(1),"paused orders reject");m.Paused=false;
        Check(m.StartWave() && !m.StartWave(),"non-overlapping wave start");m.Step(3);m.Paused=true;float distance=m.Enemies[0].Distance;int left=m.Remaining;float age=m.Shots.Count>0?m.Shots[0].Age:0;m.Step(100);
        Check(m.Enemies[0].Distance==distance && left==m.Remaining && (m.Shots.Count==0 || age==m.Shots[0].Age),"pause freezes movement spawning projectiles");
        report.AppendLine("PASS economy, invalid purchases, sale rounding, paused orders, wave overlap and pause freeze");
        m=New();m.StartWave();for(int i=0;i<20000 && m.Phase==MatchPhase.Combat;i++)m.Step(1f/60);
        Check(m.Escaped==8 && m.Killed==0 && m.Lives==12 && m.Funds==350 && m.CompletedWaves==1,"escapes cost lives, grant no reward, wave grants 50");
        m.StartWave();for(int i=0;i<20000 && !m.Ended;i++)m.Step(1f/60);
        Check(m.Phase==MatchPhase.Defeat && m.Lives==0 && m.Escaped==20 && m.CompletedWaves==1,"neglect loses immediately");
        int escapes=m.Escaped;m.Step(1000);Check(m.Escaped==escapes && m.Shots.Count==0 && !m.StartWave(),"ended state frozen");
        Check(new[]{0,1,9,10,17,18,20}.Select(MatchState.Rating).SequenceEqual(new[]{0,1,1,2,2,3,3}),"rating boundaries");
        report.AppendLine("PASS unguarded defeat, kill/escape accounting, terminal freeze, all rating boundaries");
        // Last escape of final wave must lose, not win. Use authored definitions with a short route.
        var one=ScriptableObject.CreateInstance<WaveDefinition>();one.spawns=Enumerable.Range(0,20).Select(i=>new SpawnEntry(Waves[0].spawns[0].enemy,0)).ToArray();
        m=new MatchState(new[]{one},new BattleRoute(new[]{Vector3.zero,Vector3.forward*.01f}),new[]{Vector3.right});m.StartWave();m.Step(1);
        Check(m.Phase==MatchPhase.Defeat && m.CompletedWaves==0 && m.Funds==300,"defeat priority on last final escape");UnityEngine.Object.DestroyImmediate(one);
        report.AppendLine("PASS final-escape defeat priority");
        var pair=ScriptableObject.CreateInstance<WaveDefinition>();pair.spawns=new[]{new SpawnEntry(Waves[0].spawns[0].enemy,0),new SpawnEntry(Waves[0].spawns[0].enemy,0)};
        var testGun=ScriptableObject.CreateInstance<DefenseDefinition>();testGun.cost=1;testGun.damage=40;testGun.range=10;testGun.interval=10;testGun.flightTime=.1f;testGun.splashRadius=2;
        m=new MatchState(new[]{pair},new BattleRoute(new[]{Vector3.zero,Vector3.forward*100}),new[]{Vector3.right,Vector3.left});m.Build(0,testGun);m.Build(1,testGun);m.StartWave();m.Step(.01f);
        Check(m.Target(m.Towers[0]).Id==1,"equal-distance targeting chooses stable first ID");
        m.Step(.2f);Check(m.Killed==2 && m.Funds==368 && m.Enemies.Count==0,"overlapping splash shots reward each kill exactly once");
        int funds=m.Funds;m.Step(50);Check(m.Funds==funds,"completed wave rewards exactly once");
        UnityEngine.Object.DestroyImmediate(pair);UnityEngine.Object.DestroyImmediate(testGun);
        report.AppendLine("PASS stable targeting, splash damage, simultaneous lethal shots, exactly-once kill and wave rewards");
        report.AppendLine(PlayLayout(0));report.AppendLine(PlayLayout(1));
        Debug.Log("[Verification]\n"+report);return report.ToString();
    }
    static string PlayLayout(int layout)
    {
        var m=New();var d=Defenses;float seconds=0;
        if(layout==0){m.Build(3,d[0]);m.Build(4,d[0]);m.Build(6,d[2]);}
        else {m.Build(4,d[1]);m.Build(6,d[0]);m.Build(0,d[0]);}
        while(!m.Ended && seconds<1000)
        {
            if(m.Phase==MatchPhase.Preparation)
            {
                // Spend earned funds between waves in a clear authored strategy; no income cheats.
                int[] order=layout==0?new[]{7,8,1,5,9,2,0}:new[]{7,3,8,5,1,9,2};
                foreach(int pad in order) { int type=pad==7||pad==8?1:pad==5?2:0; if(m.Towers[pad]==null)m.Build(pad,d[type]); }
                Check(m.StartWave(),"start next wave");
            }
            m.Step(1f/60);seconds+=1f/60;
        }
        Check(m.Phase==MatchPhase.Victory,"layout "+layout+" must win: lives="+m.Lives+", wave="+m.Wave);
        Check(m.Killed+m.Escaped==74 && m.CompletedWaves==5,"exact resolution totals");
        return $"PASS layout {layout}: {m.Stars} stars, {m.Lives} lives, {m.Killed} kills, {m.Escaped} escapes, {seconds:0}s combat";
    }
    public static string RuntimeState()
    {
        var g=UnityEngine.Object.FindAnyObjectByType<BattleController>();
        if(!g)return "No controller";var m=g.Match;
        return m==null?"Main menu":$"{m.Phase}; wave={m.Wave}; lives={m.Lives}; funds={m.Funds}; killed={m.Killed}; escaped={m.Escaped}; enemies={m.Enemies.Count}; shots={m.Shots.Count}; frame={Time.frameCount}";
    }
}
