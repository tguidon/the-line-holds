using UnityEngine;
namespace TheLineHolds
{
    [CreateAssetMenu(menuName = "The Line Holds/Defense")]
    public sealed class DefenseDefinition : ScriptableObject
    {
        public string displayName;
        [TextArea] public string description;
        [Min(1)] public int cost = 75;
        [Min(.1f)] public float range = 3.6f;
        [Min(.01f)] public float interval = .22f;
        [Min(.1f)] public float damage = 6;
        [Min(0)] public float splashRadius;
        [Min(.01f)] public float flightTime = .12f;
        public GameObject prefab;
        public int SellValue => cost / 2;
    }
}
