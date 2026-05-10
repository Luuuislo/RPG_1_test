using UnityEngine;
using UnityEngine.UI;

// World-space health bar rendered above a building.
// Auto-added by BuildingAttack; color shifts green → red as HP drops.
public class BuildingHealthBar : MonoBehaviour
{
    [Header("Layout")]
    public Vector3 offset = new Vector3(0f, 0.9f, 0f);
    public float   width  = 1.0f;
    public float   height = 0.1f;

    private Image      _fill;
    private GameObject _canvasGO;

    void Awake()
    {
        _canvasGO = new GameObject("HealthBarCanvas");
        _canvasGO.transform.SetParent(transform, false);
        _canvasGO.transform.localPosition = offset;

        var canvas = _canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.WorldSpace;
        canvas.sortingOrder = 10;

        var rt = _canvasGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(width, height);

        CreateImage(_canvasGO, "BG",   new Color(0.1f, 0.1f, 0.1f, 0.8f), out _);
        CreateImage(_canvasGO, "Fill", Color.green, out _fill);

        _fill.type       = Image.Type.Filled;
        _fill.fillMethod = Image.FillMethod.Horizontal;
        _fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        _fill.fillAmount = 1f;
    }

    public void UpdateHealth(int current, int max)
    {
        if (_fill == null || max <= 0) return;
        float t          = Mathf.Clamp01((float)current / max);
        _fill.fillAmount = t;
        _fill.color      = Color.Lerp(Color.red, Color.green, t);
    }

    public void HideCanvas()
    {
        if (_canvasGO != null) _canvasGO.SetActive(false);
    }

    static void CreateImage(GameObject parent, string name, Color color, out Image img)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt  = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        img       = go.AddComponent<Image>();
        img.color = color;
    }
}
