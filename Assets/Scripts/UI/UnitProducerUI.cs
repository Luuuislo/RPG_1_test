using UnityEngine;
using UnityEngine.UI;

// Panel de producción de unidades estilo Age of Empires.
// Se asigna en BuildingSelector.unitProducerUI.
// Crear el panel con: RPGTools > Create Unit Producer Panel
public class UnitProducerUI : MonoBehaviour
{
    [Header("Panel Root")]
    public GameObject panel;

    [Header("Header")]
    public Text   nameText;
    public Text   levelText;
    public Button closeButton;

    [Header("Unit Slots (max 3)")]
    public GameObject[] slotRoots     = new GameObject[3];
    public Image[]      slotIcons     = new Image[3];
    public Text[]       slotNames     = new Text[3];
    public Text[]       slotCosts     = new Text[3];
    public Button[]     trainButtons  = new Button[3];

    [Header("Training Progress")]
    public GameObject    trainingSection;
    public Text          trainingLabel;
    public RectTransform progressFill;   // fill usa anchorMax.x para ser responsivo
    public Button        cancelButton;

    [Header("Queue (5 slots)")]
    public Image[] queueSlots = new Image[5];

    [Header("Building Stats Display")]
    public Text   hpText;
    public Text   trainSpeedText;
    public Text[] trainTimeTexts = new Text[3];

    [Header("Building Level Actions")]
    public Button upgradeButton;
    public Button evolveButton;
    public Button moveButton;
    public Text   upgradeCostText;
    public Text   pointsText;
    public Button addAtkButton;
    public Button addSpdButton;
    public Button addHpButton;
    public GameObject statButtonsRow;

    // ── State ────────────────────────────────────────────────────────────

    private BuildingLevel _bl;
    private UnitProducer  _producer;

    void Awake()
    {
        closeButton?.onClick.AddListener(Hide);
        cancelButton?.onClick.AddListener(() => _producer?.CancelLast());
        upgradeButton?.onClick.AddListener(OnUpgrade);
        evolveButton?.onClick.AddListener(OnEvolve);
        moveButton?.onClick.AddListener(OnMove);
        addAtkButton?.onClick.AddListener(() => OnSpendPoint(StatType.Atk));
        addSpdButton?.onClick.AddListener(() => OnSpendPoint(StatType.AtkSpeed));
        addHpButton?.onClick.AddListener(() => OnSpendPoint(StatType.Hp));

        for (int i = 0; i < 3; i++)
        {
            int idx = i;
            trainButtons[i]?.onClick.AddListener(() => OnTrain(idx));
        }
        panel?.SetActive(false);
    }

    // ── Public API ───────────────────────────────────────────────────────

    public void Show(BuildingLevel bl)
    {
        if (_producer != null) _producer.onStateChanged = null;

        _bl       = bl;
        _producer = bl.GetComponent<UnitProducer>();

        if (_producer != null)
            _producer.onStateChanged = RefreshTraining;

        panel?.SetActive(true);
        RefreshHeader();
        RefreshUnitSlots();
        RefreshTraining();
        RefreshBuildingStats();
        RefreshBuildingActions();
    }

    public void Hide()
    {
        if (_producer != null) _producer.onStateChanged = null;
        _bl       = null;
        _producer = null;
        panel?.SetActive(false);
    }

    public void Refresh()
    {
        if (_bl == null) return;
        RefreshHeader();
        RefreshBuildingStats();
        RefreshBuildingActions();
    }

    // ── Refresh ──────────────────────────────────────────────────────────

    void RefreshHeader()
    {
        if (_bl == null) return;
        if (nameText  != null) nameText.text  = _bl.buildingName;
        if (levelText != null) levelText.text = $"Lv.{_bl.currentLevel}  T{_bl.evolutionTier}";
    }

    void RefreshUnitSlots()
    {
        if (_producer == null) return;
        for (int i = 0; i < 3; i++)
        {
            bool active = _producer.units != null && i < _producer.units.Length;
            slotRoots[i]?.SetActive(active);
            if (!active) continue;

            var entry = _producer.units[i];
            if (slotIcons[i] != null)
            {
                slotIcons[i].sprite = entry.icon;
                slotIcons[i].color  = entry.icon != null ? Color.white : new Color(0.4f, 0.4f, 0.6f);
            }
            if (slotNames[i] != null) slotNames[i].text = entry.unitName;
            if (slotCosts[i] != null) slotCosts[i].text = FormatCost(entry);
        }
    }

