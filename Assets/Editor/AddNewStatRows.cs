using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class AddNewStatRows
{
    const string PANEL_PATH = "---UI---/Canvas/++Player++/StatsPanel";
    const string BUTTON_SPRITE = "Assets/Sprites/Tiny Swords (Free Pack)/UI Elements/UI Elements/Buttons/SmallBlueSquareButton_Pressed.png";

    // Layout constants matching existing rows
    const float VALUE_X   = -36.10f;
    const float BUTTON_X  =  56.62f;
    const float VALUE_W   =  94.23f;
    const float BTN_W     =  42.04f;
    const float ROW_H     =  19.66f;
    const float ROW_STEP  = -22.47f;

    // Y positions: ATK=20.12, SPD=-2.35, HP=-24.82 → new rows continue from HP
    // Row index 0=ATK,1=SPD,2=HP,3=ATKSPD,4=CRIT%,5=CRITDMG
    static readonly (string label, string valueGoName, string btnGoName)[] NewRows =
    {
        ("ATK\nSPD", "AtkSpdValue",   "AtkSpdButton"),
        ("CRIT\n%",  "CritChanceValue","CritChanceButton"),
        ("CRIT\nDMG","CritDmgValue",  "CritDmgButton"),
    };

    public static void Execute()
    {
        var panelGo = GameObject.Find(PANEL_PATH.Replace("/", "/"));
        // Use manual search
        panelGo = FindByPath(PANEL_PATH);
        if (panelGo == null) { Debug.LogError("[AddNewStatRows] StatsPanel not found."); return; }

        var panel = panelGo.GetComponent<RectTransform>();

        // Fix existing button texts (legacy Text → TMP)
        FixButtonTexts(panelGo);

        // Remove previously created rows if re-running
        foreach (var (_, valueGoName, btnGoName) in NewRows)
        {
            var existing = panelGo.transform.Find(valueGoName);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);
            existing = panelGo.transform.Find(btnGoName);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);
        }

        var buttonSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BUTTON_SPRITE);

        float baseY = 20.12f + 2 * ROW_STEP; // HP row Y
        for (int i = 0; i < NewRows.Length; i++)
        {
            float rowY = baseY + (i + 1) * ROW_STEP;
            var (label, valueGoName, btnGoName) = NewRows[i];

            // Value text
            var valueGo = new GameObject(valueGoName);
            valueGo.transform.SetParent(panelGo.transform, false);
            var rt = valueGo.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(VALUE_X, rowY);
            rt.sizeDelta = new Vector2(VALUE_W, ROW_H);
            var tmp = valueGo.AddComponent<TextMeshProUGUI>();
            tmp.text = "—";
            tmp.fontSize = 12;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            // Label text (left of value)
            var labelGo = new GameObject(valueGoName + "_Label");
            labelGo.transform.SetParent(panelGo.transform, false);
            var labelRt = labelGo.AddComponent<RectTransform>();
            labelRt.anchorMin = labelRt.anchorMax = new Vector2(0.5f, 0.5f);
            labelRt.pivot = new Vector2(0.5f, 0.5f);
            labelRt.anchoredPosition = new Vector2(VALUE_X - 54f, rowY);
            labelRt.sizeDelta = new Vector2(50f, ROW_H * 2f);
            var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
            labelTmp.text = label;
            labelTmp.fontSize = 9;
            labelTmp.alignment = TextAlignmentOptions.Center;
            labelTmp.color = new Color(0.9f, 0.85f, 0.6f);

            // Button
            var btnGo = new GameObject(btnGoName);
            btnGo.transform.SetParent(panelGo.transform, false);
            var btnRt = btnGo.AddComponent<RectTransform>();
            btnRt.anchorMin = btnRt.anchorMax = new Vector2(0.5f, 0.5f);
            btnRt.pivot = new Vector2(0.5f, 0.5f);
            btnRt.anchoredPosition = new Vector2(BUTTON_X, rowY);
            btnRt.sizeDelta = new Vector2(BTN_W, ROW_H);
            btnGo.AddComponent<CanvasRenderer>();
            var img = btnGo.AddComponent<Image>();
            if (buttonSprite != null) img.sprite = buttonSprite;
            btnGo.AddComponent<Button>();

            // Button label TMP
            var btnLabel = new GameObject("Text");
            btnLabel.transform.SetParent(btnGo.transform, false);
            var btnLabelRt = btnLabel.AddComponent<RectTransform>();
            btnLabelRt.anchorMin = Vector2.zero;
            btnLabelRt.anchorMax = Vector2.one;
            btnLabelRt.offsetMin = btnLabelRt.offsetMax = Vector2.zero;
            var btnTmp = btnLabel.AddComponent<TextMeshProUGUI>();
            btnTmp.text = "+";
            btnTmp.fontSize = 14;
            btnTmp.fontStyle = FontStyles.Bold;
            btnTmp.alignment = TextAlignmentOptions.Center;
            btnTmp.color = Color.white;
        }

        // Expand panel height to fit 6 rows
        panel.sizeDelta = new Vector2(panel.sizeDelta.x, panel.sizeDelta.y + NewRows.Length * Mathf.Abs(ROW_STEP));

        EditorUtility.SetDirty(panelGo);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(panelGo.scene);
        Debug.Log("[AddNewStatRows] Done. Save the scene (Ctrl+S) to persist changes.");
    }

    static void FixButtonTexts(GameObject panel)
    {
        string[] btnNames = { "AtkButton", "SpdButton", "HpButton" };
        foreach (var name in btnNames)
        {
            var btn = panel.transform.Find(name);
            if (btn == null) continue;
            var textChild = btn.Find("Text");
            if (textChild == null) continue;
            var legacyText = textChild.GetComponent<Text>();
            if (legacyText == null) continue;
            string content = legacyText.text;
            Object.DestroyImmediate(legacyText);
            var tmp = textChild.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = content.Length > 0 ? content : "+";
            tmp.fontSize = 14;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
        }
    }

    static GameObject FindByPath(string path)
    {
        var parts = path.Split('/');
        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        GameObject current = null;
        foreach (var root in roots)
            if (root.name == parts[0]) { current = root; break; }
        if (current == null) return null;
        for (int i = 1; i < parts.Length; i++)
        {
            var child = current.transform.Find(parts[i]);
            if (child == null) return null;
            current = child.gameObject;
        }
        return current;
    }
}
