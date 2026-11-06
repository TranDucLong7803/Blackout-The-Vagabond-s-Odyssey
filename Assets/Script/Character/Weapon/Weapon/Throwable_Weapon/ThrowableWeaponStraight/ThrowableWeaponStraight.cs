
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Script.Weapon.Throwable_Weapon
{
    public class ThrowableWeaponStraight : BaseWeapon, IPick
    {
        public SpriteRenderer characterSR;
        public Transform firePos;
        public GameObject throwablePrefab;
        public float throwCooldown = 0.5f;
        public float throwForce = 20f;
        public int dame = 1;
        public Animator animation;
        private float lastThrowTime = -10f;

        public ThrowableWeaponStraightSO throwableWeaponSO;
        public WeaponDetailSO weaponDetailSO;
        public bool inHand;

        public void Drop()
        {
            characterSR = null;
            inHand = false;
            DropWeapon();
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

        public bool IsPickingItemOrWeapon()
        {
            return true;
        }

        void Start()
        {
            throwCooldown = throwableWeaponSO.TimeBtwFire;
            throwForce = throwableWeaponSO.throwableForce;
            dame = throwableWeaponSO.damageThrowableWeapon;
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

        public void AttackEvent()
        {
            float angle = RotateToMousePos();
            GameObject bulletTmp = PoolManagement.Instance.GetBullet(throwablePrefab);

            if (bulletTmp == null) return;
            bulletTmp.GetComponent<ThrowableWeaponHitBox>().weapon = this;
            bulletTmp.transform.position = firePos.position;
            bulletTmp.transform.rotation = Quaternion.Euler(0, 0, angle);
            Rigidbody2D rb = bulletTmp.GetComponent<Rigidbody2D>();

            rb.linearVelocity = Vector2.zero;
            rb.AddForce(transform.right * throwForce, ForceMode2D.Impulse);

            WeaponSoundEffect();

        }

        public override void Attack()
        {
            float elapsedTime = Time.time - lastThrowTime;

            if (elapsedTime >= throwCooldown)
            {
                lastThrowTime = Time.time;
                animation.SetTrigger("isAttack");
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


}
