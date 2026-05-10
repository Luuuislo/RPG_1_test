using UnityEngine;

public class BuildingSelector : MonoBehaviour
{
    public static BuildingSelector Instance { get; private set; }

    [Header("References")]
    public BuildingUI   buildingUI;
    public BuildingPlacer placer;

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
        buildingUI?.Show(bl);
    }

    public void Deselect()
    {
        Selected = null;
        buildingUI?.Hide();
    }

    public void RefreshUI()
    {
        if (Selected != null) buildingUI?.Show(Selected);
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
}
