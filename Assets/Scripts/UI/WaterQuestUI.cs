using System.Text;
using UnityEngine;
using UnityEngine.UI;

// Panel de diálogo para la misión de acceso al agua.
// Crear con RPGTools > Create Water Quest Panel.
public class WaterQuestUI : MonoBehaviour
{
    public static WaterQuestUI Instance { get; private set; }

    [Header("Panel")]
    public GameObject panel;

    [Header("Header")]
    public Text   npcNameText;
    public Button closeButton;

    [Header("Content")]
    public Text dialogueText;
    public Text requirementsText;

    [Header("Buttons")]
    public Button acceptButton;
    public Button turnInButton;

    private WaterQuestNPC    _npc;
    private PlayerExperience _playerExp;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        closeButton?.onClick.AddListener(Close);
        acceptButton?.onClick.AddListener(() => { _npc?.AcceptQuest(); });
        turnInButton?.onClick.AddListener(() => { _npc?.TurnIn(); });
        panel?.SetActive(false);
    }

    void Start() => _playerExp = FindFirstObjectByType<PlayerExperience>();

    // ── Public API ────────────────────────────────────────────────────────

    public void Open(WaterQuestNPC npc)
    {
        _npc = npc;
        panel?.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        panel?.SetActive(false);
        _npc = null;
    }

    public void Refresh()
    {
        if (_npc == null || _npc.questData == null) return;

        var data  = _npc.questData;
        var state = _npc.State;

        if (npcNameText != null) npcNameText.text = data.npcName;

        if (dialogueText != null)
            dialogueText.text = state switch
            {
                WaterQuestNPC.QuestState.NotStarted    => data.introText,
                WaterQuestNPC.QuestState.Active        => data.inProgressText,
                WaterQuestNPC.QuestState.ReadyToTurnIn => data.readyText,
                WaterQuestNPC.QuestState.Completed     => data.completedText,
                _                                      => data.introText,
            };

        RefreshRequirements();

        bool notStarted    = state == WaterQuestNPC.QuestState.NotStarted;
        bool readyToTurnIn = state == WaterQuestNPC.QuestState.ReadyToTurnIn;
        acceptButton?.gameObject.SetActive(notStarted);
        turnInButton?.gameObject.SetActive(readyToTurnIn);
    }

    public void RefreshRequirements()
    {
        if (_npc == null || requirementsText == null) return;
        var data = _npc.questData;
        if (data == null) return;

        var sb    = new StringBuilder();
        var kt    = KillTracker.Instance;
        var res   = PlayerResources.Instance;
        if (_playerExp == null) _playerExp = FindFirstObjectByType<PlayerExperience>();

        sb.AppendLine("<b>Requisitos:</b>");

        // Level
        int level  = _playerExp != null ? _playerExp.Level : 0;
        string lvl = level >= data.requiredLevel ? "<color=#55ff55>✓</color>" : "<color=#ff5555>✗</color>";
        sb.AppendLine($"  {lvl} Nivel {data.requiredLevel}  (actual: {level})");

        // Resources
        if (data.goldCost > 0)
        {
            bool ok = res != null && res.HasEnough(data.goldCost, 0, 0);
            string mark = ok ? "<color=#55ff55>✓</color>" : "<color=#ff5555>✗</color>";
            sb.AppendLine($"  {mark} Oro: {data.goldCost}G");
        }
        if (data.woodCost > 0)
        {
            bool ok = res != null && res.HasEnough(0, data.woodCost, 0);
            string mark = ok ? "<color=#55ff55>✓</color>" : "<color=#ff5555>✗</color>";
            sb.AppendLine($"  {mark} Madera: {data.woodCost}W");
        }
        if (data.meatCost > 0)
        {
            bool ok = res != null && res.HasEnough(0, 0, data.meatCost);
            string mark = ok ? "<color=#55ff55>✓</color>" : "<color=#ff5555>✗</color>";
            sb.AppendLine($"  {mark} Carne: {data.meatCost}M");
        }

        // Kill requirements
        if (data.killRequirements != null)
        {
            foreach (var req in data.killRequirements)
            {
                int current = kt != null ? kt.GetCount(req.enemyType) : 0;
                int shown   = Mathf.Min(current, req.count);
                bool done   = current >= req.count;
                string mark = done ? "<color=#55ff55>✓</color>" : "<color=#ff5555>✗</color>";
                sb.AppendLine($"  {mark} Eliminar {req.enemyType}: {shown}/{req.count}");
            }
        }

        requirementsText.text = sb.ToString();
    }
}
