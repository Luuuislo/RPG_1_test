using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class SetupHudBars
{
    public static void Execute()
    {
        SwapTextToTMP("---UI---/Canvas/++Player++/HPBar/Label");
        SwapTextToTMP("---UI---/Canvas/++Player++/XPBar/Label");
        Debug.Log("[SetupHudBars] Labels converted to TextMeshProUGUI.");
    }

    static void SwapTextToTMP(string path)
    {
        var go = GameObject.Find(path);
        if (go == null) { Debug.LogWarning($"[SetupHudBars] Not found: {path}"); return; }

        var legacy = go.GetComponent<Text>();
        if (legacy != null) Object.DestroyImmediate(legacy);

        if (go.GetComponent<TextMeshProUGUI>() == null)
        {
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize  = path.Contains("XP") ? 8 : 10;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = Color.white;
            tmp.text      = "";
        }

        EditorUtility.SetDirty(go);
    }
}
