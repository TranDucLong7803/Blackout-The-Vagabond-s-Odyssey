using Assets.Script.Weapon.Throwable_Weapon;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ThrowableWeaponHitBox : MonoBehaviour
{
    public ThrowableWeaponStraight weapon;

    private void Awake()
    {
        //weapon = GameObject.Find("Weapon").GetComponentInChildren<ThrowableWeaponStraight>();

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // if (enemy != null)
        if (collision.gameObject.TryGetComponent<IDamageable>(out var enemy))
        {
            AttackContext.DealPlayerDamage(enemy, weapon.dame, AttackType.Weapon);

            PoolManagement.Instance.ReturnBullet(gameObject, weapon.throwablePrefab);
        }

        else PoolManagement.Instance.ReturnBullet(gameObject, weapon.throwablePrefab);
        ////vuong: any collision obstacle, disable bullet
        //// this.gameObject.SetActive(false);

    }


}
