namespace TheLineHolds
{
    public enum WeaponStyle { MachineGun, Cannon, Mortar }
    public static class WeaponStyles
    {
        public static WeaponStyle For(DefenseDefinition definition) => definition.splashRadius > 0
            ? WeaponStyle.Mortar : definition.priority == TargetPriority.Armor ? WeaponStyle.Cannon : WeaponStyle.MachineGun;
    }
}
