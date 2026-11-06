using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyDetailSO enemyDetails;


    [HideInInspector] public MovementToPositionEvent movementToPositionEvent;
    [HideInInspector] public IdleEvent idleEvent;
    [HideInInspector] public Animator animator;
    [HideInInspector] public Room roomBelong;

    [HideInInspector] public Enemy_Movement_AI ai;

    public int damage;
    public int health;
    public bool CanAct => health > 0 && (roomBelong == null || roomBelong.IsCombatActive);

    public bool isHurt;
    public bool has3Attack;
    public bool isArcher = false;
    private bool isAttacking = false;

    public float attackSpeed = 1f;

    private void Awake()
    {
        movementToPositionEvent = GetComponent<MovementToPositionEvent>();
        idleEvent = GetComponent<IdleEvent>();
        ai = GetComponent<Enemy_Movement_AI>();
    }
    void Start()
    {
        // isAttack1 = false;
        animator = GetComponent<Animator>();
        isHurt = false;
        if (GameManager.Instance != null)
        {
            switch (GameManager.Instance.currentStage)
            {
                case 1:
                    health *= 1;
                    break;
                case 2:
                    float temp = (float)health;
                    temp *= 1.5f;
                    health = (int)temp;
                    break;
                case 3:
                    health *= 2;
                    
                    break;
            }
        }
    }

    void Update()
    {
        if (!CanAct) return;

        //animator.SetBool("isAttack1", checkAttack());
        if (!isAttacking && checkAttack() && !isArcher)
        {
            performAttack();
        }
        else if (!isAttacking && checkAttack() && isArcher)
        {
            performRangedAttack();
        }
        else ResetAllAttackParameters();
        isHurt = GetComponent<EnemyHealth>().isHurt;
        animator.SetBool("isHurt", isHurt);
        if (!isArcher) animator.SetBool("isAttacking", isAttacking);
    }

    void performRangedAttack()
    {
        ai.UpdateEnemyFacingDirection();
        isAttacking = true;
        ResetAllAttackParameters();
        animator.SetBool("isAttack1", true);
        StartCoroutine(AttackCooldown(attackSpeed));
    }

    void performAttack()
    {
        isAttacking = true;
        //animator.SetBool("isAttacking", isAttacking);
        //Debug.Log("set animator : isAttacking = true ");
        int attackType = has3Attack ? Random.Range(1, 4) : Random.Range(1, 3);
        ResetAllAttackParameters();

        switch (attackType)
        {
            case 1:
                animator.SetBool("isAttack1", true);
                break;
            case 2:
                animator.SetBool("isAttack2", true);
                break;
            case 3:
                animator.SetBool("isAttack3", true);
                break;
        }
        StartCoroutine(AttackCooldown(attackSpeed));
    }

    void ResetAllAttackParameters()
    {
        if (isArcher)
        {
            animator.SetBool("isAttack1", false);
        }
        else
        {
            if (has3Attack)
            {
                animator.SetBool("isAttack1", false);
                animator.SetBool("isAttack2", false);
                animator.SetBool("isAttack3", false);
            }
            else
            {
                animator.SetBool("isAttack1", false);
                animator.SetBool("isAttack2", false);
            }
        }


    }
    IEnumerator AttackCooldown(float cooldownTime)
    {
        //animator.SetBool("isAttacking", false);
        //Debug.Log("set animator : isAttacking = false ");
        yield return new WaitForSeconds(cooldownTime);
        isAttacking = false;
        ResetAllAttackParameters();
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //Bullet bullet = collision.GetComponent<Bullet>();
    //if (bullet != null)
    //{
    //    isHurt = true;
    //    Debug.Log("Is hurt is true");
    //    StartCoroutine(ResetHurt());


    //    Destroy(collision.gameObject);
    //}
    //}

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    Bullet bullet = collision.collider.GetComponent<Bullet>();
    //    if (bullet != null)
    //    {
    //        isHurt = true;
    //        Debug.Log("Is hurt is true");
    //        StartCoroutine(ResetHurt());


    //        Destroy(collision.gameObject);
    //    }
    //}

    private bool checkAttack()
    {
        return GetComponentInChildren<EnemyInteractZone>().isTouchPlayer;
    }


}
