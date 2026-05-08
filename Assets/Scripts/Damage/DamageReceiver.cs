using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageReceiver : MonoBehaviour
{

    private Rigidbody2D rb2d;

    [Header("Hit Reaction")]
    public bool applyForceOnDamage = true;
    public bool freezeRotation = true;
    public float forceImpulse = 5f;

    [Header("Stats")]
    public int maxHealth = 1;
    private int currentHealth;

    [Header("Health Bar")]
    public Slider healthBarSlider;

    [Header("XP Reward")]
    public int xpReward = 10;

    [Header("Drop")]
    public DropEntry[] itemDrops;
    public DropDisplaySettings dropDisplaySettings;

    [Header("Stump")]
    public GameObject stumpPrefab;
    public Vector3 stumpOffset;

    [Header("Respawn")]
    public bool respawnAfterDeath;
    public float respawnDelay = 10f;

    public System.Action onDeath;

    private Animator animator;
    private SpriteRenderer[] spriteRenderers;
    private Collider2D[] colliders;
    private GameObject spawnedStump;
    private bool isDead;

    private static PlayerExperience _cachedPlayerExp;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        colliders = GetComponentsInChildren<Collider2D>();

        currentHealth = maxHealth;

        if (rb2d != null && freezeRotation)
            rb2d.freezeRotation = true;

        if (healthBarSlider != null)
            healthBarSlider.value = 1f;
    }

    public void ApplyDamage(int amount, bool applyForceOrNot, bool applyHitAnimation, Vector2 hitDirection)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);

        if (healthBarSlider != null)
            healthBarSlider.value = (float)currentHealth / maxHealth;

        if (applyForceOrNot && applyForceOnDamage && rb2d != null)
        {
            if (freezeRotation)
            {
                rb2d.freezeRotation = true;
            }

            rb2d.bodyType = RigidbodyType2D.Dynamic;
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
            rb2d.AddForce(hitDirection.normalized * forceImpulse, ForceMode2D.Impulse);
            Invoke(nameof(ReturnToKinematic), 0.2f);
        }


        if (applyHitAnimation)
        {
            
        }
        if (currentHealth <= 0)
        {
            isDead = true;
            GrantXpToPlayer();
            DropItem();
            SpawnStump();

            if (respawnAfterDeath)
                StartCoroutine(RespawnAfterDelay());
            else
                Die();
        }
    }

    private void GrantXpToPlayer()
    {
        if (xpReward <= 0) return;
        if (_cachedPlayerExp == null)
            _cachedPlayerExp = FindFirstObjectByType<PlayerExperience>();
        _cachedPlayerExp?.GainXp(xpReward);
    }

    void ReturnToKinematic()
    {
        if (rb2d == null) return;

        rb2d.linearVelocity = Vector2.zero;
        rb2d.angularVelocity = 0f;
        rb2d.bodyType = RigidbodyType2D.Kinematic;
    }

    void DropItem()
    {
        if (itemDrops == null) return;
        foreach (DropEntry drop in itemDrops)
        {
            if (drop.prefab == null) continue;
            if (Random.value > drop.dropChance) continue;

            int amount = Random.Range(drop.minAmount, drop.maxAmount + 1);
            Vector2 scatter = Random.insideUnitCircle * 0.4f;
            Vector3 pos = transform.position + new Vector3(scatter.x, scatter.y + 0.5f, 0f);

            GameObject spawned = Instantiate(drop.prefab, pos, Quaternion.identity);

            // Support both pickup systems: ItemPickup (new) and DroppedItem (legacy)
            ItemPickup pickup = spawned.GetComponent<ItemPickup>();
            if (pickup != null)
                pickup.quantity = amount;
            else
            {
                DroppedItem dropped = spawned.GetComponent<DroppedItem>();
                if (dropped != null) dropped.quantity = amount;
            }

            // Visual pile + quantity label (Tibia-style)
            DropStackDisplay display = spawned.AddComponent<DropStackDisplay>();
            display.Initialize(drop.prefab, amount, dropDisplaySettings);
        }
    }

    void SpawnStump()
    {
        if (stumpPrefab != null)
        {
            spawnedStump = Instantiate(stumpPrefab, transform.position + stumpOffset, transform.rotation);
        }
    }

    IEnumerator RespawnAfterDelay()
    {
        SetTreeActive(false);

        yield return new WaitForSeconds(respawnDelay);

        if (spawnedStump != null)
        {
            Destroy(spawnedStump);
        }

        currentHealth = maxHealth;
        isDead = false;
        if (healthBarSlider != null) healthBarSlider.value = 1f;
        SetTreeActive(true);
    }

    void SetTreeActive(bool isActive)
    {
        if (animator != null)
        {
            animator.enabled = isActive;
        }

        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.enabled = isActive;
        }

        foreach (Collider2D collider in colliders)
        {
            collider.enabled = isActive;
        }

        if (rb2d != null)
        {
            CancelInvoke(nameof(ReturnToKinematic));
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
            rb2d.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    void Die()
    {
        onDeath?.Invoke();
        Destroy(gameObject);
    }
}

[System.Serializable]
public class DropEntry
{
    public GameObject prefab;
    [Min(1)] public int minAmount = 1;
    [Min(1)] public int maxAmount = 1;
    [Range(0f, 1f)] public float dropChance = 1f;
}
