using UnityEngine;

public class RangedEnemy : Enemy
{
    [Header("Attack")]
    public GameObject projectilePrefab;
    public GameObject[] projectilePrefabs;
    public Transform firePoint;

    [Header("Distances")]
    public float minAttackRange = 2f;

    [Header("Collision")]
    public LayerMask obstacleLayer;
    public float obstacleCheckDistance = 0.5f;

    [Header("Combat")]
    public float attackCooldown = 2f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;

    private bool projectileSpawnedThisAttack;
    public int spellIndex;

    protected override void Start()
    {
        base.Start();
        StopCurrentRoutine(); // prevent NPC patrol coroutine from interfering
        agent.speed = moveSpeed;
        agent.stoppingDistance = 0f;
    }

    void Update()
    {
        if (playerTransform == null) return;
        if (isAttacking) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance > chaseRange)
        {
            agent.ResetPath();
            animator.SetBool("isRunning", false);
            return;
        }

        if (distance < minAttackRange)
        {
            Vector2 fleeDir = (transform.position - playerTransform.position).normalized;
            bool cornered = IsPathBlocked(fleeDir);

            if (cornered || Time.time >= lastAttackTime + attackCooldown)
            {
                // Cornered or cooldown ready: stop and cast
                agent.ResetPath();
                animator.SetBool("isRunning", false);
                TryCastSpell();
            }
            else
            {
                // Back away while waiting for cooldown
                Flee(fleeDir);
                animator.SetBool("isRunning", agent.velocity.sqrMagnitude > 0.01f);
            }
        }
        else if (distance <= attackRange)
        {
            // Preferred range: stop and cast
            agent.ResetPath();
            animator.SetBool("isRunning", false);
            TryCastSpell();
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

    void TryCastSpell()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
            AttackPlayer();
    }

    bool IsPathBlocked(Vector2 direction)
    {
        if (obstacleLayer == 0) return false;
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, 0.2f, direction, obstacleCheckDistance, obstacleLayer);
        return hit.collider != null;
    }

    void Flee(Vector2 direction)
    {
        Vector3 target = transform.position + new Vector3(direction.x, direction.y, 0f) * 6f;
        agent.SetDestination(target);
        if (agent.desiredVelocity.x > 0.1f) transform.localScale = Vector3.one;
        else if (agent.desiredVelocity.x < -0.1f) transform.localScale = new Vector3(-1, 1, 1);
    }

    protected override void AttackPlayer()
    {
        if (isAttacking) return;
        if (Time.time < lastAttackTime + attackCooldown) return;

        lastAttackTime = Time.time;
        isAttacking = true;
        canMove = false;
        projectileSpawnedThisAttack = false;
        agent.ResetPath();

        playerDirection = playerTransform.position - transform.position;
        transform.localScale = playerTransform.position.x > transform.position.x
            ? Vector3.one
            : new Vector3(-1, 1, 1);
        animator.SetBool("isRunning", false);

        spellIndex = Random.Range(0, projectilePrefabs.Length);
        float animationSpeed = Mathf.Max(0.1f, attackAnimationSpeed);
        animator.speed = animationSpeed;
        animator.SetInteger("SpellIndex", spellIndex);
        animator.SetTrigger("CastSpell");

        CancelInvoke(nameof(ResetAttack));
        Invoke(nameof(ResetAttack), attackDuration / animationSpeed);
    }

    public void SpawnProjectile()
    {
        if (projectileSpawnedThisAttack) return;

        GameObject selectedProjectilePrefab = GetSelectedProjectilePrefab();
        if (selectedProjectilePrefab == null || firePoint == null) return;

        projectileSpawnedThisAttack = true;

        GameObject projectileObject = Instantiate(
            selectedProjectilePrefab,
            firePoint.position,
            Quaternion.identity
        );

        Projectile projectile = projectileObject.GetComponent<Projectile>();
        if (projectile != null)
            projectile.SetTarget(playerTransform);
    }

    private GameObject GetSelectedProjectilePrefab()
    {
        if (projectilePrefabs != null && projectilePrefabs.Length > 0)
        {
            int index = Mathf.Clamp(spellIndex, 0, projectilePrefabs.Length - 1);
            return projectilePrefabs[index];
        }
        return projectilePrefab;
    }
}
