using UnityEngine;
namespace TheLineHolds
{
    [CreateAssetMenu(menuName = "The Line Holds/Enemy")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        public string displayName;
        [Min(1)] public float health = 40;
        [Min(.01f)] public float speed = 1.15f;
        [Min(0)] public int reward = 10;
        public GameObject prefab;
    }
}
