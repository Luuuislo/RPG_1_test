using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

// RPGTools > Create Unit Producer Panel
// Crea el panel de producción de unidades en el Canvas de la escena
// y asigna todas las referencias automáticamente.
public static class CreateUnitProducerPanel
{
    [MenuItem("RPGTools/Create Unit Producer Panel")]
    static void Create()
    {
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("No hay Canvas en la escena."); return; }

        // ── Panel raíz ────────────────────────────────────────────────────
        var panel = UI("UnitProducerPanel", canvas.transform);
        RT(panel, new Vector2(370f, 330f), new Vector2(0.5f, 0.5f),
           new Vector2(-20f, -150f));
        Img(panel, new Color(0.08f, 0.08f, 0.12f, 0.96f));
        var vl = panel.AddComponent<VerticalLayoutGroup>();
        vl.padding  = new RectOffset(8, 8, 6, 6);
        vl.spacing  = 5f;
        vl.childControlWidth = vl.childForceExpandWidth = true;
        vl.childControlHeight = vl.childForceExpandHeight = false;
        vl.childAlignment = TextAnchor.UpperCenter;

        // ── Header ────────────────────────────────────────────────────────
        var header  = Row(panel, "Header", 24f);
        var nameTxt = Txt(header, "NameText", "", 14, TextAnchor.MiddleLeft, Color.white);
        SD(nameTxt.gameObject, 180f, 24f);
        var lvlTxt  = Txt(header, "LevelText", "", 11, TextAnchor.MiddleCenter, new Color(0.8f, 0.8f, 0.4f));
        SD(lvlTxt.gameObject, 110f, 24f);
        var closeBtn = Btn(header, "CloseBtn", "X", 60f, 24f, new Color(0.6f, 0.15f, 0.15f));

        // ── 3 Unit Slots ──────────────────────────────────────────────────
        var unitRow = Row(panel, "UnitRow", 108f);
        unitRow.GetComponent<HorizontalLayoutGroup>().spacing = 6f;

        var slotRoots  = new GameObject[3];
        var slotIcons  = new Image[3];
        var slotNames  = new Text[3];
        var slotCosts  = new Text[3];
        var trainBtns  = new Button[3];

