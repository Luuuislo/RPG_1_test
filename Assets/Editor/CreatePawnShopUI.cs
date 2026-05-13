using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class CreatePawnShopUI
{
    [MenuItem("RPGTools/Create Pawn Shop UI")]
    static void Create() => Create_Internal();

    public static void Create_Internal()
    {
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            var cGO = new GameObject("Canvas");
            canvas = cGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            cGO.AddComponent<CanvasScaler>();
            cGO.AddComponent<GraphicRaycaster>();
        }

        // ── Manager (siempre activo) ───────────────────────────────────────
        var managerGO = new GameObject("PawnShopUIManager");
        managerGO.transform.SetParent(canvas.transform, false);
        var manRT       = managerGO.AddComponent<RectTransform>();
        manRT.anchorMin = Vector2.zero;
        manRT.anchorMax = Vector2.one;
        manRT.offsetMin = manRT.offsetMax = Vector2.zero;

        // ── Panel (full-screen dimmer, toggled) ───────────────────────────
        var panelGO  = new GameObject("PawnShopPanel");
        panelGO.transform.SetParent(managerGO.transform, false);
        var panelRT       = panelGO.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = panelRT.offsetMax = Vector2.zero;
        var dimImg   = panelGO.AddComponent<Image>();
        dimImg.color = new Color(0f, 0f, 0f, 0.65f);

        // ── Dialog box 800×500 centrado ───────────────────────────────────
        var boxGO = new GameObject("DialogBox");
        boxGO.transform.SetParent(panelGO.transform, false);
        var boxRT       = boxGO.AddComponent<RectTransform>();
        boxRT.anchorMin = new Vector2(0.5f, 0.5f);
        boxRT.anchorMax = new Vector2(0.5f, 0.5f);
        boxRT.pivot     = new Vector2(0.5f, 0.5f);
        boxRT.sizeDelta = new Vector2(800f, 500f);
        var boxImg      = boxGO.AddComponent<Image>();
        boxImg.color    = new Color(0.1f, 0.08f, 0.06f, 0.98f);
        var boxVL       = boxGO.AddComponent<VerticalLayoutGroup>();
        boxVL.padding   = new RectOffset(0, 0, 0, 0);
        boxVL.spacing   = 0f;
        boxVL.childControlWidth      = true;
        boxVL.childControlHeight     = true;
        boxVL.childForceExpandWidth  = true;
        boxVL.childForceExpandHeight = false;

        // ── Header ────────────────────────────────────────────────────────
        var headerGO = new GameObject("Header");
        headerGO.transform.SetParent(boxGO.transform, false);
        var headerLE = headerGO.AddComponent<LayoutElement>();
        headerLE.preferredHeight = 48f;
        headerLE.flexibleWidth   = 1f;
        var headerImg = headerGO.AddComponent<Image>();
        headerImg.color = new Color(0.07f, 0.05f, 0.03f, 1f);
        var hlg = headerGO.AddComponent<HorizontalLayoutGroup>();
        hlg.padding  = new RectOffset(16, 8, 0, 0);
        hlg.spacing  = 8f;
        hlg.childControlWidth      = true;
        hlg.childControlHeight     = false;
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = false;

        var titleGO  = new GameObject("Title");
        titleGO.transform.SetParent(headerGO.transform, false);
        var titleLE  = titleGO.AddComponent<LayoutElement>();
        titleLE.flexibleWidth = 1f;
        var titleTxt = titleGO.AddComponent<TextMeshProUGUI>();
        titleTxt.text      = "PAWN — Constructor";
        titleTxt.fontSize  = 20f;
        titleTxt.fontStyle = FontStyles.Bold;
        titleTxt.color     = new Color(1f, 0.85f, 0.4f);
        titleTxt.alignment = TextAlignmentOptions.MidlineLeft;

        var closeBtnGO = MakeButton(headerGO.transform, "CloseButton", "X",
                                    new Color(0.55f, 0.08f, 0.08f, 1f), 48f, 48f);

        // ── Divider horizontal ────────────────────────────────────────────
        MakeDividerH(boxGO.transform);

        // ── Body (HLG: lista izq + detalle der) ──────────────────────────
        var bodyGO = new GameObject("Body");
        bodyGO.transform.SetParent(boxGO.transform, false);
        var bodyLE = bodyGO.AddComponent<LayoutElement>();
        bodyLE.flexibleHeight = 1f;
        bodyLE.flexibleWidth  = 1f;
        var bodyHLG = bodyGO.AddComponent<HorizontalLayoutGroup>();
        bodyHLG.padding  = new RectOffset(0, 0, 0, 0);
        bodyHLG.spacing  = 0f;
        bodyHLG.childControlWidth      = true;
        bodyHLG.childControlHeight     = true;
        bodyHLG.childForceExpandWidth  = false;
        bodyHLG.childForceExpandHeight = true;

        // ── Lista de edificios (izquierda, 200px) ─────────────────────────
        var listGO = new GameObject("BuildingList");
        listGO.transform.SetParent(bodyGO.transform, false);
        var listLE = listGO.AddComponent<LayoutElement>();
        listLE.preferredWidth  = 200f;
        listLE.flexibleHeight  = 1f;
        listGO.AddComponent<Image>().color = new Color(0.07f, 0.055f, 0.04f, 1f);

        var scrollGO  = new GameObject("Scroll");
        scrollGO.transform.SetParent(listGO.transform, false);
        var scrollRT  = scrollGO.AddComponent<RectTransform>();
        scrollRT.anchorMin = Vector2.zero;
        scrollRT.anchorMax = Vector2.one;
        scrollRT.offsetMin = scrollRT.offsetMax = Vector2.zero;
        var scroll    = scrollGO.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical   = true;

        var vpGO  = new GameObject("Viewport");
        vpGO.transform.SetParent(scrollGO.transform, false);
        var vpRT  = vpGO.AddComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero;
        vpRT.anchorMax = Vector2.one;
        vpRT.offsetMin = vpRT.offsetMax = Vector2.zero;
        vpGO.AddComponent<RectMask2D>();
        scroll.viewport = vpRT;

        var contentGO  = new GameObject("Content");
        contentGO.transform.SetParent(vpGO.transform, false);
        var contentRT  = contentGO.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 1f);
        contentRT.anchorMax = new Vector2(1f, 1f);
        contentRT.pivot     = new Vector2(0.5f, 1f);
        contentRT.sizeDelta = new Vector2(0f, 0f);
        var contentVLG  = contentGO.AddComponent<VerticalLayoutGroup>();
        contentVLG.padding = new RectOffset(0, 0, 0, 0);
        contentVLG.spacing = 1f;
        contentVLG.childControlWidth      = true;
        contentVLG.childControlHeight     = true;
        contentVLG.childForceExpandWidth  = true;
        contentVLG.childForceExpandHeight = false;
        var csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.content = contentRT;

        // ── Divider vertical ──────────────────────────────────────────────
        MakeDividerV(bodyGO.transform);

        // ── Panel de detalle (derecha) ────────────────────────────────────
        var detailGO  = new GameObject("DetailPanel");
        detailGO.transform.SetParent(bodyGO.transform, false);
        var detailLE  = detailGO.AddComponent<LayoutElement>();
        detailLE.flexibleWidth  = 1f;
        detailLE.flexibleHeight = 1f;
        detailGO.AddComponent<Image>().color = new Color(0.08f, 0.065f, 0.05f, 1f);
        var detailVL  = detailGO.AddComponent<VerticalLayoutGroup>();
        detailVL.padding = new RectOffset(18, 18, 16, 16);
        detailVL.spacing = 10f;
        detailVL.childControlWidth      = true;
        detailVL.childControlHeight     = true;
        detailVL.childForceExpandWidth  = true;
        detailVL.childForceExpandHeight = false;

        var dnGO  = MakeTMPText(detailGO.transform, "BuildingName",   "Selecciona un edificio", 20f, FontStyles.Bold,   new Color(1f, 0.85f, 0.4f),    32f);
        var ddGO  = MakeTMPText(detailGO.transform, "Description",    "...",                     13f, FontStyles.Italic, new Color(0.82f, 0.78f, 0.72f), 40f);
        var reqGO = MakeTMPText(detailGO.transform, "Requirements",   "",                        13f, FontStyles.Normal, new Color(0.88f, 0.84f, 0.78f), 0f);
        reqGO.GetComponent<TextMeshProUGUI>().richText = true;
        reqGO.GetComponent<LayoutElement>().flexibleHeight = 1f;

        var unlockGO = MakeButton(detailGO.transform, "UnlockButton", "DESBLOQUEAR",
                                  new Color(0.12f, 0.45f, 0.18f, 1f), 46f);
        unlockGO.GetComponent<LayoutElement>().flexibleWidth = 1f;

        // ── Wire PawnShopUI ───────────────────────────────────────────────
        var ui = managerGO.AddComponent<PawnShopUI>();
        ui.panel             = panelGO;
        ui.closeButton       = closeBtnGO.GetComponent<Button>();
        ui.cardContainer     = contentGO.transform;
        ui.detailPanel       = detailGO;
        ui.detailNameText    = dnGO.GetComponent<TextMeshProUGUI>();
        ui.detailDescText    = ddGO.GetComponent<TextMeshProUGUI>();
        ui.requirementsText  = reqGO.GetComponent<TextMeshProUGUI>();
        ui.unlockButton      = unlockGO.GetComponent<Button>();
        ui.unlockButtonLabel = unlockGO.GetComponentInChildren<TextMeshProUGUI>();

        panelGO.SetActive(false);
        Selection.activeGameObject = managerGO;
        EditorUtility.SetDirty(managerGO);
        Debug.Log("[RPGTools] PawnShopUI creado. Asigna los BuildingUnlockData en PawnShopUI.buildings.");
    }

    static void MakeDividerH(Transform parent)
    {
        var go = new GameObject("DividerH");
        go.transform.SetParent(parent, false);
        go.AddComponent<LayoutElement>().preferredHeight = 1f;
        go.AddComponent<Image>().color = new Color(0.55f, 0.45f, 0.25f, 0.5f);
    }

    static void MakeDividerV(Transform parent)
    {
        var go = new GameObject("DividerV");
        go.transform.SetParent(parent, false);
        go.AddComponent<LayoutElement>().preferredWidth = 1f;
        go.AddComponent<Image>().color = new Color(0.55f, 0.45f, 0.25f, 0.5f);
    }

    static GameObject MakeButton(Transform parent, string goName, string label,
                                  Color color, float height = 44f, float width = -1f)
    {
        var go  = new GameObject(goName);
        go.transform.SetParent(parent, false);
        var le  = go.AddComponent<LayoutElement>();
        le.preferredHeight = height;
        if (width > 0f) le.preferredWidth = width;
        var img = go.AddComponent<Image>();
        img.color = color;
        go.AddComponent<Button>();

        var lblGO = new GameObject("Label");
        lblGO.transform.SetParent(go.transform, false);
        var rt    = lblGO.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var tmp   = lblGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 15f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        return go;
    }

    static GameObject MakeTMPText(Transform parent, string goName, string text, float size,
                                   FontStyles style, Color color, float height)
    {
        var go  = new GameObject(goName);
        go.transform.SetParent(parent, false);
        var le  = go.AddComponent<LayoutElement>();
        if (height > 0f) le.preferredHeight = height;
        le.flexibleWidth = 1f;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text       = text;
        tmp.fontSize   = size;
        tmp.fontStyle  = style;
        tmp.color      = color;
        tmp.alignment  = TextAlignmentOptions.TopLeft;
        tmp.enableWordWrapping = true;
        tmp.overflowMode = TextOverflowModes.Overflow;
        return go;
    }
}
