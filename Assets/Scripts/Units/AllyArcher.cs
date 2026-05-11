using UnityEngine;

// Unidad aliada a distancia. Hereda de AllyUnit pero detiene la persecución
// al llegar a attackRange y dispara una flecha en lugar de atacar cuerpo a cuerpo.
public class AllyArcher : AllyUnit
{
    [Header("Ranged")]
    public GameObject arrowPrefab;
    public Sprite     arrowSprite;          // asignar en Inspector (Arrow.png frame 0)
    public float      arrowSpeed   = 10f;
    public float      minDistance  = 3f;   // no se acerca más que esto

    protected override void DoAttack()
    {
        if (_target == null) return;
        _isAttacking = true;

        // Cara al objetivo antes de disparar
        transform.localScale = _target.position.x >= transform.position.x
            ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);

        _animator?.SetTrigger("DoAttack");
        Invoke(nameof(FireArrow),   6f / 14f);          // frame 6 at 14 fps
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

        // Asignar sprite si el prefab no lo trae serializado
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

    // El arquero mantiene distancia mínima: si está demasiado cerca, retrocede
    protected override void TryAttack()
    {
        if (_target == null) return;
        float dist = Vector3.Distance(transform.position, _target.position);
        if (dist < minDistance)
        {
            Vector3 away = (transform.position - _target.position).normalized;
            if (_agent.isOnNavMesh) _agent.SetDestination(transform.position + away * 1f);
            return;
        }
        base.TryAttack();
    }
}
