using UnityEngine;

public class UltimateHitbox : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<IDamageable>(out var enemy))
        {
            AttackContext.DealPlayerDamage(enemy, GetComponentInParent<AssassinSkill>().DameUltimateSkill, AttackType.UltimateSkill);
        }
    }
}
