using Assets.Script.Boss.StoneGolem;
using System.Collections;
using UnityEngine;

public class RangedEnemyProjectile : MonoBehaviour
{
    public Transform target;
    public GameObject projectile;
    public Transform firePos;
    public float projectileForce;
    public int projectileDame;

    public RangedWeaponSO rangedDetail;

    public Enemy enemy;

    void Start()
    {
        enemy = GetComponent<Enemy>();
        target = GameObject.FindGameObjectWithTag("Player").transform;
        projectileForce = rangedDetail.bulletForce;
        projectileDame = rangedDetail.damageRangedWeapon;
    }

    void Fire()
    {
        if (!enemy.CanAct || target == null) return;
        GameObject bulletTmp = PoolManagement.Instance.GetBullet(projectile);
        if (bulletTmp == null) return;

        bulletTmp.transform.position = firePos.position;
        Vector2 direction = (Vector2)(target.position - firePos.position);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        bulletTmp.transform.rotation = Quaternion.Euler(0, 0, angle);

        Rigidbody2D rb = bulletTmp.GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;

        Vector2 shootDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        rb.AddForce(shootDirection * projectileForce, ForceMode2D.Impulse);
    }

}
