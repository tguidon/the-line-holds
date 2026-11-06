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
        Check(Waves.Select(w=>w.UnitCount).SequenceEqual(new[]{18,30,40,56,72}),"authored wave counts");
        Check(Waves.Select(w=>w.spawns.Where(s=>s.enemy.armored).Sum(s=>s.UnitCount)).SequenceEqual(new[]{0,0,4,8,12}),"authored armor composition");
        Check(Enumerable.Range(1,5).Select(MatchState.WaveReward).SequenceEqual(new[]{70,80,90,100,110}),"modest increasing round rewards");
        var m=New(); Check(m.Funds==300 && m.Lives==20 && m.Wave==0 && m.Enemies.Count==0,"fresh state");
        Check(m.Build(0,d[0]),"purchase");Check(m.Funds==225,"cost exactly once");Check(!m.Build(0,d[1]) && m.Funds==225,"occupied pad rejects purchase");
        Check(m.Build(1,d[2]),"mortar purchase");Check(!m.Build(2,d[1]) && m.Funds==75,"unaffordable purchase rejects");
        Check(m.Sell(0) && m.Funds==112,"floor 50 percent sale");Check(!m.Sell(0) && m.Funds==112,"duplicate sale rejects");
        m.Paused=true;Check(!m.StartWave() && !m.Build(0,d[0]) && !m.Sell(1),"paused orders reject");m.Paused=false;
        Check(m.StartWave() && !m.StartWave(),"non-overlapping wave start");m.Step(3);m.Paused=true;float distance=m.Enemies[0].Distance;int left=m.Remaining;float age=m.Shots.Count>0?m.Shots[0].Age:0;m.Step(100);
        Check(m.Enemies[0].Distance==distance && left==m.Remaining && (m.Shots.Count==0 || age==m.Shots[0].Age),"pause freezes movement spawning projectiles");
        report.AppendLine("PASS economy, invalid purchases, sale rounding, paused orders, wave overlap and pause freeze");
        m=New();m.StartWave();for(int i=0;i<20000 && m.Phase==MatchPhase.Combat;i++)m.Step(1f/60);
        Check(m.Escaped==18 && m.Killed==0 && m.Lives==2 && m.Funds==370 && m.CompletedWaves==1,"escapes cost lives, grant no reward, wave grants 70");
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
        var testGun=ScriptableObject.CreateInstance<DefenseDefinition>();testGun.cost=1;testGun.damage=80;testGun.range=10;testGun.interval=10;testGun.flightTime=.1f;testGun.splashRadius=2;
        m=new MatchState(new[]{pair},new BattleRoute(new[]{Vector3.zero,Vector3.forward*100}),new[]{Vector3.right,Vector3.left});m.Build(0,testGun);m.Build(1,testGun);m.StartWave();m.Step(.01f);
        Check(m.Target(m.Towers[0]).Id==1,"equal-distance targeting chooses stable first ID");
        m.Step(.2f);Check(m.Killed==2 && m.Funds==376 && m.Enemies.Count==0,"overlapping splash shots reward each kill exactly once");
        int funds=m.Funds;m.Step(50);Check(m.Funds==funds,"completed wave rewards exactly once");
        UnityEngine.Object.DestroyImmediate(pair);UnityEngine.Object.DestroyImmediate(testGun);
        report.AppendLine("PASS stable targeting, splash damage, simultaneous lethal shots, exactly-once kill and wave rewards");
        CheckRoles(report);
        Debug.Log("[Verification]\n"+report);return report.ToString();
    }
    static void CheckRoles(StringBuilder report)
    {
        var d=Defenses;var infantry=Waves[0].spawns[0].enemy;var tank=Waves[2].spawns.First(s=>s.enemy.armored).enemy;
        Check(CombatSystem.DamageFor(d[0],tank)<CombatSystem.DamageFor(d[0],infantry)*.2f,"armor resists machine guns");
        Check(CombatSystem.DamageFor(d[1],tank)>CombatSystem.DamageFor(d[1],infantry),"anti-tank damage profile");
        var squad=ScriptableObject.CreateInstance<WaveDefinition>();squad.spawns=new[]{new SpawnEntry(infantry,0,6,.2f)};
        var route=new BattleRoute(new[]{BattleRoute.OnGround(0,0),BattleRoute.OnGround(0,100)});
        var m=new MatchState(new[]{squad},route,new[]{Vector3.zero});m.StartWave();
        Check(m.Remaining==6,"remaining counts every squad member before spawn");
        m.Step(.01f);Check(m.Enemies.Count==1 && m.Remaining==6,"first squad member preserves total");
        for(int i=0;i<65;i++)m.Step(1f/60);
        Check(m.Enemies.Count==6 && m.Enemies.Select(e=>e.LateralOffset).Distinct().Count()==2,"six independently targetable soldiers in two road lanes");
        Check(m.Enemies.Max(e=>e.Distance)-m.Enemies.Min(e=>e.Distance)<1.6f,"squad stays compact");
        m=new MatchState(new[]{squad},route,new[]{Vector3.zero});m.Build(0,d[1]);
        var lead=new EnemyState{Id=1,Definition=infantry,Health=80,Distance=3,Position=route.Position(3)};
        var armor=new EnemyState{Id=2,Definition=tank,Health=tank.health,Distance=1,Position=route.Position(1)};
        m.Enemies.Add(lead);m.Enemies.Add(armor);
        Check(m.Target(m.Towers[0])==armor,"anti-tank prioritizes armor over leading infantry");
        m=new MatchState(new[]{squad},route,new[]{Vector3.zero});m.Build(0,d[2]);
        m.Enemies.Add(lead);
        for(int i=0;i<3;i++)m.Enemies.Add(new EnemyState{Id=3+i,Definition=infantry,Health=80,Distance=1+i*.2f,Position=route.Position(1+i*.2f)});
        Check(m.Target(m.Towers[0])!=lead,"mortar chooses dense squad over isolated leader");
        // Actual projectile resolution: one gun wounds one soldier; one mortar wounds all six.
        foreach(int type in new[]{0,2})
        {
            m=new MatchState(new[]{squad},route,new[]{route.Position(2)+Vector3.right});m.Build(0,d[type]);m.StartWave();
            m.Towers[0].Cooldown=1.1f;
            for(int i=0;i<130;i++)m.Step(1f/60);
            int wounded=m.Enemies.Count(e=>e.Health<e.Definition.health);
            Check(type==0?wounded==1:wounded==6,"single-target versus splash affects individual squad members: type="+type+", wounded="+wounded);
        }
        UnityEngine.Object.DestroyImmediate(squad);
        report.AppendLine("PASS squad totals/formation, armor resistance/priority, cluster targeting, actual single-target and multi-target projectile damage");
    }
    public static string RuntimeState()
    {
        var g=UnityEngine.Object.FindAnyObjectByType<BattleController>();
        if(!g)return "No controller";var m=g.Match;
        return m==null?"Main menu":$"{m.Phase}; wave={m.Wave}; lives={m.Lives}; funds={m.Funds}; killed={m.Killed}; escaped={m.Escaped}; enemies={m.Enemies.Count}; shots={m.Shots.Count}; frame={Time.frameCount}";
    }
}
