using UnityEngine;

// Monje aliado: en lugar de atacar enemigos, busca aliados con vida baja y los cura.
// Si todos los aliados están al máximo de vida, se queda parado (idle).
public class AllyMonk : AllyUnit
{
    [Header("Healing")]
    public float      healAmount      = 25f;
    public float      healRange       = 1.8f;
    public float      healDetectRange = 10f;
    public GameObject healEffectPrefab;

    private AllyUnit _healTarget;

    // El Monk no busca enemigos, siempre retorna null
    protected override Transform FindNearestEnemy() => null;

    protected override void Update()
    {
        if (_state == AllyState.Dead) return;

        _healTarget = FindLowestHpAlly();

        if (_healTarget != null)
        {
            float dist = Vector3.Distance(transform.position, _healTarget.transform.position);
            if (dist > healRange)
            {
                _state = AllyState.Chasing;
                if (_agent.isOnNavMesh) _agent.SetDestination(_healTarget.transform.position);
            }
            else
            {
                _state = AllyState.Attacking;
                if (_agent.isOnNavMesh) _agent.ResetPath();
                TryHeal();
            }
        }
        else
        {
            _state = AllyState.Idle;
            if (_agent.isOnNavMesh) _agent.ResetPath();
        }

        UpdateAnimatorAndFacing();
    }

    AllyUnit FindLowestHpAlly()
    {
        // Busca cualquier colisionador en el radio y filtra por AllyUnit (sin necesitar layer)
        var hits     = Physics2D.OverlapCircleAll(transform.position, healDetectRange);
        AllyUnit lowest   = null;
        float    minRatio = 0.99f;

        foreach (var h in hits)
        {
            var ally = h.GetComponentInParent<AllyUnit>();
            if (ally == null || ally == this || !ally.IsAlive) continue;
            float ratio = ally.CurrentHp / ally.maxHp;
            if (ratio < minRatio) { minRatio = ratio; lowest = ally; }
        }
        return lowest;
    }

    void TryHeal()
    {
        if (_isAttacking) return;
        if (Time.time < _lastAttackTime + attackCooldown) return;
        _lastAttackTime = Time.time;
        _isAttacking    = true;
        _animator?.SetTrigger("DoAttack");
        Invoke(nameof(ApplyHeal),   attackCooldown * 0.35f);
        Invoke(nameof(ResetAttack), attackCooldown * 0.7f);
    }

    void ApplyHeal()
    {
        if (_healTarget == null || !_healTarget.IsAlive) return;
        _healTarget.HealDamage(healAmount);
        if (healEffectPrefab != null)
        {
            var pos = new Vector3(_healTarget.transform.position.x, _healTarget.transform.position.y, 0f);
            var fx  = Instantiate(healEffectPrefab, pos, Quaternion.identity);
            var sr  = fx.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = 5;
            Destroy(fx, 1f);
        }
    }

    void UpdateAnimatorAndFacing()
    {
        _animator?.SetBool("isRunning", _state == AllyState.Chasing);

        Vector2 vel = (Vector2)_agent.velocity;
        if (_healTarget != null && _state == AllyState.Attacking)
            vel = _healTarget.transform.position - transform.position;
        if (vel.x >  0.1f) transform.localScale = new Vector3( 1, 1, 1);
        if (vel.x < -0.1f) transform.localScale = new Vector3(-1, 1, 1);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, healDetectRange);
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, healRange);
    }
}
