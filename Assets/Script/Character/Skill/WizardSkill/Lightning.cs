using UnityEngine;

public class Lightning : MonoBehaviour
{
   public WizardSkill wizardSkill;
    private void Start()
    {
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<IDamageable>(out var enemy))
        {
            if (collision.gameObject == target.gameObject)
            {
                AttackContext.DealPlayerDamage(enemy, 50, AttackType.UltimateSkill);
               // PoolManagement.Instance.
            }
           
           
           // skill.EnemyInRange.Remove(target.gameObject);
        }
    }

    public Transform target;
    protected float speed = 10f;

    protected virtual void FixedUpdate()
    {
        this.Following();
    }

    protected virtual void Following()
    {
        if (this.target == null) return;
        transform.position = new Vector3(target.position.x, target.position.y, target.position.z);
    }

    public virtual void SetTarget(Transform target)
    {
        this.target = target;
    }

    public void DestroyGameObject()
    {
        PoolManagement.Instance.ReturnBullet(gameObject, wizardSkill.ultimateBulletPrefab);
    }
}
