// Play Mode check for camera framing, live offscreen arrivals and weather simulation.
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Pipeline.Scripting;
using TheLineHolds;
var game=UnityEngine.Object.FindAnyObjectByType<BattleController>();
if(!game.winterForest)throw new Exception("Open Winter Forest");
var cam=game.battleCamera;
var snow=GameObject.Find("Light falling snow").GetComponent<ParticleSystem>();
var drift=GameObject.Find("Spindrift through clearing").GetComponent<ParticleSystem>();
var report=new System.Text.StringBuilder();
try
{
    foreach(float aspect in new[]{4f/3,16f/9,2.4f})
    {
        game.NewMatch(42);cam.aspect=aspect;game.CameraControls.ResetView();
        foreach(var p in game.Route.Points.Concat(game.Sites))
        {
            var v=cam.WorldToViewportPoint(p);
            if(v.x<.03f || v.x>.97f || v.y<.12f || v.y>.90f)throw new Exception("Clipped overview: "+v);
        }
        if(aspect>2)game.CameraControls.Rotate(110);
        game.StartNextWave();
        await Wait.Until(()=>game.Match.Enemies.Count>0,timeoutMs:6000);
        var first=game.Match.Enemies[0];var planes=GeometryUtility.CalculateFrustumPlanes(cam);
        if(first.Distance>=0 || GeometryUtility.TestPlanesAABB(planes,new Bounds(first.Position+Vector3.up,Vector3.one*2.5f)))
            throw new Exception("Arrival visible at spawn");
        report.AppendLine($"Aspect {aspect:F2}: full route and sites in overview, incoming patrol spawned {Mathf.Abs(first.Distance):F1} m before trail marker.");
    }
    if(!snow.isPlaying || snow.particleCount<20 || !drift.isPlaying || drift.particleCount==0 ||
        drift.velocityOverLifetime.x.constant<=1 || game.audioPlayer.ambienceClip.name!="WinterWind")throw new Exception("Winter weather missing");
    report.AppendLine($"Live snowfall {snow.particleCount}, low drifting snow {drift.particleCount}.");
    return report.ToString();
}
finally{cam.ResetAspect();game.CameraControls.ResetView();game.MainMenu();}
