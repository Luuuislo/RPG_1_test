using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class WireUiManager
{
    public static void Execute()
    {
        var uiGo = GameObject.Find("---UI---/Canvas/UiManager");
        if (uiGo == null) { Debug.LogError("[WireUiManager] UiManager GameObject not found."); return; }

        var ui = uiGo.GetComponent<UiManager>();
        if (ui == null) { Debug.LogError("[WireUiManager] UiManager component not found."); return; }

        // Level number text (existing)
        var lvlGo = GameObject.Find("---UI---/Canvas/++Player++/++BannerNameLvl++/Player_Text_Number_Lv");
        if (lvlGo != null) ui.levelNumberText = lvlGo.GetComponent<TextMeshProUGUI>();

        // Stats panel
        string sp = "---UI---/Canvas/++Player++/StatsPanel";

        var ptsGo = GameObject.Find(sp + "/StatPointsText");
        if (ptsGo != null) ui.statPointsText = ptsGo.GetComponent<TextMeshProUGUI>();

        var atkValGo = GameObject.Find(sp + "/AtkValue");
        if (atkValGo != null) ui.atkValueText = atkValGo.GetComponent<TextMeshProUGUI>();

        var spdValGo = GameObject.Find(sp + "/SpdValue");
        if (spdValGo != null) ui.spdValueText = spdValGo.GetComponent<TextMeshProUGUI>();

        var hpValGo = GameObject.Find(sp + "/HpValue");
        if (hpValGo != null) ui.hpValueText = hpValGo.GetComponent<TextMeshProUGUI>();

        var atkBtnGo = GameObject.Find(sp + "/AtkButton");
        if (atkBtnGo != null) ui.atkButton = atkBtnGo.GetComponent<Button>();

        var spdBtnGo = GameObject.Find(sp + "/SpdButton");
        if (spdBtnGo != null) ui.spdButton = spdBtnGo.GetComponent<Button>();

        var hpBtnGo = GameObject.Find(sp + "/HpButton");
        if (hpBtnGo != null) ui.hpButton = hpBtnGo.GetComponent<Button>();

        EditorUtility.SetDirty(uiGo);
        Debug.Log("[WireUiManager] All references wired successfully.");
    }
}
