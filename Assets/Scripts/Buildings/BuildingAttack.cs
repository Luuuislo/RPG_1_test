using UnityEngine;

public class BuildingAttack : MonoBehaviour
{
    [Header("Attack Stats")]
    public int   attackDamage  = 1;
    public float attackRange   = 5f;
    public float attackCooldown= 2f;

    [Header("Health")]
    public int maxHealth = 50;

    [Header("Attack Zone")]
    [Tooltip("Trigger radius — debe ser mayor que el margen Not Walkable del NavMesh")]
    public float attackZoneRadius = 1.5f;

    [Header("Projectile")]
    public GameObject arrowPrefab;
    public float      arrowSpeed = 8f;

    [Header("Targeting")]
    public LayerMask enemyLayers;

    private int   _currentHealth;
    private float _lastAttackTime = -999f;
    private static GameObject _damageTextPrefab;

    void Start()
    {
        _currentHealth = maxHealth;
        SetupAttackZone();
    }

    void SetupAttackZone()
    {
        // Crea un trigger circular que sobresale del margen Not Walkable
        // para que los enemigos puedan detectar y atacar el edificio
        foreach (var col in GetComponents<CircleCollider2D>())
            if (col.isTrigger) return; // ya existe

        var zone = gameObject.AddComponent<CircleCollider2D>();
        zone.isTrigger = true;
        zone.radius    = attackZoneRadius;
    }

    void Update()
    {
        if (Time.time < _lastAttackTime + attackCooldown) return;

        Collider2D target = FindClosestEnemy();
        if (target == null) return;

        Fire(target);
        _lastAttackTime = Time.time;
    }

    void Fire(Collider2D target)
    {
        if (arrowPrefab == null) return;

        GameObject arrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);

        var rb = arrow.GetComponent<Rigidbody2D>();
        if (rb != null) rb.gravityScale = 0f;

        var proj = arrow.GetComponent<Projectile>() ?? arrow.AddComponent<Projectile>();
        proj.damage          = attackDamage;
        proj.speed           = arrowSpeed;
        proj.damageableLayers= enemyLayers;
        proj.SetTarget(target.transform);
    }

    Collider2D FindClosestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayers);
        Collider2D closest = null;
        float minDist = float.MaxValue;
        foreach (var hit in hits)
        {
            float d = Vector2.Distance(transform.position, hit.transform.position);
            if (d < minDist) { minDist = d; closest = hit; }
        }
        return closest;
    }

    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;
        SpawnDamageText(amount);
        if (_currentHealth <= 0)
            Destroy(gameObject);
    }

    private void SpawnDamageText(int amount)
    {
        if (_damageTextPrefab == null)
            _damageTextPrefab = Resources.Load<GameObject>("FloatingDamageText");
        if (_damageTextPrefab == null) return;
        Vector3 pos = transform.position + new Vector3(Random.Range(-0.3f, 0.3f), 1f, 0f);
        Instantiate(_damageTextPrefab, pos, Quaternion.identity)
            .GetComponent<FloatingDamageText>()?.Setup(amount);
    }

    void OnDrawGizmosSelected()
    {
        // Rango de detección de enemigos (azul)
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.15f);
        Gizmos.DrawSphere(transform.position, attackRange);
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Zona de ataque recibido / trigger (naranja)
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.15f);
        Gizmos.DrawSphere(transform.position, attackZoneRadius);
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, attackZoneRadius);
    }
}
