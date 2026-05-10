using UnityEngine;
using UnityEngine.UI;

public class BuildingUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panel;

    [Header("Info")]
    public Text nameText;
    public Text levelText;
    public Text tierText;
    public Text costText;

    [Header("Stats")]
    public Text statsText;
    public Text pointsText;

    [Header("Stat Buttons")]
    public Button addAtkButton;
    public Button addSpdButton;
    public Button addHpButton;
    public GameObject statButtonsRow;   // parent row — hidden when no points

    [Header("Action Buttons")]
    public Button upgradeButton;
    public Button evolveButton;
    public Button moveButton;
    public Button closeButton;

    private BuildingLevel _current;

    void Awake()
    {
        upgradeButton?.onClick.AddListener(OnUpgrade);
        evolveButton?.onClick.AddListener(OnEvolve);
        moveButton?.onClick.AddListener(OnMove);
        closeButton?.onClick.AddListener(Hide);

        addAtkButton?.onClick.AddListener(() => OnSpendPoint(StatType.Atk));
        addSpdButton?.onClick.AddListener(() => OnSpendPoint(StatType.AtkSpeed));
        addHpButton?.onClick.AddListener(() => OnSpendPoint(StatType.Hp));

        SetButtonLabel(upgradeButton, "Upgrade");
        SetButtonLabel(evolveButton,  "Evolve!");
        SetButtonLabel(moveButton,    "Move");
        SetButtonLabel(closeButton,   "X");
        SetButtonLabel(addAtkButton,  "+ATK");
        SetButtonLabel(addSpdButton,  "+SPD");
        SetButtonLabel(addHpButton,   "+HP");
    }

    void SetButtonLabel(Button btn, string label)
    {
        if (btn == null) return;
        var txt = btn.GetComponentInChildren<Text>();
        if (txt != null) txt.text = label;
    }

    public void Show(BuildingLevel bl)
    {
        _current = bl;
        panel?.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        panel?.SetActive(false);
        _current = null;
    }

    void Refresh()
    {
        if (_current == null) return;

        if (nameText  != null) nameText.text  = _current.buildingName;
        if (levelText != null) levelText.text = $"Lv. {_current.currentLevel}";
        if (tierText  != null) tierText.text  = $"Tier {_current.evolutionTier}";

        if (costText != null)
            costText.text = $"Cost: {_current.UpgradeCostGold}G  {_current.UpgradeCostWood}W  {_current.UpgradeCostMeat}M";

        // Stats
        if (statsText != null)
            statsText.text =
                $"ATK {_current.currentAtk:0.#}  " +
                $"SPD {_current.currentAtkSpeed:0.##}  " +
                $"HP {_current.currentHp:0.#}  " +
                $"RNG {_current.currentRange:0.#}";

        // Pending points row
        bool hasPoints = _current.pendingPoints > 0;
        if (pointsText != null)
        {
            pointsText.gameObject.SetActive(hasPoints);
            if (hasPoints) pointsText.text = $"Points: {_current.pendingPoints}";
        }
        if (statButtonsRow != null) statButtonsRow.SetActive(hasPoints);

        // Upgrade button
        if (upgradeButton != null)
            upgradeButton.interactable = _current.CanUpgrade;

        // Evolve button
        if (evolveButton != null)
            evolveButton.gameObject.SetActive(_current.CanEvolve);
    }

    void OnUpgrade()
    {
        _current?.Upgrade();
        Refresh();
    }

    void OnEvolve()
    {
        _current?.Evolve();
    }

    void OnMove()
    {
        if (_current == null) return;
        BuildingSelector.Instance?.StartMove(_current);
    }

    void OnSpendPoint(StatType stat)
    {
        if (_current == null) return;
        _current.SpendPoint(stat);
        Refresh();
    }
}
