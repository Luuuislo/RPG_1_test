using UnityEngine;
using UnityEngine.InputSystem;

// Tibia-style target selection.
// Right-click on enemy  → toggle target (marca / desmarca).
// Right-click on empty  → clear target.
// Tab                   → toggle auto-follow.
public class PlayerTargeting : MonoBehaviour
{
    public static PlayerTargeting Instance { get; private set; }

    [Header("Layers")]
    public LayerMask enemyLayers;

    [Header("Auto Follow")]
    public bool autoFollow = true;

    [Header("Target Frame")]
    public Color frameColor    = Color.red;
    public float frameWidth    = 0.04f;
    [Tooltip("Multiplier sobre el tamaño del sprite del enemigo (< 1 = más pequeño)")]
    public float frameSizeMultiplier = 0.55f;
    [Tooltip("Padding fijo añadido al radio calculado")]
    public float framePadding        = 0.05f;

    public Transform CurrentTarget { get; private set; }

    private LineRenderer _frame;
    private GameObject   _frameGO;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(this); return; }
        BuildFrame();
    }

    void BuildFrame()
    {
        _frameGO = new GameObject("PlayerTargetFrame");
        _frameGO.transform.SetParent(transform);

        _frame = _frameGO.AddComponent<LineRenderer>();
        _frame.useWorldSpace   = true;
        _frame.loop            = true;
        _frame.positionCount   = 4;
        _frame.widthMultiplier = frameWidth;

        var mat = new Material(Shader.Find("Sprites/Default")) { color = frameColor };
        _frame.material     = mat;
        _frame.sortingOrder = 20;

        _frameGO.SetActive(false);
    }

    void Update()
    {
        HandleRightClick();
        HandleToggleKey();
        RefreshFrame();
    }

    void HandleRightClick()
    {
        var mouse = Mouse.current;
        if (mouse == null || !mouse.rightButton.wasPressedThisFrame) return;
        if (UnityEngine.EventSystems.EventSystem.current?.IsPointerOverGameObject() == true) return;

        Vector2 screen   = mouse.position.ReadValue();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(
                               new Vector3(screen.x, screen.y, -Camera.main.transform.position.z));
        worldPos.z = 0f;

        Collider2D hit = Physics2D.OverlapPoint(worldPos, enemyLayers);
        if (hit != null && hit.GetComponentInParent<DamageReceiver>() != null)
        {
            // Toggle: si ya es el target actual → desmarca
            if (CurrentTarget == hit.transform)
                ClearTarget();
            else
                SetTarget(hit.transform);
            return;
        }

        // Click en espacio vacío → limpia target
        if (Physics2D.OverlapPoint(worldPos) == null)
            ClearTarget();
    }

    void HandleToggleKey()
    {
        var kb = Keyboard.current;
        if (kb != null && kb.tabKey.wasPressedThisFrame)
            autoFollow = !autoFollow;
    }

    void RefreshFrame()
    {
        if (CurrentTarget == null)
        {
            _frameGO.SetActive(false);
            return;
        }

        if (!CurrentTarget.gameObject.activeInHierarchy)
        {
            ClearTarget();
            return;
        }

        _frameGO.SetActive(true);

        float s = GetHalfSize(CurrentTarget) * frameSizeMultiplier + framePadding;
        Vector3 c = CurrentTarget.position;
        _frame.SetPosition(0, new Vector3(c.x - s, c.y - s, 0f));
        _frame.SetPosition(1, new Vector3(c.x + s, c.y - s, 0f));
        _frame.SetPosition(2, new Vector3(c.x + s, c.y + s, 0f));
        _frame.SetPosition(3, new Vector3(c.x - s, c.y + s, 0f));
    }

    static float GetHalfSize(Transform t)
    {
        var sr = t.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) return Mathf.Max(sr.bounds.extents.x, sr.bounds.extents.y);
        var col = t.GetComponent<Collider2D>();
        if (col != null) return Mathf.Max(col.bounds.extents.x, col.bounds.extents.y);
        return 0.4f;
    }

    public void SetTarget(Transform t)
    {
        CurrentTarget = t;
        _frameGO.SetActive(true);
    }

    public void ClearTarget()
    {
        CurrentTarget = null;
        _frameGO.SetActive(false);
    }
}
