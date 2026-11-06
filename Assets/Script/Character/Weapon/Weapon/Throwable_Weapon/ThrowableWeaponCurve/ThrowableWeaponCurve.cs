
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.XR;

public class ThrowableWeaponCurve : BaseWeapon, IPick
{
    //[SerializeField] public GameObject projectilePrefab;

    //[SerializeField] private float shootRate;
    //[SerializeField] private float projectileMaxMoveSpeed;
    //[SerializeField] private float projectileMaxHeight;

    //[SerializeField] private AnimationCurve trajectoryAnimationCurve;
    //[SerializeField] private AnimationCurve axisCorrectionAnimationCurve;
    //[SerializeField] private AnimationCurve projectileSpeedAnimationCurve;

    //private float shootTimer;
    //public SpriteRenderer currentCharacterSR;
    //private SpriteRenderer weaponSpriteRenderer;
    //private void Start()
    //{
    //    weaponSpriteRenderer=GetComponent<SpriteRenderer>();
    //    Transform player = transform.parent.parent;
    //    currentCharacterSR = player.GetComponentInChildren<SpriteRenderer>();
    //  //  inHand = true;
    //    projectilePrefab.GetComponent<ThrowableWeapon_Curve>().curveManager=GetComponent<ThrowableWeapon_CurveManager>();
    //}

    //private void Update()
    //{
    //    shootTimer -= Time.deltaTime;

    //    RotateGun();
    //    if (Input.GetMouseButtonDown(0) && shootTimer <= 0)
    //    {
    //        TurnOffSpriteRenderer();
    //        shootTimer = shootRate;
    //        //ThrowableWeapon_Curve projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity).GetComponent<ThrowableWeapon_Curve>();


    //        //Vector3 mousePos = Input.mousePosition;

    //        //mousePos.z = Mathf.Abs(Camera.main.transform.position.z);

    //        //Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
    //        GameObject bullet = PoolManagement.Instance.GetBullet(projectilePrefab);

    //        bullet.transform.position = transform.position;
    //        bullet.transform.rotation = Quaternion.identity;

    //        ThrowableWeapon_Curve projectile = bullet.GetComponent<ThrowableWeapon_Curve>();


    //        Vector3 mousePos = Input.mousePosition;
    //        mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
    //        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);

    //        projectile.InitializeProjectile(worldMousePos, projectileMaxMoveSpeed, projectileMaxHeight);
    //        projectile.InitializeAnimationCurves(trajectoryAnimationCurve, axisCorrectionAnimationCurve, projectileSpeedAnimationCurve);
    //    }
    //}

    //void RotateGun()
    //{

    //    float angle = RotateToMousePos();
    //    transform.rotation = Quaternion.Euler(0, 0, angle);

    //    bool isFlipped = angle > 90 || angle < -90;

    //    currentCharacterSR.gameObject.transform.localScale = new Vector3(isFlipped ? -1 : 1,
    //        currentCharacterSR.gameObject.transform.localScale.y, 1);

    //    transform.localScale = new Vector3(0.6f, isFlipped ? -0.6f : 0.6f, 1);
    //}

    //private float RotateToMousePos()
    //{
    //    Vector3 mousePos = Input.mousePosition;
    //    mousePos.z = Camera.main.nearClipPlane;

    //    Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
    //    Vector2 lookDir = (Vector2)(worldMousePos - transform.position);
    //    return Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
    //}

    //public void TurnOffSpriteRenderer()
    //{
    //    weaponSpriteRenderer.enabled=false;
    //}

    //public void TurnOnSpriteRenderer()
    //{
    //    weaponSpriteRenderer.enabled = true;
    //}

    [SerializeField] public GameObject projectilePrefab;

    [SerializeField] private float projectileMaxMoveSpeed;
    [SerializeField] private float projectileMaxHeight;

    [SerializeField] private AnimationCurve trajectoryCurve;
    [SerializeField] private AnimationCurve axisCorrectionCurve;
    [SerializeField] private AnimationCurve speedCurve;

    public float throwCooldown;
    public SpriteRenderer characterSR;
    private SpriteRenderer weaponSpriteRenderer;
    public WeaponDetailSO weaponDetailSO;
    public ThrowableWeaponCurveSO weaponCurveSO;
    //public ThrowableWeaponCurveService weaponCurveService;

