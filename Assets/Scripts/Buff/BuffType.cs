namespace Buff
{
    /// <summary>
    /// バフ/デバフの種類を定義する列挙型
    /// </summary>
    public enum BuffType
    {
        // Behaviour
        JumpTime,
        MoveSpeed,
        JumpForce,
        JumpSpeed,
        CorePower,
        Strength,
        Control,
        Thickness,
        Endurance,
        AdditionalGravity,

        // Attribute
        Frame,
        Aqua,
        Plant,
        Electric,
        Ground,
        Ice,
        Oil,
        Toxin,
        Wind,
        Spirit,

        // Battle
        Shield,
        Health,
        Stamina,
        GuardHealth,
        Rate,
        ShieldAttackRate,
        AttackPower,
        DefensePower,
        BaseAttributePower,
        BaseResistancePower,
        MinRange,
        MaxRange,
        DiffusionRate,
        CriticalDamageRate,
        CriticalChance,
        GuardNum,

        // Meta
        Luck,

        // Other
        AutoGuardNum,
        CooldownAccelerate,
        HealHitPoint,
        HealStamina,
        HealGuardHealth,
    }
}