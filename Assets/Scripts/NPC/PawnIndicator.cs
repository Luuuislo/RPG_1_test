using UnityEngine;

// Attach to the Pawn NPC GameObject alongside PawnNPC.
// Assign a child "MissionIndicator" GameObject (the yellow "!" sprite/canvas) in the Inspector.
public class PawnIndicator : MonoBehaviour
{
    [Tooltip("Child GameObject with the '!' visual. Assign in Inspector.")]
    public GameObject indicatorRoot;

    [Tooltip("Seconds between availability checks.")]
    public float checkInterval = 0.5f;

    private float _timer;

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < checkInterval) return;
        _timer = 0f;

        if (indicatorRoot == null) return;
        var mgr = BuildingUnlockManager.Instance;
        var ui  = PawnShopUI.Instance;
        indicatorRoot.SetActive(mgr != null && ui != null && mgr.HasPendingMissions(ui.buildings));
    }
}
