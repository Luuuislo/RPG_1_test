using UnityEngine;
using UnityEngine.UI;

public class TestBuildMenu
{
    public static void Execute()
    {
        var placer = GameObject.FindFirstObjectByType<BuildingPlacer>();
        if (placer == null) { Debug.LogError("[TestBuild] BuildingPlacer not found"); return; }

        // Check panel active state
        bool panelActive = placer.buildMenuPanel.activeSelf;
        Debug.Log($"[TestBuild] Panel active before toggle: {panelActive}");

        // Toggle the panel via reflection to mimic B press
        placer.buildMenuPanel.SetActive(!panelActive);
        Debug.Log($"[TestBuild] Panel active after toggle: {placer.buildMenuPanel.activeSelf}");

        // Check buttons in slots
        var slots = new System.Collections.Generic.List<Transform>();
        Transform root = placer.buildMenuPanel.transform.Find("--Icons--") ?? placer.buildMenuPanel.transform;
        foreach (Transform child in root)
            if (child.name.StartsWith("BuildMenuBackground")) slots.Add(child);

        Debug.Log($"[TestBuild] Slot count: {slots.Count}");
        for (int i = 0; i < slots.Count; i++)
        {
            var btn = slots[i].GetComponent<Button>();
            var tmp = slots[i].GetComponentInChildren<TMPro.TextMeshProUGUI>();
            Debug.Log($"[TestBuild] Slot[{i}] '{slots[i].name}' | Button: {btn != null} | Label: {(tmp != null ? tmp.text : "NO TMP")}");
        }
    }
}
