using UnityEngine;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System;

[DisallowMultipleComponent]
public class Enemy_Movement_AI : MonoBehaviour
{
    [SerializeField] private bool defaultFacingRight = true;
    [SerializeField] private MovementDetailsSO movementDetails;
    private Enemy enemy;

    [HideInInspector] public Transform target;
    [HideInInspector] public float nextWaypointDistance = 0.3f;
    private int currentWaypointIndex = 0;

    public float stopChaseDistance = 1f; 
    public float resumeChaseDistance = 2f; 
    public float resumeChaseDelay = 1f; 

    private Path path;
    private Seeker seeker;
    private Rigidbody2D rb;

    [HideInInspector] public float moveSpeed;
    private bool chasePlayer = false;
    private bool isWaitingToResumeChase = false;

    void Start()
    {
        enemy = GetComponent<Enemy>();
        target = GameObject.FindGameObjectWithTag("Player")?.transform;
        moveSpeed = movementDetails.GetMoveSpeed();

        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();

        InvokeRepeating("UpdatePath", 0f, 0.2f);
    }
    void UpdatePath()
    {
        if (enemy.CanAct && chasePlayer && target != null && seeker.IsDone())
        {
            Vector3 adjustedTargetPosition = GetChasePosition(target.position + Vector3.up * 0.5f);
            seeker.StartPath(rb.position, adjustedTargetPosition, OnPathComplete);
        }
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypointIndex = 0;
        }
    }

    void Update()
    {
        if (enemy.CanAct && target != null)
        {
            CheckChaseCondition();
            
            MoveEnemy();
        }
        else 
        {
            rb.linearVelocity = Vector2.zero;
            path = null;
            chasePlayer = false;
        }
    }

    private void CheckChaseCondition()
    {

        float distanceToPlayer = Vector3.Distance(transform.position, target.position);

       
        if (!chasePlayer && (enemy.roomBelong != null || distanceToPlayer < enemy.enemyDetails.chaseDistance)
            && !isWaitingToResumeChase )
        {
            chasePlayer = true;
            var chasePosition = GetChasePosition(target.position);
            enemy.movementToPositionEvent.CallMovementToPositionEvent(
                chasePosition,
                transform.position,
                moveSpeed,
                (chasePosition - transform.position).normalized
            );
        }

        if (chasePlayer && distanceToPlayer < stopChaseDistance)
        {
            chasePlayer = false;
            path = null;
            enemy.idleEvent.CallIdleEvent();
            StartCoroutine(ResumeChaseAfterDelay());
        }
    }

    private IEnumerator ResumeChaseAfterDelay()
    {
        isWaitingToResumeChase = true;
        yield return new WaitForSeconds(resumeChaseDelay);
        if (enemy.CanAct && target != null && Vector3.Distance(transform.position, target.position) > resumeChaseDistance)
        {
            chasePlayer = true;
        }
        isWaitingToResumeChase = false;
    }
    public void UpdateEnemyFacingDirection()
    {
        if (target == null) return;

        Vector2 directionToPlayer = target.position - transform.position;
        bool shouldFaceRight = directionToPlayer.x > 0; 

        
        float targetScaleX = Mathf.Abs(transform.localScale.x);

     
        if (shouldFaceRight != defaultFacingRight)
        {
            targetScaleX *= -1;
        }

        transform.localScale = new Vector3(targetScaleX, transform.localScale.y, transform.localScale.z);
    }
    private void MoveEnemy()
    {
        if (!chasePlayer || path == null || path.vectorPath.Count == 0) return;


        float distanceToWaypoint = Vector2.Distance(rb.position, (Vector2)path.vectorPath[currentWaypointIndex]);
        if (distanceToWaypoint < nextWaypointDistance)
        {
            currentWaypointIndex++;

           
            if (currentWaypointIndex >= path.vectorPath.Count)
            {
                currentWaypointIndex = 0;
                return;
            }
        }

        Vector2 targetPosition = (Vector2)path.vectorPath[currentWaypointIndex];
        if (enemy.roomBelong is BoxRoom room && !room.roomTiles.Contains(Vector2Int.FloorToInt(targetPosition)))
        {
            enemy.idleEvent.CallIdleEvent();
            return;
        }
        Vector2 direction = (targetPosition - rb.position).normalized;

        
        enemy.movementToPositionEvent.CallMovementToPositionEvent(
            targetPosition,
            transform.position,
            moveSpeed,
            direction
        );
        UpdateEnemyFacingDirection();
    }

    private Vector3 GetChasePosition(Vector3 position)
    {
        if (enemy.roomBelong is BoxRoom room)
        {
            Rect interior = room.Interior;
            position.x = Mathf.Clamp(position.x, interior.xMin, interior.xMax);
            position.y = Mathf.Clamp(position.y, interior.yMin, interior.yMax);
        }
        return position;
    }

    public void StartChase()
    {
        if (enemy.CanAct) chasePlayer = true;
    }
    void OnDisable()
    {
        StopAllCoroutines();
    }

}
