namespace TheLineHolds
{
    public enum WeaponStyle { MachineGun, Cannon, Mortar }
    public static class WeaponStyles
    {
        public static WeaponStyle For(DefenseDefinition definition) => (WeaponStyle)definition.role;
    }
}
