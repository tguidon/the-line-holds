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
                        if(ahead!=enemy && ahead.Definition.spawnSeparation>0 && match.ShareTraffic(enemy,ahead,UnityEngine.Mathf.Max(enemy.Definition.spawnSeparation,ahead.Definition.spawnSeparation))
                            && (ahead.Distance>enemy.Distance || (ahead.Distance==enemy.Distance && ahead.Id<enemy.Id)))
                            next=UnityEngine.Mathf.Min(next,UnityEngine.Mathf.Max(enemy.Distance,ahead.Distance-UnityEngine.Mathf.Max(enemy.Definition.spawnSeparation,ahead.Definition.spawnSeparation)));
                enemy.Distance=next;
                float passing=enemy.Definition.spawnSeparation<=0?UnityEngine.Mathf.Sin(enemy.Distance*.65f+enemy.Id*2.4f)*.12f:0;
                if(enemy.Definition.spawnSeparation<=0)
                    foreach(var vehicle in match.Enemies)
                        if(vehicle.Definition.spawnSeparation>0 && match.ShareTraffic(enemy,vehicle,2.4f) && UnityEngine.Mathf.Abs(vehicle.Distance-enemy.Distance)<2.4f)
                        {float side=enemy.LateralOffset>.05f?1:enemy.LateralOffset<-.05f?-1:enemy.Id%2==0?1:-1;passing=side*(1.02f+(enemy.Id%17)*.01f)-enemy.LateralOffset;break;}
                enemy.AvoidanceOffset=UnityEngine.Mathf.MoveTowards(enemy.AvoidanceOffset,passing,dt*1.6f);
                enemy.Position = match.RouteFor(enemy.Entrance).Position(enemy.Distance, enemy.LateralOffset+enemy.AvoidanceOffset);
                if (enemy.Distance < match.RouteFor(enemy.Entrance).Length) continue;
                match.ResolveEscape(enemy);
                if (match.Ended) return false;
            }
            return true;
        }
    }
}
