using UnityEngine;

public class GnollEnemy : Enemy
{
    [Header("Ranged Attack")]
    public float rangedThreshold = 7f;
    public float chaseRange = 12f;
    public GameObject bonePrefab;
    public Transform firePoint;
    public float rangedCooldown = 2.5f;
    [SerializeField] private float moveSpeed = 2.5f;

    [Header("Collision")]
    public LayerMask obstacleLayer;
    public float obstacleCheckDistance = 0.6f;

    private float lastRangedAttackTime;
    private bool boneSpawned;
    private bool isDead;

    protected override void Start()
    {
        base.Start();
        StopCurrentRoutine(); // prevent NPC patrol coroutine from interfering
        agent.speed = moveSpeed;
        agent.stoppingDistance = 0f;

        // Cancel pending bone throw if DamageReceiver is on a child object and dies first
        DamageReceiver dr = GetComponentInChildren<DamageReceiver>();
        if (dr != null) dr.onDeath += OnDeath;
    }

    private void OnDeath()
    {
        isDead = true;
        CancelInvoke();
        agent.ResetPath();
    }

    new void Update()
    {
        if (playerTransform == null) return;
        if (isAttacking) return;
        if (isDead) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);

        if (dist > chaseRange)
        {
            agent.ResetPath();
            animator.SetBool("isRunning", false);
            return;
        }

        if (dist <= attackRange)
        {
            // Too close: try to back away and keep shooting
            Vector2 fleeDir = (transform.position - playerTransform.position).normalized;
            bool cornered = IsPathBlocked(fleeDir);

            if (cornered)
            {
                agent.ResetPath();
                animator.SetBool("isRunning", false);
                if (Time.time >= lastAttackTime + attackCoolDown)
                    DoMeleeAttack();
            }
            else if (Time.time >= lastRangedAttackTime + rangedCooldown)
            {
                agent.ResetPath();
                animator.SetBool("isRunning", false);
                DoRangedAttack();
            }
            else
            {
                BackAway(fleeDir);
                animator.SetBool("isRunning", true);
            }
        }
        else if (dist <= rangedThreshold)
        {
            // Preferred range: stop and shoot
            agent.ResetPath();
            animator.SetBool("isRunning", false);
            if (Time.time >= lastRangedAttackTime + rangedCooldown)
                DoRangedAttack();
        }
        else if (dist <= rangedThreshold + 0.3f)
        {
            // Buffer zone: stop cleanly to avoid jitter at boundary
            agent.ResetPath();
            animator.SetBool("isRunning", false);
        }
        else
        {
            // Too far: chase via NavMesh
            agent.SetDestination(playerTransform.position);
            animator.SetBool("isRunning", agent.velocity.sqrMagnitude > 0.01f);
            if (agent.desiredVelocity.x > 0.1f) transform.localScale = Vector3.one;
            else if (agent.desiredVelocity.x < -0.1f) transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    bool IsPathBlocked(Vector2 direction)
    {
        if (obstacleLayer == 0) return false;
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, 0.2f, direction, obstacleCheckDistance, obstacleLayer);
        return hit.collider != null;
    }

    void BackAway(Vector2 fleeDir)
    {
        Vector3 target = transform.position + new Vector3(fleeDir.x, fleeDir.y, 0f) * 6f;
        agent.SetDestination(target);
        // Always face the player while backing away
        transform.localScale = playerTransform.position.x > transform.position.x
            ? Vector3.one
            : new Vector3(-1, 1, 1);
    }

    private void DoRangedAttack()
    {
        lastRangedAttackTime = Time.time;
        isAttacking = true;
        boneSpawned = false;

        playerDirection = playerTransform.position - transform.position;
        transform.localScale = playerDirection.x >= 0 ? Vector3.one : new Vector3(-1, 1, 1);
        animator.SetBool("isRunning", false);

        float speed = Mathf.Max(0.1f, attackAnimationSpeed);
        animator.speed = speed;
        animator.SetTrigger("Throw");

        float duration = attackDuration / speed;
        Invoke(nameof(SpawnBone), duration * 0.4f);
        CancelInvoke(nameof(ResetAttack));
        Invoke(nameof(ResetAttack), duration);
    }

    private void DoMeleeAttack()
    {
        lastAttackTime = Time.time;
        isAttacking = true;
        canMove = false;

        playerDirection = playerTransform.position - transform.position;
        transform.localScale = playerDirection.x >= 0 ? Vector3.one : new Vector3(-1, 1, 1);
        animator.SetBool("isRunning", false);

        float speed = Mathf.Max(0.1f, attackAnimationSpeed);
        animator.speed = speed;
        animator.SetInteger("AttackDirection", 1);
        animator.SetTrigger("DoAttack");

        float duration = attackDuration / speed;
        Invoke(nameof(MeleeDamage), duration * 0.5f);
        CancelInvoke(nameof(ResetAttack));
        Invoke(nameof(ResetAttack), duration);
    }

    private void MeleeDamage()
    {
        DetectAndDamageTarget();
    }

    private void SpawnBone()
    {
        if (isDead || boneSpawned || bonePrefab == null || firePoint == null) return;
        boneSpawned = true;
        GameObject bone = Instantiate(bonePrefab, firePoint.position, Quaternion.identity);
        Projectile proj = bone.GetComponent<Projectile>();
        proj?.SetTarget(playerTransform);
    }
}
