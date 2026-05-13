using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildingPlacer : MonoBehaviour
{
    [Header("Building Prefabs")]
    public GameObject[] buildingPrefabs;

    [Header("Unlock Data (mismo orden que buildingPrefabs)")]
    [Tooltip("Uno por cada prefab. Dejar null = sin restricción.")]
    public BuildingUnlockData[] unlockData;

    [Header("Placement")]
    public LayerMask blockingLayers;
    public float     snapSize = 0.5f;

    [Header("UI")]
    public GameObject buildMenuPanel;
    public Transform  buttonContainer;
    public GameObject buttonPrefab;   // Prefab con Image (fondo), child "Icon" (Image), child "Label" (TMP)

    public bool IsPlacing => _isPlacing;

    private GameObject       _ghost;
    private SpriteRenderer[] _ghostSRs;
    private Vector2          _buildingSize;
    private Vector2          _buildingOffset;
    private GameObject       _selectedPrefab;
    private bool             _isPlacing;
    private int              _selectedPrefabIndex = -1;

    // Building being moved (null when placing new)
    private BuildingLevel    _movingBuilding;

    static readonly Color ValidColor   = new Color(0f, 1f, 0f, 0.5f);
    static readonly Color InvalidColor = new Color(1f, 0f, 0f, 0.5f);

    void Start()
    {
        if (buildMenuPanel != null) buildMenuPanel.SetActive(false);
        BuildButtons();
    }

    void BuildButtons()
    {
        if (buildMenuPanel == null || buildingPrefabs == null) return;

        var slots = new System.Collections.Generic.List<Transform>();

        // If the panel has an --Icons-- container, use its children as slots.
        // Otherwise fall back to direct children named BuildMenuBackground*.
        Transform iconsRoot = buildMenuPanel.transform.Find("--Icons--");
        if (iconsRoot != null)
        {
            foreach (Transform child in iconsRoot)
                slots.Add(child);
        }
        else
        {
            foreach (Transform child in buildMenuPanel.transform)
                if (child.name.StartsWith("BuildMenuBackground"))
                    slots.Add(child);
        }

        // Wire each slot to a building prefab by index
        for (int i = 0; i < slots.Count && i < buildingPrefabs.Length; i++)
        {
            var slot   = slots[i];
            var prefab = buildingPrefabs[i];
            if (prefab == null) continue;

            int captured = i;

            var btn = slot.GetComponent<Button>() ?? slot.gameObject.AddComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => TrySelectBuilding(captured));

            var label = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = prefab.name.Replace("_", " ").Replace("Blue Base", "").Trim();

            RefreshSlotState(slot, i);
        }
    }

    void Update()
    {
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb == null) return;

        if (kb.bKey.wasPressedThisFrame) ToggleMenu();

        if (!_isPlacing) return;

        UpdateGhost();

        var mouse = UnityEngine.InputSystem.Mouse.current;
        if (mouse.leftButton.wasPressedThisFrame && CanPlace())
        {
            PlaceBuilding();
            return;
        }

        if (kb.escapeKey.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame)
        {
            if (_movingBuilding != null) _movingBuilding.gameObject.SetActive(true);
            CancelPlacement();
        }
    }

    void UpdateGhost()
    {
        Vector2 screen = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        Vector3 world  = Camera.main.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -Camera.main.transform.position.z));
        world.z = 0f;

        if (snapSize > 0f)
        {
            world.x = Mathf.Round(world.x / snapSize) * snapSize;
            world.y = Mathf.Round(world.y / snapSize) * snapSize;
        }

        _ghost.transform.position = world;

        Color c = CanPlace() ? ValidColor : InvalidColor;
        foreach (var s in _ghostSRs) s.color = c;
    }

    bool CanPlace()
    {
        Vector2 center = (Vector2)_ghost.transform.position + _buildingOffset;
        Collider2D hit = Physics2D.OverlapBox(center, _buildingSize * 0.8f, 0f, blockingLayers);
        return hit == null;
    }

    void TrySelectBuilding(int index)
    {
        if (index < 0 || index >= buildingPrefabs.Length) return;
        var data = (unlockData != null && index < unlockData.Length) ? unlockData[index] : null;
        var mgr  = BuildingUnlockManager.Instance;

        if (data != null && mgr != null && !mgr.CanBuild(data))
        {
            buildMenuPanel?.SetActive(false);
            PawnShopUI.Instance?.OpenWithBuilding(data);
            return;
        }

        _selectedPrefabIndex = index;
        SelectBuilding(buildingPrefabs[index]);
    }

    public void SelectBuilding(GameObject prefab)
    {
        CancelPlacement();
        _selectedPrefab = prefab;
        _isPlacing      = true;
        buildMenuPanel?.SetActive(false);

        _ghost      = Instantiate(prefab);
        _ghost.name = "BuildGhost";

        SetLayerAll(_ghost, 2); // Ignore Raycast — excluded from blockingLayers check

        CaptureSize(_ghost);

        foreach (var col in _ghost.GetComponentsInChildren<Collider2D>()) col.enabled = false;
        var rb = _ghost.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;
        foreach (var mb in _ghost.GetComponentsInChildren<MonoBehaviour>()) mb.enabled = false;

        _ghostSRs = _ghost.GetComponentsInChildren<SpriteRenderer>();
        foreach (var s in _ghostSRs) s.color = ValidColor;

        EnableGhostRange(_ghost);
    }

    void CaptureSize(GameObject go)
    {
        foreach (var col in go.GetComponentsInChildren<Collider2D>())
        {
            if (col.isTrigger) continue;
            _buildingOffset = col.offset;
            if (col is BoxCollider2D box)   { _buildingSize = box.size; return; }
            if (col is CircleCollider2D c)  { float d = c.radius * 2f; _buildingSize = new Vector2(d, d); return; }
        }
        var sr = go.GetComponentInChildren<SpriteRenderer>();
        _buildingSize   = sr != null ? (Vector2)sr.bounds.size : Vector2.one;
        _buildingOffset = Vector2.zero;
    }

    void PlaceBuilding()
    {
        if (_movingBuilding != null)
        {
            // Repositioning an existing building
            _movingBuilding.transform.position = _ghost.transform.position;
            _movingBuilding.gameObject.SetActive(true);
            BuildingSelector.Instance?.ConfirmMove(_ghost.transform.position);
            _movingBuilding = null;
        }
        else
        {
            var go     = Instantiate(_selectedPrefab, _ghost.transform.position, Quaternion.identity);
            var parent = GameObject.Find("---BUILDS---");
            if (parent != null) go.transform.SetParent(parent.transform);

            // Register placed count
            if (_selectedPrefabIndex >= 0 && unlockData != null && _selectedPrefabIndex < unlockData.Length)
                BuildingUnlockManager.Instance?.RegisterPlaced(unlockData[_selectedPrefabIndex]);
        }
        _selectedPrefabIndex = -1;
        CancelPlacement();
    }

    void CancelPlacement()
    {
        _isPlacing      = false;
        _selectedPrefab = null;
        _movingBuilding = null;
        if (_ghost != null) { Destroy(_ghost); _ghost = null; }
        _ghostSRs = null;
    }

    public void CancelPlacementPublic() => CancelPlacement();

    // Starts ghost mode for an already-placed building (move)
    public void StartMoveExisting(BuildingLevel bl)
    {
        CancelPlacement();
        _movingBuilding = bl;
        _isPlacing      = true;

        bl.gameObject.SetActive(false);

        _ghost      = Instantiate(bl.gameObject);
        _ghost.name = "BuildGhost";
        _ghost.SetActive(true);

        SetLayerAll(_ghost, 2);
        CaptureSize(_ghost);

        foreach (var col in _ghost.GetComponentsInChildren<Collider2D>()) col.enabled = false;
        var rb = _ghost.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;
        foreach (var mb in _ghost.GetComponentsInChildren<MonoBehaviour>()) mb.enabled = false;

        _ghostSRs = _ghost.GetComponentsInChildren<SpriteRenderer>();
        foreach (var s in _ghostSRs) s.color = ValidColor;

        EnableGhostRange(_ghost);
    }

    void EnableGhostRange(GameObject ghost)
    {
        // Hide health bar — not relevant during placement
        ghost.GetComponentInChildren<BuildingHealthBar>()?.HideCanvas();

        // Re-enable the range indicator MonoBehaviour (was disabled with all others)
        var ri = ghost.GetComponent<BuildingRangeIndicator>();
        if (ri == null) return;
        ri.enabled = true;

        float range = ghost.GetComponent<BuildingAttack>()?.attackRange
                   ?? ghost.GetComponent<BuildingLevel>()?.baseRange
                   ?? 5f;
        ri.SetRadius(range);
        ri.Show();
    }

    public void RefreshBuildButtons()
    {
        if (buildMenuPanel == null || buildingPrefabs == null) return;

        var slots = new System.Collections.Generic.List<Transform>();
        Transform iconsRoot = buildMenuPanel.transform.Find("--Icons--");
        if (iconsRoot != null)
            foreach (Transform child in iconsRoot) slots.Add(child);
        else
            foreach (Transform child in buildMenuPanel.transform)
                if (child.name.StartsWith("BuildMenuBackground")) slots.Add(child);

        for (int i = 0; i < slots.Count && i < buildingPrefabs.Length; i++)
            RefreshSlotState(slots[i], i);
    }

    void RefreshSlotState(Transform slot, int index)
    {
        var data = (unlockData != null && index < unlockData.Length) ? unlockData[index] : null;
        var mgr  = BuildingUnlockManager.Instance;
        var btn  = slot.GetComponent<Button>();

        if (data == null || mgr == null) { if (btn != null) btn.interactable = true; return; }

        bool canBuild = mgr.CanBuild(data);
        if (btn != null) btn.interactable = canBuild;

        // Show lock / count suffix on label
        var label = slot.GetComponentInChildren<TextMeshProUGUI>();
        if (label == null) return;

        string baseName = buildingPrefabs[index].name.Replace("_", " ").Replace("Blue Base", "").Trim();
        if (!mgr.IsUnlocked(data))
            label.text = $"{baseName}\n🔒";
        else if (data.maxCount > 0 && mgr.GetPlacedCount(data) >= data.maxCount)
            label.text = $"{baseName}\nMAX";
        else
        {
            int placed = mgr.GetPlacedCount(data);
            int max    = data.maxCount > 0 ? data.maxCount : 99;
            label.text = $"{baseName}\n{placed}/{max}";
        }
    }

    void ToggleMenu()
    {
        if (buildMenuPanel == null) return;
        bool show = !buildMenuPanel.activeSelf;
        buildMenuPanel.SetActive(show);
        if (!show) CancelPlacement();
    }

    static void SetLayerAll(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform) SetLayerAll(child.gameObject, layer);
    }
}
