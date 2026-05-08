using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public float speed = 5f;

    Rigidbody2D rb2d;
    Vector2 movementInput;
    Animator animator;

    private int currentHealth;
    public  int maxHealth = 100;

    private PlayerHitEffect hitEffect;
    private bool gameIsPaused = false;

    private bool isAttacking = false;
    private bool isGuarding = false;
    private bool canMove = true;
    private float ignoreAttackInputUntil;

    Vector2 attackDir = Vector2.right;
    public int attackDamage = 1;
    public float attackRange = 1.2f;
    [FormerlySerializedAs("targetLayer")]
    public LayerMask damageableLayers;
    public GameObject floatingDamageTextPrefab;



    void Start()
    {
        rb2d      = GetComponent<Rigidbody2D>();
        animator  = GetComponent<Animator>();
        hitEffect = GetComponent<PlayerHitEffect>();
        if (floatingDamageTextPrefab == null)
            floatingDamageTextPrefab = Resources.Load<GameObject>("FloatingDamageText");
        currentHealth = maxHealth;
        ignoreAttackInputUntil = Time.time + 0.2f;
        animator.ResetTrigger("DoAttack");
        animator.SetBool("IsGuarding", false);
        animator.SetFloat("Speed", 0f);
        UiManager.Instance?.UpdateHealth(currentHealth, maxHealth);
    }

    void Update()
    {
        HandleInventoryInput();
        OpenClosePauseMenu();
        DebugInput();
        Guard();
        Attack();

        if (isAttacking || isGuarding)
        {
            canMove = false;
            movementInput = Vector2.zero;
            animator.SetFloat("Speed", 0f);
            return;
        }

        canMove = true;

        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard == null) return;

        movementInput = Vector2.zero;

        if (keyboard.wKey.isPressed) movementInput.y += 1;
        if (keyboard.sKey.isPressed) movementInput.y -= 1;
        if (keyboard.aKey.isPressed) movementInput.x -= 1;
        if (keyboard.dKey.isPressed) movementInput.x += 1;

        movementInput = movementInput.normalized;
        if (movementInput != Vector2.zero)
        {
            attackDir = movementInput;
        }

        float speedValue = movementInput.magnitude;
        if (speedValue < 0.05f) speedValue = 0f;

        animator.SetFloat("Speed", speedValue);

        if (movementInput.x != 0)
        {
            Vector3 scale = transform.localScale;

            scale.x = movementInput.x > 0
                ? Mathf.Abs(scale.x)
                : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

    }

    void FixedUpdate()
    {
        if (canMove)
        {
        rb2d.linearVelocity = movementInput * speed;
            
        } else
        {
            rb2d.linearVelocity = Vector2.zero;
        }
    }

    void DebugInput()
    {
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard != null && keyboard.digit9Key.wasPressedThisFrame)
            TakeDamage(10);
    }

    void HandleInventoryInput()
    {
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard != null && keyboard.iKey.wasPressedThisFrame)
        {
            UiManager.Instance?.OpenOrCloseInventory();
        }
    }

    void OpenClosePauseMenu ()
    {
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard != null && keyboard.pKey.wasPressedThisFrame)
        {
            if (gameIsPaused)
            {
                UiManager.Instance?.ResumeGame();
                gameIsPaused = false;
            }
            else
            {
                UiManager.Instance?.PauseGame();
                gameIsPaused = true;
            }
        }
    }

    void Attack ()
    {
        if (Time.time < ignoreAttackInputUntil) return;

        var mouse = UnityEngine.InputSystem.Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame && !isAttacking && !isGuarding)
        {
            isAttacking = true;
            canMove = false;
            movementInput = Vector2.zero;
            rb2d.linearVelocity = Vector2.zero;
            animator.SetFloat("Speed", 0f);

            int randomIndex = Random.Range(0, 2);
            animator.SetInteger("AttackIndex", randomIndex);
            animator.SetTrigger("DoAttack");
        }
    }

    void Guard()
    {
        var mouse = UnityEngine.InputSystem.Mouse.current;
        isGuarding = mouse != null && mouse.rightButton.isPressed && !isAttacking;
        animator.SetBool("IsGuarding", isGuarding);
    }

    public void StartAttack()
    {
        isAttacking = true;
    }

    public void EndAttack ()
    {
        isAttacking = false;
    }

    public int GetCurrentHealth() => currentHealth;

    public void AddMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UiManager.Instance?.UpdateHealth(currentHealth, maxHealth);
    }

    public void FullHeal()
    {
        currentHealth = maxHealth;
        UiManager.Instance?.UpdateHealth(currentHealth, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Max(0, currentHealth - amount);
        UiManager.Instance?.UpdateHealth(currentHealth, maxHealth);
        SpawnDamageText(amount);
        hitEffect?.Play();
    }

    private void SpawnDamageText(int amount)
    {
        if (floatingDamageTextPrefab == null) return;
        Vector3 spawnPos = transform.position + new Vector3(Random.Range(-0.3f, 0.3f), 0.8f, 0f);
        GameObject obj = Instantiate(floatingDamageTextPrefab, spawnPos, Quaternion.identity);
        obj.GetComponent<FloatingDamageText>()?.Setup(amount);
    }

    public void DetectAndDamageTarget()
    {
        Vector2 attackPoint = (Vector2)transform.position + attackDir.normalized * attackRange * 0.5f;
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(attackPoint, attackRange, damageableLayers);
        foreach (Collider2D target in hitTargets)
        {
            DamageReceiver damageReceiver = target.GetComponentInParent<DamageReceiver>();
            if (damageReceiver != null)
            {
                damageReceiver.ApplyDamage(attackDamage, true, true, attackDir);
                continue;
            }

        }
    }

    void OnDrawGizmosSelected()
    {
        Vector2 direction = attackDir == Vector2.zero ? Vector2.right : attackDir.normalized;
        Vector2 attackPoint = (Vector2)transform.position + direction * attackRange * 0.5f;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint, attackRange);
    }

}
