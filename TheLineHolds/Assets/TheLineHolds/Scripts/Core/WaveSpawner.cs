using System.Collections.Generic;
using UnityEngine;
namespace TheLineHolds
{
    public sealed class WaveSpawner
    {
        struct Ticket { public EnemyDefinition Enemy; public float At; public int Member, Entrance; }
        readonly List<Ticket> infantry=new List<Ticket>(),vehicles=new List<Ticket>();
        WaveDefinition wave;System.Random variation;int nextFoot,nextVehicle,enemyId,unspawned;float clock;
        System.Func<int,float> entryDistance;
        readonly float[] spawnDistance=new float[2];
        public int Unspawned=>unspawned;
        public bool Complete=>wave!=null && unspawned==0;
        public void Begin(WaveDefinition definition,int seed=1944,System.Func<int,float> entryDistance=null,int entrancePattern=0)
        {
            this.entryDistance=entryDistance;spawnDistance[0]=spawnDistance[1]=0;
            int scheduled=0;
            wave=definition;variation=new System.Random(seed^731);nextFoot=nextVehicle=0;clock=0;unspawned=wave.UnitCount;infantry.Clear();vehicles.Clear();float at=0;
            foreach(var entry in WavePlanner.Create(wave,seed))
            {
                at+=entry.delay;
                for(int member=0;member<entry.UnitCount;member++)
                {
                    var ticket=new Ticket{Enemy=entry.enemy,At=at,Member=member,Entrance=entrancePattern==2 ? scheduled%2 : entrancePattern};
                    (entry.enemy.spawnSeparation>0?vehicles:infantry).Add(ticket);
                    scheduled++;
                    if(member<entry.UnitCount-1)at+=Mathf.Max(.06f,entry.interval*(wave.varied?.55f+(float)variation.NextDouble()*.9f:1));
                }
            }
        }
        public void Step(MatchState match,float dt)
        {
            clock+=dt;
            if(entryDistance!=null && ((nextFoot<infantry.Count && infantry[nextFoot].At<=clock) ||
                (nextVehicle<vehicles.Count && vehicles[nextVehicle].At<=clock)))
                for(int i=0;i<(match.SecondRoute==null?1:2);i++)
                    spawnDistance[i]=Mathf.Min(spawnDistance[i],entryDistance(i));
            // Infantry escort the convoy while queued vehicles wait for a safe road gap.
            while(nextFoot<infantry.Count && infantry[nextFoot].At<=clock)Spawn(match,infantry[nextFoot++]);
            while(nextVehicle<vehicles.Count && vehicles[nextVehicle].At<=clock)
            {
                var ticket=vehicles[nextVehicle];bool blocked=false;
                foreach(var active in match.Enemies)
                    if(active.Entrance==ticket.Entrance && active.Definition.spawnSeparation>0 && active.Distance-spawnDistance[ticket.Entrance]<Mathf.Max(ticket.Enemy.spawnSeparation,active.Definition.spawnSeparation)){blocked=true;break;}
                if(blocked)break;
                Spawn(match,ticket);nextVehicle++;
            }
        }
        void Spawn(MatchState match,Ticket ticket)
        {
            var enemy=ticket.Enemy;
            float lane=enemy.spawnSeparation>0?0:wave.varied?(float)(variation.NextDouble()-.5)*1.56f:(ticket.Member%2==0?-.27f:.27f);
            float hp=enemy.health*wave.healthMultiplier;
            match.Enemies.Add(new EnemyState{Id=++enemyId,Entrance=ticket.Entrance,Definition=enemy,Health=hp,MaxHealth=hp,SpeedMultiplier=wave.varied?.92f+(float)variation.NextDouble()*.16f:1,LateralOffset=lane,Distance=spawnDistance[ticket.Entrance],Position=match.RouteFor(ticket.Entrance).Position(spawnDistance[ticket.Entrance],lane)});
            unspawned--;
        }
    }
}
