using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class WireBuildingUI
{
    public static void Execute()
    {
        // --- Set button labels ---
        SetLabelText("UpgradeButton/Text", "Upgrade");
        SetLabelText("EvolveButton/Text",  "Evolve!");
        SetLabelText("MoveButton/Text",    "Move");
        SetLabelText("CloseButton/Text",   "X");

        // --- Add BuildingUI script to panel (search includes inactive) ---
        GameObject panelGO = null;
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            if (go.name == "BuildingInfoPanel" && go.scene.isLoaded) { panelGO = go; break; }
        if (panelGO == null) { Debug.LogError("[WireBuildingUI] BuildingInfoPanel not found"); return; }

        var bui = panelGO.GetComponent<BuildingUI>() ?? panelGO.AddComponent<BuildingUI>();

        // Wire panel self-reference
        bui.panel = panelGO;

        // Wire text fields
        bui.nameText  = FindText(panelGO, "NameText");
        bui.levelText = FindText(panelGO, "LevelText");
        bui.tierText  = FindText(panelGO, "TierText");
        bui.costText  = FindText(panelGO, "CostText");

        // Wire buttons
        bui.upgradeButton = FindButton(panelGO, "UpgradeButton");
        bui.evolveButton  = FindButton(panelGO, "EvolveButton");
        bui.moveButton    = FindButton(panelGO, "MoveButton");
        bui.closeButton   = FindButton(panelGO, "CloseButton");

        // Hide panel by default
        panelGO.SetActive(false);

        // --- Add BuildingSelector to BuildingPlacer ---
        var placer = GameObject.FindFirstObjectByType<BuildingPlacer>();
        if (placer == null) { Debug.LogError("[WireBuildingUI] BuildingPlacer not found"); return; }

        var selector = placer.GetComponent<BuildingSelector>() ?? placer.gameObject.AddComponent<BuildingSelector>();
        selector.buildingUI = bui;
        selector.placer     = placer;

        // Mark dirty
        EditorUtility.SetDirty(panelGO);
        EditorUtility.SetDirty(placer.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(placer.gameObject.scene);

        Debug.Log("[WireBuildingUI] Done.");
    }

    static void SetLabelText(string path, string text)
    {
        // Find in inactive too
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (!go.scene.isLoaded) continue;
            if (go.name != System.IO.Path.GetFileName(path)) continue;
            var t = go.GetComponent<Text>();
            if (t != null) { t.text = text; return; }
        }
        Debug.LogWarning($"[WireBuildingUI] Text not found: {path}");
    }

    static Text FindText(GameObject parent, string name)
    {
        var t = parent.transform.Find(name);
        return t != null ? t.GetComponent<Text>() : null;
    }

    static Button FindButton(GameObject parent, string name)
    {
        var t = parent.transform.Find(name);
        return t != null ? t.GetComponent<Button>() : null;
    }
}
