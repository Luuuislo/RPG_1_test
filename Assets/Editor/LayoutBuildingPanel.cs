using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class LayoutBuildingPanel
{
    public static void Execute()
    {
        GameObject panelGO = null;
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            if (go.name == "BuildingInfoPanel" && go.scene.isLoaded) { panelGO = go; break; }
        if (panelGO == null) { Debug.LogError("[Layout] BuildingInfoPanel not found"); return; }

        // Size and position the panel (bottom-right area)
        var rt = panelGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot     = new Vector2(1, 0);
        rt.anchoredPosition = new Vector2(-10, 10);
        rt.sizeDelta = new Vector2(200, 280);

        // Configure VerticalLayoutGroup
        var vlg = panelGO.GetComponent<VerticalLayoutGroup>();
        vlg.padding     = new RectOffset(8, 8, 8, 8);
        vlg.spacing     = 4;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlHeight     = true;
        vlg.childControlWidth      = true;

        // Give each child a LayoutElement with preferred height
        SetHeight(panelGO, "NameText",      28);
        SetHeight(panelGO, "LevelText",     22);
        SetHeight(panelGO, "TierText",      22);
        SetHeight(panelGO, "CostText",      22);
        SetHeight(panelGO, "UpgradeButton", 30);
        SetHeight(panelGO, "EvolveButton",  30);
        SetHeight(panelGO, "MoveButton",    30);
        SetHeight(panelGO, "CloseButton",   30);

        // Set text font sizes
        SetFontSize(panelGO, "NameText",  16);
        SetFontSize(panelGO, "LevelText", 14);
        SetFontSize(panelGO, "TierText",  14);
        SetFontSize(panelGO, "CostText",  12);

        EditorUtility.SetDirty(panelGO);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(panelGO.scene);
        Debug.Log("[Layout] BuildingInfoPanel layout configured.");
    }

    static void SetHeight(GameObject parent, string childName, float h)
    {
        var t = parent.transform.Find(childName);
        if (t == null) return;
        var le = t.GetComponent<LayoutElement>() ?? t.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = h;
        le.minHeight = h;
    }

    static void SetFontSize(GameObject parent, string childName, int size)
    {
        var t = parent.transform.Find(childName);
        if (t == null) return;
        var txt = t.GetComponent<Text>();
        if (txt != null) txt.fontSize = size;
    }
}
