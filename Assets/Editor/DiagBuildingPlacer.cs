using UnityEngine;

public class DiagBuildingPlacer
{
    public static void Execute()
    {
        var placer = GameObject.FindFirstObjectByType<BuildingPlacer>();
        if (placer == null) { Debug.LogError("[Diag] BuildingPlacer not found"); return; }

        Debug.Log($"[Diag] buildMenuPanel == null (Unity ==): {placer.buildMenuPanel == null}");
        Debug.Log($"[Diag] buildMenuPanel is null (C# ReferenceEquals): {object.ReferenceEquals(placer.buildMenuPanel, null)}");
        Debug.Log($"[Diag] buildMenuPanel object name: {(placer.buildMenuPanel != null ? placer.buildMenuPanel.name : "NULL")}");
        Debug.Log($"[Diag] buildingPrefabs count: {(placer.buildingPrefabs != null ? placer.buildingPrefabs.Length.ToString() : "NULL")}");

        var panel = GameObject.Find("BackGroundMenuBuild");
        Debug.Log($"[Diag] Find('BackGroundMenuBuild') result: {(panel != null ? panel.name : "NOT FOUND")}");
    }
}
