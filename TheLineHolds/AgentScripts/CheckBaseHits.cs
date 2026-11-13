// Play Mode or development player. Only shorten travel time; movement resolves each arrival.
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Pipeline.Scripting;
using TheLineHolds;
void Check(bool value,string detail){if(!value)throw new Exception(detail);}
var report=new System.Text.StringBuilder();
foreach(string scene in new[]{"Countryside","WinterForest"})
{
    var load=SceneManager.LoadSceneAsync(scene);
    await Wait.Until(()=>load.isDone,timeoutMs:20000);
    BattleController Game()=>UnityEngine.Object.FindAnyObjectByType<BattleController>();
    await Wait.Until(()=>Game() && Game().gameObject.scene.name==scene && Game().Match!=null,timeoutMs:20000);
    var g=Game();
    var authored=g.waves;
    var probe=ScriptableObject.CreateInstance<WaveDefinition>();
    try
    {
        var roster=authored.SelectMany(w=>w.spawns).Select(s=>s.enemy).Distinct().ToArray();
        Check(roster.Length==7 && roster.Select(e=>e.baseHit).Distinct().Count()==4,"Authored weight coverage");
        foreach(var definition in roster)
        {
            probe.spawns=new[]{new SpawnEntry(definition,0)};
            g.waves=new[]{probe,probe}.Concat(authored).ToArray();g.NewMatch(42);
            var m=g.Match;
            int arrivals=0;m.EnemyReachedBase+=e=>arrivals++;
            m.StartWave();m.Step(.02f);
            var unit=m.Enemies.Single();unit.Distance=m.Route.Length-.001f;
            m.Step(.02f);
            Check(arrivals==1 && m.Escaped==1 && m.Lives==19 && m.Killed==0 && m.Phase==MatchPhase.Preparation,"Arrival accounting");
            var weight=definition.baseHit;
            int chips=weight==BaseHitWeight.HeavyTank?13:weight==BaseHitWeight.LightTank?9:weight==BaseHitWeight.Wheeled?6:3;
            int expected=1+chips*2+(weight==BaseHitWeight.HeavyTank || weight==BaseHitWeight.LightTank?1:0);
            Check(g.EnemyViews.RetiringCount==1 && g.combatEffects.ParticleCount==expected,definition.name+" impact animation");
            var clips=weight==BaseHitWeight.HeavyTank?g.audioPlayer.mortarImpact:weight==BaseHitWeight.LightTank?g.audioPlayer.shellImpact:weight==BaseHitWeight.Wheeled?g.audioPlayer.armorHit:g.audioPlayer.softHit;
            var voice=g.audioPlayer.GetComponentsInChildren<AudioSource>().Single(v=>v.isPlaying && clips.Contains(v.clip));
            Check(voice.volume>0 && Vector3.Distance(voice.transform.position,m.Route.Points.Last())<.01f,"Missing objective audio");
            g.Pause();await Task.Delay(120);
            Check(g.EnemyViews.RetiringCount==1 && !voice.isPlaying && g.combatEffects.ParticleCount==expected,$"{scene} {definition.name}: paused={m.Paused} views={g.EnemyViews.RetiringCount}, audio={voice.isPlaying}, particles={g.combatEffects.ParticleCount}/{expected}");
            g.Pause();
            await Wait.Until(()=>g.EnemyViews.RetiringCount==0 && g.combatEffects.ParticleCount==0,timeoutMs:4000);
        }
        probe.spawns=new[]{new SpawnEntry(roster.First(e=>e.baseHit==BaseHitWeight.Infantry),0,20,.06f)};
        g.NewMatch(11);var final=g.Match;int hits=0;final.EnemyReachedBase+=e=>hits++;final.StartWave();
        for(int i=0;i<2000 && !final.Ended;i++)
        {
            foreach(var e in final.Enemies)e.Distance=final.Route.Length-.001f;
            final.Step(.016f);
        }
        Check(hits==20 && final.Escaped==20 && final.Lives==0 && final.Phase==MatchPhase.Defeat && final.CompletedWaves==0,"Final life");
        final.Step(1);Check(hits==20,"Duplicate terminal impact");
        g.NewMatch();Check(g.EnemyViews.RetiringCount==0 && g.combatEffects.ParticleCount==0 && g.audioPlayer.ActiveWorldVoices==0,"Restart left base hits active");
        report.AppendLine(scene+": seven unit arrivals animated and sounded, pause/resume, final life and restart passed");
    }
    finally {g.waves=authored;UnityEngine.Object.Destroy(probe);g.NewMatch();}
}
UnityEngine.Object.FindAnyObjectByType<BattleController>().MainMenu();
return report.ToString();
