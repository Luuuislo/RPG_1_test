using UnityEngine;
using UnityEngine.AI;

// Base para todas las unidades aliadas (Warrior, Lancer).
// Detecta enemigos, los persigue y ataca cuerpo a cuerpo.
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class AllyUnit : MonoBehaviour
{
    [Header("Stats")]
    public float maxHp          = 100f;
    public float attackDamage   = 15f;
    public float attackRange    = 1.4f;
    public float detectRange    = 8f;
    public float attackCooldown = 1.5f;
    public float moveSpeed      = 3f;

    [Header("Layers")]
    public LayerMask enemyLayers;

    // Estado legible desde UI/editor
    public float CurrentHp  { get; private set; }
    public bool  IsAlive    => _state != AllyState.Dead;

    protected NavMeshAgent _agent;
    protected Animator     _animator;
    protected Transform    _target;
    protected Vector3      _spawnPos;
    protected float        _lastAttackTime;
    protected bool         _isAttacking;

    private BuildingHealthBar         _healthBar;
    private static GameObject         _damageTextPrefab;

    protected enum AllyState { Idle, Chasing, Attacking, Dead }
    protected AllyState _state;

    // ── Lifecycle ────────────────────────────────────────────────────────

    protected virtual void Awake()
    {
        _agent     = GetComponent<NavMeshAgent>();
        _animator  = GetComponent<Animator>();
        _healthBar = GetComponent<BuildingHealthBar>();
        CurrentHp  = maxHp;
    }

    protected virtual void Start()
    {
        _spawnPos = transform.position;
        _agent.updateRotation = false;
        _agent.updateUpAxis   = false;
        _agent.speed          = moveSpeed;

        // Auto-asigna el layer Enemy si no está configurado en el Inspector
        if (enemyLayers == 0)
        {
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer >= 0) enemyLayers = 1 << enemyLayer;
        }
    }

    protected virtual void Update()
    {
        if (_state == AllyState.Dead) return;

        _target = FindNearestEnemy();
        UpdateStateMachine();
        UpdateAnimator();
        UpdateFacing();
    }

    protected virtual void FixedUpdate()
    {
        if (_agent != null && _agent.isOnNavMesh)
            _agent.isStopped = _isAttacking || _state == AllyState.Attacking || _state == AllyState.Dead;
    }

    // ── Target detection ─────────────────────────────────────────────────

    protected virtual Transform FindNearestEnemy()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, detectRange, enemyLayers);
        Transform closest  = null;
        float     minDist  = float.MaxValue;
        foreach (var h in hits)
        {
            if (!h.GetComponentInParent<DamageReceiver>()) continue;
            float d = Vector2.Distance(transform.position, h.transform.position);
            if (d < minDist) { minDist = d; closest = h.transform; }
        }
        return closest;
    }

    // ── State machine ────────────────────────────────────────────────────

    void UpdateStateMachine()
    {
        if (_target == null)
        {
            _state = AllyState.Idle;
            if (_agent.isOnNavMesh) _agent.ResetPath();
            return;
        }

        float dist = Vector2.Distance(transform.position, _target.position);

        if (dist <= attackRange)
        {
            _state = AllyState.Attacking;
            if (_agent.isOnNavMesh)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
            }
            TryAttack();
        }
        else
        {
            _state = AllyState.Chasing;
            if (_agent.isOnNavMesh)
            {
                _agent.isStopped = false;
                _agent.SetDestination(_target.position);
            }
        }
    }

    // ── Attack ───────────────────────────────────────────────────────────

    protected virtual void TryAttack()
    {
        if (_isAttacking) return;
        if (Time.time < _lastAttackTime + attackCooldown) return;
        _lastAttackTime = Time.time;
        DoAttack();
    }

    protected virtual void DoAttack()
    {
        if (_target == null) return;
        _isAttacking = true;
        _animator?.SetTrigger("DoAttack");
        Invoke(nameof(ApplyMeleeDamage), attackCooldown * 0.35f);
        Invoke(nameof(ResetAttack),      attackCooldown * 0.7f);
    }

    protected virtual void ApplyMeleeDamage()
    {
        if (_target == null) return;
        Vector2 dir = (_target.position - transform.position).normalized;
        var dr = _target.GetComponentInParent<DamageReceiver>();
        dr?.ApplyDamage((int)attackDamage, true, false, dir);
    }

    protected void ResetAttack() => _isAttacking = false;

    // ── Health ───────────────────────────────────────────────────────────

    public virtual void TakeDamage(float amount)
    {
        if (_state == AllyState.Dead) return;
        CurrentHp -= amount;
        SpawnDamageText((int)amount);
        _healthBar?.UpdateHealth((int)CurrentHp, (int)maxHp);
        if (CurrentHp <= 0f) Die();
    }

    public void HealDamage(float amount)
    {
        if (_state == AllyState.Dead) return;
        CurrentHp = Mathf.Min(CurrentHp + amount, maxHp);
        _healthBar?.UpdateHealth((int)CurrentHp, (int)maxHp);
        SpawnHealText((int)amount);
    }

    void SpawnDamageText(int amount)
    {
        if (_damageTextPrefab == null)
            _damageTextPrefab = Resources.Load<GameObject>("FloatingDamageText");
        if (_damageTextPrefab == null) return;
        var pos = transform.position + Vector3.up * 0.5f;
        Instantiate(_damageTextPrefab, pos, Quaternion.identity)
            .GetComponent<FloatingDamageText>()?.Setup(amount);
    }

    void SpawnHealText(int amount)
    {
        if (_damageTextPrefab == null)
            _damageTextPrefab = Resources.Load<GameObject>("FloatingDamageText");
        if (_damageTextPrefab == null) return;
        var pos = transform.position + Vector3.up * 0.5f;
        Instantiate(_damageTextPrefab, pos, Quaternion.identity)
            .GetComponent<FloatingDamageText>()?.Setup(amount, Color.green);
    }

    protected virtual void Die()
    {
        _state = AllyState.Dead;
        _agent.isStopped = true;
        _animator?.SetTrigger("Die");
        Destroy(gameObject, 2f);
    }

    // ── Visuals ──────────────────────────────────────────────────────────

    void UpdateAnimator()
    {
        if (_animator == null) return;
        _animator.SetBool("isRunning", _state == AllyState.Chasing);
    }

    void UpdateFacing()
    {
        Vector2 vel = (Vector2)_agent.velocity;
        if (_target != null && _state == AllyState.Attacking)
            vel = _target.position - transform.position;
        if (vel.x >  0.1f) transform.localScale = new Vector3( 1, 1, 1);
        if (vel.x < -0.1f) transform.localScale = new Vector3(-1, 1, 1);
    }

    // ── Gizmos ───────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
