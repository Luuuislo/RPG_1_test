using UnityEngine;

public class TestBuildingSelect
{
    public static void Execute()
    {
        // Place a Tower at (0,0) for testing
        var placer = GameObject.FindFirstObjectByType<BuildingPlacer>();
        if (placer == null || placer.buildingPrefabs.Length == 0)
        { Debug.LogError("[Test] BuildingPlacer not found or no prefabs"); return; }

        var go = GameObject.Instantiate(placer.buildingPrefabs[0], Vector3.zero, Quaternion.identity);
        go.name = "TestTower";
        var parent = GameObject.Find("---BUILDS---");
        if (parent != null) go.transform.SetParent(parent.transform);

        // Select it via BuildingSelector
        var bl = go.GetComponent<BuildingLevel>();
        if (bl == null) { Debug.LogError("[Test] BuildingLevel not found on placed building"); return; }

        var selector = GameObject.FindFirstObjectByType<BuildingSelector>();
        if (selector == null) { Debug.LogError("[Test] BuildingSelector not found"); return; }

        selector.Select(bl);
        Debug.Log($"[Test] Selected '{bl.buildingName}' Lv{bl.currentLevel} Tier{bl.evolutionTier} | CanUpgrade:{bl.CanUpgrade} | CanEvolve:{bl.CanEvolve}");
    }
}
