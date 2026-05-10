using UnityEngine;

public class BuildingLevel : MonoBehaviour
{
    [Header("Identity")]
    public string buildingName = "Building";
    public int    evolutionTier = 1;

    [Header("Level")]
    public int currentLevel = 1;
    public int maxLevel     = 100;

    [Header("Upgrade Cost (per level)")]
    public int goldCostPerLevel = 10;
    public int woodCostPerLevel = 5;
    public int meatCostPerLevel = 0;

    [Header("Evolution Thresholds (tier 2,3,4,5)")]
    public int[] evolutionThresholds = { 20, 40, 60, 80 };

    [Header("Evolution Prefabs (tier 2,3,4,5)")]
    public GameObject[] evolutionPrefabs = new GameObject[4];

    public bool CanUpgrade => currentLevel < maxLevel &&
                              PlayerResources.Instance != null &&
                              PlayerResources.Instance.HasEnough(UpgradeCostGold, UpgradeCostWood, UpgradeCostMeat);

    public bool CanEvolve
    {
        get
        {
            int nextTier = evolutionTier;      // 1-based
            if (nextTier >= 5) return false;
            int thresholdIdx = nextTier - 1;   // threshold[0] → evolve from tier1 to tier2
            if (thresholdIdx >= evolutionThresholds.Length) return false;
            if (nextTier - 1 >= evolutionPrefabs.Length || evolutionPrefabs[nextTier - 1] == null) return false;
            return currentLevel >= evolutionThresholds[thresholdIdx];
        }
    }

    public int UpgradeCostGold => goldCostPerLevel * currentLevel;
    public int UpgradeCostWood => woodCostPerLevel * currentLevel;
    public int UpgradeCostMeat => meatCostPerLevel * currentLevel;

    public void Upgrade()
    {
        if (!CanUpgrade) return;
        if (!PlayerResources.Instance.Spend(UpgradeCostGold, UpgradeCostWood, UpgradeCostMeat)) return;
        currentLevel++;
        BuildingSelector.Instance?.RefreshUI();
    }

    public void Evolve()
    {
        if (!CanEvolve) return;
        int prefabIdx = evolutionTier - 1;
        GameObject nextPrefab = evolutionPrefabs[prefabIdx];

        GameObject next = Instantiate(nextPrefab, transform.position, transform.rotation);

        var nextLevel = next.GetComponent<BuildingLevel>();
        if (nextLevel != null)
        {
            nextLevel.currentLevel       = currentLevel;
            nextLevel.evolutionTier      = evolutionTier + 1;
            nextLevel.evolutionThresholds = evolutionThresholds;
            nextLevel.evolutionPrefabs   = evolutionPrefabs;
            nextLevel.goldCostPerLevel   = goldCostPerLevel;
            nextLevel.woodCostPerLevel   = woodCostPerLevel;
            nextLevel.meatCostPerLevel   = meatCostPerLevel;
        }

        // Reparent to same container
        if (transform.parent != null)
            next.transform.SetParent(transform.parent);

        BuildingSelector.Instance?.Deselect();
        Destroy(gameObject);
    }
}
