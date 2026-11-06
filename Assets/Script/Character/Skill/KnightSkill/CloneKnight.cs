using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Cinemachine.CinemachineTriggerAction.ActionSettings;

public class CloneKnight : MonoBehaviour
{
    public EnemyDetailSO CloneKnightDetails;
    public KnightSkill knightSkill;

    [HideInInspector] public MovementToPositionEvent movementToPositionEvent;
    [HideInInspector] public IdleEvent idleEvent;
    [HideInInspector] public Animator animator;

    [HideInInspector] public Clone_Knight_Movement_AI ai;
    private float timetoDes;
    private CloneKnightWeaponController playerWeaponController;
    private void Awake()
    {
        playerWeaponController = gameObject.GetComponentInChildren<CloneKnightWeaponController>();
        animator = GetComponent<Animator>();
        movementToPositionEvent = GetComponent<MovementToPositionEvent>();
        idleEvent = GetComponent<IdleEvent>();
        ai = GetComponent<Clone_Knight_Movement_AI>();
    }

    private void OnEnable()
    {
       
        timetoDes = knightSkill.cloneCoolDown;
        Invoke(nameof(ResetClone), timetoDes);
    }

    private void ResetClone()
    {
        PoolManagement.Instance.ReturnBullet(gameObject, knightSkill.clonePrefab);
    }

    private void OnDisable()
    {
       
        CancelInvoke(nameof(ResetClone));
    }
    private void Update()
    {
        if (InputManager.GameplayBlocked) return;

        playerWeaponController.RotateWeapon();
        if (InputManager.Instance.IsAttackHeld)
        {
            playerWeaponController.Attack();
        }
    }
    public void SetWeaponForClone(GameObject obj)
    {
        List<GameObject> gameObjects = new List<GameObject>();

       
        GameObject weaponController = gameObject.transform.Find("Weapon").gameObject;

        if (weaponController != null) 
        {
            Transform trans = weaponController.transform; 

            if (trans.childCount > 0)
            {

                foreach (Transform child in trans)
                {
                    Destroy(child.gameObject);
                }
                GameObject weapon = Instantiate(obj);
                weapon.transform.SetParent(weaponController.transform, false);
                weapon.transform.localPosition = new Vector3(0, obj.transform.localPosition.y, 0);
                playerWeaponController.baseWeapon = weapon.GetComponent<BaseWeapon>();
            }
            else
            {
                GameObject weapon = Instantiate(obj);
                weapon.transform.SetParent(weaponController.transform, false);
                weapon.transform.localPosition = new Vector3(0, obj.transform.localPosition.y, 0);
                playerWeaponController.baseWeapon=weapon.GetComponent<BaseWeapon>();
                Debug.LogWarning("PlayerWeaponController ko co con");
            }
        }
       
    }
}
