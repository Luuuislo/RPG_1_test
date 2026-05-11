using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

// RPGTools > Create Unit Producer Panel
// Panel estilo Rise of Kingdoms — bottom sheet full-width, mobile-first.
public static class CreateUnitProducerPanel
{
    static readonly Color BgDark     = new Color(0.10f, 0.08f, 0.06f, 0.97f);
    static readonly Color BgCard     = new Color(0.16f, 0.12f, 0.09f, 0.95f);
    static readonly Color GoldLine   = new Color(0.85f, 0.65f, 0.08f, 1.00f);
    static readonly Color GoldText   = new Color(1.00f, 0.85f, 0.20f, 1.00f);
    static readonly Color GreenTrain = new Color(0.13f, 0.42f, 0.09f, 1.00f);
    static readonly Color RedDanger  = new Color(0.52f, 0.10f, 0.08f, 1.00f);
    static readonly Color BlueMid    = new Color(0.15f, 0.18f, 0.38f, 1.00f);
    static readonly Color BrownMid   = new Color(0.42f, 0.24f, 0.04f, 1.00f);
    static readonly Color SlotEmpty  = new Color(0.18f, 0.14f, 0.10f, 0.85f);

    [MenuItem("RPGTools/Create Unit Producer Panel")]
    static void Create()
    {
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("No hay Canvas en la escena."); return; }

        // ── Panel raíz: full-width, anclado al fondo ──────────────────────
        // offsetMax.y = 320 → el panel mide 320px de alto
        var panel = MakeGO("UnitProducerPanel", canvas.transform);
        var rt    = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot     = new Vector2(0.5f, 0f);
        rt.offsetMin = new Vector2(0f,   0f);
        rt.offsetMax = new Vector2(0f, 320f);
        AddImg(panel, BgDark);

        // childControlHeight = true → LayoutElement.preferredHeight es respetado
        var rootVL = panel.AddComponent<VerticalLayoutGroup>();
        rootVL.padding                = new RectOffset(10, 10, 0, 8);
        rootVL.spacing                = 4f;
        rootVL.childControlWidth      = true;
        rootVL.childForceExpandWidth  = true;
        rootVL.childControlHeight     = true;   // ← clave: respeta preferredHeight
        rootVL.childForceExpandHeight = false;
        rootVL.childAlignment         = TextAnchor.UpperCenter;

        // ── Barra dorada (3px) ────────────────────────────────────────────
        var topBar = MakeGO("TopBar", panel.transform);
        AddImg(topBar, GoldLine);
        LE(topBar, h: 3f);

        // ── Header (46px) ─────────────────────────────────────────────────
        var header = MakeGO("Header", panel.transform);
        LE(header, h: 46f);
        var hHLG = header.AddComponent<HorizontalLayoutGroup>();
        hHLG.spacing               = 6f;
        hHLG.childControlWidth     = true;  hHLG.childForceExpandWidth  = false;
        hHLG.childControlHeight    = true;  hHLG.childForceExpandHeight = true;
        hHLG.childAlignment        = TextAnchor.MiddleLeft;

        var nameTxt = MakeTxt(header, "NameText", "Barracks", 18, TextAnchor.MiddleLeft, GoldText);
        nameTxt.fontStyle = FontStyle.Bold;
        LE(nameTxt.gameObject, fw: 1f);

        var lvlTxt = MakeTxt(header, "LevelText", "Lv.1  T1", 11,
                              TextAnchor.MiddleCenter, new Color(0.65f, 0.65f, 0.65f));
        LE(lvlTxt.gameObject, pw: 76f);

        var closeBtn = MakeBtn(header, "CloseBtn", "✕", RedDanger);
        closeBtn.GetComponentInChildren<Text>().fontSize = 20;
        LE(closeBtn.gameObject, pw: 46f);

        // ── Cards (flexibleHeight=1 → ocupa el espacio sobrante) ──────────
        var cardsRow = MakeGO("CardsRow", panel.transform);
        LE(cardsRow, fh: 1f);
        var crHLG = cardsRow.AddComponent<HorizontalLayoutGroup>();
        crHLG.spacing               = 8f;
        crHLG.childControlWidth     = true;  crHLG.childForceExpandWidth  = true;
        crHLG.childControlHeight    = true;  crHLG.childForceExpandHeight = true;
        crHLG.childAlignment        = TextAnchor.UpperCenter;

        var slotRoots = new GameObject[3];
        var slotIcons = new Image[3];
        var slotNames = new Text[3];
        var slotCosts = new Text[3];
        var trainBtns = new Button[3];

