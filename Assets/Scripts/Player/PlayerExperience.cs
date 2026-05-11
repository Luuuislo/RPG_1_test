using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerExperience : MonoBehaviour
{
    [Header("VFX")]
    public ParticleSystem    levelUpParticles;
    public ParticleSystem    levelUpAura;
    public LevelUpFlashEffect levelUpFlash;

    [Header("Curva de experiencia")]
    [Tooltip("XP base para pasar del nivel 1 al 2")]
    public int baseXpPerLevel = 100;
    [Tooltip("1.0 = lineal (+100 por nivel) | 1.3 = curva suave | 2.0 = muy difícil")]
    public float xpExponent = 1.3f;

    [Header("Ganancia por punto de stat")]
    public int   attackGainPerPoint    = 1;
    public float speedGainPerPoint     = 0.003f;
    public int   healthGainPerPoint    = 10;
    public float atkSpdGainPerPoint    = 0.03f;
    public float critChanceGainPerPoint= 0.5f;
    public float critDmgGainPerPoint   = 0.1f;

    // ── State ────────────────────────────────────────────────────────────
    public int Level        { get; private set; } = 1;
    public int CurrentXp    { get; private set; } = 0;
    public int StatPoints   { get; private set; } = 0;
    public int XpToNextLevel => Mathf.Max(1, Mathf.RoundToInt(baseXpPerLevel * Mathf.Pow(Level, xpExponent)));

    private Player player;

    // ── DEBUG / Test manual ───────────────────────────────────────────────
    [Header("DEBUG — Test manual (teclas solo en Editor)")]
    [Tooltip("Tecla 2 en Play Mode: añade esta cantidad de XP")]
    public int debugXpAmount    = 50;
    [Tooltip("Tecla 3 en Play Mode: añade estos puntos de stat")]
    public int debugStatPoints  = 3;

    private void Awake() => player = GetComponent<Player>();

    private void Start() => RefreshAllUI();

    private void Update()
    {
#if UNITY_EDITOR
        HandleDebugInput();
#endif
    }

    // ── API pública ───────────────────────────────────────────────────────

    public void GainXp(int amount)
    {
        if (amount <= 0) return;
        CurrentXp += amount;
        while (CurrentXp >= XpToNextLevel)
        {
            CurrentXp -= XpToNextLevel;
            LevelUp();
        }
        UiManager.Instance?.UpdateExp(CurrentXp, XpToNextLevel);
    }

    public void InvestAttack()
    {
        if (StatPoints <= 0) return;
        StatPoints--;
        player.attackDamage += attackGainPerPoint;
        RefreshStatUI();
    }

    public void InvestSpeed()
    {
        if (StatPoints <= 0) return;
        StatPoints--;
        player.speed += speedGainPerPoint;
        RefreshStatUI();
    }

    public void InvestHealth()
    {
        if (StatPoints <= 0) return;
        StatPoints--;
        player.AddMaxHealth(healthGainPerPoint);
        RefreshStatUI();
    }

    public void InvestAttackSpeed()
    {
        if (StatPoints <= 0) return;
        StatPoints--;
        player.attackCooldown = Mathf.Max(0.1f, player.attackCooldown - atkSpdGainPerPoint);
        RefreshStatUI();
    }

    public void InvestCritChance()
    {
        if (StatPoints <= 0) return;
        StatPoints--;
        player.critChance += critChanceGainPerPoint;
        RefreshStatUI();
    }

    public void InvestCritDamage()
    {
        if (StatPoints <= 0) return;
        StatPoints--;
        player.critMultiplier += critDmgGainPerPoint;
        RefreshStatUI();
    }

    private void RefreshStatUI()
    {
        UiManager.Instance?.UpdateStatPanel(
            StatPoints, player.attackDamage, player.speed, player.maxHealth,
            player.attackCooldown, player.critChance, player.critMultiplier);
    }

    // ── Privado ───────────────────────────────────────────────────────────

    private void LevelUp()
    {
        Level++;
        StatPoints += 3;
        UiManager.Instance?.UpdateLevel(Level);
        UiManager.Instance?.ShowLevelUpFeedback(Level);
        RefreshStatUI();
        player.FullHeal();
        LevelUpVFX.Spawn(transform.position);
        if (levelUpParticles != null) levelUpParticles.Play();
        if (levelUpAura      != null) levelUpAura.Play();
        if (levelUpFlash     != null) levelUpFlash.Play();
    }

    private void RefreshAllUI()
    {
        UiManager.Instance?.UpdateExp(CurrentXp, XpToNextLevel);
        UiManager.Instance?.UpdateLevel(Level);
        RefreshStatUI();
    }

    // ── ContextMenu (clic derecho en el componente en Inspector) ──────────

    [ContextMenu("DEBUG: Añadir XP")]
    public void DebugAddXp() => GainXp(debugXpAmount);

    [ContextMenu("DEBUG: Forzar Level Up")]
    public void DebugForceLevelUp()
    {
        CurrentXp = 0;
        LevelUp();
        UiManager.Instance?.UpdateExp(CurrentXp, XpToNextLevel);
    }

    [ContextMenu("DEBUG: Añadir Puntos de Stat")]
    public void DebugGiveStatPoints()
    {
        StatPoints += debugStatPoints;
        RefreshStatUI();
    }

#if UNITY_EDITOR
    // 1 = Force Level Up | 2 = Add XP | 3 = Add Stat Points
    private void HandleDebugInput()
    {
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb == null) return;
        if (kb.digit1Key.wasPressedThisFrame) DebugForceLevelUp();
        if (kb.digit2Key.wasPressedThisFrame) DebugAddXp();
        if (kb.digit3Key.wasPressedThisFrame) DebugGiveStatPoints();
    }
#endif
}
