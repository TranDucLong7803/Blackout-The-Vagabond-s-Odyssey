using UnityEngine;

public class ShieldHitBox : MonoBehaviour
{
    public int shieldDame;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.gameObject.TryGetComponent<IDamageable>(out var enemy))
        {
            AttackContext.DealPlayerDamage(enemy, shieldDame, AttackType.NormalSkill);
            Debug.Log("Dealt: "+ shieldDame);
        }
    }


}
