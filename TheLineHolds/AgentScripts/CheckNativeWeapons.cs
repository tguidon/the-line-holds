using UnityEngine;
using TheLineHolds;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
var game=Object.FindAnyObjectByType<BattleController>();game.NewMatch();
void Check(bool value,string message){if(!value)throw new System.Exception(message);}
var fx=game.combatEffects;var audio=game.audioPlayer;
var sets=new[]{audio.machineGun,audio.cannon,audio.mortarLaunch,audio.softHit,audio.armorHit,audio.shellImpact,audio.mortarImpact};
Check(sets.All(a=>a.Length==3 && a.All(c=>c && c.channels==1 && c.frequency==48000 && c.length<1)),"Native sound bank missing");
Check(new[]{fx.flashMaterial,fx.smokeMaterial,fx.dustMaterial,fx.sparkMaterial,fx.brassMaterial,fx.shellMaterial}.All(m=>m && m.shader.isSupported),"Native effect shader missing");
Check(System.IO.File.Exists(System.IO.Path.Combine(Application.streamingAssetsPath,"THIRD_PARTY_NOTICES.txt")),"Shipped audio attribution missing");
int created=fx.ProjectileObjectsCreated,emitters=fx.GetComponentsInChildren<ParticleSystem>().Length;
var root=new GameObject("Temporary native weapon checks");
float peak=0;var buffer=new float[1024];int frames=Time.frameCount;game.hud.Refresh();Canvas.ForceUpdateCanvases();game.enabled=false;
try
{
    var shots=new List<ShotState>();
    for(int i=0;i<3;i++)
    {
        var p=game.Match.Pads[3+i];var view=Object.Instantiate(game.defenses[i].prefab,p,Quaternion.identity,root.transform);
        var visual=view.GetComponent<TowerVisual>();var destination=BattleRoute.OnGround(p.x+.3f,p.z-2.5f)+Vector3.up*.4f;
        visual.FireAt(destination);
        var target=new EnemyState{Definition=i==1?game.waves[4].spawns.First(e=>e.enemy.armored).enemy:game.waves[0].spawns[0].enemy,Position=destination-Vector3.up*.4f,Health=100};
        Object.Instantiate(target.Definition.prefab,target.Position,Quaternion.identity,root.transform);
        var shot=new ShotState{Id=1000+i,Definition=game.defenses[i],Target=target,Origin=p+Vector3.up*.65f,Destination=destination};
        shots.Add(shot);fx.Fire(shot,visual);audio.Fire(shot.Definition,visual.MuzzlePosition);
        shot.Age=shot.Definition.flightTime*.4f;
    }
    fx.Tick(.03f,shots);Check(fx.ParticleCount>0 && fx.ActiveProjectiles==3,"Native effects not live");
    game.CameraControls.Zoom(-6);await Task.Delay(40);
    ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(System.IO.Path.GetTempPath(),"weapons-native-launch.png"));
    for(int i=0;i<12;i++){AudioListener.GetOutputData(buffer,0);foreach(var v in buffer)peak=Mathf.Max(peak,Mathf.Abs(v));await Task.Delay(15);}
    Check(peak>.0001f && peak<.95f,"Native mixed audio silent or saturated: "+peak);
    audio.SetPaused(true);await Task.Delay(50);Check(audio.ActiveWorldVoices==0,"Native audio pause failed");audio.SetPaused(false);
    foreach(var shot in shots) { fx.Impact(shot);audio.Impact(shot); }
    fx.Tick(.06f,new ShotState[0]);Check(fx.ActiveProjectiles==0,"Native projectile leak");
    var sparkSystem=fx.GetComponentsInChildren<ParticleSystem>().Single(p=>p.name=="Armor sparks");
    var particles=new ParticleSystem.Particle[32];int sparkCount=sparkSystem.GetParticles(particles);
    Check(sparkCount>=4 && particles.Take(sparkCount).All(p=>Vector3.Distance(p.position,shots[1].Destination)>.5f),"Ricochets hidden inside armor centre");
    ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(System.IO.Path.GetTempPath(),"weapons-native-impact.png"));await Task.Delay(150);
    for(int i=0;i<30;i++){var shot=shots[i%3];fx.Fire(shot,null);fx.Impact(shot);}
    Check(fx.ProjectileObjectsCreated==created && fx.GetComponentsInChildren<ParticleSystem>().Length==emitters,"Native pool grew");
    fx.Clear();audio.ResetAudio();Check(fx.ParticleCount==0 && audio.ActiveWorldVoices==0,"Native reset leaked effects/audio");
    string result=$"PASS native 21-clip bank, supported effect shaders, shipped credits, three roles, {created} stable projectiles / {emitters} emitters, 30 repeated shots, audio peak={peak:0.0000}, pause/reset, {Time.frameCount-frames} live frames";
    Debug.Log("[Native weapons] "+result);return result;
}
finally{Object.Destroy(root);game.enabled=true;game.CameraControls.ResetView();game.MainMenu();}
