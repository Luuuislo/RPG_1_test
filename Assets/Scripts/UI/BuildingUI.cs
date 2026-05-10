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

    [Header("Buttons")]
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
        // Panel starts inactive via scene setup — no SetActive(false) here
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
            costText.text = $"Upgrade: {_current.UpgradeCostGold}G  {_current.UpgradeCostWood}W  {_current.UpgradeCostMeat}M";

        if (upgradeButton != null)
            upgradeButton.interactable = _current.CanUpgrade;

        if (evolveButton != null)
        {
            evolveButton.gameObject.SetActive(_current.CanEvolve);
        }
    }

    void OnUpgrade()
    {
        _current?.Upgrade();
        Refresh();
    }

    void OnEvolve()
    {
        _current?.Evolve();
        // Evolve destroys the object and deselects — panel hides via Deselect
    }

    void OnMove()
    {
        if (_current == null) return;
        BuildingSelector.Instance?.StartMove(_current);
    }
}
