// Exercise movement and pooled presentation together, from the approach through every bend.
using System;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Pipeline.Scripting;
using TheLineHolds;

var report=new StringBuilder();
bool smooth=true;
try
{
    foreach(string scene in new[]{"WinterForest","Countryside"})
    {
        var load=SceneManager.LoadSceneAsync(scene);
        await Wait.Until(()=>load.isDone && UnityEngine.Object.FindAnyObjectByType<BattleController>()?.Match!=null,timeoutMs:20000);
        var g=UnityEngine.Object.FindAnyObjectByType<BattleController>();
        var roster=g.waves.SelectMany(w=>w.spawns).Select(s=>s.enemy).Distinct().Where(e=>e.spawnSeparation>0).ToArray();
        foreach(var definition in roster)
        {
            g.NewMatch(42);
            var match=g.Match;
            var unit=new EnemyState{Definition=definition, Id=7, Distance=-24, Health=definition.health, MaxHealth=definition.health};
            match.Enemies.Add(unit);
            Transform actor=null,body=null;
            var bind=definition.prefab.transform.Find("Miniature");
            float maxRock=0,maxTurn=0,maxDrift=0;
            Quaternion last=Quaternion.identity;
            const float dt=1f/60;
            int frames=0;
            while(unit.Distance+unit.Speed*dt<match.Route.Length)
            {
                RouteMovement.Step(match,dt);
                // Repeated hits must not tip a travelling chassis on every impact.
                if(++frames%24==0)g.EnemyViews.Hit(unit);
                g.EnemyViews.Sync(match,dt);
                if(!actor)
                {
                    actor=g.dynamicRoot.GetComponentsInChildren<EnemyVisual>().Single().transform;
                    body=actor.Find("Miniature");last=actor.rotation;
                }
                maxRock=Mathf.Max(maxRock,Quaternion.Angle(bind.localRotation,body.localRotation));
                maxTurn=Mathf.Max(maxTurn,Quaternion.Angle(last,actor.rotation));
                maxDrift=Mathf.Max(maxDrift,Vector3.Distance(actor.position,unit.Position));
                last=actor.rotation;
            }
            report.AppendLine($"{scene} {definition.name}: hull rock {maxRock:F2} deg, peak turn per frame {maxTurn:F2} deg, position error {maxDrift:F3} m");
            smooth &= maxRock<.1f && maxTurn<4f && maxDrift<.001f;
        }
    }
    if(!smooth)throw new Exception(report.ToString());
    return report.ToString();
}
finally{var g=UnityEngine.Object.FindAnyObjectByType<BattleController>();if(g)g.MainMenu();}
