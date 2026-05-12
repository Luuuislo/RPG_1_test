using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

// RPGTools > Create Unit Producer Panel
// Cards horizontales de ancho fijo. El ícono se adapta al alto disponible.
// Fila de stats: HP del edificio + velocidad de producción.
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
    static readonly Color StatBg     = new Color(0.08f, 0.07f, 0.05f, 0.80f);
    static readonly Color SlotEmpty  = new Color(0.18f, 0.14f, 0.10f, 0.85f);

    const float CardWidth = 112f;   // ancho fijo de cada card

    [MenuItem("RPGTools/Create Unit Producer Panel")]
    static void Create()
    {
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("No hay Canvas en la escena."); return; }

        // ── Panel raíz: full-width, anclado al fondo ──────────────────────
        var panel = MakeGO("UnitProducerPanel", canvas.transform);
        var rt    = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot     = new Vector2(0.5f, 0f);
        rt.offsetMin = new Vector2(0f,   0f);
        rt.offsetMax = new Vector2(0f, 360f);   // Height ajustable en Inspector
        AddImg(panel, BgDark);

        var rootVL = panel.AddComponent<VerticalLayoutGroup>();
        rootVL.padding                = new RectOffset(10, 10, 0, 8);
        rootVL.spacing                = 4f;
        rootVL.childControlWidth      = true;
        rootVL.childForceExpandWidth  = true;
        rootVL.childControlHeight     = true;
        rootVL.childForceExpandHeight = false;
        rootVL.childAlignment         = TextAnchor.UpperCenter;

        // ── Barra dorada (3px) ────────────────────────────────────────────
        var topBar = MakeGO("TopBar", panel.transform);
        AddImg(topBar, GoldLine);
        LE(topBar, h: 3f);

        // ── Header ────────────────────────────────────────────────────────
        var header = MakeGO("Header", panel.transform);
        LE(header, h: 44f);
        var hHLG = header.AddComponent<HorizontalLayoutGroup>();
        hHLG.spacing = 6f;
        hHLG.childControlWidth = true;  hHLG.childForceExpandWidth  = false;
        hHLG.childControlHeight = true; hHLG.childForceExpandHeight = true;
        hHLG.childAlignment = TextAnchor.MiddleLeft;

        var nameTxt = MakeTxt(header, "NameText", "Barracks", 17,
                              TextAnchor.MiddleLeft, GoldText);
        nameTxt.fontStyle = FontStyle.Bold;
        LE(nameTxt.gameObject, fw: 1f);

        var lvlTxt = MakeTxt(header, "LevelText", "Lv.1  T1", 11,
                             TextAnchor.MiddleCenter, new Color(0.65f, 0.65f, 0.65f));
        LE(lvlTxt.gameObject, pw: 72f);

        var closeBtn = MakeBtn(header, "CloseBtn", "✕", RedDanger);
        closeBtn.GetComponentInChildren<Text>().fontSize = 18;
        LE(closeBtn.gameObject, pw: 44f);

        // ── Stats del edificio ────────────────────────────────────────────
        var statsRow = MakeGO("StatsRow", panel.transform);
        LE(statsRow, h: 24f);
        AddImg(statsRow, StatBg);
        var srHLG = statsRow.AddComponent<HorizontalLayoutGroup>();
        srHLG.padding = new RectOffset(8, 8, 0, 0);
        srHLG.spacing = 12f;
        srHLG.childControlWidth = true;  srHLG.childForceExpandWidth  = false;
        srHLG.childControlHeight = true; srHLG.childForceExpandHeight = true;
        srHLG.childAlignment = TextAnchor.MiddleLeft;

        var hpTxt = MakeTxt(statsRow, "HpText", "HP  —",
                            10, TextAnchor.MiddleLeft, new Color(0.85f, 0.35f, 0.35f));
        LE(hpTxt.gameObject, pw: 100f);

        var velTxt = MakeTxt(statsRow, "TrainSpeedText", "Vel  x1.0",
                             10, TextAnchor.MiddleLeft, new Color(0.35f, 0.80f, 0.45f));
        LE(velTxt.gameObject, pw: 80f);

        // ── Cards horizontales (ancho fijo, alto flexible) ─────────────────
        var cardsRow = MakeGO("CardsRow", panel.transform);
        LE(cardsRow, fh: 1f);
        var crHLG = cardsRow.AddComponent<HorizontalLayoutGroup>();
        crHLG.spacing = 8f;
        crHLG.childControlWidth  = true;  crHLG.childForceExpandWidth  = false;
        crHLG.childControlHeight = true;  crHLG.childForceExpandHeight = true;
        crHLG.childAlignment = TextAnchor.MiddleCenter;

        var slotRoots     = new GameObject[3];
        var slotIcons     = new Image[3];
        var slotNames     = new Text[3];
        var slotCosts     = new Text[3];
        var trainBtns     = new Button[3];
        var trainTimeTxts = new Text[3];

        for (int i = 0; i < 3; i++)
        {
            var card = MakeGO($"UnitSlot{i}", cardsRow.transform);
            LE(card, pw: CardWidth);          // ancho fijo — no se estira
            AddImg(card, BgCard);
            var cardVL = card.AddComponent<VerticalLayoutGroup>();
            cardVL.padding = new RectOffset(6, 6, 6, 6);
            cardVL.spacing = 3f;
            cardVL.childControlWidth  = true;  cardVL.childForceExpandWidth  = true;
            cardVL.childControlHeight = true;  cardVL.childForceExpandHeight = false;
            cardVL.childAlignment = TextAnchor.UpperCenter;

            // Ícono cuadrado fijo 64x64 (resolución nativa de sprites del juego)
            var iconGO  = MakeGO("Icon", card.transform);
            var iconImg = iconGO.AddComponent<Image>();
            iconImg.color          = new Color(0.22f, 0.18f, 0.13f);
            iconImg.preserveAspect = true;
            var iconLE = iconGO.AddComponent<LayoutElement>();
            iconLE.minWidth       = 64f;
            iconLE.minHeight      = 64f;
            iconLE.preferredWidth  = 64f;
            iconLE.preferredHeight = 64f;
            slotIcons[i] = iconImg;

            // Nombre
            slotNames[i] = MakeTxt(card, "UnitName", "Unit", 11,
                                   TextAnchor.MiddleCenter, Color.white);
            slotNames[i].fontStyle = FontStyle.Bold;
            LE(slotNames[i].gameObject, h: 14f);

            // Costo
            slotCosts[i] = MakeTxt(card, "Cost", "—", 10,
                                   TextAnchor.MiddleCenter, GoldText);
            LE(slotCosts[i].gameObject, h: 12f);

            // Tiempo de entrenamiento
            trainTimeTxts[i] = MakeTxt(card, "TrainTime", "⏱ —s", 9,
                                       TextAnchor.MiddleCenter,
                                       new Color(0.55f, 0.80f, 0.55f));
            LE(trainTimeTxts[i].gameObject, h: 11f);

            // Botón recruit
            trainBtns[i] = MakeBtn(card, "TrainBtn", "RECRUIT", GreenTrain);
            LE(trainBtns[i].gameObject, h: 26f);
            var tb = trainBtns[i].GetComponentInChildren<Text>();
            tb.fontSize = 10; tb.fontStyle = FontStyle.Bold;

            slotRoots[i] = card;
        }

        // ── Cola ──────────────────────────────────────────────────────────
        var queueRow = MakeGO("QueueRow", panel.transform);
        LE(queueRow, h: 28f);
        var qrHLG = queueRow.AddComponent<HorizontalLayoutGroup>();
        qrHLG.spacing = 5f;
        qrHLG.childControlWidth  = true;  qrHLG.childForceExpandWidth  = false;
        qrHLG.childControlHeight = true;  qrHLG.childForceExpandHeight = true;
        qrHLG.childAlignment = TextAnchor.MiddleLeft;

        var qLabel = MakeTxt(queueRow, "QLabel", "Cola:", 9,
                             TextAnchor.MiddleLeft, new Color(0.60f, 0.60f, 0.60f));
        LE(qLabel.gameObject, pw: 34f);

        var qSlots = new Image[5];
        for (int i = 0; i < 5; i++)
        {
            var s  = MakeGO($"QSlot{i}", queueRow.transform);
            LE(s, pw: 26f);
            var si = s.AddComponent<Image>();
            si.color = SlotEmpty; si.preserveAspect = true;
            qSlots[i] = si;
        }

        var spacer = MakeGO("Spacer", queueRow.transform);
        LE(spacer, fw: 1f);

        var cancelBtn = MakeBtn(queueRow, "CancelBtn", "Cancelar", RedDanger);
        LE(cancelBtn.gameObject, pw: 78f);
        cancelBtn.GetComponentInChildren<Text>().fontSize = 10;

        // ── Entrenamiento ─────────────────────────────────────────────────
        var trainSec = MakeGO("TrainingSection", panel.transform);
        LE(trainSec, h: 26f);
        var tsVL = trainSec.AddComponent<VerticalLayoutGroup>();
        tsVL.padding = new RectOffset(0, 0, 0, 0); tsVL.spacing = 3f;
        tsVL.childControlWidth  = true;  tsVL.childForceExpandWidth  = true;
        tsVL.childControlHeight = true;  tsVL.childForceExpandHeight = false;

        var trainLbl = MakeTxt(trainSec, "TrainingLabel", "Entrenando: —", 10,
                               TextAnchor.MiddleLeft, new Color(0.85f, 0.85f, 0.85f));
        LE(trainLbl.gameObject, h: 13f);

        var progressBG = MakeGO("ProgressBG", trainSec.transform);
        LE(progressBG, h: 10f);
        AddImg(progressBG, new Color(0.08f, 0.08f, 0.08f, 0.9f));

        var fillGO = MakeGO("Fill", progressBG.transform);
        var fillRT = fillGO.GetComponent<RectTransform>();
        fillRT.anchorMin = new Vector2(0f, 0f);
        fillRT.anchorMax = new Vector2(0f, 1f);
        fillRT.pivot     = new Vector2(0f, 0.5f);
        fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
        AddImg(fillGO, new Color(0.14f, 0.58f, 0.09f));

        // ── Acciones ──────────────────────────────────────────────────────
        var actRow = MakeGO("ActionsRow", panel.transform);
        LE(actRow, h: 36f);
        var arHLG = actRow.AddComponent<HorizontalLayoutGroup>();
        arHLG.spacing = 6f;
        arHLG.childControlWidth  = true;  arHLG.childForceExpandWidth  = false;
        arHLG.childControlHeight = true;  arHLG.childForceExpandHeight = true;
        arHLG.childAlignment = TextAnchor.MiddleCenter;

        var upCostTxt = MakeTxt(actRow, "UpgradeCostText", "100G  50W", 9,
                                TextAnchor.MiddleCenter, GoldText);
        LE(upCostTxt.gameObject, pw: 68f);

        var upgradeBtn = MakeBtn(actRow, "UpgradeBtn", "MEJORAR", GreenTrain);
        upgradeBtn.GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;
        LE(upgradeBtn.gameObject, pw: 96f);

        var evolveBtn = MakeBtn(actRow, "EvolveBtn", "EVOLVE!", BrownMid);
        evolveBtn.GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;
        LE(evolveBtn.gameObject, pw: 88f);
        evolveBtn.gameObject.SetActive(false);

        var moveBtn = MakeBtn(actRow, "MoveBtn", "Mover", BlueMid);
        LE(moveBtn.gameObject, pw: 68f);

        // ── Stat points (oculta) ──────────────────────────────────────────
        var pointsRow = MakeGO("StatPointsRow", panel.transform);
        LE(pointsRow, h: 36f);
        var prHLG = pointsRow.AddComponent<HorizontalLayoutGroup>();
        prHLG.spacing = 6f;
        prHLG.childControlWidth  = true;  prHLG.childForceExpandWidth  = false;
        prHLG.childControlHeight = true;  prHLG.childForceExpandHeight = true;
        var pointsTxt = MakeTxt(pointsRow, "PointsText", "Points: 0", 10,
                                TextAnchor.MiddleLeft, GoldText);
        LE(pointsTxt.gameObject, pw: 76f);
        var addHpBtn = MakeBtn(pointsRow, "AddHpBtn", "+HP", BlueMid);
        LE(addHpBtn.gameObject, pw: 64f);
        pointsRow.SetActive(false);

        // ── Conectar UnitProducerUI ───────────────────────────────────────
        var ui = panel.AddComponent<UnitProducerUI>();
        ui.panel            = panel;
        ui.nameText         = nameTxt;
        ui.levelText        = lvlTxt;
        ui.closeButton      = closeBtn;
        ui.hpText           = hpTxt;
        ui.trainSpeedText   = velTxt;
        ui.trainTimeTexts   = trainTimeTxts;
        ui.slotRoots        = slotRoots;
        ui.slotIcons        = slotIcons;
        ui.slotNames        = slotNames;
        ui.slotCosts        = slotCosts;
        ui.trainButtons     = trainBtns;
        ui.trainingSection  = trainSec;
        ui.trainingLabel    = trainLbl;
        ui.progressFill     = fillRT;
        ui.cancelButton     = cancelBtn;
        ui.queueSlots       = qSlots;
        ui.upgradeButton    = upgradeBtn;
        ui.evolveButton     = evolveBtn;
        ui.moveButton       = moveBtn;
        ui.upgradeCostText  = upCostTxt;
        ui.pointsText       = pointsTxt;
        ui.addHpButton      = addHpBtn;
        ui.statButtonsRow   = pointsRow;

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
        t.text = text;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = size; t.alignment = align; t.color = color;
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
        txt.text = label;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 12; txt.alignment = TextAnchor.MiddleCenter; txt.color = Color.white;
        return btn;
    }

    static Image AddImg(GameObject go, Color color)
    {
        var img = go.AddComponent<Image>(); img.color = color; return img;
    }

    // pw=preferredWidth  h=preferredHeight  fw=flexibleWidth  fh=flexibleHeight
    static void LE(GameObject go, float pw = 0, float h = 0, float fw = 0, float fh = 0)
    {
        var le = go.AddComponent<LayoutElement>();
        if (pw > 0) le.preferredWidth  = pw;
        if (h  > 0) le.preferredHeight = h;
        if (fw > 0) le.flexibleWidth   = fw;
        if (fh > 0) le.flexibleHeight  = fh;
    }
}
