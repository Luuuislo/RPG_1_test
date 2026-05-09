using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using UnityEditor.SceneManagement;

public class MinimapSetup
{
    public static void Execute()
    {
        // --- RenderTexture ---
        if (!AssetDatabase.IsValidFolder("Assets/Minimap"))
            AssetDatabase.CreateFolder("Assets", "Minimap");

        string rtPath = "Assets/Minimap/MinimapRT.renderTexture";
        var existingRT = AssetDatabase.LoadAssetAtPath<RenderTexture>(rtPath);
        if (existingRT != null) AssetDatabase.DeleteAsset(rtPath);

        var rt = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
        rt.name = "MinimapRT";
        AssetDatabase.CreateAsset(rt, rtPath);
        AssetDatabase.SaveAssets();
        rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(rtPath);

        // --- Minimap Camera ---
        var existing = GameObject.Find("MinimapCamera");
        if (existing != null) Object.DestroyImmediate(existing);

        var camGO = new GameObject("MinimapCamera");
        camGO.transform.position = new Vector3(0f, 0f, -10f);

        var cam = camGO.AddComponent<Camera>();
        cam.orthographic       = true;
        cam.orthographicSize   = 20f;
        cam.targetTexture      = rt;
        cam.depth              = 2f;
        cam.clearFlags         = CameraClearFlags.SolidColor;
        cam.backgroundColor    = new Color(0.08f, 0.12f, 0.08f, 1f);
        cam.cullingMask        = ~(1 << LayerMask.NameToLayer("UI"));
        cam.nearClipPlane      = 0.1f;
        cam.farClipPlane       = 1000f;

        var urp = camGO.AddComponent<UniversalAdditionalCameraData>();
        urp.renderType         = CameraRenderType.Base;
        urp.renderPostProcessing = false;

        camGO.AddComponent<MinimapFollow>();

        // --- Canvas ---
        var canvas = GameObject.Find("---UI---/Canvas");
        if (canvas == null) { Debug.LogError("Canvas not found"); return; }

        // Remove old minimap if exists
        var oldMinimap = canvas.transform.Find("++Minimap++");
        if (oldMinimap != null) Object.DestroyImmediate(oldMinimap.gameObject);

        // --- Outer frame ---
        var frameGO = new GameObject("++Minimap++");
        frameGO.transform.SetParent(canvas.transform, false);
        var frameRect = frameGO.AddComponent<RectTransform>();
        frameRect.anchorMin       = new Vector2(1f, 1f);
        frameRect.anchorMax       = new Vector2(1f, 1f);
        frameRect.pivot           = new Vector2(1f, 1f);
        frameRect.anchoredPosition = new Vector2(-12f, -12f);
        frameRect.sizeDelta       = new Vector2(164f, 164f);
        var frameImg = frameGO.AddComponent<Image>();
        frameImg.color = new Color(0.05f, 0.05f, 0.05f, 0.9f);

        // --- Inner raw image ---
        var rawGO = new GameObject("MinimapImage");
        rawGO.transform.SetParent(frameGO.transform, false);
        var rawRect = rawGO.AddComponent<RectTransform>();
        rawRect.anchorMin  = Vector2.zero;
        rawRect.anchorMax  = Vector2.one;
        rawRect.offsetMin  = new Vector2(4f,  4f);
        rawRect.offsetMax  = new Vector2(-4f, -4f);
        var raw = rawGO.AddComponent<RawImage>();
        raw.texture = rt;

        // --- Player dot (center indicator) ---
        var dotGO = new GameObject("PlayerDot");
        dotGO.transform.SetParent(rawGO.transform, false);
        var dotRect = dotGO.AddComponent<RectTransform>();
        dotRect.anchorMin       = new Vector2(0.5f, 0.5f);
        dotRect.anchorMax       = new Vector2(0.5f, 0.5f);
        dotRect.pivot           = new Vector2(0.5f, 0.5f);
        dotRect.anchoredPosition = Vector2.zero;
        dotRect.sizeDelta       = new Vector2(8f, 8f);
        var dotImg = dotGO.AddComponent<Image>();
        dotImg.color = Color.yellow;

        // --- Save ---
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.Refresh();

        Debug.Log("[Minimap] Setup complete.");
    }
}
