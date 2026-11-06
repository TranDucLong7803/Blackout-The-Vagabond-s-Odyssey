using System.Collections;
using UnityEngine;

public class FireBall : MonoBehaviour
{
    public WizardSkill wizardSkill;

    //private void OnEnable()
    //{
    //    lifeTimer = StartCoroutine(DeactivateAfterTime());
    //}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<IDamageable>(out var enemy))
        {
            AttackContext.DealPlayerDamage(enemy, wizardSkill.normalSkillDame, AttackType.NormalSkill);
           
        }
        else PoolManagement.Instance.ReturnBullet(gameObject, wizardSkill.normalBulletPrefab);
    }

    //private IEnumerator DeactivateAfterTime()
    //{
    //    yield return new WaitForSeconds(lifeTime);
    //    ReturnToPool();
    //}

    //private void ReturnToPool()
    //{
    //    if (lifeTimer != null) StopCoroutine(lifeTimer);
    //    PoolManagement.Instance.ReturnBullet(gameObject, wizardSkill.normalBulletPrefab);
    //}
}
