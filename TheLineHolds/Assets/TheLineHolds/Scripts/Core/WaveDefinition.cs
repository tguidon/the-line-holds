using System;
using UnityEngine;
namespace TheLineHolds
{
    [Serializable] public struct SpawnEntry
    {
        public EnemyDefinition enemy;
        [Min(0)] public float delay;
        public SpawnEntry(EnemyDefinition enemy, float delay) { this.enemy = enemy; this.delay = delay; }
    }
    [CreateAssetMenu(menuName = "The Line Holds/Wave")]
    public sealed class WaveDefinition : ScriptableObject
    {
        public string briefing;
        public SpawnEntry[] spawns;
    }
}
