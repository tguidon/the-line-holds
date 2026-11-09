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
            int bestScore = -1;
            foreach (var enemy in match.Enemies)
            {
                if (enemy.Resolved || FlatDistance(enemy.Position, match.Pads[tower.Pad]) > tower.Definition.range) continue;
                int score = 0;
                if (tower.Definition.priority == TargetPriority.Armor) score = enemy.Definition.armored ? 1 : 0;
                if (tower.Definition.priority == TargetPriority.Cluster)
                {
                    var impact = PredictedPosition(match, enemy, tower.Definition.flightTime);
                    foreach (var other in match.Enemies)
                        if (!other.Resolved && FlatDistance(PredictedPosition(match, other, tower.Definition.flightTime), impact) <= tower.Definition.splashRadius)
                            score++;
                }
                if (best == null || score > bestScore || (score == bestScore &&
                    (enemy.Distance > best.Distance || (enemy.Distance == best.Distance && enemy.Id < best.Id))))
                { best = enemy; bestScore = score; }
            }
            return best;
        }
        static Vector3 PredictedPosition(MatchState match, EnemyState enemy, float seconds) =>
            match.Route.Position(enemy.Distance + enemy.Definition.speed * seconds, enemy.LateralOffset);
        public static float DamageFor(DefenseDefinition defense, EnemyDefinition enemy) =>
            defense.damage * (enemy.armored ? defense.armorDamageMultiplier : defense.infantryDamageMultiplier);
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
                var shot = new ShotState {
                    Id = ++shotId, TowerPad = tower.Pad, Definition = tower.Definition, Target = target,
                    Origin = origin, Destination = (tower.Definition.splashRadius > 0 ? PredictedPosition(match, target, tower.Definition.flightTime) : target.Position) + Vector3.up * .4f
                };
                match.Shots.Add(shot);
                match.NotifyShotFired(shot);
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
                match.NotifyShotImpacted(shot);
                match.Emit("impact", shot.Destination);
                if (shot.Definition.splashRadius > 0)
                {
                    for (int j = match.Enemies.Count - 1; j >= 0; j--)
                        if (FlatDistance(match.Enemies[j].Position, shot.Destination) <= shot.Definition.splashRadius)
                            Damage(match, match.Enemies[j], DamageFor(shot.Definition, match.Enemies[j].Definition));
                }
                else Damage(match, shot.Target, DamageFor(shot.Definition, shot.Target.Definition));
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
