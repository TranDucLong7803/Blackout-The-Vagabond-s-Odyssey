using Assets.Script.Boss.StoneGolem;
using System.Collections;
using UnityEngine;

public class RedZoneHitBox : MonoBehaviour
{
    private float damageInterval = 0.5f;
    private Coroutine damageCoroutine;
    public int damage = 0;
    private void Start()
    {
        damage=GameObject.FindWithTag("Player").GetComponentInChildren<ThrowableWeaponCurve>().damage;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<IDamageable>(out var enemy))
        {
            
            if (damageCoroutine == null)
            {
                damageCoroutine = StartCoroutine(DealDamage(enemy));
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<IDamageable>(out var enemy))
        {
           
            if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
                damageCoroutine = null;
            }
        }
    }

    private IEnumerator DealDamage(IDamageable enemy)
    {
        while (true)
        {
            AttackContext.DealPlayerDamage(enemy, damage, AttackType.Weapon);
            yield return new WaitForSeconds(damageInterval);
        }
    }
}
