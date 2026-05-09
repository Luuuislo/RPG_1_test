using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TestBarsPlayMode
{
    public static void DamagePlayer()
    {
        var player = Object.FindFirstObjectByType<Player>();
        if (player == null) { Debug.LogError("[TestBars] Player no encontrado"); return; }
        player.TakeDamage(35);
        Debug.Log($"[TestBars] TakeDamage(35) → hp={player.GetCurrentHealth()}/{player.maxHealth}");
    }

    public static void GiveXp()
    {
        var exp = Object.FindFirstObjectByType<PlayerExperience>();
        if (exp == null) { Debug.LogError("[TestBars] PlayerExperience no encontrado"); return; }
        exp.GainXp(60);
        Debug.Log($"[TestBars] GainXp(60) → xp={exp.CurrentXp}/{exp.XpToNextLevel} lvl={exp.Level}");
    }

    // Aplica daño progresivo para ver el slide y el cambio de color
    public static void HeavyDamage()
    {
        var player = Object.FindFirstObjectByType<Player>();
        if (player == null) { Debug.LogError("[TestBars] Player no encontrado"); return; }
        player.TakeDamage(60);
        Debug.Log($"[TestBars] TakeDamage(60) → hp={player.GetCurrentHealth()}/{player.maxHealth}");
    }

    public static void DiagnoseUI()
    {
        foreach (var barName in new[] { "HPBar", "XPBar" })
        {
            var go = GameObject.Find(barName);
            if (go == null) { Debug.LogError($"[Diag] {barName}: NOT FOUND"); continue; }
            var img = go.transform.Find("Fill")?.GetComponent<Image>();
            var tmp = go.transform.Find("Label")?.GetComponent<TMP_Text>();
            Debug.Log($"[Diag] {barName} fill={img?.fillAmount:F2} color={img?.color} label='{tmp?.text}'");
        }
    }
}
