using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// RPGTools > Add Stat UI
/// Adds StatsText, PointsText and the +ATK/+SPD/+HP button row to
/// BuildingInfoPanel, then re-wires BuildingUI references.
public class AddStatUI
{
    [MenuItem("RPGTools/Add Stat UI to Building Panel")]
    public static void Execute()
    {
        // ── Find panel (works even if inactive) ──────────────────
        GameObject panelGO = null;
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            if (go.name == "BuildingInfoPanel" && go.scene.isLoaded) { panelGO = go; break; }
        if (panelGO == null) { Debug.LogError("[AddStatUI] BuildingInfoPanel not found"); return; }

        // ── Expand panel height to 380 ───────────────────────────
        var rt = panelGO.GetComponent<RectTransform>();
        if (rt != null) rt.sizeDelta = new Vector2(rt.sizeDelta.x, 380);

        var bui = panelGO.GetComponent<BuildingUI>();
        if (bui == null) { Debug.LogError("[AddStatUI] BuildingUI not found on panel"); return; }

        // ── StatsText ────────────────────────────────────────────
        if (panelGO.transform.Find("StatsText") == null)
        {
            var statsGO = CreateText(panelGO, "StatsText", "ATK 0  SPD 0  HP 0  RNG 0", 11);
            var le = statsGO.AddComponent<LayoutElement>();
            le.preferredHeight = 28;
            le.minHeight       = 28;
        }

        // ── PointsText ───────────────────────────────────────────
        if (panelGO.transform.Find("PointsText") == null)
        {
            var ptGO = CreateText(panelGO, "PointsText", "Points: 0", 13);
            var le = ptGO.AddComponent<LayoutElement>();
            le.preferredHeight = 22;
            le.minHeight       = 22;
        }

        // ── StatButtonsRow ───────────────────────────────────────
        if (panelGO.transform.Find("StatButtonsRow") == null)
        {
            var row = new GameObject("StatButtonsRow");
            row.transform.SetParent(panelGO.transform, false);

            var rowRT = row.AddComponent<RectTransform>();
            rowRT.anchorMin = Vector2.zero;
            rowRT.anchorMax = Vector2.one;

            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4;
            hlg.childForceExpandWidth  = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth  = true;
            hlg.childControlHeight = true;

            var le = row.AddComponent<LayoutElement>();
            le.preferredHeight = 30;
            le.minHeight       = 30;

            CreateButtonInRow(row, "AddAtkButton", "+ATK");
            CreateButtonInRow(row, "AddSpdButton", "+SPD");
            CreateButtonInRow(row, "AddHpButton",  "+HP");
        }

        // ── Re-order: insert after CostText, before UpgradeButton ─
        ReorderChildren(panelGO);

        // ── Wire BuildingUI references ───────────────────────────
        bui.statsText = FindText(panelGO, "StatsText");
        bui.pointsText = FindText(panelGO, "PointsText");

        var row2 = panelGO.transform.Find("StatButtonsRow");
        if (row2 != null)
        {
            bui.statButtonsRow = row2.gameObject;
            bui.addAtkButton   = FindButtonInRow(row2, "AddAtkButton");
            bui.addSpdButton   = FindButtonInRow(row2, "AddSpdButton");
            bui.addHpButton    = FindButtonInRow(row2, "AddHpButton");
        }

        EditorUtility.SetDirty(panelGO);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(panelGO.scene);
        Debug.Log("[AddStatUI] Done — StatsText, PointsText and StatButtonsRow added & wired.");
    }

    // ── Helpers ──────────────────────────────────────────────────

    static GameObject CreateText(GameObject parent, string name, string content, int fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var txt = go.AddComponent<Text>();
        txt.text      = content;
        txt.fontSize  = fontSize;
        txt.color     = Color.white;
        txt.alignment = TextAnchor.MiddleLeft;
        if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return go;
    }

    static void CreateButtonInRow(GameObject row, string name, string label)
    {
        var go = new GameObject(name);
        go.transform.SetParent(row.transform, false);

        var img = go.AddComponent<Image>();
        img.color = new Color(0.25f, 0.25f, 0.35f);

        go.AddComponent<Button>();

        var labelGO = new GameObject("Text");
        labelGO.transform.SetParent(go.transform, false);

        var rt = labelGO.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var txt = labelGO.AddComponent<Text>();
        txt.text      = label;
        txt.fontSize  = 11;
        txt.color     = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;
        if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    static void ReorderChildren(GameObject panel)
    {
        // Desired order
        string[] order = {
            "NameText", "LevelText", "TierText", "CostText",
            "StatsText", "PointsText", "StatButtonsRow",
            "UpgradeButton", "EvolveButton", "MoveButton", "CloseButton"
        };
        for (int i = 0; i < order.Length; i++)
        {
            var t = panel.transform.Find(order[i]);
            if (t != null) t.SetSiblingIndex(i);
        }
    }

    static Text FindText(GameObject parent, string name)
    {
        var t = parent.transform.Find(name);
        return t != null ? t.GetComponent<Text>() : null;
    }

    static Button FindButtonInRow(Transform row, string name)
    {
        var t = row.Find(name);
        return t != null ? t.GetComponent<Button>() : null;
    }
}
