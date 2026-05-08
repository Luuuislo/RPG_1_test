using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class SpawnPoint : MonoBehaviour
{
    [Header("Enemy")]
    public GameObject enemyPrefab;
    public int spawnCount = 1;

    [Header("Timing")]
    public float respawnDelay = 10f;

    [Header("Patrol Area")]
    public float patrolRadius = 5f;

    void Start()
    {
        for (int i = 0; i < spawnCount; i++)
            SpawnEnemy();
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        Vector3 spawnPos = GetSpawnPosition();
        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        // Configure patrol area and home point on the enemy
        NPC npc = enemy.GetComponent<NPC>();
        if (npc != null)
            npc.movementRadius = patrolRadius;

        Enemy enemyComp = enemy.GetComponent<Enemy>();
        if (enemyComp != null)
            enemyComp.spawnPoint = transform;

        // Subscribe to death so we can respawn.
        // Force respawnAfterDeath = false so the old tree-respawn system doesn't
        // intercept the death and hide the enemy in place instead of notifying us.
        DamageReceiver dr = enemy.GetComponentInChildren<DamageReceiver>();
        if (dr != null)
        {
            dr.respawnAfterDeath = false;
            dr.onDeath += OnEnemyDied;
        }
    }

    void OnEnemyDied()
    {
        StartCoroutine(RespawnAfterDelay());
    }

    IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        SpawnEnemy();
    }

    // Returns a random walkable position near this point, falls back to exact position
    Vector3 GetSpawnPosition()
    {
        if (spawnCount <= 1) return transform.position;

        Vector2 offset = Random.insideUnitCircle * patrolRadius * 0.4f;
        Vector3 candidate = transform.position + new Vector3(offset.x, offset.y, 0f);

        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
            return hit.position;

        return transform.position;
    }

    void OnDrawGizmos()
    {
        if (enemyPrefab == null) return;
        Gizmos.color = new Color(0f, 0.8f, 1f, 0.1f);
        Gizmos.DrawSphere(transform.position, patrolRadius);
        Gizmos.color = new Color(0f, 0.8f, 1f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, patrolRadius);
    }

    void OnDrawGizmosSelected()
    {
        // Cross marker at center
        Gizmos.color = Color.white;
        float s = 0.4f;
        Gizmos.DrawLine(transform.position - Vector3.right * s, transform.position + Vector3.right * s);
        Gizmos.DrawLine(transform.position - Vector3.up * s, transform.position + Vector3.up * s);
    }
}
