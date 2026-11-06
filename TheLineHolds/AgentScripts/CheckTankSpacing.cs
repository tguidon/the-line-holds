using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using TheLineHolds;
public static class CheckTankSpacing
{
    public static string All()
    {
        var tank=AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/TheLineHolds/Data/Enemies/LightTank.asset");
        var bounds=new Bounds();bool first=true;
        foreach(var r in tank.prefab.GetComponentsInChildren<Renderer>()) { if(first){bounds=r.bounds;first=false;}else bounds.Encapsulate(r.bounds); }
        var waves=Enumerable.Range(1,5).Select(i=>AssetDatabase.LoadAssetAtPath<WaveDefinition>($"Assets/TheLineHolds/Data/Waves/Wave{i}.asset")).ToArray();
        float minimum=float.MaxValue;
        var route=new BattleRoute(BattleRoute.DefaultPoints);
        var stress=ScriptableObject.CreateInstance<WaveDefinition>();
        stress.spawns=Enumerable.Range(0,4).Select(i=>new SpawnEntry(tank,0,3,.01f)).ToArray();
        foreach(var wave in waves.Concat(new[]{stress}))
        {
            var column=ScriptableObject.CreateInstance<WaveDefinition>();
            column.spawns=wave.spawns.Where(s=>s.enemy.armored).ToArray();
            var m=new MatchState(new[]{column},route,BattleRoute.DefaultPads);m.StartWave();
            for(int step=0;step<30000 && !m.Ended && m.Phase==MatchPhase.Combat;step++)
            {
                m.Step(1f/60);
                var tanks=m.Enemies.Where(e=>e.Definition.armored).ToArray();
                for(int a=0;a<tanks.Length;a++)for(int b=a+1;b<tanks.Length;b++)
                    minimum=Mathf.Min(minimum,Vector3.Distance(tanks[a].Position,tanks[b].Position));
            }
            UnityEngine.Object.DestroyImmediate(column);
        }
        UnityEngine.Object.DestroyImmediate(stress);
        string result=$"Tank model size={bounds.size}; minimum centre separation={minimum:0.00}m";Debug.Log("[Tank regression] "+result);
        // A conservative horizontal bounding circle remains safe when vehicles turn through a bend.
        float diameter=new Vector2(bounds.size.x,bounds.size.z).magnitude;
        if(minimum<diameter+.1f)throw new Exception("Tanks can overlap: "+result+", required="+(diameter+.1f));
        return result;
    }
}
