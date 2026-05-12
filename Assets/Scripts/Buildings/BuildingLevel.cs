using UnityEngine;

public enum StatType { Atk, AtkSpeed, Hp }

public class BuildingLevel : MonoBehaviour
{
    [Header("Identity")]
    public string buildingName  = "Building";
    public int    evolutionTier = 1;

    [Header("Level")]
    public int currentLevel    = 1;
    public int maxLevel        = 100;
    public int pointsPerLevelUp = 3;

    [Header("Upgrade Cost (per level)")]
    public int goldCostPerLevel = 10;
    public int woodCostPerLevel = 5;
    public int meatCostPerLevel = 0;

    [Header("Evolution Thresholds (tier 2,3,4,5)")]
    public int[] evolutionThresholds = { 20, 40, 60, 80 };

    [Header("Evolution Prefabs (tier 2,3,4,5)")]
    public GameObject[] evolutionPrefabs = new GameObject[4];

    // ── Base Stats ─────────────────────────────────────────────
    [Header("Base Stats")]
    public float baseAtk      = 10f;
    public float baseAtkSpeed = 1f;
    public float baseHp       = 100f;
    public float baseRange    = 5f;

    [Header("Stat Gain per Point")]
    public float atkPerPoint      = 1f;
    public float atkSpeedPerPoint = 0.1f;
    public float hpPerPoint       = 10f;

    [Header("Evolution Bonuses")]
    [Tooltip("% bonus applied to current stats on each evolution (tier1→2, 2→3, 3→4, 4→5)")]
    public float[] evolutionBonusPercent    = { 20f, 40f, 60f, 80f };
    public float   rangeIncreasePerEvolution = 2f;

    // ── Runtime Stats (read-only in inspector) ─────────────────
    [Header("Runtime Stats")]
    public float currentAtk;
    public float currentAtkSpeed;
    public float currentHp;
    public float currentRange;
    public int   pendingPoints;

    [Header("Spent Points")]
    public int spentOnAtk;
    public int spentOnAtkSpeed;
    public int spentOnHp;

    // ── Computed Properties ────────────────────────────────────
    public bool CanUpgrade => currentLevel < maxLevel &&
                              PlayerResources.Instance != null &&
                              PlayerResources.Instance.HasEnough(UpgradeCostGold, UpgradeCostWood, UpgradeCostMeat);

    public bool CanEvolve
    {
        get
        {
            int nextTier = evolutionTier;
            if (nextTier >= 5) return false;
            int tIdx = nextTier - 1;
            if (tIdx >= evolutionThresholds.Length) return false;
            if (tIdx >= evolutionPrefabs.Length || evolutionPrefabs[tIdx] == null) return false;
            return currentLevel >= evolutionThresholds[tIdx];
        }
    }

    public int UpgradeCostGold => goldCostPerLevel * currentLevel;
    public int UpgradeCostWood => woodCostPerLevel * currentLevel;
    public int UpgradeCostMeat => meatCostPerLevel * currentLevel;

    // ── Lifecycle ──────────────────────────────────────────────
    void Awake()
    {
        // Only initialise if stats haven't been set by an Evolve() call
        if (currentAtk == 0f && currentHp == 0f)
        {
            currentAtk      = baseAtk;
            currentAtkSpeed = baseAtkSpeed;
            currentHp       = baseHp;
            currentRange    = baseRange;
        }
    }

    void Start()
    {
        GetComponent<BuildingHealthBar>()?.UpdateLevel(currentLevel);
    }

    // ── Actions ────────────────────────────────────────────────
    public void Upgrade()
    {
        if (!CanUpgrade) return;
        if (!PlayerResources.Instance.Spend(UpgradeCostGold, UpgradeCostWood, UpgradeCostMeat)) return;
        currentLevel++;
        pendingPoints += pointsPerLevelUp;
        GetComponent<BuildingAttack>()?.SyncStats(this);
        GetComponent<BuildingHealthBar>()?.UpdateLevel(currentLevel);
        BuildingSelector.Instance?.RefreshUI();
        LevelUpVFX.Spawn(transform.position);
    }

    public bool SpendPoint(StatType stat)
    {
        if (pendingPoints <= 0) return false;
        switch (stat)
        {
            case StatType.Atk:
                spentOnAtk++;
                currentAtk += atkPerPoint;
                break;
            case StatType.AtkSpeed:
                spentOnAtkSpeed++;
                currentAtkSpeed += atkSpeedPerPoint;
                break;
            case StatType.Hp:
                spentOnHp++;
                currentHp += hpPerPoint;
                break;
            default: return false;
        }
        pendingPoints--;
        GetComponent<BuildingAttack>()?.SyncStats(this);
        BuildingSelector.Instance?.RefreshUI();
        return true;
    }

    public void Evolve()
    {
        if (!CanEvolve) return;
        int prefabIdx  = evolutionTier - 1;
        int bonusIdx   = evolutionTier - 1;   // tier1→tier2 → bonusPercent[0] = 20%
        float bonusMult = 1f + (bonusIdx < evolutionBonusPercent.Length
                                ? evolutionBonusPercent[bonusIdx] / 100f
                                : 0f);

        GameObject next = Instantiate(evolutionPrefabs[prefabIdx], transform.position, transform.rotation);
        var nbl = next.GetComponent<BuildingLevel>();
        if (nbl != null)
        {
            // Identity & level
            nbl.currentLevel        = currentLevel;
            nbl.evolutionTier       = evolutionTier + 1;
            nbl.evolutionThresholds = evolutionThresholds;
            nbl.evolutionPrefabs    = evolutionPrefabs;
            nbl.maxLevel            = maxLevel;
            nbl.pointsPerLevelUp    = pointsPerLevelUp;
            nbl.goldCostPerLevel    = goldCostPerLevel;
            nbl.woodCostPerLevel    = woodCostPerLevel;
            nbl.meatCostPerLevel    = meatCostPerLevel;

            // Stat config
            nbl.baseAtk              = baseAtk;
            nbl.baseAtkSpeed         = baseAtkSpeed;
            nbl.baseHp               = baseHp;
            nbl.atkPerPoint          = atkPerPoint;
            nbl.atkSpeedPerPoint     = atkSpeedPerPoint;
            nbl.hpPerPoint           = hpPerPoint;
            nbl.evolutionBonusPercent = evolutionBonusPercent;
            nbl.rangeIncreasePerEvolution = rangeIncreasePerEvolution;

            // Carry over spent points
            nbl.spentOnAtk      = spentOnAtk;
            nbl.spentOnAtkSpeed = spentOnAtkSpeed;
            nbl.spentOnHp       = spentOnHp;
            nbl.pendingPoints   = pendingPoints;

            // Apply evolution bonus to current stats (Awake already ran, we override)
            nbl.currentAtk      = currentAtk      * bonusMult;
            nbl.currentAtkSpeed = currentAtkSpeed * bonusMult;
            nbl.currentHp       = currentHp       * bonusMult;
            nbl.currentRange    = currentRange + rangeIncreasePerEvolution;
        }

        if (transform.parent != null)
            next.transform.SetParent(transform.parent);

        // Apply evolved stats to BuildingAttack immediately (Start() may not have run yet)
        if (nbl != null)
            next.GetComponent<BuildingAttack>()?.SyncStats(nbl);

        BuildingSelector.Instance?.Deselect();
        Destroy(gameObject);
    }
}