    void RefreshTraining()
    {
        bool training = _producer != null && _producer.IsTraining;
        trainingSection?.SetActive(training);

        if (training && _producer.CurrentUnit != null)
        {
            if (trainingLabel != null)
                trainingLabel.text = $"Entrenando: {_producer.CurrentUnit.unitName}";
            if (progressFill != null)
            {
                progressFill.anchorMax = new Vector2(_producer.TrainProgress, 1f);
                progressFill.offsetMin = progressFill.offsetMax = Vector2.zero;
            }
        }

        // Queue slots
        var queue = _producer?.GetQueueSnapshot();
        for (int i = 0; i < queueSlots.Length; i++)
        {
            if (queueSlots[i] == null) continue;
            bool occupied = queue != null && i < queue.Length;
            queueSlots[i].color  = occupied
                ? new Color(0.25f, 0.45f, 0.85f)
                : new Color(0.15f, 0.15f, 0.15f, 0.6f);
            queueSlots[i].sprite = (occupied && queue[i].icon != null) ? queue[i].icon : null;
        }
    }

    void RefreshBuildingStats()
    {
        if (_bl == null) return;

        // HP del edificio
        if (hpText != null)
        {
            var ba = _bl.GetComponent<BuildingAttack>();
            hpText.text = ba != null
                ? $"HP  {ba.CurrentHealth}/{ba.maxHealth}"
                : $"HP  {Mathf.RoundToInt(_bl.currentHp)}";
        }

        // Velocidad de producción
        if (trainSpeedText != null)
            trainSpeedText.text = $"Vel  x{_bl.currentAtkSpeed:F1}";

        // Tiempo de entrenamiento por unidad (dividido por velocidad)
        float speed = Mathf.Max(_bl.currentAtkSpeed, 0.1f);
        for (int i = 0; i < trainTimeTexts.Length; i++)
        {
            if (trainTimeTexts[i] == null) continue;
            bool active = _producer?.units != null && i < _producer.units.Length;
            trainTimeTexts[i].text = active
                ? $"⏱ {(_producer.units[i].trainTime / speed):F0}s"
                : "";
        }
    }

    void RefreshBuildingActions()
    {
        if (_bl == null) return;
        if (upgradeButton   != null) upgradeButton.interactable = _bl.CanUpgrade;
        if (upgradeCostText != null)
            upgradeCostText.text = $"{_bl.UpgradeCostGold}G  {_bl.UpgradeCostWood}W";
        if (evolveButton    != null) evolveButton.gameObject.SetActive(_bl.CanEvolve);

        bool hasPoints = _bl.pendingPoints > 0;
        if (pointsText != null)
        {
            pointsText.gameObject.SetActive(hasPoints);
            if (hasPoints) pointsText.text = $"Points: {_bl.pendingPoints}";
        }
        if (statButtonsRow != null) statButtonsRow.SetActive(hasPoints);
    }

    // ── Actions ──────────────────────────────────────────────────────────

    void OnTrain(int idx)       => _producer?.TryEnqueue(idx);
    void OnUpgrade()            { _bl?.Upgrade(); RefreshHeader(); RefreshBuildingStats(); RefreshBuildingActions(); }
    void OnEvolve()             => _bl?.Evolve();
    void OnMove()               { if (_bl != null) BuildingSelector.Instance?.StartMove(_bl); }
    void OnSpendPoint(StatType s) { _bl?.SpendPoint(s); RefreshBuildingStats(); RefreshBuildingActions(); }

    static string FormatCost(UnitEntry e)
    {
        var parts = new System.Collections.Generic.List<string>();
        if (e.goldCost > 0) parts.Add($"{e.goldCost}G");
        if (e.woodCost > 0) parts.Add($"{e.woodCost}W");
        if (e.meatCost > 0) parts.Add($"{e.meatCost}M");
        return string.Join(" ", parts);
    }
}
