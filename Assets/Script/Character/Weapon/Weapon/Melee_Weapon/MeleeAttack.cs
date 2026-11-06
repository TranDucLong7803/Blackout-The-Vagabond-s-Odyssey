
using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    public int Damege;

    private void Start()
    {
        Damege=GetComponentInParent<MeleeWeapon>().damage;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<IDamageable>(out var prob))
        {
            AttackContext.DealPlayerDamage(prob, Damege, AttackType.Weapon);
            Debug.Log("Dealt: " + Damege);
        }
    }
}
