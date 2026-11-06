using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AssassinSkill : BaseSkill
{
    public float CoolDownNormalSkill;
    public float CoolDownUltimateSkill;
    public int DameNormalSkill = 5;
    public int DameUltimateSkill;

    public Animator animator;

    public float dashDistance = 5f;
    public float dashDuration = 0.1f;
  //  public float dashCooldownTime = 5f; //đoạn này thừa hay sao ý

   // public float ultimateCooldownTime; //đây nữa

    public LayerMask obstacleMask;

    public bool isUsingNormal = false;
    public bool isUsingUltimate = false;
    public bool canDash = true;
    public bool canUltimate = true;

    public Light2D light;
    public float lightdefault;
    public float buffed;


    public PlayerWeaponController weaponController;
    private void Start()
    {
        //weaponController = GetComponent<WeaponController>();
        // animator = GetComponentInChildren<Animator>();
    }

    public override void NormalSkill()
    {

        Vector3 mouseScreenPos = InputManager.PointerPosition;
        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        Vector3 dashDirection = (mouseWorldPos - transform.root.position).normalized;

        Rigidbody2D rb = transform.root.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(obstacleMask);  
        filter.useTriggers = false;

        RaycastHit2D[] hits = new RaycastHit2D[1];
        int hitCount = rb.Cast(dashDirection, filter, hits, dashDistance);

        Vector3 targetPos;

        if (hitCount > 0)
        {
            float safeDistance = Mathf.Max(hits[0].distance - 0.05f, 0f);
            targetPos = (Vector3)rb.position + dashDirection * safeDistance;
        }
        else
        {
            targetPos = (Vector3)rb.position + dashDirection * dashDistance;
        }



        StartCoroutine(Dash(targetPos));
        StartCoroutine(DashCooldown());
        GameManager.Instance.UpdateNormalSkillUsed();
        //bat dau cooldown
        SkillCooldownUI.Instance.TriggerCooldown_E(CoolDownNormalSkill);
    }

    public override void UltimmateSkill()
    {
        light.GetComponent<Light2D>().pointLightOuterRadius = buffed;
        isUsingUltimate = true;
        StartCoroutine(UltimateCoolDown());
        GameManager.Instance.UpdateUltimateSkillUsed();
        //bat dau cooldown
        SkillCooldownUI.Instance.TriggerCooldown_Q(CoolDownUltimateSkill);
    }

    public void SetLight()
    {
        light.GetComponent<Light2D>().pointLightOuterRadius = lightdefault;
    }

    IEnumerator Dash(Vector3 targetPos)
    {


        Vector3 startPos = transform.root.position;
        float elapsedTime = 0f;

        while (elapsedTime < dashDuration)
        {
            transform.root.position = Vector3.Lerp(startPos, targetPos, elapsedTime / dashDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.root.position = targetPos;
        //isDashing = false;
        //animator.SetBool("isSkill1", false);
    }

    IEnumerator DashCooldown()
    {
        canDash = false;
        yield return new WaitForSeconds(CoolDownNormalSkill);
        canDash = true;
    }



    IEnumerator UltimateCoolDown()
    {
        canUltimate = false;
        yield return new WaitForSeconds(CoolDownUltimateSkill);
        canUltimate = true;
    }

    public override void SetActiveWeapon()
    {
        weaponController.gameObject.SetActive(true);
    }
    public override void setActiveFalseWeapon()
    {
        weaponController.gameObject.SetActive(false);
    }

    public void SetAnimationUltimate()
    {
        animator.SetBool("isSkill2", false);
    }

    public void SetIsUltiamteTrue()
    {
        isUsingUltimate = true;
    }

    public void SetAnimationDash()
    {
        animator.SetBool("isSkill1", false);
    }

    public override bool CanUseSkill1()
    {
        return canDash;
    }

    public override bool CanUseSkill2()
    {
        return canUltimate;
    }

    public override bool IsUsingSkill()
    {
        return isUsingNormal || isUsingUltimate;
    }

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
