
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class RangedWeapon : BaseWeapon, IPick
{
    public SpriteRenderer characterSR;
    public Transform firePos;
    public GameObject bulletPrefab;
    public float fireRate = 0.5f;
    public float bulletForce = 20f;
    public int dam = 1;
   // public RangedWeaponService rangedWeapon;
    //private bool isFiring=false;
    public bool inHand;
    private float lastFireTime = -10f;
    public RangedWeaponSO RangedWeaponSO;
    public WeaponDetailSO weaponDetailSO;
    public RangedWeaponFireType type;
    private int burstShotCount = 0;
    private float lastBurstShotTime;
    private bool isBursting = false;
    private int bulletsRemaining;
    private float nextBulletTime;
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

    void Start()
    {
        fireRate = RangedWeaponSO.TimeBtwFire;
        bulletForce = RangedWeaponSO.bulletForce;
        dam = RangedWeaponSO.damageRangedWeapon;
        type= RangedWeaponSO.type;
        if (!GetComponentInParent<Transform>().root.Find("Character").IsUnityNull())
        {
            characterSR = transform.root.GetComponentInChildren<SpriteRenderer>();
            inHand = true;
            
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
        }
        else
        {

            inHand = false;
           // rangedWeapon.SetInHand(inHand);
            DropWeapon();
        }
    }


    //public override void Attack()
    //{
    //    float angle = RotateToMousePos();
    //    float elapsedTime = Time.time - lastFireTime;

    //    if (elapsedTime >= fireRate)
    //    {
    //        lastFireTime = Time.time;
    //        GameObject bulletTmp = PoolManagement.Instance.GetBullet(bulletPrefab);

    //        if (bulletTmp == null) return;


    //        WeaponSoundEffect();


    //        bulletTmp.transform.position = firePos.position;
    //        bulletTmp.transform.rotation = Quaternion.Euler(0, 0, angle - 90);
    //        Rigidbody2D rb = bulletTmp.GetComponent<Rigidbody2D>();

    //        rb.linearVelocity = Vector2.zero;
    //        rb.AddForce(transform.right * bulletForce, ForceMode2D.Impulse);


    //    }
    //}

    public override void Attack()
    {
        float currentTime = Time.time;
        float angle = RotateToMousePos();

        switch (type)
        {
            case RangedWeaponFireType.SingleShot:
                if (currentTime - lastFireTime >= RangedWeaponSO.TimeBtwFire)
                {
                    FireBullet(angle);
                    lastFireTime = currentTime;
                }
                break;

            case RangedWeaponFireType.BurstShot:
                Debug.Log("burst shot");
               
                if (!isBursting && currentTime - lastFireTime >= RangedWeaponSO.TimeBtwFire)
                {
                    isBursting = true;
                    bulletsRemaining = Mathf.RoundToInt(RangedWeaponSO.bulletSequentialShot);
                    lastFireTime = currentTime;
                    nextBulletTime = currentTime;  
                }

               
                if (isBursting && currentTime >= nextBulletTime)
                {
                    FireBullet(angle);
                    bulletsRemaining--;

                    if (bulletsRemaining > 0)
                        nextBulletTime = currentTime + RangedWeaponSO.timeBtwEachBullet;
                    else
                        isBursting = false;  
                }
                break;

            case RangedWeaponFireType.MultiShot:
                if (currentTime - lastFireTime >= RangedWeaponSO.TimeBtwFire)
                {
                    int bulletCount = Mathf.RoundToInt(RangedWeaponSO.bulletMultiShot);
                    float totalSpread = RangedWeaponSO.spreadAngle * (bulletCount - 1);
                    float startAngle = angle - totalSpread / 2f;

                    for (int i = 0; i < bulletCount; i++)
                    {
                        float bulletAngle = startAngle + i * RangedWeaponSO.spreadAngle;
                        FireBullet(bulletAngle); 
                    }

                    lastFireTime = currentTime;
                }
                break;
        }
    }

    private void FireBullet(float angle)
    {
        GameObject bulletTmp = PoolManagement.Instance.GetBullet(bulletPrefab);
        if (bulletTmp == null) return;

        WeaponSoundEffect();

        bulletTmp.transform.position = firePos.position;
        bulletTmp.transform.rotation = Quaternion.Euler(0, 0, angle - 90);
        bulletTmp.GetComponent<Bullet>().weapon = this;
        Rigidbody2D rb = bulletTmp.GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(Quaternion.Euler(0, 0, angle) * Vector2.right * RangedWeaponSO.bulletForce, ForceMode2D.Impulse);
    }

    private IEnumerator BurstFireRoutine(float angle)
    {
        isBursting = true;

        try
        {
            int bulletCount = Mathf.RoundToInt(RangedWeaponSO.bulletSequentialShot);
            for (int i = 0; i < bulletCount; i++)
            {
                FireBullet(angle);
                yield return new WaitForSeconds(RangedWeaponSO.timeBtwEachBullet);
            }
        }
        finally
        {
            
            isBursting = false;
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
        transform.localPosition = new Vector3(0, 0.03f, 0);
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
