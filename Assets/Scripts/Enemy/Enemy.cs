using UnityEngine;
using UnityEngine.AI;

public class Enemy : NPC
{
    [Header("Attack")]
    public int   attackDamage          = 1;
    public float attackRange           = 1.5f;
    public float stopDistance          = 0.5f;
    public float attackCoolDown        = 2f;
    public float attackDuration        = 0.8f;
    public float attackAnimationSpeed  = 1.5f;
    public LayerMask damageableLayers;

    [Header("Detection")]
    public float detectionRange    = 6f;
    public float chaseRange        = 10f;
    public LayerMask buildingLayers;
    public float buildingAttackRange = 4f;

    [Header("Spawn Point")]
    public Transform spawnPoint;

    protected float     lastAttackTime = 0f;
    protected bool      isAttacking    = false;
    protected bool      canMove        = true;
    protected Vector2   playerDirection;
    protected Transform _currentTarget;

    protected EnemyState currentState = EnemyState.Patrolling;
    protected enum EnemyState { Patrolling, Chasing, SearchingLastKnown, Returning }

    bool _activateAsWaveEnemy;

    // ── Path tracking ────────────────────────────────────────────────────
    private NavMeshPath _navPath;
    private Vector3     _cachedDest;
    private float       _nextPathUpdate;
    private const float PathUpdateInterval   = 0.2f;
    private const float DestChangedThreshold = 0.4f;
    private const float FallbackSampleStep   = 0.5f;

    // ── Last known position (busca al player antes de rendirse) ──────────
    private Vector3 _lastKnownPos;
    private bool    _hasLastKnownPos;

    // ── Predicción de intercepción ────────────────────────────────────────
    private const float PredictMaxTime = 0.45f;

    // ── Stuck detection ───────────────────────────────────────────────────
    private Vector3 _posAtLastStuckCheck;
    private float   _nextStuckCheck;
    private const float StuckCheckInterval  = 1.2f;
    private const float StuckMoveThreshold  = 0.15f;

    // ── Target search throttle ────────────────────────────────────────────
    private Transform _cachedTarget;
    private float     _nextTargetSearch;
    private const float TargetSearchInterval = 0.15f;

    // ── Lifecycle ────────────────────────────────────────────────────────

    protected override void Start()
    {
        useSkinSystem = false;
        movementType  = MovementType.RandomMovement;
        base.Start();
        if (spawnPoint != null)
            patrolCenter = spawnPoint.position;
        _navPath             = new NavMeshPath();
        _posAtLastStuckCheck = transform.position;

        if (_activateAsWaveEnemy)
        {
            detectionRange = chaseRange;
            currentState   = EnemyState.Chasing;
        }
    }

    // Llámalo después de Instantiate y antes del primer frame (Start aún no corrió).
    public void ActivateAsWaveEnemy() => _activateAsWaveEnemy = true;