        for (int i = 0; i < 3; i++)
        {
            var card = MakeGO($"UnitSlot{i}", cardsRow.transform);
            LE(card, fw: 1f);
            AddImg(card, BgCard);
            var cardVL = card.AddComponent<VerticalLayoutGroup>();
            cardVL.padding               = new RectOffset(6, 6, 6, 6);
            cardVL.spacing               = 4f;
            cardVL.childControlWidth     = true;  cardVL.childForceExpandWidth  = true;
            cardVL.childControlHeight    = true;  cardVL.childForceExpandHeight = false;
            cardVL.childAlignment        = TextAnchor.UpperCenter;

            var iconGO  = MakeGO("Icon", card.transform);
            var iconImg = iconGO.AddComponent<Image>();
            iconImg.color          = new Color(0.22f, 0.18f, 0.13f);
            iconImg.preserveAspect = true;
            LE(iconGO, fh: 1f);          // icono ocupa el espacio sobrante de la card
            slotIcons[i] = iconImg;

            slotNames[i] = MakeTxt(card, "UnitName", "Unit", 11,
                                   TextAnchor.MiddleCenter, Color.white);
            slotNames[i].fontStyle = FontStyle.Bold;
            LE(slotNames[i].gameObject, h: 16f);

            slotCosts[i] = MakeTxt(card, "Cost", "—", 10,
                                   TextAnchor.MiddleCenter, GoldText);
            LE(slotCosts[i].gameObject, h: 14f);

            trainBtns[i] = MakeBtn(card, "TrainBtn", "RECRUIT", GreenTrain);
            LE(trainBtns[i].gameObject, h: 30f);
            var tb = trainBtns[i].GetComponentInChildren<Text>();
            tb.fontSize = 11; tb.fontStyle = FontStyle.Bold;

            slotRoots[i] = card;
        }

        // ── Cola (32px) ───────────────────────────────────────────────────
        var queueRow = MakeGO("QueueRow", panel.transform);
        LE(queueRow, h: 32f);
        var qrHLG = queueRow.AddComponent<HorizontalLayoutGroup>();
        qrHLG.spacing               = 5f;
        qrHLG.childControlWidth     = true;  qrHLG.childForceExpandWidth  = false;
        qrHLG.childControlHeight    = true;  qrHLG.childForceExpandHeight = true;
        qrHLG.childAlignment        = TextAnchor.MiddleLeft;

        var qLabel = MakeTxt(queueRow, "QLabel", "Cola:", 9,
                             TextAnchor.MiddleLeft, new Color(0.60f, 0.60f, 0.60f));
        LE(qLabel.gameObject, pw: 36f);

        var qSlots = new Image[5];
        for (int i = 0; i < 5; i++)
        {
            var s  = MakeGO($"QSlot{i}", queueRow.transform);
            LE(s, pw: 30f);
            var si = s.AddComponent<Image>();
            si.color          = SlotEmpty;
            si.preserveAspect = true;
            qSlots[i] = si;
        }

        var spacer = MakeGO("Spacer", queueRow.transform);
        LE(spacer, fw: 1f);

        var cancelBtn = MakeBtn(queueRow, "CancelBtn", "Cancelar", RedDanger);
        LE(cancelBtn.gameObject, pw: 80f);
        cancelBtn.GetComponentInChildren<Text>().fontSize = 10;

        // ── Sección entrenamiento (30px) ──────────────────────────────────
        var trainSec = MakeGO("TrainingSection", panel.transform);
        LE(trainSec, h: 30f);
        var tsVL = trainSec.AddComponent<VerticalLayoutGroup>();
        tsVL.padding               = new RectOffset(0, 0, 0, 0);
        tsVL.spacing               = 3f;
        tsVL.childControlWidth     = true;  tsVL.childForceExpandWidth  = true;
        tsVL.childControlHeight    = true;  tsVL.childForceExpandHeight = false;

        var trainLbl = MakeTxt(trainSec, "TrainingLabel", "Entrenando: —", 10,
                               TextAnchor.MiddleLeft, new Color(0.85f, 0.85f, 0.85f));
        LE(trainLbl.gameObject, h: 14f);

        var progressBG = MakeGO("ProgressBG", trainSec.transform);
        LE(progressBG, h: 13f);
        AddImg(progressBG, new Color(0.08f, 0.08f, 0.08f, 0.9f));

        var fillGO = MakeGO("Fill", progressBG.transform);
        var fillRT = fillGO.GetComponent<RectTransform>();
        fillRT.anchorMin = new Vector2(0f, 0f);
        fillRT.anchorMax = new Vector2(0f, 1f);
        fillRT.pivot     = new Vector2(0f, 0.5f);
        fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
        AddImg(fillGO, new Color(0.14f, 0.58f, 0.09f));

        // ── Acciones (38px) ───────────────────────────────────────────────
        var actRow = MakeGO("ActionsRow", panel.transform);
        LE(actRow, h: 38f);
        var arHLG = actRow.AddComponent<HorizontalLayoutGroup>();
        arHLG.spacing               = 6f;
        arHLG.childControlWidth     = true;  arHLG.childForceExpandWidth  = false;
        arHLG.childControlHeight    = true;  arHLG.childForceExpandHeight = true;
        arHLG.childAlignment        = TextAnchor.MiddleCenter;

