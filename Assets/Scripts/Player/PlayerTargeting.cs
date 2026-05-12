using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Tibia-style target selection.
// Right-click on enemy/resource → toggle target.
// Right-click on empty         → clear target.
// Tab                          → toggle auto-follow.
public class PlayerTargeting : MonoBehaviour
{
    public static PlayerTargeting Instance { get; private set; }

    [Header("Layers")]
    public LayerMask enemyLayers;
    public LayerMask resourceLayers;

    [Header("Auto Follow")]
    public bool autoFollow = true;

    [Header("Target Shadow — Colors")]
    public Color enemyColor    = Color.red;
    public Color resourceColor = new Color(1f, 0.5f, 0f, 1f); // naranja

    [Header("Target Shadow — Shape")]
    public float shadowLineWidth = 0.04f;
    [Tooltip("Radio horizontal de la elipse")]
    public float shadowRadiusX   = 0.28f;
    [Tooltip("Radio vertical (aplana la elipse para efecto sombra)")]
    public float shadowRadiusY   = 0.09f;
    [Tooltip("Ajuste vertical adicional desde los pies del sprite (negativo = más abajo)")]
    public float feetOffsetY     = 0f;
    [Range(8, 32)] public int shadowSegments = 20;

    public Transform CurrentTarget { get; private set; }
    private bool _isResourceTarget;

    private LineRenderer _shadow;
    private GameObject   _shadowGO;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(this); return; }
        BuildShadow();
    }

    void BuildShadow()
    {
        _shadowGO = new GameObject("PlayerTargetShadow");
        _shadowGO.transform.SetParent(transform);

        _shadow = _shadowGO.AddComponent<LineRenderer>();
        _shadow.useWorldSpace   = true;
        _shadow.loop            = true;
        _shadow.positionCount   = shadowSegments;
        _shadow.widthMultiplier = shadowLineWidth;

        _shadow.material     = new Material(Shader.Find("Sprites/Default")) { color = enemyColor };
        _shadow.sortingOrder = 20;

        _shadowGO.SetActive(false);
    }

    void Update()
    {
        HandleRightClick();
        HandleToggleKey();
        RefreshShadow();
    }

    void HandleRightClick()
    {
        var mouse = Mouse.current;
        if (mouse == null || !mouse.rightButton.wasPressedThisFrame) return;
        if (IsPointerOverUI()) return;

        Vector2 screen   = mouse.position.ReadValue();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(
                               new Vector3(screen.x, screen.y, -Camera.main.transform.position.z));
        worldPos.z = 0f;

        bool isResource = false;
        Collider2D hit  = Physics2D.OverlapPoint(worldPos, enemyLayers);
        if (hit == null)
        {
            hit        = Physics2D.OverlapPoint(worldPos, resourceLayers);
            isResource = hit != null;
        }

        if (hit != null && hit.GetComponentInParent<DamageReceiver>() != null)
        {
            if (CurrentTarget == hit.transform)
                ClearTarget();
            else
                SetTarget(hit.transform, isResource);
            return;
        }

        if (Physics2D.OverlapPoint(worldPos) == null)
            ClearTarget();
    }

    void HandleToggleKey()
    {
        var kb = Keyboard.current;
        if (kb != null && kb.tabKey.wasPressedThisFrame)
            autoFollow = !autoFollow;
    }

    void RefreshShadow()
    {
        if (CurrentTarget == null)
        {
            _shadowGO.SetActive(false);
            return;
        }

        if (!CurrentTarget.gameObject.activeInHierarchy)
        {
            ClearTarget();
            return;
        }

        _shadowGO.SetActive(true);

        // Pie del sprite = posición - mitad de altura del bounds
        float halfH   = GetHalfHeight(CurrentTarget);
        Vector3 pivot = CurrentTarget.position + new Vector3(0f, -halfH + feetOffsetY, 0f);

        _shadow.material.color = _isResourceTarget ? resourceColor : enemyColor;

        for (int i = 0; i < shadowSegments; i++)
        {
            float angle = (float)i / shadowSegments * Mathf.PI * 2f;
            _shadow.SetPosition(i, new Vector3(
                pivot.x + Mathf.Cos(angle) * shadowRadiusX,
                pivot.y + Mathf.Sin(angle) * shadowRadiusY,
                0f));
        }
    }

    static float GetHalfHeight(Transform t)
    {
        var sr = t.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) return sr.bounds.extents.y;
        var col = t.GetComponent<Collider2D>();
        if (col != null) return col.bounds.extents.y;
        return 0.4f;
    }

    public void SetTarget(Transform t, bool isResource = false)
    {
        CurrentTarget     = t;
        _isResourceTarget = isResource;
        _shadowGO.SetActive(true);
    }

    public void ClearTarget()
    {
        CurrentTarget = null;
        _shadowGO.SetActive(false);
    }

    // Devuelve true solo si el puntero está sobre un elemento UI real (Image/Text),
    // ignorando los hits del Physics2D Raycaster sobre objetos del mundo.
    static bool IsPointerOverUI()
    {
        var es = EventSystem.current;
        if (es == null) return false;
        var data = new PointerEventData(es) { position = Mouse.current.position.ReadValue() };
        var results = new List<RaycastResult>();
        es.RaycastAll(data, results);
        foreach (var r in results)
            if (r.gameObject.GetComponent<Graphic>() != null) return true;
        return false;
    }
}
