using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private BulletImpact impactPrefab;
    public RangedWeapon weapon;

    private Collider2D hitCollider;
    private bool hasHit;

    private void Awake()
    {
        hitCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        hasHit = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit || weapon == null) return;
        hasHit = true;

        var pool = PoolManagement.Instance;
        var bulletPrefab = weapon.bulletPrefab;
        bool damageable = collision.TryGetComponent<IDamageable>(out var target);

        if (impactPrefab != null)
        {
            var contact = hitCollider.Distance(collision);
            Vector2 point = contact.isValid ? contact.pointB : collision.ClosestPoint(transform.position);
            pool.PlayBulletImpact(impactPrefab, new Vector3(point.x, point.y, transform.position.z), damageable);
        }

        if (damageable)
        {
            AttackContext.CurrentAttackType = AttackType.Weapon;
            target.takeDame(weapon.dam);
        }

        pool.ReturnBullet(gameObject, bulletPrefab);
    }
}
