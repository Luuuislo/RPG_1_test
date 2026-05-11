using UnityEngine;
using UnityEngine.AI;

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
    public LayerMask buildingLayers;
    public float buildingAttackRange = 4f;

    [Header("Spawn Point")]
    public Transform spawnPoint;

    protected float lastAttackTime = 0f;
    protected bool isAttacking = false;
    protected bool canMove = true;
    protected Vector2 playerDirection;
    protected Transform _currentTarget;

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

        _currentTarget = FindClosestTarget();

        switch (currentState)
        {
            case EnemyState.Patrolling:
                if (_currentTarget != null &&
                    Vector3.Distance(transform.position, _currentTarget.position) <= detectionRange)
                    EnterChaseState();
                break;

            case EnemyState.Chasing:
                if (isAttacking) break;
                if (_currentTarget == null)
                {
                    EnterReturnState();
                    break;
                }
                float distToTarget = Vector3.Distance(transform.position, _currentTarget.position);
                bool targetIsBuilding = _currentTarget.GetComponentInParent<BuildingAttack>() != null;
                float effectiveAttackRange = targetIsBuilding ? buildingAttackRange : attackRange;
                if (distToTarget > chaseRange)
                {
                    EnterReturnState();
                }
                else if (distToTarget <= effectiveAttackRange)
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
                    agent.SetDestination(GetDestinationFor(_currentTarget));
                }
                break;

            case EnemyState.Returning:
                Vector3 home = spawnPoint != null ? spawnPoint.position : patrolCenter;
                if (Vector3.Distance(transform.position, home) < 0.8f)
                    EnterPatrolState();
                break;
        }
    }

    Vector3 GetDestinationFor(Transform target)
    {
        if (target.GetComponentInParent<BuildingAttack>() != null)
        {
            // Navigate to the closest walkable NavMesh point near the building
            if (NavMesh.SamplePosition(target.position, out NavMeshHit hit, buildingAttackRange, NavMesh.AllAreas))
                return hit.position;
        }
        return target.position;
    }

    Transform FindClosestTarget()
    {
        Transform closest = null;
        float minDist = float.MaxValue;

        if (playerTransform != null)
        {
            float d = Vector3.Distance(transform.position, playerTransform.position);
            if (d < minDist) { minDist = d; closest = playerTransform; }
        }

        if (buildingLayers != 0)
        {
            Collider2D[] buildings = Physics2D.OverlapCircleAll(
                transform.position, chaseRange, buildingLayers);
            foreach (var b in buildings)
            {
                float d = Vector3.Distance(transform.position, b.transform.position);
                if (d < minDist) { minDist = d; closest = b.transform; }
            }
        }

        // También atacar unidades aliadas del jugador
        Collider2D[] allCols = Physics2D.OverlapCircleAll(transform.position, chaseRange);
        foreach (var c in allCols)
        {
            var ally = c.GetComponentInParent<AllyUnit>();
            if (ally == null || !ally.IsAlive) continue;
            float d = Vector3.Distance(transform.position, c.transform.position);
            if (d < minDist) { minDist = d; closest = c.transform; }
        }

        return closest;
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
        if (_currentTarget == null) return;

        isAttacking = true;
        canMove = false;
        agent.ResetPath();
        playerDirection = _currentTarget.position - transform.position;

        transform.localScale = _currentTarget.position.x > transform.position.x
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
        if (_currentTarget == null) return;

        Vector2 attackDir = playerDirection == Vector2.zero ? Vector2.right : playerDirection.normalized;

        BuildingAttack building = _currentTarget.GetComponentInParent<BuildingAttack>();
        if (building != null) { building.TakeDamage(attackDamage); return; }

        DamageReceiver dr = _currentTarget.GetComponentInParent<DamageReceiver>();
        if (dr != null) { dr.ApplyDamage(attackDamage, true, true, attackDir); return; }

        DamageReceiverPlayer drp = _currentTarget.GetComponentInParent<DamageReceiverPlayer>();
        if (drp != null) { drp.ApplyDamage(attackDamage, attackDir); return; }

        // Ally units
        AllyUnit ally = _currentTarget.GetComponentInParent<AllyUnit>();
        ally?.TakeDamage(attackDamage);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}
