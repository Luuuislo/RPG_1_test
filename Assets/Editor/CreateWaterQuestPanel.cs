using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class CreateWaterQuestPanel
{
    [MenuItem("RPGTools/Create Water Quest Panel")]
    static void Create()
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

        // ── Root (full-screen dimmer) ─────────────────────────────────────
        var rootGO = new GameObject("WaterQuestPanel");
        rootGO.transform.SetParent(canvas.transform, false);
        var rootRT       = rootGO.AddComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero;
        rootRT.anchorMax = Vector2.one;
        rootRT.offsetMin = rootRT.offsetMax = Vector2.zero;
        var rootImg      = rootGO.AddComponent<Image>();
        rootImg.color    = new Color(0f, 0f, 0f, 0.65f);

        // ── Dialog box (centered 650×500) ─────────────────────────────────
        var boxGO = new GameObject("DialogBox");
        boxGO.transform.SetParent(rootGO.transform, false);
        var boxRT       = boxGO.AddComponent<RectTransform>();
        boxRT.anchorMin = new Vector2(0.5f, 0.5f);
        boxRT.anchorMax = new Vector2(0.5f, 0.5f);
        boxRT.pivot     = new Vector2(0.5f, 0.5f);
        boxRT.sizeDelta = new Vector2(650f, 500f);
        var boxImg      = boxGO.AddComponent<Image>();
        boxImg.color    = new Color(0.09f, 0.07f, 0.05f, 0.97f);
        var boxVL       = boxGO.AddComponent<VerticalLayoutGroup>();
        boxVL.padding   = new RectOffset(22, 22, 18, 18);
        boxVL.spacing   = 14f;
        boxVL.childControlWidth      = true;
        boxVL.childControlHeight     = true;
        boxVL.childForceExpandWidth  = true;
        boxVL.childForceExpandHeight = false;

        // ── Header ────────────────────────────────────────────────────────
        var headerGO      = new GameObject("Header");
        headerGO.transform.SetParent(boxGO.transform, false);
        var headerLE      = headerGO.AddComponent<LayoutElement>();
        headerLE.preferredHeight = 44f;
        headerLE.flexibleWidth   = 1f;
        var headerHLG    = headerGO.AddComponent<HorizontalLayoutGroup>();
        headerHLG.childControlWidth      = true;
        headerHLG.childControlHeight     = true;
        headerHLG.childForceExpandHeight = true;

        var nameGO  = new GameObject("NPCName");
        nameGO.transform.SetParent(headerGO.transform, false);
        var nameLE  = nameGO.AddComponent<LayoutElement>();
        nameLE.flexibleWidth = 1f;
        var nameText       = nameGO.AddComponent<Text>();
        nameText.text      = "NPC";
        nameText.fontSize  = 22;
        nameText.fontStyle = FontStyle.Bold;
        nameText.color     = new Color(1f, 0.85f, 0.4f);
        nameText.alignment = TextAnchor.MiddleLeft;

        var closeBtnGO = MakeButton(headerGO.transform, "CloseButton", "✕", new Color(0.65f, 0.1f, 0.1f));
        var closeBtnLE = closeBtnGO.GetComponent<LayoutElement>() ?? closeBtnGO.AddComponent<LayoutElement>();
        closeBtnLE.preferredWidth  = 44f;
        closeBtnLE.preferredHeight = 44f;
        closeBtnLE.flexibleWidth   = 0f;

        // ── Divider ───────────────────────────────────────────────────────
        var divGO   = new GameObject("Divider");
        divGO.transform.SetParent(boxGO.transform, false);
        var divLE   = divGO.AddComponent<LayoutElement>();
        divLE.preferredHeight = 1f;
        divLE.flexibleWidth   = 1f;
        var divImg  = divGO.AddComponent<Image>();
        divImg.color = new Color(0.6f, 0.5f, 0.3f, 0.5f);

        // ── Dialogue text ─────────────────────────────────────────────────
        var dialogGO  = new GameObject("DialogueText");
        dialogGO.transform.SetParent(boxGO.transform, false);
        var dialogLE  = dialogGO.AddComponent<LayoutElement>();
        dialogLE.preferredHeight = 100f;
        dialogLE.flexibleWidth   = 1f;
        var dialogText              = dialogGO.AddComponent<Text>();
        dialogText.text             = "...";
        dialogText.fontSize         = 16;
        dialogText.color            = new Color(0.92f, 0.88f, 0.8f);
        dialogText.alignment        = TextAnchor.UpperLeft;
        dialogText.horizontalOverflow = HorizontalWrapMode.Wrap;
        dialogText.verticalOverflow   = VerticalWrapMode.Overflow;
        dialogText.supportRichText    = true;

        // ── Requirements section ──────────────────────────────────────────
        var reqSectionGO = new GameObject("RequirementsSection");
        reqSectionGO.transform.SetParent(boxGO.transform, false);
        var reqSectionLE       = reqSectionGO.AddComponent<LayoutElement>();
        reqSectionLE.preferredHeight = 180f;
        reqSectionLE.flexibleWidth   = 1f;
        var reqSectionImg = reqSectionGO.AddComponent<Image>();
        reqSectionImg.color = new Color(0.04f, 0.04f, 0.04f, 0.6f);

        var reqTxtGO  = new GameObject("RequirementsText");
        reqTxtGO.transform.SetParent(reqSectionGO.transform, false);
        var reqTxtRT  = reqTxtGO.AddComponent<RectTransform>();
        reqTxtRT.anchorMin = Vector2.zero;
        reqTxtRT.anchorMax = Vector2.one;
        reqTxtRT.offsetMin = new Vector2(14f, 10f);
        reqTxtRT.offsetMax = new Vector2(-14f, -10f);
        var reqText               = reqTxtGO.AddComponent<Text>();
        reqText.text              = "";
        reqText.fontSize          = 15;
        reqText.color             = new Color(0.88f, 0.84f, 0.78f);
        reqText.alignment         = TextAnchor.UpperLeft;
        reqText.horizontalOverflow= HorizontalWrapMode.Wrap;
        reqText.verticalOverflow  = VerticalWrapMode.Overflow;
        reqText.supportRichText   = true;

        // ── Button row ────────────────────────────────────────────────────
        var btnRowGO  = new GameObject("ButtonRow");
        btnRowGO.transform.SetParent(boxGO.transform, false);
        var btnRowLE  = btnRowGO.AddComponent<LayoutElement>();
        btnRowLE.preferredHeight = 56f;
        btnRowLE.flexibleWidth   = 1f;
        var btnRowHLG = btnRowGO.AddComponent<HorizontalLayoutGroup>();
        btnRowHLG.spacing              = 14f;
        btnRowHLG.childControlWidth    = true;
        btnRowHLG.childControlHeight   = true;
        btnRowHLG.childForceExpandWidth  = true;
        btnRowHLG.childForceExpandHeight = true;

        var acceptBtnGO = MakeButton(btnRowGO.transform, "AcceptButton",  "ACEPTAR MISIÓN",  new Color(0.12f, 0.52f, 0.18f));
        var turnInBtnGO = MakeButton(btnRowGO.transform, "TurnInButton",  "ENTREGAR MISIÓN", new Color(0.18f, 0.38f, 0.72f));

        // ── WaterQuestUI manager (siempre activo, padre del panel) ───────
        // El panel se desactiva; el manager queda activo para que Awake() corra.
        var managerGO = new GameObject("WaterQuestUIManager");
        managerGO.transform.SetParent(canvas.transform, false);
        rootGO.transform.SetParent(managerGO.transform, false);

        var ui = managerGO.AddComponent<WaterQuestUI>();
        ui.panel            = rootGO;
        ui.npcNameText      = nameText;
        ui.closeButton      = closeBtnGO.GetComponent<Button>();
        ui.dialogueText     = dialogText;
        ui.requirementsText = reqText;
        ui.acceptButton     = acceptBtnGO.GetComponent<Button>();
        ui.turnInButton     = turnInBtnGO.GetComponent<Button>();

        rootGO.SetActive(false);
        Selection.activeGameObject = managerGO;
        EditorUtility.SetDirty(rootGO);
        Debug.Log("[RPGTools] WaterQuestPanel created. Attach WaterQuestNPC to your NPC and WaterAccessTrigger to Trigger_Acceso_Agua.");
    }

    static GameObject MakeButton(Transform parent, string goName, string label, Color color)
    {
        var btnGO = new GameObject(goName);
        btnGO.transform.SetParent(parent, false);
        btnGO.AddComponent<LayoutElement>();
        var img   = btnGO.AddComponent<Image>();
        img.color = color;
        btnGO.AddComponent<Button>();

        var lblGO = new GameObject("Label");
        lblGO.transform.SetParent(btnGO.transform, false);
        var lblRT       = lblGO.AddComponent<RectTransform>();
        lblRT.anchorMin = Vector2.zero;
        lblRT.anchorMax = Vector2.one;
        lblRT.offsetMin = lblRT.offsetMax = Vector2.zero;
        var lbl         = lblGO.AddComponent<Text>();
        lbl.text        = label;
        lbl.fontSize    = 16;
        lbl.fontStyle   = FontStyle.Bold;
        lbl.color       = Color.white;
        lbl.alignment   = TextAnchor.MiddleCenter;

        return btnGO;
    }
}