    private float lastThrowTime = -10f;
    public int damage;
    public bool inHand;
    void Start()
    {
        projectileMaxMoveSpeed = weaponCurveSO.throwableForce;
        damage = weaponCurveSO.damageThrowableWeapon;
        throwCooldown = weaponCurveSO.TimeBtwFire;
        weaponSpriteRenderer = GetComponent<SpriteRenderer>();
        projectilePrefab.GetComponent<ThrowableWeaponCurveProjectile>().curveManager = GetComponent<ThrowableWeaponCurve>();


        if (!GetComponentInParent<Transform>().root.Find("Character").IsUnityNull())
        {
            characterSR = transform.root.GetComponentInChildren<SpriteRenderer>();
            inHand = true;

        }
        else
        {

            inHand = false;

            DropWeapon();
        }
    }

    public void TurnOffSpriteRenderer()
    {
        weaponSpriteRenderer.enabled = false;
    }

    public void TurnOnSpriteRenderer()
    {
        weaponSpriteRenderer.enabled = true;
    }

    public GameObject GetPickGameOject()
    {
        return gameObject;
    }

    public void Pick()
    {
        characterSR = transform.root.GetComponentInChildren<SpriteRenderer>();
        inHand = true;
        PickWeapon();
    }

    public void Drop()
    {
        characterSR = null;
        inHand = false;
        DropWeapon();
    }

    public bool IsPickingItemOrWeapon()
    {
        return true;
    }

    public override void Attack()
    {
        if (Time.time >= lastThrowTime + throwCooldown)
        {
            lastThrowTime = Time.time;

            //Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
            GameObject bullet = PoolManagement.Instance.GetBullet(projectilePrefab);

            bullet.transform.position = transform.position;
            bullet.transform.rotation = Quaternion.identity;

            ThrowableWeaponCurveProjectile projectile = bullet.GetComponent<ThrowableWeaponCurveProjectile>();


            Vector3 mousePos = InputManager.PointerPosition;
            mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);

            projectile.InitializeProjectile(worldMousePos, projectileMaxMoveSpeed, projectileMaxHeight);
            projectile.InitializeAnimationCurves(trajectoryCurve, axisCorrectionCurve, speedCurve);
            WeaponSoundEffect();
        }
    }

    public override void RotateWeapon()
    {
        if (!inHand) return;
        float angle = RotateToMousePos();
        transform.rotation = Quaternion.Euler(0, 0, angle);

        bool isFlipped = angle > 90 || angle < -90;

        characterSR.gameObject.transform.localScale = new Vector3(isFlipped ? -1 : 1,
                characterSR.gameObject.transform.localScale.y, 1);

        transform.localScale = new Vector3(0.6f, isFlipped ? -0.6f : 0.6f, 1);
    }

    public override void PickWeapon()
    {
        transform.gameObject.GetComponent<BoxCollider2D>().enabled = false;
        characterSR = transform.root.GetComponentInChildren<SpriteRenderer>();
        transform.localScale = new Vector3(0.6f, 0.6f, 0);
        transform.localPosition = new Vector3(0, 0.04f, 0);
        inHand = true;
    }

    public override void DropWeapon()
    {
        characterSR = null;
        transform.rotation = Quaternion.identity;
        transform.localScale = new Vector3(5, 5, 0);
        transform.gameObject.GetComponent<BoxCollider2D>().enabled = true;
        inHand = false;
    }

    public override float RotateToMousePos()
    {
        Vector3 mousePos = InputManager.PointerPosition;
        mousePos.z = Camera.main.nearClipPlane;

        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
        Vector2 lookDir = (Vector2)(worldMousePos - transform.position);
        return Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
    }

    public override WeaponDetailSO GetWeaponDetailSO()
    {
        return weaponDetailSO;
    }
    private void WeaponSoundEffect()
    {
        if (this.GetWeaponDetailSO().weaponFiringSoundEffect != null)
        {
            SoundEffectManager.Instance.PlaySoundEffect(this.GetWeaponDetailSO().weaponFiringSoundEffect);
        }
        else Debug.Log("Sth wrong with weaponSoundSO");
    }
}
