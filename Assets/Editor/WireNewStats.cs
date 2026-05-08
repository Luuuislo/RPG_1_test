using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class WireNewStats
{
    public static void Execute()
    {
        var uiManagerGo = GameObject.FindFirstObjectByType<UiManager>()?.gameObject;
        if (uiManagerGo == null) { Debug.LogError("[WireNewStats] UiManager not found."); return; }

        var so = new SerializedObject(uiManagerGo.GetComponent<UiManager>());

        SetTmpField(so, "atkSpdValueText",    "AtkSpdValue");
        SetTmpField(so, "critChanceValueText", "CritChanceValue");
        SetTmpField(so, "critDmgValueText",    "CritDmgValue");
        SetBtnField(so, "atkSpdButton",        "AtkSpdButton");
        SetBtnField(so, "critChanceButton",    "CritChanceButton");
        SetBtnField(so, "critDmgButton",       "CritDmgButton");

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(uiManagerGo);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(uiManagerGo.scene);
        Debug.Log("[WireNewStats] Done. Save scene (Ctrl+S).");
    }

    static void SetTmpField(SerializedObject so, string fieldName, string goName)
    {
        var go = GameObject.Find(goName);
        if (go == null) { Debug.LogWarning($"[WireNewStats] GameObject '{goName}' not found."); return; }
        var tmp = go.GetComponent<TextMeshProUGUI>();
        if (tmp == null) { Debug.LogWarning($"[WireNewStats] No TMP on '{goName}'."); return; }
        var prop = so.FindProperty(fieldName);
        if (prop == null) { Debug.LogWarning($"[WireNewStats] Field '{fieldName}' not found in UiManager."); return; }
        prop.objectReferenceValue = tmp;
    }

    static void SetBtnField(SerializedObject so, string fieldName, string goName)
    {
        var go = GameObject.Find(goName);
        if (go == null) { Debug.LogWarning($"[WireNewStats] GameObject '{goName}' not found."); return; }
        var btn = go.GetComponent<Button>();
        if (btn == null) { Debug.LogWarning($"[WireNewStats] No Button on '{goName}'."); return; }
        var prop = so.FindProperty(fieldName);
        if (prop == null) { Debug.LogWarning($"[WireNewStats] Field '{fieldName}' not found in UiManager."); return; }
        prop.objectReferenceValue = btn;
    }
}
