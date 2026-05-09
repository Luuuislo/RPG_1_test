using UnityEngine;
using TMPro;
using UnityEditor;

public class FixFloatingText
{
    public static void Execute()
    {
        string path = "Assets/Resources/FloatingDamageText.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) { Debug.LogError("[FixFloatingText] Prefab not found."); return; }

        var tmp = prefab.GetComponent<TextMeshPro>();
        if (tmp != null)
        {
            tmp.fontSize            = 5f;
            tmp.fontStyle           = FontStyles.Bold;
            tmp.horizontalAlignment = HorizontalAlignmentOptions.Center;
            tmp.sortingOrder        = 100;
        }

        var mr = prefab.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 100;

        var fdt = prefab.GetComponent<FloatingDamageText>();
        if (fdt != null)
        {
            fdt.floatSpeed   = 2.5f;
            fdt.fadeDuration = 10f; // long enough to verify, then restore to 2.5
        }

        EditorUtility.SetDirty(prefab);
        AssetDatabase.SaveAssets();
        Debug.Log("[FixFloatingText] fadeDuration=10 for testing.");
    }
}
