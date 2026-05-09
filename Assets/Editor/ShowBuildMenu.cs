using UnityEngine;

public class ShowBuildMenu
{
    public static void Execute()
    {
        var placer = GameObject.FindFirstObjectByType<BuildingPlacer>();
        if (placer == null) { Debug.LogError("BuildingPlacer not found"); return; }
        var go = placer.buildMenuPanel;
        if (go != null) go.SetActive(true);
        Debug.Log($"BuildMenu active: {go?.activeSelf}");
    }
}
