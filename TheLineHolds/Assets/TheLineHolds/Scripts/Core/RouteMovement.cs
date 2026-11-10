namespace TheLineHolds
{
    public static class RouteMovement
    {
        // Returns immediately when an escape ends the match; combat and wave completion must not run afterward.
        public static bool Step(MatchState match, float dt)
        {
            for (int i = match.Enemies.Count - 1; i >= 0; i--)
            {
                var enemy = match.Enemies[i];
                enemy.SlowRemaining=UnityEngine.Mathf.Max(0,enemy.SlowRemaining-dt);
                float next=enemy.Distance+enemy.Speed*dt;
                if(enemy.Definition.spawnSeparation>0)
                    foreach(var ahead in match.Enemies)
                        if(ahead!=enemy && ahead.Definition.spawnSeparation>0 && ahead.Distance>enemy.Distance)
                            next=UnityEngine.Mathf.Min(next,UnityEngine.Mathf.Max(enemy.Distance,ahead.Distance-UnityEngine.Mathf.Max(enemy.Definition.spawnSeparation,ahead.Definition.spawnSeparation)));
                enemy.Distance=next;
                enemy.Position = match.Route.Position(enemy.Distance, enemy.LateralOffset);
                if (enemy.Distance < match.Route.Length) continue;
                match.ResolveEscape(enemy);
                if (match.Ended) return false;
            }
            return true;
        }
    }
}
