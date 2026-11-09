using System;
using System.Linq;
using UnityEngine;
using TheLineHolds;
public static class ShowWeaponFeedback
{
    public static string Stage()
    {
        var game=UnityEngine.Object.FindAnyObjectByType<BattleController>();game.NewMatch();game.enabled=false;
        var old=GameObject.Find("Weapon feedback visual review");if(old)UnityEngine.Object.DestroyImmediate(old);
        var root=new GameObject("Weapon feedback visual review");
        var shots=new System.Collections.Generic.List<ShotState>();
        for(int i=0;i<3;i++)
        {
            var p=BattleRoute.DefaultPads[3+i];
            var view=UnityEngine.Object.Instantiate(game.defenses[i].prefab,p,Quaternion.identity,root.transform);
            var visual=view.GetComponent<TowerVisual>();
            var destination=BattleRoute.OnGround(p.x+.3f,-2.5f)+Vector3.up*.4f;
            var target=new EnemyState{Definition=game.waves[0].spawns[0].enemy,Position=destination,Health=100};
            var enemy=UnityEngine.Object.Instantiate(target.Definition.prefab,destination-Vector3.up*.4f,Quaternion.identity,root.transform);
            var shot=new ShotState{Id=1000+i,Definition=game.defenses[i],Target=target,Origin=p+Vector3.up*.65f,Destination=destination};
            visual.FireAt(destination);game.combatEffects.Fire(shot,visual);game.audioPlayer.Fire(shot.Definition,visual.MuzzlePosition);
            shot.Age=shot.Definition.flightTime*(i==2?.45f:.12f);shots.Add(shot);
        }
        game.combatEffects.Tick(.025f,shots);
        // Add representative ground/armor responses alongside the launch samples.
        foreach(var s in shots)
        {
            var copy=new ShotState{Id=2000+s.Id,Definition=s.Definition,Target=s.Target,Destination=s.Destination};
            game.combatEffects.Impact(copy);
        }
        game.combatEffects.Tick(.01f,shots);
        game.CameraControls.ResetView();game.CameraControls.Zoom(-20);game.CameraControls.enabled=false;
        var focus=BattleRoute.OnGround(-1.5f,1.8f)+Vector3.up*.7f;game.battleCamera.transform.position=focus-game.battleCamera.transform.forward*40;
        game.hud.Refresh();Canvas.ForceUpdateCanvases();
        Debug.Log($"[Weapon review] {game.combatEffects.ParticleCount} live particles; three muzzle sockets, cartridge ejection, cannon recoil and airborne mortar shell. Temporary staged view.");
        return "Temporary three-role weapon review; stop Play Mode to remove";
    }
}
