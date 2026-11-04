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
                enemy.Distance += enemy.Definition.speed * dt;
                enemy.Position = match.Route.Position(enemy.Distance, enemy.LateralOffset);
                if (enemy.Distance < match.Route.Length) continue;
                match.ResolveEscape(enemy);
                if (match.Ended) return false;
            }
            return true;
        }
    }
}
