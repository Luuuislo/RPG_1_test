using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Panel de la tienda/misiones de Pawn. Crear con RPGTools > Create Pawn Shop UI.
public class PawnShopUI : MonoBehaviour
{
    public static PawnShopUI Instance { get; private set; }

    [Header("Panel")]
    public GameObject panel;
    public Button     closeButton;

    [Header("Building Cards Container")]
    public Transform cardContainer;

    [Header("Detail Panel")]
    public GameObject      detailPanel;
    public TextMeshProUGUI detailNameText;
    public TextMeshProUGUI detailDescText;
    public TextMeshProUGUI requirementsText;
    public Button          unlockButton;
    public TextMeshProUGUI unlockButtonLabel;

    [Header("Buildings (misma lista que BuildingPlacer.unlockData)")]
    public BuildingUnlockData[] buildings;

    private BuildingUnlockData _selected;
    private PlayerExperience   _playerExp;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        closeButton?.onClick.AddListener(Close);
        unlockButton?.onClick.AddListener(OnUnlock);
        panel?.SetActive(false);
    }

    void Start()
    {
        _playerExp = FindFirstObjectByType<PlayerExperience>();
        BuildCards();
    }

    // ── Public API ────────────────────────────────────────────────────────

    public void Open()
    {
        panel?.SetActive(true);
        Canvas.ForceUpdateCanvases();
        RefreshCards();
        if (_selected != null) ShowDetail(_selected);
        else
        {
            detailPanel?.SetActive(false);
            unlockButton?.gameObject.SetActive(false);
        }
    }

    public void OpenWithBuilding(BuildingUnlockData data)
    {
        Open();
        if (data != null) ShowDetail(data);
    }

    public void Close()
    {
        panel?.SetActive(false);
        _selected = null;
    }

    public void RefreshCards()
    {
        if (cardContainer == null) return;
        var mgr = BuildingUnlockManager.Instance;
        int idx = 0;

        foreach (Transform card in cardContainer)
        {
            if (idx >= buildings.Length) break;
            var data = buildings[idx++];
            if (data == null || mgr == null) continue;

            // Highlight selected
            var bg = card.GetComponent<Image>();
            if (bg != null)
                bg.color = data == _selected
                    ? new Color(0.28f, 0.22f, 0.12f, 0.98f)
                    : new Color(0.12f, 0.1f, 0.08f, 0.95f);

            // Status badge (nested under Texts/Status)
            var badge = card.Find("Texts/Status")?.GetComponent<TextMeshProUGUI>();
            if (badge == null) continue;

            if (mgr.IsUnlocked(data))
            {
                // Expansion missions (no build limit) just show a tick
                if (data.maxCount == 0)
                {
                    badge.text  = "ACTIVO";
                    badge.color = new Color(0.4f, 0.85f, 0.4f);
                }
                else if (mgr.GetPlacedCount(data) >= mgr.GetAllowedCount(data))
                {
                    badge.text  = "MAX";
                    badge.color = new Color(0.4f, 0.85f, 0.4f);
                }
                else
                {
                    int placed = mgr.GetPlacedCount(data);
                    int max    = mgr.GetAllowedCount(data);
                    badge.text  = $"{placed}/{max}";
                    badge.color = new Color(0.4f, 0.75f, 1f);
                }
            }
            else if (mgr.MeetsUnlockRequirements(data))
            {
                badge.text  = "DISPONIBLE";
                badge.color = new Color(1f, 0.85f, 0.2f);
            }
            else
            {
                badge.text  = "BLOQUEADA";
                badge.color = new Color(0.6f, 0.25f, 0.25f);
            }
        }

        if (_selected != null) RefreshDetail();
    }

    // ── Cards ─────────────────────────────────────────────────────────────

    void BuildCards()
    {
        if (cardContainer == null || buildings == null) return;
        foreach (Transform c in cardContainer) Destroy(c.gameObject);

        foreach (var data in buildings)
        {
            if (data == null) continue;
            var card    = CreateCard(data);
            var captured = data;
            card.GetComponent<Button>()?.onClick.AddListener(() => ShowDetail(captured));
        }
    }

    GameObject CreateCard(BuildingUnlockData data)
    {
        var go = new GameObject(data.buildingName);
        go.transform.SetParent(cardContainer, false);

        var le      = go.AddComponent<LayoutElement>();
        le.preferredHeight = 72f;
        le.flexibleWidth   = 1f;

        var bg      = go.AddComponent<Image>();
        bg.color    = new Color(0.22f, 0.17f, 0.12f, 1f);
        go.AddComponent<Button>();

        var hl      = go.AddComponent<HorizontalLayoutGroup>();
        hl.padding  = new RectOffset(8, 8, 6, 6);
        hl.spacing  = 8f;
        hl.childControlWidth      = true;
        hl.childControlHeight     = true;
        hl.childForceExpandWidth  = false;
        hl.childForceExpandHeight = true;

        // Icon
        var iconGO  = new GameObject("Icon");
        iconGO.transform.SetParent(go.transform, false);
        var iconLE  = iconGO.AddComponent<LayoutElement>();
        iconLE.preferredWidth  = 56f;
        iconLE.preferredHeight = 56f;
        iconLE.flexibleWidth   = 0f;
        var iconImg = iconGO.AddComponent<Image>();
        iconImg.sprite = data.icon;
        iconImg.color  = Color.white;
        iconImg.preserveAspect = true;
        if (data.icon == null)
        {
            iconImg.color = new Color(0.4f, 0.4f, 0.5f, 1f);
        }

        // Text column
        var textGO  = new GameObject("Texts");
        textGO.transform.SetParent(go.transform, false);
        var textLE  = textGO.AddComponent<LayoutElement>();
        textLE.flexibleWidth  = 1f;
        textLE.flexibleHeight = 1f;
        var textVL  = textGO.AddComponent<VerticalLayoutGroup>();
        textVL.childControlWidth      = true;
        textVL.childControlHeight     = true;
        textVL.childForceExpandWidth  = true;
        textVL.childForceExpandHeight = false;
        textVL.spacing = 2f;

        var nameGO  = new GameObject("Name");
        nameGO.transform.SetParent(textGO.transform, false);
        nameGO.AddComponent<LayoutElement>().preferredHeight = 22f;
        var nameTxt = nameGO.AddComponent<TextMeshProUGUI>();
        nameTxt.text      = data.buildingName;
        nameTxt.fontSize  = 13f;
        nameTxt.fontStyle = FontStyles.Bold;
        nameTxt.alignment = TextAlignmentOptions.MidlineLeft;
        nameTxt.color     = Color.white;
        nameTxt.enableWordWrapping = false;

        // Status badge
        var badgeGO = new GameObject("Status");
        badgeGO.transform.SetParent(textGO.transform, false);
        badgeGO.AddComponent<LayoutElement>().preferredHeight = 18f;
        var badge = badgeGO.AddComponent<TextMeshProUGUI>();
        badge.text      = "LOCK";
        badge.fontSize  = 11f;
        badge.alignment = TextAlignmentOptions.MidlineLeft;
        badge.color     = new Color(0.9f, 0.35f, 0.35f);

        return go;
    }

    // ── Detail ────────────────────────────────────────────────────────────

    void ShowDetail(BuildingUnlockData data)
    {
        _selected = data;
        detailPanel?.SetActive(true);
        RefreshDetail();
        RefreshCards();
    }

    void RefreshDetail()
    {
        if (_selected == null) return;
        var data = _selected;
        var mgr  = BuildingUnlockManager.Instance;
        bool unlocked = mgr != null && mgr.IsUnlocked(data);

        if (detailNameText != null) detailNameText.text = data.buildingName;
        if (detailDescText != null)
            detailDescText.text = unlocked ? data.description : data.lockedDescription;

        RefreshRequirements(data);
        RefreshUnlockButton(data);
    }

    void RefreshRequirements(BuildingUnlockData data)
    {
        if (requirementsText == null) return;
        var mgr = BuildingUnlockManager.Instance;
        var kt  = KillTracker.Instance;
        var res = PlayerResources.Instance;
        if (_playerExp == null) _playerExp = FindFirstObjectByType<PlayerExperience>();

        bool unlocked = mgr != null && mgr.IsUnlocked(data);
        if (unlocked)
        {
            var sbU = new StringBuilder();
            if (!string.IsNullOrEmpty(data.pawnDialogue))
                sbU.AppendLine($"<color=#d4c49a><i>\"{data.pawnDialogue}\"</i></color>\n");
            if (data.maxCount == 0)
                sbU.Append("<color=#55ff55>✓ Desbloqueado</color>");
            else
            {
                int placed = mgr.GetPlacedCount(data);
                int max    = mgr.GetAllowedCount(data);
                sbU.Append($"<color=#55ff55>✓ Desbloqueado</color>\nConstruidos: {placed}/{max}");
            }
            requirementsText.text = sbU.ToString();
            return;
        }

        var sb = new StringBuilder();

        // Show Pawn's dialogue if available
        if (!string.IsNullOrEmpty(data.pawnDialogue))
            sb.AppendLine($"<color=#d4c49a><i>\"{data.pawnDialogue}\"</i></color>\n");

        sb.AppendLine("<b>Requisitos para desbloquear:</b>");

        if (data.prerequisite != null)
            sb.AppendLine($"  {Mark(mgr != null && mgr.IsUnlocked(data.prerequisite))} Mision anterior: {data.prerequisite.buildingName}");

        if (data.requiredLevel > 0)
        {
            int level = _playerExp != null ? _playerExp.Level : 0;
            sb.AppendLine($"  {Mark(level >= data.requiredLevel)} Nivel {data.requiredLevel}  (actual: {level})");
        }
        if (data.goldCost > 0)
            sb.AppendLine($"  {Mark(res != null && res.HasEnough(data.goldCost, 0, 0))} Oro: {data.goldCost}G");
        if (data.woodCost > 0)
            sb.AppendLine($"  {Mark(res != null && res.HasEnough(0, data.woodCost, 0))} Madera: {data.woodCost}W");
        if (data.meatCost > 0)
            sb.AppendLine($"  {Mark(res != null && res.HasEnough(0, 0, data.meatCost))} Carne: {data.meatCost}M");

        if (data.killRequirements != null)
            foreach (var req in data.killRequirements)
            {
                int cur = kt != null ? kt.GetCount(req.enemyType) : 0;
                sb.AppendLine($"  {Mark(cur >= req.count)} Eliminar {req.enemyType}: {Mathf.Min(cur, req.count)}/{req.count}");
            }

        if (data.requiredBuildings != null)
            foreach (var req in data.requiredBuildings)
            {
                if (req.building == null) continue;
                int placed = mgr != null ? mgr.GetPlacedCount(req.building) : 0;
                sb.AppendLine($"  {Mark(placed >= req.count)} {req.building.buildingName} construido: {Mathf.Min(placed, req.count)}/{req.count}");
            }

        requirementsText.text = sb.ToString();
    }

    void RefreshUnlockButton(BuildingUnlockData data)
    {
        if (unlockButton == null) return;
        var mgr = BuildingUnlockManager.Instance;
        bool unlocked = mgr != null && mgr.IsUnlocked(data);

        unlockButton.gameObject.SetActive(!unlocked);
        if (!unlocked)
        {
            bool canUnlock = mgr != null && mgr.MeetsUnlockRequirements(data);
            unlockButton.interactable = canUnlock;
            if (unlockButtonLabel != null)
                unlockButtonLabel.text = canUnlock ? "DESBLOQUEAR" : "REQUISITOS PENDIENTES";
        }
    }

    void OnUnlock()
    {
        if (_selected == null) return;
        BuildingUnlockManager.Instance?.Unlock(_selected);
        RefreshDetail();
        RefreshCards();
    }

    static string Mark(bool ok) => ok ? "<color=#55ff55>✓</color>" : "<color=#ff5555>✗</color>";
}
