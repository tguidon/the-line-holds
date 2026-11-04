using System;
using UnityEngine;
namespace TheLineHolds
{
    [Serializable] public struct SpawnEntry
    {
        public EnemyDefinition enemy;
        [Min(0)] public float delay;
        [Min(1)] public int count;
        [Min(.01f)] public float interval;
        public int UnitCount => Mathf.Max(1, count);
        public SpawnEntry(EnemyDefinition enemy, float delay, int count = 1, float interval = .2f)
        { this.enemy = enemy; this.delay = delay; this.count = count; this.interval = interval; }
    }
    [CreateAssetMenu(menuName = "The Line Holds/Wave")]
    public sealed class WaveDefinition : ScriptableObject
    {
        public string briefing;
        // Each entry is a squad or vehicle section. Members retain individual health and rewards.
        public SpawnEntry[] spawns;
        public int UnitCount
        {
            get { int total = 0; foreach (var entry in spawns) total += entry.UnitCount; return total; }
        }
    }
}
