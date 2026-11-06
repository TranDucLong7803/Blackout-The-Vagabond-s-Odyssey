using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WizardSkill : BaseSkill
{
    public List<GameObject> EnemyInRange = new List<GameObject>();
    public CircleCollider2D Collider;
    public Animator animator;

    public GameObject ultimateBulletPrefab;
    public float fireRate = 0.2f;
    public float ultimateCoolDown = 10f;
    public int ultimateDame;

    public GameObject normalBulletPrefab;
    public Transform firePos;
    public float bulletForce = 5f;
    private float lastFireTime = -10f;
    public float normalSkillCoolDown = 5f;
    public int normalSkillDame;

    public bool isUsingNormal = false;
    public bool isUsingUltimate = false;
    public bool canUseNormal = true;
    public bool canUseUltimate = true;
    public bool canUseTeleport = false;
    // public List<GameObject> EnemyInRange = new List<GameObject>();

    private Vector3 lastBulletPosition;

    public PlayerWeaponController weaponController;

    private void Start()
    {
        animator = GetComponent<Animator>();
        ultimateBulletPrefab.GetComponent<Lightning>().wizardSkill = this;
        normalBulletPrefab.GetComponent<FireBall>().wizardSkill = this;
        Collider = GetComponentInChildren<CircleCollider2D>();
        Collider.enabled = false;
        // EnemyInRange = GetComponentInChildren<WizardSkill>().EnemyInRange;

    }

    IEnumerator AutoFire()
    {
        while (true)
        {
            yield return new WaitForSeconds(fireRate);

            if (EnemyInRange.Count > 0)
            {
                FireAtEnemy();
            }
        }
    }

    private void FireAtEnemy()
    {
        if (EnemyInRange.Count == 0) return;


        GameObject targetEnemy = EnemyInRange[0];

        if (targetEnemy != null)
        {

            GameObject lightning = PoolManagement.Instance.GetBullet(ultimateBulletPrefab);
            lightning.GetComponent<Lightning>().transform.position = targetEnemy.transform.position;

            // Instantiate(bulletPrefab, targetEnemy.transform.position, Quaternion.identity);
            lightning.GetComponent<Lightning>().target = targetEnemy.transform;
            EnemyInRange.RemoveAt(0);
        }
    }
    IEnumerator SetFalse()
    {
        Collider.enabled = true;
        yield return new WaitForSeconds(0.5f);
        Collider.enabled = false;
    }

    public void ShootFireBall()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(InputManager.PointerPosition);
        mouseWorldPos.z = 0f;

        Vector3 direction = (mouseWorldPos - firePos.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        firePos.rotation = Quaternion.Euler(0, 0, angle);
        float elapsedTime = Time.time - lastFireTime;

        if (elapsedTime >= normalSkillCoolDown)
        {
            lastFireTime = Time.time;
            GameObject bulletTmp = PoolManagement.Instance.GetBullet(normalBulletPrefab);

            if (bulletTmp == null) return;

            bulletTmp.transform.position = firePos.position;
            bulletTmp.transform.rotation = Quaternion.Euler(0, 0, angle);
            Rigidbody2D rb = bulletTmp.GetComponent<Rigidbody2D>();

            rb.linearVelocity = direction * bulletForce;

            //cooldown lan 1
            SkillCooldownUI.Instance.StartWizardESkill();

            StartCoroutine(TrackBulletPosition(bulletTmp));
            canUseTeleport = true;
            canUseNormal = false;

        }

    }

    private IEnumerator TrackBulletPosition(GameObject bullet)
    {
        while (bullet.activeInHierarchy)
        {
            lastBulletPosition = bullet.transform.position;
            yield return null;
        }
        canUseTeleport = false;
        StartCoroutine(ResetNormalSkillCoolDown());

        // cooldown
        SkillCooldownUI.Instance.TriggerCooldown_E(normalSkillCoolDown);
        SkillCooldownUI.Instance.glowEffect_E.enabled = false;
    }

    public void Teleport()
    {
        transform.root.position = lastBulletPosition;
        canUseTeleport = false;
        SkillCooldownUI.Instance.ActivateWizardESkill(normalSkillCoolDown);
    }

    public override bool CanUseSkill1()
    {
        return canUseNormal || canUseTeleport;
    }

    public override bool CanUseSkill2()
    {
        return canUseUltimate;
    }

    public override bool IsUsingSkill()
    {
        return isUsingNormal || isUsingUltimate;
    }

    public override void NormalSkill()
    {
        animator.SetBool("isSkill1", false);
        GameManager.Instance.UpdateNormalSkillUsed();
        if (canUseNormal)
        {
            ShootFireBall();
            canUseTeleport = true;
            
        }
        else if (canUseTeleport)
        {
            Teleport();
        }
    }

    IEnumerator ResetNormalSkillCoolDown()
    {
        //canUseTeleport = true;
        canUseNormal = false;
        yield return new WaitForSeconds(normalSkillCoolDown);
        canUseNormal = true;
        //canUseTeleport = false;
    }

    IEnumerator ResetUltimateSkillCoolDown()
    {
        canUseUltimate = false;
        yield return new WaitForSeconds(ultimateCoolDown);
        canUseUltimate = true;
    }
    public override void UltimmateSkill()
    {
        animator.SetBool("isSkill2", false);
        GameManager.Instance.UpdateUltimateSkillUsed();
        StartCoroutine(AutoFire());
        StartCoroutine(SetFalse());
        StartCoroutine(ResetUltimateSkillCoolDown());

        //cooldown
        SkillCooldownUI.Instance.TriggerCooldown_Q(ultimateCoolDown);
    }
    public override void SetActiveWeapon()
    {
        weaponController.gameObject.SetActive(true);
    }
    public override void setActiveFalseWeapon()
    {
        weaponController.gameObject.SetActive(false);
    }


    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.gameObject.TryGetComponent<IDamageable>(out var enemy))
    //    {
    //        EnemyInRange.Add(collision.gameObject);
    //       // enemy.takeDame(DameNormalSkill);
    //        //Debug.Log("Dealth dame nomal:" + DameNormalSkill);



    //    }
    //}
    public override void SetIsUsingNormalFalse()
    {
        isUsingNormal = false;
    }
    public override void SetIsUsingUltimateFalse()
    {
        isUsingUltimate = false;
    }

    public override void SetIsUsingUltimate()
    {
        isUsingUltimate = true;
    }

    public override void SetIsUsingNormal()
    {
        isUsingNormal = true;
    }
}
