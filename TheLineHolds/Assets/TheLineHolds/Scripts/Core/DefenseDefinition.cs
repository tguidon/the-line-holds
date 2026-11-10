using UnityEngine;
namespace TheLineHolds
{
    public enum DefenseRole { MachineGun, Cannon, Mortar }
    [System.Serializable] public sealed class UpgradeOption { public DefenseDefinition defense; public int price; }
    public enum TargetPriority { First, Armor, Cluster }
    [CreateAssetMenu(menuName = "The Line Holds/Defense")]
    public sealed class DefenseDefinition : ScriptableObject
    {
        public string displayName;
        public DefenseRole role;
        public int tier=1;
        public UpgradeOption[] upgrades=System.Array.Empty<UpgradeOption>();
        [Range(.2f,1)] public float slowMultiplier=1;
        public float slowDuration;
        [TextArea] public string description;
        [Min(1)] public int cost = 75;
        [Min(.1f)] public float range = 3.6f;
        [Min(.01f)] public float interval = .22f;
        [Min(.1f)] public float damage = 6;
        [Min(0)] public float splashRadius;
        [Min(.01f)] public float flightTime = .12f;
        [Min(0)] public float infantryDamageMultiplier = 1;
        [Min(0)] public float armorDamageMultiplier = 1;
        public TargetPriority priority;
        public GameObject prefab;
        public int SellValue => cost / 2;
    }
}
