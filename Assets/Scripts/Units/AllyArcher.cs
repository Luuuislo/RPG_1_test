using UnityEngine;
using UnityEngine.AI;

public class AllyArcher : AllyUnit
{
    [Header("Ranged")]
    public GameObject arrowPrefab;
    public Sprite     arrowSprite;
    public float      arrowSpeed   = 10f;
    public float      minDistance  = 3f;

    protected override void Update()
    {
        base.Update();
        // Forzar animación de correr cuando está huyendo (el estado base es Attacking)
        if (_target != null && Vector2.Distance(transform.position, _target.position) < minDistance)
            _animator?.SetBool("isRunning", true);
    }

    protected override void DoAttack()
    {
        if (_target == null) return;
        _isAttacking = true;

        transform.localScale = _target.position.x >= transform.position.x
            ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);

        _animator?.SetTrigger("DoAttack");
        Invoke(nameof(FireArrow),   6f / 14f);
        Invoke(nameof(ResetAttack), attackCooldown * 0.8f);
    }

    void FireArrow()
    {
        if (_target == null) return;
        if (arrowPrefab == null) { Debug.LogWarning("[AllyArcher] arrowPrefab no asignado."); return; }

        Vector3 dir   = (_target.position - transform.position).normalized;
        Vector3 spawn = transform.position + dir * 0.4f;

        var go = Instantiate(arrowPrefab, spawn, Quaternion.identity);
        go.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

        if (arrowSprite != null)
        {
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite == null) sr.sprite = arrowSprite;
        }

        var rb = go.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = dir * arrowSpeed;

        var proj = go.GetComponent<AllyProjectile>();
        if (proj != null) { proj.damage = (int)attackDamage; proj.enemyLayers = enemyLayers; }
    }

    // Huye buscando la mejor salida y sigue atacando mientras se aleja
    protected override void TryAttack()
    {
        if (_target == null) return;
        float dist = Vector2.Distance(transform.position, _target.position);
        if (dist < minDistance)
        {
            Vector3 flee = FindFleeDestination();
            if (_agent.isOnNavMesh) _agent.SetDestination(flee);
        }
        base.TryAttack();
    }

    // Prueba 7 ángulos desde la dirección opuesta al enemigo y elige el primero
    // que tenga NavMesh válido y aleje más del enemigo que la posición actual.
    Vector3 FindFleeDestination()
    {
        Vector3 awayDir      = (transform.position - _target.position).normalized;
        float   fleeDistance = 4f;
        float[] angles       = { 0f, 45f, -45f, 90f, -90f, 135f, -135f };

        float currentDist = Vector2.Distance(transform.position, _target.position);

        foreach (float angle in angles)
        {
            Vector3 dir       = Quaternion.Euler(0, 0, angle) * awayDir;
            Vector3 candidate = transform.position + dir * fleeDistance;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, fleeDistance * 0.5f, NavMesh.AllAreas))
            {
                if (Vector2.Distance(hit.position, _target.position) > currentDist)
                    return hit.position;
            }
        }

        // Fallback: dirección opuesta aunque no aleje más (acorralado sin salida)
        return transform.position + awayDir * fleeDistance;
    }

    // Permite moverse mientras ataca si está huyendo
    protected override void FixedUpdate()
    {
        if (_agent == null || !_agent.isOnNavMesh) return;
        if (_state == AllyState.Dead) { _agent.isStopped = true; return; }
        bool fleeing = _target != null && Vector2.Distance(transform.position, _target.position) < minDistance;
        _agent.isStopped = !fleeing && (_isAttacking || _state == AllyState.Attacking);
    }
}
