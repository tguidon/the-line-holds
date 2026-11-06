using UnityEngine;
namespace TheLineHolds
{
    // An authored entry releases a compact squad, then waits before the next entry.
    public sealed class WaveSpawner
    {
        WaveDefinition wave;
        int next, member, enemyId, unspawned;
        float clock;
        public int Unspawned => unspawned;
        public bool Complete => wave != null && unspawned == 0;
        public void Begin(WaveDefinition definition)
        {
            wave = definition; next = member = 0; unspawned = wave.UnitCount;
            clock = wave.spawns.Length == 0 ? 0 : wave.spawns[0].delay;
        }
        public void Step(MatchState match, float dt)
        {
            clock -= dt;
            while (next < wave.spawns.Length && clock <= 0)
            {
                var entry = wave.spawns[next];
                // Delay the whole following column rather than releasing a catch-up burst.
                // This applies across authored entries as well as within a vehicle section.
                bool blocked=false;
                if(entry.enemy.spawnSeparation>0)
                    foreach(var active in match.Enemies)
                        if(active.Definition==entry.enemy && active.Distance<entry.enemy.spawnSeparation) { blocked=true;break; }
                if(blocked) { clock=0;return; }
                float lane = entry.enemy.armored ? 0 : (member % 2 == 0 ? -.27f : .27f);
                match.Enemies.Add(new EnemyState {
                    Id = ++enemyId, Definition = entry.enemy, Health = entry.enemy.health,
                    LateralOffset = lane, Position = match.Route.Position(0, lane)
                });
                unspawned--; member++;
                if (member < entry.UnitCount) clock += Mathf.Max(.01f, entry.interval);
                else
                {
                    next++; member = 0;
                    if (next < wave.spawns.Length) clock += wave.spawns[next].delay;
                }
            }
        }
    }
}
