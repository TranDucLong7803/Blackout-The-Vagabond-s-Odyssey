using UnityEngine;

public enum AttackType
{
    Weapon,
    NormalSkill,
    UltimateSkill
}

public static class AttackContext
{
    public static AttackType CurrentAttackType { get; private set; } = AttackType.Weapon;

    public static void DealPlayerDamage(IDamageable target, int damage, AttackType attackType)
    {
        CurrentAttackType = attackType;
        target.takeDame(damage);
    }
}
