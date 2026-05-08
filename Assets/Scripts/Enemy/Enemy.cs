using UnityEngine;

public class Enemy : NPC
{
    [Header("Attack")]
    public int attackDamage = 1;
    public float attackRange = 1.5f;
    public float stopDistance = 0.5f;
    public float attackCoolDown = 2f;
    public float attackDuration = 0.8f;
    public float attackAnimationSpeed = 1.5f;
    public LayerMask damageableLayers;

    [Header("Detection")]
    public float detectionRange = 6f;
    public float chaseRange = 10f;

    [Header("Spawn Point")]
    public Transform spawnPoint;

    protected float lastAttackTime = 0f;
    protected bool isAttacking = false;
    protected bool canMove = true;
    protected Vector2 playerDirection;

    protected EnemyState currentState = EnemyState.Patrolling;
    protected enum EnemyState { Patrolling, Chasing, Returning }

    protected override void Start()
    {
        useSkinSystem = false;
        movementType = MovementType.RandomMovement;
        base.Start();
        if (spawnPoint != null)
            patrolCenter = spawnPoint.position;
    }

    protected override void Update()
    {
        if (!isAttacking)
            base.Update();

        if (playerTransform == null) return;

        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        switch (currentState)
        {
            case EnemyState.Patrolling:
                if (distToPlayer <= detectionRange)
                    EnterChaseState();
                break;

            case EnemyState.Chasing:
                if (isAttacking) break;
                if (distToPlayer > chaseRange)
                {
                    EnterReturnState();
                }
                else if (distToPlayer <= attackRange)
                {
                    agent.ResetPath();
                    if (Time.time >= lastAttackTime + attackCoolDown)
                    {
                        AttackPlayer();
                        lastAttackTime = Time.time;
                    }
                }
                else
                {
                    agent.SetDestination(playerTransform.position);
                }
                break;

            case EnemyState.Returning:
                Vector3 home = spawnPoint != null ? spawnPoint.position : patrolCenter;
                if (Vector3.Distance(transform.position, home) < 0.8f)
                    EnterPatrolState();
                break;
        }
    }

    private void EnterChaseState()
    {
        currentState = EnemyState.Chasing;
        StopCurrentRoutine();
    }

    private void EnterReturnState()
    {
        currentState = EnemyState.Returning;
        StopCurrentRoutine();
        Vector3 home = spawnPoint != null ? spawnPoint.position : patrolCenter;
        agent.SetDestination(home);
    }

    private void EnterPatrolState()
    {
        currentState = EnemyState.Patrolling;
        StartRandomRoutine();
    }

    protected virtual void AttackPlayer()
    {
        isAttacking = true;
        canMove = false;
        agent.ResetPath();
        playerDirection = playerTransform.position - transform.position;

        transform.localScale = playerTransform.position.x > transform.position.x
            ? new Vector3(1, 1, 1)
            : new Vector3(-1, 1, 1);

        animator.SetBool("isRunning", false);
        float animationSpeed = Mathf.Max(0.1f, attackAnimationSpeed);
        animator.speed = animationSpeed;
        if (animator.parameters != null)
            foreach (var p in animator.parameters)
                if (p.name == "AttackDirection") { animator.SetInteger("AttackDirection", 1); break; }
        animator.SetTrigger("DoAttack");

        float duration = attackDuration / animationSpeed;
        CancelInvoke(nameof(ResetAttack));
        Invoke(nameof(DetectAndDamageTarget), duration * 0.5f);
        Invoke(nameof(ResetAttack), duration);
    }

    protected void ResetAttack()
    {
        isAttacking = false;
        canMove = true;
        animator.speed = 1f;
    }

    private void FixedUpdate()
    {
        if (agent != null && agent.isOnNavMesh)
            agent.isStopped = !canMove;
    }

    public void DetectAndDamageTarget()
    {
        Vector2 attackDirection = playerDirection == Vector2.zero ? Vector2.right : playerDirection.normalized;
        Vector2 attackPoint = (Vector2)transform.position + attackDirection * attackRange * 0.5f;
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(attackPoint, attackRange, damageableLayers);
        foreach (Collider2D target in hitTargets)
        {
            DamageReceiver damageReceiver = target.GetComponentInParent<DamageReceiver>();
            if (damageReceiver != null)
            {
                damageReceiver.ApplyDamage(attackDamage, true, true, attackDirection);
            }
            else
            {
                DamageReceiverPlayer playerReceiver = target.GetComponentInParent<DamageReceiverPlayer>();
                playerReceiver?.ApplyDamage(attackDamage, attackDirection);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}
