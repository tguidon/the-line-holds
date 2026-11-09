using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using TheLineHolds;
public static class CheckWeaponFeedback
{
    static void Check(bool value,string message){if(!value)throw new Exception(message);}
    public static string Assets()
    {
        var audio=UnityEngine.Object.FindAnyObjectByType<BattleAudio>();
        var sets=new[]{audio.machineGun,audio.cannon,audio.mortarLaunch,audio.softHit,audio.armorHit,audio.shellImpact,audio.mortarImpact};
        Check(sets.All(a=>a.Length==3),"Missing sound variations");
        foreach(var clip in sets.SelectMany(a=>a))
        {
            Check(clip && clip.channels==1 && clip.frequency==48000 && clip.length>.1f && clip.length<1,"Invalid combat clip format");
            var samples=new float[clip.samples];Check(clip.GetData(samples,0),"Audio data unavailable");
            Check(samples.Max(s=>Mathf.Abs(s))>.15f && samples.Max(s=>Mathf.Abs(s))<.8f,"Audio silent or clipped");
            Check(Mathf.Abs(samples[0])<.001f && Mathf.Abs(samples.Last())<.001f,"Missing edge fade");
        }
        foreach(var definition in UnityEngine.Object.FindAnyObjectByType<BattleController>().defenses)
        {
            var parts=definition.prefab.GetComponentsInChildren<Transform>();
            Check(parts.Any(t=>t.name.StartsWith("Muzzle socket")) && parts.Any(t=>t.name.StartsWith("Recoil")),"Weapon sockets missing");
        }
        string result="PASS 21 non-silent mono 48kHz short PCM variations with fades/headroom; all muzzle and recoil anchors";Debug.Log("[Weapon feedback] "+result);return result;
    }
    public static string Events()
    {
        var game=UnityEngine.Object.FindAnyObjectByType<BattleController>();
        var match=new MatchState(game.waves,new BattleRoute(BattleRoute.DefaultPoints),BattleRoute.DefaultPads);
        Check(match.Build(0,game.defenses[0]),"Could not build test tower");
        int fired=0,impacted=0,legacyFire=0,legacyImpact=0;
        var ids=new System.Collections.Generic.HashSet<int>();
        match.ShotFired+=s=>{fired++;Check(ids.Add(s.Id),"Duplicate fire event");Check(s.TowerPad==0,"Wrong source pad");Check(s.Origin==match.Pads[0]+Vector3.up*.65f,"Simulation origin changed");};
        match.ShotImpacted+=s=>{impacted++;Check(ids.Remove(s.Id),"Impact without source event");};
        match.Cue+=(cue,p)=>{if(cue=="fire")legacyFire++;if(cue=="impact")legacyImpact++;};
        match.StartWave();for(int i=0;i<6000 && !match.Ended && match.Phase==MatchPhase.Combat;i++)match.Step(1f/60);
        Check(fired>0 && impacted>0 && fired==legacyFire && impacted==legacyImpact,"Typed/legacy events disagree");
        string result=$"PASS shot context and exactly-once typed events: {fired} fire / {impacted} impacts; original simulation origin";Debug.Log("[Weapon feedback] "+result);return result;
    }
    public static async Task<string> Play()
    {
        var game=UnityEngine.Object.FindAnyObjectByType<BattleController>();game.NewMatch();
        var fx=game.combatEffects;var audio=game.audioPlayer;
        int objects=fx.ProjectileObjectsCreated,systems=fx.GetComponentsInChildren<ParticleSystem>().Length;
        var root=new GameObject("Temporary weapon feedback checks");var shots=new System.Collections.Generic.List<ShotState>();
        try
        {
            for(int i=0;i<3;i++)
            {
                var p=game.Match.Pads[3+i];var view=UnityEngine.Object.Instantiate(game.defenses[i].prefab,p,Quaternion.identity,root.transform);
                var visual=view.GetComponent<TowerVisual>();var parts=view.GetComponentsInChildren<Transform>();
                var bag=parts.First(t=>t.name.StartsWith("Sandbag"));var fixedBag=bag.position;
                var fixedPart=parts.First(t=>t.name.StartsWith(i==1?"Rubber tire":i==2?"Base plate":"Mount"));var fixedPosition=fixedPart.position;
                var destination=p+new Vector3(0,0,3);visual.Aim(destination-p,1);fixedPosition=fixedPart.position;
                var tip=visual.MuzzlePosition;Check(Vector3.Distance(tip,p)>.45f,"Muzzle at generic root");
                Check(Vector3.Dot(Vector3.ProjectOnPlane(visual.MuzzleForward,Vector3.up).normalized,Vector3.forward)>.99f,"Barrel faces away from target");
                if(i==2)Check(visual.MuzzleForward.y>.45f,"Mortar tube not elevated");
                visual.FireAt(destination);Check(visual.RecoilDistance>.02f,"Recoil absent");
                Check(bag.position==fixedBag && fixedPart.position==fixedPosition,"Recoil moved fixed fortification/carriage");
                var target=new EnemyState{Definition=game.waves[0].spawns[0].enemy,Position=destination,Health=100};
                for(int j=0;j<10;j++)
                {
                    var shot=new ShotState{Id=i*100+j,Definition=game.defenses[i],Target=target,Origin=p+Vector3.up*.65f,Destination=destination};
                    shots.Add(shot);fx.Fire(shot,visual);audio.Fire(shot.Definition,tip);
                    Check(fx.ParticleCount>0,"No visible particles emitted");
                    shot.Age=shot.Definition.flightTime*.5f;fx.Tick(.02f,shots);
                    fx.Impact(shot);audio.Impact(shot);shots.Remove(shot);
                }
                var frozen=parts.First(t=>t.name.StartsWith("Recoil")).localPosition;visual.Aim(destination-p,0);Check(parts.First(t=>t.name.StartsWith("Recoil")).localPosition==frozen,"Paused recoil moved");
                visual.Aim(destination-p,1);Check(visual.RecoilDistance<.001f,"Recoil failed to recover");
            }
            Check(fx.ProjectileObjectsCreated==objects && fx.GetComponentsInChildren<ParticleSystem>().Length==systems,"Effect pool grew during repeated shots");
            Check(fx.ActiveProjectiles==0,"Projectile leaked after impacts");
            Check(audio.ActiveWorldVoices>0,"No sound voices playing");
            audio.SetPaused(true);await Task.Delay(100);Check(audio.ActiveWorldVoices==0,"Audio did not pause");audio.SetPaused(false);
            audio.sfxGain=0;await Task.Delay(40);Check(audio.GetComponentsInChildren<AudioSource>().All(v=>v.volume==0),"SFX gain did not reach all voices");audio.sfxGain=.65f;
            fx.Tick(1,new ShotState[0]);Check(fx.ParticleCount==0,"Particles exceeded short lifetime");
            audio.ResetAudio();Check(audio.ActiveWorldVoices==0,"Reset left voices playing");
            game.MainMenu();Check(fx.ActiveProjectiles==0 && fx.ParticleCount==0,"Menu left effects active");
            string result=$"PASS all three muzzle directions, mortar elevation, barrel-only recoil/recovery/freeze, {objects} stable pooled projectiles, {systems} stable emitters, 30 repeated shot effects, live audio/pause/gain/reset";Debug.Log("[Weapon feedback] "+result);return result;
        }
        finally{UnityEngine.Object.Destroy(root);fx.Clear();audio.sfxGain=.65f;game.MainMenu();}
    }
}
