namespace TheLineHolds
{
    // Plays one authored list at a time. MatchState decides when a new list may begin.
    public sealed class WaveSpawner
    {
        WaveDefinition wave;
        int next, enemyId;
        float clock;
        public int Unspawned => wave == null ? 0 : wave.spawns.Length - next;
        public bool Complete => wave != null && next == wave.spawns.Length;
        public void Begin(WaveDefinition definition)
        {
            wave = definition;
            next = 0;
            clock = wave.spawns.Length == 0 ? 0 : wave.spawns[0].delay;
        }
        public void Step(MatchState match, float dt)
        {
            clock -= dt;
            while (next < wave.spawns.Length && clock <= 0)
            {
                var definition = wave.spawns[next++].enemy;
                match.Enemies.Add(new EnemyState {
                    Id = ++enemyId, Definition = definition, Health = definition.health,
                    Position = match.Route.Position(0)
                });
                if (next < wave.spawns.Length) clock += wave.spawns[next].delay;
            }
        }
    }
}
