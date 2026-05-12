using UnityEngine;
using UnityEngine.EventSystems;

// Añadir a Shark_NPC_Boat (o cualquier NPC que entregue la misión de agua).
// Requiere: Collider2D en el mismo GameObject + Physics2D Raycaster en la cámara.
public class WaterQuestNPC : MonoBehaviour, IPointerClickHandler
{
    [Header("Quest")]
    public WaterQuestData     questData;
    public WaterAccessTrigger waterTrigger;

    public enum QuestState { NotStarted, Active, ReadyToTurnIn, Completed }
    public QuestState State { get; private set; } = QuestState.NotStarted;

    private PlayerExperience _playerExp;

    void Start()
    {
        _playerExp = FindFirstObjectByType<PlayerExperience>();
    }

    void Update()
    {
        if (State == QuestState.Active && CheckAllRequirements())
            State = QuestState.ReadyToTurnIn;
    }

    public void OnPointerClick(PointerEventData eventData) => WaterQuestUI.Instance?.Open(this);

    // ── Quest logic ───────────────────────────────────────────────────────

    public bool CheckAllRequirements()
    {
        if (questData == null) return false;

        if (_playerExp == null) _playerExp = FindFirstObjectByType<PlayerExperience>();
        if (_playerExp != null && _playerExp.Level < questData.requiredLevel) return false;

        var res = PlayerResources.Instance;
        if (res != null && !res.HasEnough(questData.goldCost, questData.woodCost, questData.meatCost)) return false;

        var kt = KillTracker.Instance;
        if (questData.killRequirements != null)
            foreach (var req in questData.killRequirements)
                if (kt == null || kt.GetCount(req.enemyType) < req.count) return false;

        return true;
    }

    public void AcceptQuest()
    {
        if (State != QuestState.NotStarted) return;
        State = QuestState.Active;
        WaterQuestUI.Instance?.Refresh();
    }

    public void TurnIn()
    {
        if (State != QuestState.ReadyToTurnIn) return;
        if (!CheckAllRequirements()) { WaterQuestUI.Instance?.Refresh(); return; }

        PlayerResources.Instance?.Spend(questData.goldCost, questData.woodCost, questData.meatCost);
        State = QuestState.Completed;
        waterTrigger?.UnlockAccess();
        WaterQuestUI.Instance?.Refresh();
    }
}
