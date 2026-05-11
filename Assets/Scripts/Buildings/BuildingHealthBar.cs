using UnityEngine;
using UnityEngine.UI;

// World-space health bar rendered above a building.
// Uses RectTransform width scaling — no sprite required.
// Inspector fields: offset, width, height on the BuildingHealthBar component.
// During Play Mode the children appear as: building → HealthBarCanvas → BG → Fill
public class BuildingHealthBar : MonoBehaviour
{
    [Header("Layout")]
    public Vector3 offset = new Vector3(0f, 0.9f, 0f);
    public float   width  = 1.0f;
    public float   height = 0.12f;

    [Header("Colors")]
    public Color bgColor       = new Color(0.1f, 0.1f, 0.1f, 0.85f);
    public Color healthyColor  = Color.green;
    public Color criticalColor = Color.red;

    private RectTransform _fillRT;
    private Image         _fillImg;
    private GameObject    _canvasGO;
    private float         _maxWidth;

    void Awake()
    {
        // Canvas
        _canvasGO = new GameObject("HealthBarCanvas");
        _canvasGO.transform.SetParent(transform, false);
        _canvasGO.transform.localPosition = offset;

        var canvas          = _canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.WorldSpace;
        canvas.sortingOrder = 10;

        var canvasRT        = _canvasGO.GetComponent<RectTransform>();
        canvasRT.sizeDelta  = new Vector2(width, height);
        _maxWidth           = width;

        // Background
        var bgGO            = new GameObject("BG");
        bgGO.transform.SetParent(_canvasGO.transform, false);
        var bgRT            = bgGO.AddComponent<RectTransform>();
        bgRT.anchorMin      = Vector2.zero;
        bgRT.anchorMax      = Vector2.one;
        bgRT.offsetMin      = bgRT.offsetMax = Vector2.zero;
        var bgImg           = bgGO.AddComponent<Image>();
        bgImg.color         = bgColor;

        // Fill (anchored left, width controlled manually)
        var fillGO          = new GameObject("Fill");
        fillGO.transform.SetParent(_canvasGO.transform, false);
        _fillRT             = fillGO.AddComponent<RectTransform>();
        _fillRT.anchorMin   = new Vector2(0f, 0f);
        _fillRT.anchorMax   = new Vector2(0f, 1f); // anchor left edge only
        _fillRT.pivot       = new Vector2(0f, 0.5f);
        _fillRT.offsetMin   = Vector2.zero;
        _fillRT.offsetMax   = Vector2.zero;
        _fillRT.sizeDelta   = new Vector2(width, 0f); // full at start
        _fillImg            = fillGO.AddComponent<Image>();
        _fillImg.color      = healthyColor;
    }

    public void UpdateHealth(int current, int max)
    {
        if (_fillRT == null || max <= 0) return;
        float t             = Mathf.Clamp01((float)current / max);
        _fillRT.sizeDelta   = new Vector2(_maxWidth * t, 0f);
        _fillImg.color      = Color.Lerp(criticalColor, healthyColor, t);
    }

    public void HideCanvas()
    {
        if (_canvasGO != null) _canvasGO.SetActive(false);
    }

    public void ShowCanvas()
    {
        if (_canvasGO != null) _canvasGO.SetActive(true);
    }
}