        var upCostTxt = MakeTxt(actRow, "UpgradeCostText", "100G  50W", 9,
                                TextAnchor.MiddleCenter, GoldText);
        LE(upCostTxt.gameObject, pw: 70f);

        var upgradeBtn = MakeBtn(actRow, "UpgradeBtn", "MEJORAR", GreenTrain);
        upgradeBtn.GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;
        LE(upgradeBtn.gameObject, pw: 100f);

        var evolveBtn = MakeBtn(actRow, "EvolveBtn", "EVOLVE!", BrownMid);
        evolveBtn.GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;
        LE(evolveBtn.gameObject, pw: 90f);
        evolveBtn.gameObject.SetActive(false);

        var moveBtn = MakeBtn(actRow, "MoveBtn", "Mover", BlueMid);
        LE(moveBtn.gameObject, pw: 72f);

        // ── Stat points (oculta) ──────────────────────────────────────────
        var pointsRow = MakeGO("StatPointsRow", panel.transform);
        LE(pointsRow, h: 38f);
        var prHLG = pointsRow.AddComponent<HorizontalLayoutGroup>();
        prHLG.spacing               = 6f;
        prHLG.childControlWidth     = true;  prHLG.childForceExpandWidth  = false;
        prHLG.childControlHeight    = true;  prHLG.childForceExpandHeight = true;

        var pointsTxt = MakeTxt(pointsRow, "PointsText", "Points: 0", 10,
                                TextAnchor.MiddleLeft, GoldText);
        LE(pointsTxt.gameObject, pw: 78f);
        var addHpBtn = MakeBtn(pointsRow, "AddHpBtn", "+HP", BlueMid);
        LE(addHpBtn.gameObject, pw: 66f);
        pointsRow.SetActive(false);

        // ── Conectar UnitProducerUI ───────────────────────────────────────
        var ui = panel.AddComponent<UnitProducerUI>();
        ui.panel           = panel;
        ui.nameText        = nameTxt;
        ui.levelText       = lvlTxt;
        ui.closeButton     = closeBtn;
        ui.slotRoots       = slotRoots;
        ui.slotIcons       = slotIcons;
        ui.slotNames       = slotNames;
        ui.slotCosts       = slotCosts;
        ui.trainButtons    = trainBtns;
        ui.trainingSection = trainSec;
        ui.trainingLabel   = trainLbl;
        ui.progressFill    = fillRT;
        ui.cancelButton    = cancelBtn;
        ui.queueSlots      = qSlots;
        ui.upgradeButton   = upgradeBtn;
        ui.evolveButton    = evolveBtn;
        ui.moveButton      = moveBtn;
        ui.upgradeCostText = upCostTxt;
        ui.pointsText      = pointsTxt;
        ui.addHpButton     = addHpBtn;
        ui.statButtonsRow  = pointsRow;

        var sel = Object.FindFirstObjectByType<BuildingSelector>();
        if (sel != null) { sel.unitProducerUI = ui; EditorUtility.SetDirty(sel); }
        else Debug.LogWarning("[Unit Producer Panel] BuildingSelector no encontrado.");

        panel.SetActive(false);
        EditorUtility.SetDirty(panel);
        Selection.activeGameObject = panel;
        Debug.Log("[Unit Producer Panel] Creado.");
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    static GameObject MakeGO(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static Text MakeTxt(GameObject parent, string name, string text,
                        int size, TextAnchor align, Color color)
    {
        var go = MakeGO(name, parent.transform);
        var t  = go.AddComponent<Text>();
        t.text      = text;
        t.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize  = size;
        t.alignment = align;
        t.color     = color;
        return t;
    }

    static Button MakeBtn(GameObject parent, string name, string label, Color bg)
    {
        var go  = MakeGO(name, parent.transform);
        var img = go.AddComponent<Image>(); img.color = bg;
        var btn = go.AddComponent<Button>(); btn.targetGraphic = img;

        var lbl = MakeGO("Text", go.transform);
        var lrt = lbl.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        var txt = lbl.AddComponent<Text>();
        txt.text      = label;
        txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize  = 12;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color     = Color.white;
        return btn;
    }

    static Image AddImg(GameObject go, Color color)
    {
        var img = go.AddComponent<Image>(); img.color = color; return img;
    }

    // pw=preferredWidth, h=preferredHeight, fw=flexibleWidth, fh=flexibleHeight
    static void LE(GameObject go, float pw = 0, float h = 0, float fw = 0, float fh = 0)
    {
        var le = go.AddComponent<LayoutElement>();
        if (pw > 0) le.preferredWidth  = pw;
        if (h  > 0) le.preferredHeight = h;
        if (fw > 0) le.flexibleWidth   = fw;
        if (fh > 0) le.flexibleHeight  = fh;
    }
}
