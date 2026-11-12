using System.Collections.Generic;
using UnityEngine;
namespace TheLineHolds
{
    public sealed class WaveSpawner
    {
        struct Ticket { public EnemyDefinition Enemy; public float At; public int Member; }
        readonly List<Ticket> infantry=new List<Ticket>(),vehicles=new List<Ticket>();
        WaveDefinition wave;System.Random variation;int nextFoot,nextVehicle,enemyId,unspawned;float clock;
        System.Func<float> entryDistance;
        float spawnDistance;
        public int Unspawned=>unspawned;
        public bool Complete=>wave!=null && unspawned==0;
        public void Begin(WaveDefinition definition,int seed=1944,System.Func<float> entryDistance=null)
        {
            this.entryDistance=entryDistance;spawnDistance=0;
            wave=definition;variation=new System.Random(seed^731);nextFoot=nextVehicle=0;clock=0;unspawned=wave.UnitCount;infantry.Clear();vehicles.Clear();float at=0;
            foreach(var entry in WavePlanner.Create(wave,seed))
            {
                at+=entry.delay;
                for(int member=0;member<entry.UnitCount;member++)
                {
                    var ticket=new Ticket{Enemy=entry.enemy,At=at,Member=member};
                    (entry.enemy.spawnSeparation>0?vehicles:infantry).Add(ticket);
                    if(member<entry.UnitCount-1)at+=Mathf.Max(.06f,entry.interval*(wave.varied?.55f+(float)variation.NextDouble()*.9f:1));
                }
            }
        }
        public void Step(MatchState match,float dt)
        {
            clock+=dt;
            if(entryDistance!=null && ((nextFoot<infantry.Count && infantry[nextFoot].At<=clock) ||
                (nextVehicle<vehicles.Count && vehicles[nextVehicle].At<=clock)))
                // A wider view may move later arrivals upstream. Keep their order when zooming back in.
                spawnDistance=Mathf.Min(spawnDistance,entryDistance());
            // Infantry escort the convoy while queued vehicles wait for a safe road gap.
            while(nextFoot<infantry.Count && infantry[nextFoot].At<=clock)Spawn(match,infantry[nextFoot++]);
            while(nextVehicle<vehicles.Count && vehicles[nextVehicle].At<=clock)
            {
                var ticket=vehicles[nextVehicle];bool blocked=false;
                foreach(var active in match.Enemies)
                    if(active.Definition.spawnSeparation>0 && active.Distance-spawnDistance<Mathf.Max(ticket.Enemy.spawnSeparation,active.Definition.spawnSeparation)){blocked=true;break;}
                if(blocked)break;
                Spawn(match,ticket);nextVehicle++;
            }
        }
        void Spawn(MatchState match,Ticket ticket)
        {
            var enemy=ticket.Enemy;
            float lane=enemy.spawnSeparation>0?0:wave.varied?(float)(variation.NextDouble()-.5)*1.56f:(ticket.Member%2==0?-.27f:.27f);
            float hp=enemy.health*wave.healthMultiplier;
            match.Enemies.Add(new EnemyState{Id=++enemyId,Definition=enemy,Health=hp,MaxHealth=hp,SpeedMultiplier=wave.varied?.92f+(float)variation.NextDouble()*.16f:1,LateralOffset=lane,Distance=spawnDistance,Position=match.Route.Position(spawnDistance,lane)});
            unspawned--;
        }
    }
}
