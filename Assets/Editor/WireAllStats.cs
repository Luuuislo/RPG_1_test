using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class WireAllStats
{
    static readonly (string field, string goName, bool isButton)[] Mappings =
    {
        ("statPointsText",    "StatPointsText",   false),
        ("atkValueText",      "AtkValue",         false),
        ("spdValueText",      "SpdValue",         false),
        ("hpValueText",       "HpValue",          false),
        ("atkSpdValueText",   "AtkSpdValue",      false),
        ("critChanceValueText","CritChanceValue", false),
        ("critDmgValueText",  "CritDmgValue",     false),
        ("atkButton",         "AtkButton",        true),
        ("spdButton",         "SpdButton",        true),
        ("hpButton",          "HpButton",         true),
        ("atkSpdButton",      "AtkSpdButton",     true),
        ("critChanceButton",  "CritChanceButton", true),
        ("critDmgButton",     "CritDmgButton",    true),
    };

    // Default display values until UiManager updates them at runtime
    static readonly (string goName, string defaultText)[] DefaultTexts =
    {
        ("AtkValue",       "1"),
        ("SpdValue",       "1.000"),
        ("HpValue",        "100"),
        ("AtkSpdValue",    "2.00s"),
        ("CritChanceValue","5.0%"),
        ("CritDmgValue",   "1.3x"),
    };

    public static void Execute()
    {
        var uiMgr = GameObject.FindFirstObjectByType<UiManager>();
        if (uiMgr == null) { Debug.LogError("[WireAllStats] UiManager not found."); return; }

        var so = new SerializedObject(uiMgr);

        foreach (var (field, goName, isButton) in Mappings)
        {
            var go = GameObject.Find(goName);
            if (go == null) { Debug.LogWarning($"[WireAllStats] '{goName}' not found."); continue; }

            var prop = so.FindProperty(field);
            if (prop == null) { Debug.LogWarning($"[WireAllStats] Field '{field}' not found."); continue; }

            if (isButton)
                prop.objectReferenceValue = go.GetComponent<Button>();
            else
                prop.objectReferenceValue = go.GetComponent<TextMeshProUGUI>();
        }

        so.ApplyModifiedProperties();

        // Set clean default texts on value elements
        foreach (var (goName, text) in DefaultTexts)
        {
            var go = GameObject.Find(goName);
            var tmp = go?.GetComponent<TextMeshProUGUI>();
            if (tmp != null) tmp.text = text;
        }

        EditorUtility.SetDirty(uiMgr.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(uiMgr.gameObject.scene);
        Debug.Log("[WireAllStats] All fields wired. Save scene (Ctrl+S).");
    }
}
