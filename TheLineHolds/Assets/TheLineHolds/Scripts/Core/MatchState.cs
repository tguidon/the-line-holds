using System;
using System.Collections.Generic;
using UnityEngine;
namespace TheLineHolds
{
    public enum MatchPhase { Preparation, Combat, Victory, Defeat }
    public sealed class EnemyState
    {
        public int Id; public EnemyDefinition Definition; public float Health, Distance, LateralOffset;
        public float AvoidanceOffset;
        public float MaxHealth, SpeedMultiplier=1, SlowMultiplier=1, SlowRemaining;
        public float MaximumHealth => MaxHealth>0?MaxHealth:Definition.health;
        public float Speed => Definition.speed*SpeedMultiplier*(SlowRemaining>0?SlowMultiplier:1); public bool Resolved;
        public Vector3 Position;
    }
    public sealed class TowerState
    {
        public int Pad; public DefenseDefinition Definition; public float Cooldown; public int Invested;
        public int SellValue => Invested/2;
    }
    public sealed class ShotState
    {
        public int Id, TowerPad; public DefenseDefinition Definition; public EnemyState Target;
        public Vector3 Origin, Destination; public float Age;
    }
    // Coordinates the small simulation systems and owns match outcomes. Views never award rewards or damage.
    public sealed class MatchState
    {
        public const int StartingFunds = 300, StartingLives = 20;
        public static int WaveReward(int wave) => 60 + 10 * Mathf.Clamp(wave,1,10);
        public int TotalWaves => waves.Length;
        public int Seed {get;}
        public WaveDefinition NextWave => waves[Mathf.Min(Wave,waves.Length-1)];
        public int Funds => economy.Funds;
        public int Lives { get; private set; } = StartingLives;
        public int Wave { get; private set; }
        public int CompletedWaves { get; private set; }
        public int Killed { get; private set; }
        public int Escaped { get; private set; }
        public MatchPhase Phase { get; private set; } = MatchPhase.Preparation;
        public bool Paused { get; set; }
        public int Stars => Phase == MatchPhase.Victory ? Rating(Lives) : 0;
        public static int Rating(int lives) => lives >= 18 ? 3 : lives >= 10 ? 2 : lives > 0 ? 1 : 0;
        public bool Ended => Phase == MatchPhase.Victory || Phase == MatchPhase.Defeat;
        public int Remaining => Phase == MatchPhase.Combat ? spawner.Unspawned + Enemies.Count : 0;
        public readonly List<EnemyState> Enemies = new List<EnemyState>();
        public readonly List<ShotState> Shots = new List<ShotState>();
        public readonly TowerState[] Towers;
        public readonly BattleRoute Route;
        public readonly Vector3[] Pads;
        readonly WaveDefinition[] waves;
        readonly Economy economy = new Economy(StartingFunds);
        readonly WaveSpawner spawner = new WaveSpawner();
        readonly CombatSystem combat = new CombatSystem();
        public event Action<string, Vector3> Cue;
        public event Action<EnemyState> EnemyDamaged, EnemyKilled;
        public event Action<ShotState> ShotFired;
        public event Action<ShotState> ShotImpacted;
        public event Action<string> Journal;
        public MatchState(WaveDefinition[] waves, BattleRoute route, Vector3[] pads,int seed=1944)
        {
            if (waves == null || waves.Length == 0) throw new ArgumentException("Waves required.");
            Seed=seed; this.waves = waves; Route = route; Pads = (Vector3[])pads.Clone(); Towers = new TowerState[pads.Length];
        }
        public bool Build(int pad, DefenseDefinition defense)
        {
            if (Ended || Paused || defense == null || pad < 0 || pad >= Towers.Length || Towers[pad] != null) return false;
            if (!economy.TrySpend(defense.cost)) return false;
            Towers[pad] = new TowerState { Pad = pad, Definition = defense, Invested=defense.cost };
            Emit("place", Pads[pad]);
            return true;
        }
        public bool Sell(int pad)
        {
            if (Ended || Paused || pad < 0 || pad >= Towers.Length || Towers[pad] == null) return false;
            economy.Credit(Towers[pad].SellValue);
            Towers[pad] = null;
            Emit("place", Pads[pad]);
            return true;
        }
        public bool Upgrade(int pad,int option)
        {
            if(Ended || Paused || pad<0 || pad>=Towers.Length || Towers[pad]==null)return false;
            var tower=Towers[pad];var options=tower.Definition.upgrades;
            if(option<0 || option>=options.Length || options[option].defense==null || options[option].price<=0)return false;
            var upgrade=options[option];if(!economy.TrySpend(upgrade.price))return false;
            tower.Invested+=upgrade.price;tower.Definition=upgrade.defense;
            // Retain elapsed cooldown: upgrading cannot grant a free extra shot.
            tower.Cooldown=Mathf.Min(tower.Cooldown,tower.Definition.interval);
            Emit("place",Pads[pad]);return true;
        }
        public bool StartWave()
        {
            if (Paused || Phase != MatchPhase.Preparation) return false;
            Wave++;
            spawner.Begin(waves[Wave - 1],Seed+Wave*7919);
            Phase = MatchPhase.Combat;
            Journal?.Invoke($"Wave {Wave} started; funds={Funds}, lives={Lives}");
            return true;
        }
        public EnemyState Target(TowerState tower) => CombatSystem.Target(this, tower);
        public void Step(float dt)
        {
            if (Paused || Ended || Phase != MatchPhase.Combat || dt <= 0) return;
            spawner.Step(this, dt);
            if (!RouteMovement.Step(this, dt)) return;
            combat.Step(this, dt);
            if (!spawner.Complete || Enemies.Count != 0) return;
            Shots.Clear();
            CompletedWaves++;
            economy.Credit(WaveReward(Wave));
            Journal?.Invoke($"Wave {Wave} complete; lives={Lives}, kills={Killed}, escapes={Escaped}, funds={Funds}");
            if (Wave == waves.Length) Finish(MatchPhase.Victory);
            else { Phase = MatchPhase.Preparation; Emit("wave", Vector3.zero); }
        }
        internal void ResolveKill(EnemyState enemy)
        {
            if (enemy.Resolved) return;
            enemy.Resolved = true;
            Enemies.Remove(enemy);
            Killed++;
            economy.Credit(enemy.Definition.reward);
            EnemyKilled?.Invoke(enemy);
        }
        internal void ResolveEscape(EnemyState enemy)
        {
            if (enemy.Resolved) return;
            enemy.Resolved = true;
            Enemies.Remove(enemy);
            Escaped++;
            Lives--;
            Emit("escape", enemy.Position);
            if (Lives <= 0) Finish(MatchPhase.Defeat);
        }
        internal void Emit(string cue, Vector3 position) => Cue?.Invoke(cue, position);
        internal void NotifyEnemyDamaged(EnemyState enemy) => EnemyDamaged?.Invoke(enemy);
        internal void NotifyShotFired(ShotState shot) => ShotFired?.Invoke(shot);
        internal void NotifyShotImpacted(ShotState shot) => ShotImpacted?.Invoke(shot);
        void Finish(MatchPhase phase)
        {
            Phase = phase;
            Shots.Clear();
            Journal?.Invoke($"{phase}; stars={Stars}, lives={Lives}, waves={CompletedWaves}, kills={Killed}, escapes={Escaped}");
            Emit(phase == MatchPhase.Victory ? "victory" : "defeat", Vector3.zero);
        }
    }
}
