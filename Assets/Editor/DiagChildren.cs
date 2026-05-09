using UnityEngine;

public class DiagChildren
{
    public static void Execute()
    {
        var placer = GameObject.FindFirstObjectByType<BuildingPlacer>();
        if (placer == null) { Debug.LogError("[DiagChildren] BuildingPlacer not found"); return; }

        var panel = placer.buildMenuPanel.transform;
        Debug.Log($"[DiagChildren] Panel: '{panel.name}' childCount: {panel.childCount}");

        for (int i = 0; i < panel.childCount; i++)
        {
            var child = panel.GetChild(i);
            Debug.Log($"[DiagChildren]   Child[{i}]: '{child.name}' active:{child.gameObject.activeSelf} childCount:{child.childCount}");

            // Also list grandchildren
            for (int j = 0; j < child.childCount; j++)
            {
                var gc = child.GetChild(j);
                Debug.Log($"[DiagChildren]     GrandChild[{j}]: '{gc.name}' active:{gc.gameObject.activeSelf}");
            }
        }
    }
}
