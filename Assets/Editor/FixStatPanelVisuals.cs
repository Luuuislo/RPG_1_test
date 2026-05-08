using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class FixStatPanelVisuals
{
    // ── Layout constants ─────────────────────────────────────────────────
    const float LABEL_X    = -68f;
    const float LABEL_W    =  58f;
    const float VALUE_X    =  -3f;
    const float VALUE_W    =  60f;
    const float BTN_X      =  55f;
    const float BTN_W      =  36f;
    const float ROW_H      =  18f;
    const float ROW_STEP   = -22f;
    const float FIRST_ROW_Y=  20f;

    static readonly Color LabelColor = new Color(0.87f, 0.78f, 0.50f); // warm gold
    static readonly Color ValueColor = Color.white;
    static readonly Color BtnColor   = Color.white;

    static readonly (string labelText, string valueGoName, string btnGoName)[] Rows =
    {
        ("ATK",     "AtkValue",        "AtkButton"),
        ("SPD",     "SpdValue",        "SpdButton"),
        ("HP MAX",  "HpValue",         "HpButton"),
        ("ATK SPD", "AtkSpdValue",     "AtkSpdButton"),
        ("CRIT %",  "CritChanceValue", "CritChanceButton"),
        ("CRIT DMG","CritDmgValue",    "CritDmgButton"),
    };

    public static void Execute()
    {
        var panel = FindByPath("---UI---/Canvas/++Player++/StatsPanel");
        if (panel == null) { Debug.LogError("[FixStatPanelVisuals] StatsPanel not found."); return; }

        // ── Pts header ───────────────────────────────────────────────────
        StylePtsText(panel);

        // ── Rows ─────────────────────────────────────────────────────────
        for (int i = 0; i < Rows.Length; i++)
        {
            float rowY = FIRST_ROW_Y + i * ROW_STEP;
            var (labelText, valueGoName, btnGoName) = Rows[i];

            EnsureLabel(panel, valueGoName + "_Label", labelText, rowY);
            StyleValue(panel, valueGoName, rowY);
            StyleButton(panel, btnGoName, rowY);
        }

        // ── Resize panel to fit all rows ─────────────────────────────────
        var rt = panel.GetComponent<RectTransform>();
        float topPad = 28f;   // space for Pts header
        float botPad = 10f;
        float contentH = Mathf.Abs(ROW_STEP) * Rows.Length + ROW_H;
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, topPad + contentH + botPad);

        // Shift panel anchor/pivot so it grows downward from top-left
        // (no change to anchor — keep whatever the designer set)

        EditorUtility.SetDirty(panel);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(panel.scene);
        Debug.Log("[FixStatPanelVisuals] Done — save scene (Ctrl+S).");
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    static void StylePtsText(GameObject panel)
    {
        var t = panel.transform.Find("StatPointsText");
        if (t == null) return;
        var rt = t.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0f, 33f);
        rt.sizeDelta = new Vector2(140f, 18f);
        var tmp = t.GetComponent<TextMeshProUGUI>();
        if (tmp == null) return;
        tmp.fontSize  = 12;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = new Color(1f, 0.95f, 0.6f);
    }

    static void EnsureLabel(GameObject panel, string labelGoName, string text, float rowY)
    {
        // Remove old multi-line label if it exists
        var oldLabel = panel.transform.Find(labelGoName);
        if (oldLabel != null) Object.DestroyImmediate(oldLabel.gameObject);

        var go = new GameObject(labelGoName);
        go.transform.SetParent(panel.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(LABEL_X, rowY);
        rt.sizeDelta = new Vector2(LABEL_W, ROW_H);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = 10;
        tmp.fontStyle = FontStyles.Normal;
        tmp.alignment = TextAlignmentOptions.MidlineGeoAligned;
        tmp.color     = LabelColor;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Overflow;
    }

    static void StyleValue(GameObject panel, string goName, float rowY)
    {
        var t = panel.transform.Find(goName);
        if (t == null) return;
        var rt = t.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(VALUE_X, rowY);
        rt.sizeDelta = new Vector2(VALUE_W, ROW_H);
        var tmp = t.GetComponent<TextMeshProUGUI>();
        if (tmp == null) return;
        tmp.fontSize  = 11;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Midline;
        tmp.color     = ValueColor;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Overflow;
    }

    static void StyleButton(GameObject panel, string goName, float rowY)
    {
        var t = panel.transform.Find(goName);
        if (t == null) return;
        var rt = t.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(BTN_X, rowY);
        rt.sizeDelta = new Vector2(BTN_W, ROW_H);

        // Fix button child text (may be legacy Text or TMP)
        var textChild = t.Find("Text");
        if (textChild != null)
        {
            // Remove legacy Text if present
            var legacyText = textChild.GetComponent<Text>();
            if (legacyText != null) Object.DestroyImmediate(legacyText);

            var tmp = textChild.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = textChild.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text      = "+";
            tmp.fontSize  = 14;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = BtnColor;

            var btnRt = textChild.GetComponent<RectTransform>();
            btnRt.anchorMin  = Vector2.zero;
            btnRt.anchorMax  = Vector2.one;
            btnRt.offsetMin  = Vector2.zero;
            btnRt.offsetMax  = Vector2.zero;
        }
    }

    static GameObject FindByPath(string path)
    {
        var parts = path.Split('/');
        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        GameObject cur = null;
        foreach (var r in roots) if (r.name == parts[0]) { cur = r; break; }
        if (cur == null) return null;
        for (int i = 1; i < parts.Length; i++)
        {
            var c = cur.transform.Find(parts[i]);
            if (c == null) return null;
            cur = c.gameObject;
        }
        return cur;
    }
}
