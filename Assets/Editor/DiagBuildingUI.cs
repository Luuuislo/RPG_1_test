using UnityEngine;

public class DiagBuildingUI
{
    public static void Execute()
    {
        var selector = GameObject.FindFirstObjectByType<BuildingSelector>();
        if (selector == null) { Debug.LogError("[DiagBUI] BuildingSelector not found"); return; }
        Debug.Log($"[DiagBUI] Selector found. buildingUI={selector.buildingUI != null} placer={selector.placer != null}");

        if (selector.buildingUI != null)
        {
            Debug.Log($"[DiagBUI] BuildingUI panel={selector.buildingUI.panel != null} nameText={selector.buildingUI.nameText != null}");
            // Force show
            var bl = GameObject.FindFirstObjectByType<BuildingLevel>();
            if (bl != null)
            {
                selector.Select(bl);
                Debug.Log($"[DiagBUI] Panel active after Select: {selector.buildingUI.panel?.activeSelf}");
            }
        }
        else
        {
            // Try to find BuildingUI any other way
            var bui = GameObject.FindFirstObjectByType<BuildingUI>();
            Debug.Log($"[DiagBUI] FindFirstObjectOfType<BuildingUI>: {bui != null}");
        }
    }
}
