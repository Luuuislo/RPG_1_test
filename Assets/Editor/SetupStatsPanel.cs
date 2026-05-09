using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class SetupStatsPanel
{
    public static void Execute()
    {
        string panelPath = "---UI---/Canvas/++Player++/StatsPanel";

        // StatPointsText — top, full width
        SetupText(panelPath + "/StatPointsText", "Pts: 0", 10, TextAlignmentOptions.Center,
            new Color(0.95f, 0.78f, 0.05f), new Vector2(0, 24), new Vector2(200, 14));

        // ATK row
        SetupText(panelPath + "/AtkValue", "ATK: 1", 9, TextAlignmentOptions.Left,
            Color.white, new Vector2(-45, 8), new Vector2(100, 14));
        SetupButton(panelPath + "/AtkButton", "+ATK",
            new Vector2(70, 8), new Vector2(58, 14));

        // SPD row
        SetupText(panelPath + "/SpdValue", "SPD: 1.00", 9, TextAlignmentOptions.Left,
            Color.white, new Vector2(-45, -8), new Vector2(100, 14));
        SetupButton(panelPath + "/SpdButton", "+SPD",
            new Vector2(70, -8), new Vector2(58, 14));

        // HP row
        SetupText(panelPath + "/HpValue", "HP: 100", 9, TextAlignmentOptions.Left,
            Color.white, new Vector2(-45, -24), new Vector2(100, 14));
        SetupButton(panelPath + "/HpButton", "+HP",
            new Vector2(70, -24), new Vector2(58, 14));

        Debug.Log("[SetupStatsPanel] Done.");
    }

    static void SetupText(string path, string defaultText, float fontSize,
        TextAlignmentOptions align, Color color, Vector2 pos, Vector2 size)
    {
        var go = GameObject.Find(path);
        if (go == null) { Debug.LogWarning("[SetupStatsPanel] Not found: " + path); return; }

        var legacy = go.GetComponent<Text>();
        if (legacy != null) Object.DestroyImmediate(legacy);

        var tmp = go.GetComponent<TextMeshProUGUI>() ?? go.AddComponent<TextMeshProUGUI>();
        tmp.text      = defaultText;
        tmp.fontSize  = fontSize;
        tmp.alignment = align;
        tmp.color     = color;

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;

        EditorUtility.SetDirty(go);
    }

    static void SetupButton(string path, string label, Vector2 pos, Vector2 size)
    {
        var go = GameObject.Find(path);
        if (go == null) { Debug.LogWarning("[SetupStatsPanel] Not found: " + path); return; }

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;

        // Set button label text (child "Text" legacy or TMP)
        var childTmp = go.GetComponentInChildren<TextMeshProUGUI>();
        if (childTmp != null) { childTmp.text = label; childTmp.fontSize = 8; }
        else
        {
            var childTxt = go.GetComponentInChildren<Text>();
            if (childTxt != null) childTxt.text = label;
        }

        EditorUtility.SetDirty(go);
    }
}