    protected override void Update()
    {
        if (!isAttacking) base.Update();

        _currentTarget = FindClosestTarget();

        switch (currentState)
        {
            case EnemyState.Patrolling:
                if (_currentTarget != null &&
                    Vector2.Distance(transform.position, _currentTarget.position) <= detectionRange)
                    EnterChaseState();
                break;

            case EnemyState.Chasing:
                if (isAttacking) break;

                if (_currentTarget == null)
                {
                    // Ir a la última posición conocida antes de rendirse
                    if (_hasLastKnownPos)
                    {
                        currentState = EnemyState.SearchingLastKnown;
                        agent.SetDestination(_lastKnownPos);
                    }
                    else
                        EnterReturnState();
                    break;
                }

                _lastKnownPos    = _currentTarget.position;
                _hasLastKnownPos = true;

                float dist = Vector2.Distance(transform.position, _currentTarget.position);
                bool  isBuilding = _currentTarget.GetComponentInParent<BuildingAttack>() != null;
                float atkRange   = isBuilding ? buildingAttackRange : attackRange;

                if (dist > chaseRange)
                    EnterReturnState();
                else if (dist <= atkRange)
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
                    UpdateChaseDestination();
                    CheckStuck();
                }
                break;

            case EnemyState.SearchingLastKnown:
                // Target reaparecido → reanudar persecución
                if (_currentTarget != null)
                {
                    EnterChaseState();
                    break;
                }
                // Llegó a la última posición conocida sin encontrar al target → volver
                if (Vector2.Distance(transform.position, _lastKnownPos) < 1.2f)
                {
                    _hasLastKnownPos = false;
                    EnterReturnState();
                }
                break;

            case EnemyState.Returning:
                Vector3 home = spawnPoint != null ? spawnPoint.position : patrolCenter;
                if (Vector2.Distance(transform.position, home) < 0.8f)
                    EnterPatrolState();
                break;
        }
    }

    // ── Pathfinding ──────────────────────────────────────────────────────

    void UpdateChaseDestination()
    {
        Vector3 dest = GetInterceptDestination();

        bool destMoved = Vector3.Distance(dest, _cachedDest) > DestChangedThreshold;
        if (!destMoved && Time.time < _nextPathUpdate) return;

        _nextPathUpdate = Time.time + PathUpdateInterval;
        _cachedDest     = dest;

        if (agent.CalculatePath(dest, _navPath) && _navPath.status == NavMeshPathStatus.PathComplete)
        {
            agent.SetPath(_navPath);
            return;
        }

        // Camino bloqueado → buscar punto alcanzable más cercano al destino
        Vector3 best = FindClosestReachablePointToward(dest);
        if (agent.CalculatePath(best, _navPath) && _navPath.status == NavMeshPathStatus.PathComplete)
            agent.SetPath(_navPath);
        else
            agent.SetDestination(best);
    }

    // Predice la posición futura del target según su velocidad para interceptarlo.
    Vector3 GetInterceptDestination()
    {
        if (_currentTarget == null) return transform.position;

        var rb = _currentTarget.GetComponent<Rigidbody2D>();
        if (rb != null && rb.linearVelocity.sqrMagnitude > 0.05f)
        {
            float dist    = Vector2.Distance(transform.position, _currentTarget.position);
            float eta     = dist / Mathf.Max(agent.speed, 0.1f);
            float predict = Mathf.Clamp(eta * 0.5f, 0f, PredictMaxTime);
            Vector3 predicted = _currentTarget.position + (Vector3)(rb.linearVelocity * predict);

            // Solo usar predicción si el punto está en el NavMesh
            if (NavMesh.SamplePosition(predicted, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                return hit.position;
        }

        return GetDestinationFor(_currentTarget);
    }

    // Si el enemy lleva más de StuckCheckInterval sin moverse lo suficiente,
    // invalida el caché de ruta para forzar recálculo.
    void CheckStuck()
    {
        if (Time.time < _nextStuckCheck) return;
        _nextStuckCheck = Time.time + StuckCheckInterval;

        if (Vector3.Distance(transform.position, _posAtLastStuckCheck) < StuckMoveThreshold)
        {
            _nextPathUpdate = 0f;
            _cachedDest     = Vector3.zero;
        }
        _posAtLastStuckCheck = transform.position;
    }

    Vector3 FindClosestReachablePointToward(Vector3 dest)
    {
        Vector3 dir   = (dest - transform.position).normalized;
        float   total = Vector3.Distance(transform.position, dest);
        int     steps = Mathf.CeilToInt(total / FallbackSampleStep);

        for (int i = steps; i > 0; i--)
        {
            Vector3 candidate = transform.position + dir * (i * FallbackSampleStep);
            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 1f, NavMesh.AllAreas)) continue;
            if (agent.CalculatePath(hit.position, _navPath) &&
                _navPath.status == NavMeshPathStatus.PathComplete)
                return hit.position;
        }
        return transform.position;
    }

    Vector3 GetDestinationFor(Transform target)
    {
        if (target.GetComponentInParent<BuildingAttack>() != null)
        {
            if (NavMesh.SamplePosition(target.position, out NavMeshHit hit,
                                       buildingAttackRange, NavMesh.AllAreas))
                return hit.position;
        }
        return target.position;
    }

    // ── Target search (throttled) ─────────────────────────────────────────

    Transform FindClosestTarget()
    {
        // Solo buscar cada TargetSearchInterval para no pagar OverlapCircleAll cada frame
        if (Time.time < _nextTargetSearch && _cachedTarget != null)
            return _cachedTarget;
        _nextTargetSearch = Time.time + TargetSearchInterval;

        Transform closest = null;
        float     minDist = float.MaxValue;

        if (playerTransform != null)
        {
            float d = Vector2.Distance(transform.position, playerTransform.position);
            if (d < chaseRange && d < minDist) { minDist = d; closest = playerTransform; }
        }

        if (buildingLayers != 0)
        {
            foreach (var b in Physics2D.OverlapCircleAll(transform.position, chaseRange, buildingLayers))
            {
                float d = Vector2.Distance(transform.position, b.transform.position);
                if (d < minDist) { minDist = d; closest = b.transform; }
            }
        }

        foreach (var c in Physics2D.OverlapCircleAll(transform.position, chaseRange))
        {
            var ally = c.GetComponentInParent<AllyUnit>();
            if (ally == null || !ally.IsAlive) continue;
            float d = Vector2.Distance(transform.position, c.transform.position);
            if (d < minDist) { minDist = d; closest = c.transform; }
        }

        _cachedTarget = closest;
        return closest;
    }

    // ── State transitions ────────────────────────────────────────────────

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
        _hasLastKnownPos = false;
        StartRandomRoutine();
    }

    // ── Attack ───────────────────────────────────────────────────────────

    protected virtual void AttackPlayer()
    {
        if (_currentTarget == null) return;

        isAttacking     = true;
        canMove         = false;
        agent.ResetPath();
        playerDirection = _currentTarget.position - transform.position;

        transform.localScale = _currentTarget.position.x > transform.position.x
            ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);

        animator.SetBool("isRunning", false);
        float animationSpeed = Mathf.Max(0.1f, attackAnimationSpeed);
        animator.speed = animationSpeed;
        if (animator.parameters != null)
            foreach (var p in animator.parameters)
                if (p.name == "AttackDirection")
                    { animator.SetInteger("AttackDirection", 1); break; }
        animator.SetTrigger("DoAttack");

        float duration = attackDuration / animationSpeed;
        CancelInvoke(nameof(ResetAttack));
        Invoke(nameof(DetectAndDamageTarget), duration * 0.5f);
        Invoke(nameof(ResetAttack), duration);
    }

    protected void ResetAttack()
    {
        isAttacking = false;
        canMove     = true;
        animator.speed = 1f;
    }

    public void DetectAndDamageTarget()
    {
        if (_currentTarget == null) return;

        Vector2 attackDir = playerDirection == Vector2.zero
            ? Vector2.right : playerDirection.normalized;

        var building = _currentTarget.GetComponentInParent<BuildingAttack>();
        if (building != null) { building.TakeDamage(attackDamage); return; }

        var dr = _currentTarget.GetComponentInParent<DamageReceiver>();
        if (dr != null) { dr.ApplyDamage(attackDamage, true, true, attackDir); return; }

        var drp = _currentTarget.GetComponentInParent<DamageReceiverPlayer>();
        if (drp != null) { drp.ApplyDamage(attackDamage, attackDir); return; }

        _currentTarget.GetComponentInParent<AllyUnit>()?.TakeDamage(attackDamage);
    }

    private void FixedUpdate()
    {
        if (agent != null && agent.isOnNavMesh)
            agent.isStopped = !canMove;
    }

    // ── Gizmos ───────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}