        for (int i = 0; i < 3; i++)
        {
            var slot = UI($"UnitSlot{i}", unitRow.transform);
            SD(slot, 110f, 108f);
            Img(slot, new Color(0.13f, 0.13f, 0.20f, 0.90f));
            var sv = slot.AddComponent<VerticalLayoutGroup>();
            sv.padding = new RectOffset(5, 5, 5, 5); sv.spacing = 2f;
            sv.childControlWidth = sv.childForceExpandWidth = true;
            sv.childControlHeight = sv.childForceExpandHeight = false;
            sv.childAlignment = TextAnchor.UpperCenter;

            var iconGO = UI("Icon", slot.transform); SD(iconGO, 60f, 55f);
            slotIcons[i] = Img(iconGO, new Color(0.4f, 0.4f, 0.6f));

            slotNames[i] = Txt(slot, "UnitName", "Unit", 9, TextAnchor.MiddleCenter, Color.white);
            SD(slotNames[i].gameObject, 0f, 13f);
            slotCosts[i] = Txt(slot, "Cost", "10G", 8, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.2f));
            SD(slotCosts[i].gameObject, 0f, 12f);
            trainBtns[i] = Btn(slot, "TrainBtn", "Train", 0f, 20f, new Color(0.15f, 0.45f, 0.15f));
            slotRoots[i] = slot;
        }

        // ── Training section ──────────────────────────────────────────────
        var trainSec = UI("TrainingSection", panel.transform); SD(trainSec, 0f, 46f);
        var tsVL = trainSec.AddComponent<VerticalLayoutGroup>();
        tsVL.padding = new RectOffset(0, 0, 2, 2); tsVL.spacing = 3f;
        tsVL.childControlWidth = tsVL.childForceExpandWidth = true;
        tsVL.childControlHeight = tsVL.childForceExpandHeight = false;

        var barRow = Row(trainSec, "BarRow", 20f);
        barRow.GetComponent<HorizontalLayoutGroup>().spacing = 6f;
        var trainLbl = Txt(barRow, "TrainingLabel", "Entrenando: —", 10,
                           TextAnchor.MiddleLeft, Color.white);
        SD(trainLbl.gameObject, 130f, 20f);

        var progressBG = UI("ProgressBG", barRow.transform); SD(progressBG, 130f, 16f);
        Img(progressBG, new Color(0.1f, 0.1f, 0.1f, 0.9f));

        var fillGO = UI("Fill", progressBG.transform);
        var fillRT = fillGO.GetComponent<RectTransform>();
        fillRT.anchorMin = new Vector2(0, 0); fillRT.anchorMax = new Vector2(0, 1);
        fillRT.pivot     = new Vector2(0, 0.5f);
        fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
        fillRT.sizeDelta = new Vector2(130f, 0f);
        Img(fillGO, new Color(0.2f, 0.7f, 0.2f));

        var cancelRow = Row(trainSec, "CancelRow", 20f);
        var cancelBtn = Btn(cancelRow, "CancelBtn", "Cancelar último",
                            140f, 20f, new Color(0.5f, 0.15f, 0.1f));

        // ── Queue (5 slots) ───────────────────────────────────────────────
        var queueRow = Row(panel, "QueueRow", 28f);
        var qHlg = queueRow.GetComponent<HorizontalLayoutGroup>();
        qHlg.spacing = 5f; qHlg.childAlignment = TextAnchor.MiddleCenter;
        var qLabel = Txt(queueRow, "QLabel", "Cola:", 9,
                         TextAnchor.MiddleLeft, new Color(0.7f, 0.7f, 0.7f));
        SD(qLabel.gameObject, 32f, 28f);
        var qSlots = new Image[5];
        for (int i = 0; i < 5; i++)
        {
            var s = UI($"QSlot{i}", queueRow.transform); SD(s, 36f, 28f);
            qSlots[i] = Img(s, new Color(0.15f, 0.15f, 0.15f, 0.7f));
        }

        // ── Stat points row ───────────────────────────────────────────────
        var pointsRow = Row(panel, "StatPointsRow", 22f);
        pointsRow.GetComponent<HorizontalLayoutGroup>().spacing = 5f;
        var pointsTxt = Txt(pointsRow, "PointsText", "Points: 0", 10,
                            TextAnchor.MiddleLeft, new Color(1f, 0.9f, 0.3f));
        SD(pointsTxt.gameObject, 80f, 22f);
        var addHpBtn = Btn(pointsRow, "AddHpBtn", "+HP", 60f, 22f,
                           new Color(0.2f, 0.3f, 0.5f));
        pointsRow.SetActive(false);

        // ── Actions row ───────────────────────────────────────────────────
        var actRow = Row(panel, "ActionsRow", 26f);
        actRow.GetComponent<HorizontalLayoutGroup>().spacing = 5f;
        var upCostTxt  = Txt(actRow, "UpgradeCostText", "", 8,
                             TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.2f));
        SD(upCostTxt.gameObject, 70f, 26f);
        var upgradeBtn = Btn(actRow, "UpgradeBtn", "Upgrade", 90f, 26f,
                             new Color(0.2f, 0.4f, 0.2f));
        var evolveBtn  = Btn(actRow, "EvolveBtn",  "Evolve!",  80f, 26f,
                             new Color(0.5f, 0.3f, 0.0f));
        var moveBtn    = Btn(actRow, "MoveBtn",    "Move",     70f, 26f,
                             new Color(0.2f, 0.2f, 0.4f));
        evolveBtn.gameObject.SetActive(false);

        // ── Wire UnitProducerUI ───────────────────────────────────────────
        var ui = panel.AddComponent<UnitProducerUI>();
        ui.panel            = panel;
        ui.nameText         = nameTxt;
        ui.levelText        = lvlTxt;
        ui.closeButton      = closeBtn;
        ui.slotRoots        = slotRoots;
        ui.slotIcons        = slotIcons;
        ui.slotNames        = slotNames;
        ui.slotCosts        = slotCosts;
        ui.trainButtons     = trainBtns;
        ui.trainingSection  = trainSec;
        ui.trainingLabel    = trainLbl;
        ui.progressFill     = fillRT;
        ui.progressMaxWidth = 130f;
        ui.cancelButton     = cancelBtn;
        ui.queueSlots       = qSlots;
        ui.upgradeButton    = upgradeBtn;
        ui.evolveButton     = evolveBtn;
        ui.moveButton       = moveBtn;
        ui.upgradeCostText  = upCostTxt;
        ui.pointsText       = pointsTxt;
        ui.addHpButton      = addHpBtn;
        ui.statButtonsRow   = pointsRow;

        // Assign to BuildingSelector
        var sel = Object.FindFirstObjectByType<BuildingSelector>();
        if (sel != null) { sel.unitProducerUI = ui; EditorUtility.SetDirty(sel); }
        else Debug.LogWarning("BuildingSelector no encontrado — asigna UnitProducerUI manualmente.");

        panel.SetActive(false);
        EditorUtility.SetDirty(panel);
        Selection.activeGameObject = panel;
        Debug.Log("[Unit Producer Panel] Creado correctamente.");
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    // Crea un GO con RectTransform desde el inicio (patrón correcto para UI)
    static GameObject UI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    // Configura RectTransform (el GO ya debe tenerlo)
    static void RT(GameObject go, Vector2 size, Vector2 pivot, Vector2 pos)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta        = size;
        rt.pivot            = pivot;
        rt.anchoredPosition = pos;
    }

    static void SD(GameObject go, float w, float h)
    {
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
    }

    static GameObject Row(GameObject parent, string name, float height)
    {
        var go = UI(name, parent.transform);
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, height);
        var hlg = go.AddComponent<HorizontalLayoutGroup>();
        hlg.childControlWidth  = false; hlg.childControlHeight  = true;
        hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = true;
        return go;
    }

    static Text Txt(GameObject parent, string name, string text,
                    int size, TextAnchor align, Color color)
    {
        var go = UI(name, parent.transform);
        var t  = go.AddComponent<Text>();
        t.text = text; t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = size; t.alignment = align; t.color = color;
        return t;
    }

    static Button Btn(GameObject parent, string name, string label,
                      float w, float h, Color bg)
    {
        var go = UI(name, parent.transform);
        if (w > 0) go.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
        var img = go.AddComponent<Image>(); img.color = bg;
        var btn = go.AddComponent<Button>(); btn.targetGraphic = img;

        var lbl = UI("Text", go.transform);
        var lrt = lbl.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        var txt = lbl.AddComponent<Text>();
        txt.text = label; txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 10; txt.alignment = TextAnchor.MiddleCenter; txt.color = Color.white;
        return btn;
    }

    static Image Img(GameObject go, Color color)
    {
        var img = go.AddComponent<Image>(); img.color = color; return img;
    }
}
