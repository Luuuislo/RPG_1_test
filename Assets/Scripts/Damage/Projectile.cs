using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 1;
    public float speed = 6f;
    public float lifeTime = 3f;
    public LayerMask damageableLayers;
    public GameObject impactPrefab;
    public GameObject impactPrefab2;
    public GameObject[] impactPrefabs;
    public Vector3 impactOffset;
    public float impactLifeTime = 1f;
    public float impactDistance = 0.1f;

    private Vector2 direction = Vector2.right;
    private Transform targetTransform;
    private bool hasHit;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (targetTransform != null)
        {
            Vector2 targetPoint = targetTransform.position;
            Vector2 nextPosition = Vector2.MoveTowards(transform.position, targetPoint, speed * Time.deltaTime);
            direction = (targetPoint - (Vector2)transform.position).normalized;
            transform.position = nextPosition;

            if (Vector2.Distance(transform.position, targetPoint) <= impactDistance)
            {
                HitTarget(targetTransform.gameObject, targetPoint);
            }

            return;
        }

        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;

        if (direction.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (direction.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    public void SetTarget(Transform target)
    {
        targetTransform = target;

        if (targetTransform != null)
        {
            SetDirection(targetTransform.position - transform.position);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        bool isInDamageableLayer = ((1 << other.gameObject.layer) & damageableLayers) != 0;
        bool isPlayer = other.GetComponentInParent<Player>() != null;

        if (!isInDamageableLayer && !isPlayer)
        {
            return;
        }

        HitTarget(other.gameObject, other.bounds.center);
    }

    private void HitTarget(GameObject target, Vector3 impactPosition)
    {
        if (hasHit) return;

        hasHit = true;

        DamageReceiver damageReceiver = target.GetComponentInParent<DamageReceiver>();
        if (damageReceiver != null)
        {
            damageReceiver.ApplyDamage(damage, true, true, direction);
        }
        else
        {
            DamageReceiverPlayer playerReceiver = target.GetComponentInParent<DamageReceiverPlayer>();
            playerReceiver?.ApplyDamage(damage, direction);
        }

        GameObject selectedImpactPrefab = GetImpactPrefab();
        if (selectedImpactPrefab != null)
        {
            GameObject impact = Instantiate(selectedImpactPrefab, impactPosition + impactOffset, Quaternion.identity);
            Destroy(impact, impactLifeTime);
        }

        Destroy(gameObject);
    }

    private GameObject GetImpactPrefab()
    {
        if (impactPrefabs != null && impactPrefabs.Length > 0)
        {
            int index = Random.Range(0, impactPrefabs.Length);
            if (impactPrefabs[index] != null)
            {
                return impactPrefabs[index];
            }
        }

        if (impactPrefab != null && impactPrefab2 != null)
        {
            return Random.Range(0, 2) == 0 ? impactPrefab : impactPrefab2;
        }

        return impactPrefab;
    }
}
