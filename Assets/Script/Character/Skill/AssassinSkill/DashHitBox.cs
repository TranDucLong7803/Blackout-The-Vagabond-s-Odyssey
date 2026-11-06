using UnityEngine;

public class DashHitBox : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<IDamageable>(out var enemy))
        {
            AttackContext.DealPlayerDamage(enemy, GetComponentInParent<AssassinSkill>().DameNormalSkill, AttackType.NormalSkill);
        }
    }
}
