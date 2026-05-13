using UnityEngine;

public class BuildingSelector : MonoBehaviour
{
    public static BuildingSelector Instance { get; private set; }

    [Header("References")]
    public BuildingUI        buildingUI;
    public UnitProducerUI    unitProducerUI;
    public BuildingPlacer    placer;

    public BuildingLevel Selected { get; private set; }

    // Move mode state
    private bool         _isMoving;
    private BuildingLevel _movingBuilding;
    private Vector3       _originalPosition;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        var mouse = UnityEngine.InputSystem.Mouse.current;
        var kb    = UnityEngine.InputSystem.Keyboard.current;
        if (mouse == null || kb == null) return;

        // Move mode is handled by BuildingPlacer's ghost system — just listen for cancel
        if (_isMoving)
        {
            if (kb.escapeKey.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame)
                CancelMove();
            return;
        }

        // Don't intercept clicks while placement mode is active
        if (placer != null && placer.IsPlacing) return;

        if (!mouse.leftButton.wasPressedThisFrame) return;

        // Don't process world clicks when the pointer is over a UI element.
        // IsPointerOverGameObject() returns true for Physics2D hits too when
        // Physics2DRaycaster is on the camera, so we filter by GraphicRaycaster only.
        if (IsPointerOverUI()) return;

        Vector2 screen = mouse.position.ReadValue();
        Vector3 world  = Camera.main.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -Camera.main.transform.position.z));

        // Check for building under cursor
        Collider2D hit = Physics2D.OverlapPoint(world);
        if (hit != null)
        {
            var bl = hit.GetComponentInParent<BuildingLevel>();
            if (bl != null) { Select(bl); return; }
        }

        // Click on empty space → deselect
        Deselect();
    }

    public void Select(BuildingLevel bl)
    {
        Selected = bl;
        if (bl.GetComponent<UnitProducer>() != null)
        {
            buildingUI?.Hide();
            unitProducerUI?.Show(bl);
        }
        else
        {
            unitProducerUI?.Hide();
            buildingUI?.Show(bl);
        }
        bl.GetComponent<BuildingRangeIndicator>()?.Show();
    }

    public void Deselect()
    {
        Selected?.GetComponent<BuildingRangeIndicator>()?.Hide();
        Selected = null;
        buildingUI?.Hide();
        unitProducerUI?.Hide();
    }

    public void RefreshUI()
    {
        if (Selected == null) return;
        if (Selected.GetComponent<UnitProducer>() != null)
            unitProducerUI?.Refresh();
        else
            buildingUI?.Show(Selected);
    }

    // --- Move ---
    public void StartMove(BuildingLevel bl)
    {
        if (placer == null) return;
        _isMoving        = true;
        _movingBuilding  = bl;
        _originalPosition = bl.transform.position;

        Deselect();
        placer.StartMoveExisting(bl);
    }

    public void CancelMove()
    {
        if (_movingBuilding != null)
        {
            _movingBuilding.transform.position = _originalPosition;
            _movingBuilding.gameObject.SetActive(true);
        }
        _isMoving       = false;
        _movingBuilding = null;
        placer?.CancelPlacementPublic();
    }

    public void ConfirmMove(Vector3 newPosition)
    {
        if (_movingBuilding != null)
            _movingBuilding.transform.position = newPosition;
        _isMoving       = false;
        _movingBuilding = null;
    }

    static bool IsPointerOverUI()
    {
        var es = UnityEngine.EventSystems.EventSystem.current;
        if (es == null) return false;

        var ed = new UnityEngine.EventSystems.PointerEventData(es)
        {
            position = UnityEngine.InputSystem.Mouse.current.position.ReadValue()
        };

        var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
        es.RaycastAll(ed, results);

        foreach (var r in results)
            if (r.module is UnityEngine.UI.GraphicRaycaster) return true;

        return false;
    }
}
