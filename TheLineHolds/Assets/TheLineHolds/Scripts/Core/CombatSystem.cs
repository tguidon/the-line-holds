using UnityEngine;
namespace TheLineHolds
{
    // Stable target selection and explicit delayed hits. Rendering never resolves a projectile.
    public sealed class CombatSystem
    {
        int shotId;
        public static EnemyState Target(MatchState match, TowerState tower)
        {
            EnemyState best = null;
            foreach (var enemy in match.Enemies)
                if (!enemy.Resolved && FlatDistance(enemy.Position, match.Pads[tower.Pad]) <= tower.Definition.range &&
                    (best == null || enemy.Distance > best.Distance || (enemy.Distance == best.Distance && enemy.Id < best.Id)))
                    best = enemy;
            return best;
        }
        static float FlatDistance(Vector3 a, Vector3 b)
        {
            a.y = b.y = 0;
            return Vector3.Distance(a, b);
        }
        public void Step(MatchState match, float dt)
        {
            foreach (var tower in match.Towers)
            {
                if (tower == null) continue;
                tower.Cooldown = Mathf.Max(0, tower.Cooldown - dt);
                if (tower.Cooldown > 0) continue;
                var target = Target(match, tower);
                if (target == null) continue;
                tower.Cooldown = tower.Definition.interval;
                var origin = match.Pads[tower.Pad] + Vector3.up * .65f;
                match.Shots.Add(new ShotState {
                    Id = ++shotId, Definition = tower.Definition, Target = target,
                    Origin = origin, Destination = target.Position + Vector3.up * .4f
                });
                match.Emit("fire", origin);
            }
            for (int i = match.Shots.Count - 1; i >= 0; i--)
            {
                var shot = match.Shots[i];
                shot.Age += dt;
                if (shot.Definition.splashRadius <= 0 && !shot.Target.Resolved)
                    shot.Destination = shot.Target.Position + Vector3.up * .4f;
                if (shot.Age < shot.Definition.flightTime) continue;
                match.Shots.RemoveAt(i);
                match.Emit("impact", shot.Destination);
                if (shot.Definition.splashRadius > 0)
                {
                    for (int j = match.Enemies.Count - 1; j >= 0; j--)
                        if (FlatDistance(match.Enemies[j].Position, shot.Destination) <= shot.Definition.splashRadius)
                            Damage(match, match.Enemies[j], shot.Definition.damage);
                }
                else Damage(match, shot.Target, shot.Definition.damage);
            }
        }
        static void Damage(MatchState match, EnemyState enemy, float amount)
        {
            if (enemy.Resolved) return;
            enemy.Health -= amount;
            if (enemy.Health <= 0) match.ResolveKill(enemy);
        }
    }
}
