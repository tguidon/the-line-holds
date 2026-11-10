using UnityEngine;
namespace TheLineHolds
{
    // Stable target selection and explicit delayed hits. Rendering never resolves a projectile.
    public sealed class CombatSystem
    {
        int shotId;
        static Vector3[] predicted=new Vector3[256];
        public static EnemyState Target(MatchState match, TowerState tower)
        {
            EnemyState best = null;
            int bestScore = -1;
            if(predicted.Length<match.Enemies.Count)System.Array.Resize(ref predicted,match.Enemies.Count*2);
            if(tower.Definition.priority==TargetPriority.Cluster)
                for(int i=0;i<match.Enemies.Count;i++)predicted[i]=PredictedPosition(match,match.Enemies[i],tower.Definition.flightTime);
            for(int index=0;index<match.Enemies.Count;index++)
            {
                var enemy=match.Enemies[index];
                if (enemy.Resolved || FlatDistance(enemy.Position, match.Pads[tower.Pad]) > tower.Definition.range) continue;
                int score = 0;
                if (tower.Definition.priority == TargetPriority.Armor) score = enemy.Definition.armored ? 1 : 0;
                if (tower.Definition.priority == TargetPriority.Cluster)
                {
                    var impact = predicted[index];
                    for(int j=0;j<match.Enemies.Count;j++)
                        if (!match.Enemies[j].Resolved && FlatDistance(predicted[j], impact) <= tower.Definition.splashRadius)
                            score++;
                }
                if (best == null || score > bestScore || (score == bestScore &&
                    (enemy.Distance > best.Distance || (enemy.Distance == best.Distance && enemy.Id < best.Id))))
                { best = enemy; bestScore = score; }
            }
            return best;
        }
        static Vector3 PredictedPosition(MatchState match, EnemyState enemy, float seconds) =>
            match.Route.Position(enemy.Distance + enemy.Speed * seconds, enemy.LateralOffset+enemy.AvoidanceOffset);
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
                            Damage(match, match.Enemies[j], DamageFor(shot.Definition, match.Enemies[j].Definition),shot.Definition);
                }
                else Damage(match, shot.Target, DamageFor(shot.Definition, shot.Target.Definition),shot.Definition);
            }
        }
        static void Damage(MatchState match, EnemyState enemy, float amount,DefenseDefinition defense)
        {
            if (enemy.Resolved) return;
            enemy.Health -= amount;
            if(defense.slowDuration>0){enemy.SlowMultiplier=enemy.SlowRemaining>0?Mathf.Min(enemy.SlowMultiplier,defense.slowMultiplier):defense.slowMultiplier;enemy.SlowRemaining=Mathf.Max(enemy.SlowRemaining,defense.slowDuration);}
            match.NotifyEnemyDamaged(enemy);
            if (enemy.Health <= 0) match.ResolveKill(enemy);
        }
    }
}
