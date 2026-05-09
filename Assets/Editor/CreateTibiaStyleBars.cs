using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class CreateTibiaStyleBars
{
    [MenuItem("RPG/Create Tibia Style Bars")]
    public static void Execute()
    {
        // Busca el Canvas principal
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("[TibiaBars] No hay Canvas en la escena."); return; }

        // Borra barras anteriores si existen
        DestroyChild(canvas.transform, "HPBar");
        DestroyChild(canvas.transform, "XPBar");
        DestroyChild(canvas.transform, "++HealthBar_V2++");
        DestroyChild(canvas.transform, "++ExpBar_V2++");

        // ── Barra de HP ──────────────────────────────────────────────────────
        // Posición: esquina superior izquierda
        //   Ancho 200 px, alto 22 px
        //   Color fill: rojo Tibia
        CreateBar(
            canvas.transform,
            name:      "HPBar",
            anchorMin: new Vector2(0f, 1f),
            anchorMax: new Vector2(0f, 1f),
            pivot:     new Vector2(0f, 1f),
            anchoredPosition: new Vector2(12f, -12f),
            size:      new Vector2(200f, 22f),
            bgColor:   new Color(0.12f, 0.05f, 0.05f),
            fillColor: new Color(0.85f, 0.10f, 0.10f),
            labelText: "100 / 100"
        );

        // ── Barra de XP ──────────────────────────────────────────────────────
        // Justo debajo de la HP, más delgada
        CreateBar(
            canvas.transform,
            name:      "XPBar",
            anchorMin: new Vector2(0f, 1f),
            anchorMax: new Vector2(0f, 1f),
            pivot:     new Vector2(0f, 1f),
            anchoredPosition: new Vector2(12f, -40f),
            size:      new Vector2(200f, 14f),
            bgColor:   new Color(0.10f, 0.08f, 0.02f),
            fillColor: new Color(0.95f, 0.78f, 0.05f),
            labelText: "0 / 100"
        );

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[TibiaBars] HPBar (rojo) y XPBar (amarillo) creadas. UiManager las encuentra por nombre en runtime.");
    }

    // ── Crea barra + fondo + fill + label de texto ───────────────────────
    static void CreateBar(Transform parent, string name,
                          Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
                          Vector2 anchoredPosition, Vector2 size,
                          Color bgColor, Color fillColor, string labelText)
    {
        // Contenedor
        var go   = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin        = anchorMin;
        rect.anchorMax        = anchorMax;
        rect.pivot            = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta        = size;

        // Borde/fondo oscuro
        var bg     = new GameObject("Background");
        bg.transform.SetParent(go.transform, false);
        var bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin        = Vector2.zero;
        bgRect.anchorMax        = Vector2.one;
        bgRect.sizeDelta        = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        bg.AddComponent<Image>().color = bgColor;

        // Fill — Image.Type.Filled, horizontal
        var fillGO   = new GameObject("Fill");
        fillGO.transform.SetParent(go.transform, false);
        var fillRect = fillGO.AddComponent<RectTransform>();
        fillRect.anchorMin        = new Vector2(0f, 0f);
        fillRect.anchorMax        = new Vector2(1f, 1f);
        fillRect.sizeDelta        = new Vector2(-2f, -2f);
        fillRect.anchoredPosition = Vector2.zero;
        var img        = fillGO.AddComponent<Image>();
        img.color      = fillColor;
        img.type       = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Horizontal;
        img.fillOrigin = (int)Image.OriginHorizontal.Left;
        img.fillAmount = name == "HPBar" ? 1f : 0f;

        // Texto encima (Label)
        var labelGO   = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        var labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.anchorMin        = Vector2.zero;
        labelRect.anchorMax        = Vector2.one;
        labelRect.sizeDelta        = Vector2.zero;
        labelRect.anchoredPosition = Vector2.zero;
        var tmp             = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text            = labelText;
        tmp.fontSize        = 9f;
        tmp.color           = Color.white;
        tmp.alignment       = TextAlignmentOptions.Center;
        tmp.fontStyle       = FontStyles.Bold;
        tmp.enableWordWrapping = false;

        EditorUtility.SetDirty(go);
    }

    static void DestroyChild(Transform parent, string childName)
    {
        // Busca también en la raíz del canvas o en toda la escena
        var found = GameObject.Find(childName);
        if (found != null) Object.DestroyImmediate(found);
    }
}
