using System;
using System.Collections.Generic;
namespace TheLineHolds
{
    // Composition is announced and fixed. Seeded formations change the rhythm without surprise counters.
    public static class WavePlanner
    {
        public static SpawnEntry[] Create(WaveDefinition wave,int seed)
        {
            if(!wave.varied)return wave.spawns;
            var random=new Random(seed);var remaining=new int[wave.spawns.Length];int total=0;
            for(int i=0;i<remaining.Length;i++){remaining[i]=wave.spawns[i].UnitCount;total+=remaining[i];}
            var plan=new List<SpawnEntry>();
            while(total>0)
            {
                int roll=random.Next(total),kind=0;while(roll>=remaining[kind])roll-=remaining[kind++];
                var enemy=wave.spawns[kind].enemy;bool vehicle=enemy.spawnSeparation>0;
                bool stream=wave.formation==WaveFormation.Stream || (wave.formation==WaveFormation.Mixed && random.Next(3)==0);
                int count=Math.Min(remaining[kind],vehicle?random.Next(1,3):stream?random.Next(4,10):random.Next(2,9));
                float delay=plan.Count==0?1.2f:stream?.3f+(float)random.NextDouble()*.35f:1.5f+(float)random.NextDouble()*2.2f;
                float gap=vehicle?1.8f:stream?.35f+(float)random.NextDouble()*.25f:.18f+(float)random.NextDouble()*.12f;
                plan.Add(new SpawnEntry(enemy,delay,count,gap));remaining[kind]-=count;total-=count;
            }
            return plan.ToArray();
        }
    }
}
