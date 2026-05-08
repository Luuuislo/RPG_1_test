using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiManager : MonoBehaviour
{
    // ── Inventory / Pause ────────────────────────────────────────────────
    public GameObject inventory;
    public GameObject pauseMenu;

    // ── Resources ────────────────────────────────────────────────────────
    public TMP_Text goldCountText;
    public TMP_Text meatCountText;
    public TMP_Text woodCountText;

    // ── Health text ──────────────────────────────────────────────────────
    public TMP_Text healthText;

    // ── Level display ────────────────────────────────────────────────────
    [Header("Level")]
    public TMP_Text levelNumberText;

    // ── Stats panel ──────────────────────────────────────────────────────
    [Header("Stats Panel")]
    public TMP_Text statPointsText;
    public TMP_Text atkValueText;
    public TMP_Text spdValueText;
    public TMP_Text hpValueText;
    public Button   atkButton;
    public Button   spdButton;
    public Button   hpButton;

    // ── Bar fills (found at runtime, no Inspector wiring needed) ─────────
    private Image   _hpFill;
    private Image   _xpFill;
    private TMP_Text _hpText;
    private TMP_Text _xpText;

    private float _targetHp  = 1f;
    private float _targetXp  = 0f;
    private const float BarSpeed = 6f;

    private static readonly Color HpColorHigh = new Color(0.10f, 0.80f, 0.10f);
    private static readonly Color HpColorMid  = new Color(0.85f, 0.70f, 0.00f);
    private static readonly Color HpColorLow  = new Color(0.85f, 0.05f, 0.05f);
    private static readonly Color XpColor     = new Color(0.95f, 0.78f, 0.05f);

    public static UiManager Instance { get; private set; }

    private PlayerExperience _playerExp;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        _playerExp = FindFirstObjectByType<PlayerExperience>();

        if (atkButton != null) atkButton.onClick.AddListener(() => _playerExp?.InvestAttack());
        if (spdButton != null) spdButton.onClick.AddListener(() => _playerExp?.InvestSpeed());
        if (hpButton  != null) hpButton.onClick.AddListener( () => _playerExp?.InvestHealth());

        _hpFill = FindBarFill("HPBar");
        _xpFill = FindBarFill("XPBar");
        _hpText = FindBarText("HPBar");
        _xpText = FindBarText("XPBar");

        if (_hpFill != null) { _hpFill.fillAmount = 1f; _hpFill.color = HpColorHigh; }
        if (_xpFill != null) { _xpFill.fillAmount = 0f; _xpFill.color = XpColor; }
    }

    private void Update()
    {
        LerpFill(_hpFill, ref _targetHp, useHpColor: true);
        LerpFill(_xpFill, ref _targetXp, useHpColor: false);
    }

    // ── Inventory / Pause ────────────────────────────────────────────────
    public void OpenOrCloseInventory() { inventory.SetActive(!inventory.activeSelf); }
    public void PauseGame()  { pauseMenu.SetActive(true);  Time.timeScale = 0; }
    public void ResumeGame() { pauseMenu.SetActive(false); Time.timeScale = 1; }

    // ── Resources ────────────────────────────────────────────────────────
    public void UpdateGold(int v) { if (goldCountText != null) goldCountText.text = v.ToString(); }
    public void UpdateMeat(int v) { if (meatCountText != null) meatCountText.text = v.ToString(); }
    public void UpdateWood(int v) { if (woodCountText != null) woodCountText.text = v.ToString(); }

    // ── Health ───────────────────────────────────────────────────────────
    public void UpdateHealth(float current, float max)
    {
        _targetHp = Mathf.Clamp01(current / max);
        string label = $"{(int)current} / {(int)max}";
        if (healthText != null) healthText.text = label;
        if (_hpText    != null) _hpText.text    = label;
    }

    // ── XP ───────────────────────────────────────────────────────────────
    public void UpdateExp(int current, int max)
    {
        _targetXp = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
        if (_xpText != null) _xpText.text = $"{current} / {max}";
    }

    // ── Level ────────────────────────────────────────────────────────────
    public void UpdateLevel(int level)
    {
        if (levelNumberText != null) levelNumberText.text = level.ToString();
    }

    // ── Stats Panel ──────────────────────────────────────────────────────
    public void UpdateStatPanel(int points, int atk, float spd, int hp)
    {
        bool hasPoints = points > 0;
        if (statPointsText != null) statPointsText.text = $"Pts: {points}";
        if (atkValueText   != null) atkValueText.text   = atk.ToString();
        if (spdValueText   != null) spdValueText.text   = spd.ToString("F2");
        if (hpValueText    != null) hpValueText.text    = hp.ToString();
        if (atkButton != null) atkButton.interactable = hasPoints;
        if (spdButton != null) spdButton.interactable = hasPoints;
        if (hpButton  != null) hpButton.interactable  = hasPoints;
    }

    public void ShowLevelUpFeedback(int newLevel)
    {
        Debug.Log($"[LevelUp] Nivel {newLevel}. +3 stat points.");
    }

    // ── Helpers ──────────────────────────────────────────────────────────
    private void LerpFill(Image fill, ref float target, bool useHpColor)
    {
        if (fill == null) return;
        fill.fillAmount = Mathf.Lerp(fill.fillAmount, target, BarSpeed * Time.deltaTime);
        if (useHpColor) fill.color = HpColor(fill.fillAmount);
    }

    private static Color HpColor(float pct)
    {
        if (pct > 0.60f) return Color.Lerp(HpColorMid, HpColorHigh, (pct - 0.60f) / 0.40f);
        if (pct > 0.30f) return Color.Lerp(HpColorLow, HpColorMid,  (pct - 0.30f) / 0.30f);
        return HpColorLow;
    }

    private static Image FindBarFill(string barName)
    {
        var go = GameObject.Find(barName);
        if (go == null) return null;
        var fillChild = go.transform.Find("Fill");
        var img = fillChild != null
            ? fillChild.GetComponent<Image>()
            : go.GetComponent<Image>();
        if (img == null) return null;
        img.type       = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Horizontal;
        img.fillOrigin = (int)Image.OriginHorizontal.Left;
        if (img.sprite == null)
            img.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
        return img;
    }

    private static TMP_Text FindBarText(string barName)
    {
        var go = GameObject.Find(barName);
        if (go == null) return null;
        var t = go.transform.Find("Label");
        return t?.GetComponent<TMP_Text>();
    }
}
