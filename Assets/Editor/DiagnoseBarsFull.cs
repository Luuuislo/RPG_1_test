using UnityEngine;
using UnityEngine.UI;

public class DiagnoseBarsFull
{
    public static void Run()
    {
        foreach (var barName in new[] { "HPBar", "XPBar" })
        {
            var go = GameObject.Find(barName);
            if (go == null) { Debug.LogError($"[BarDiag] {barName}: NOT FOUND"); continue; }

            var bg   = go.transform.Find("Background")?.GetComponent<Image>();
            var fill = go.transform.Find("Fill")?.GetComponent<Image>();
            var rt   = go.transform.Find("Fill")?.GetComponent<RectTransform>();

            Debug.Log($"[BarDiag] {barName}/Background: color={bg?.color} hasSprite={bg?.sprite != null}");
            Debug.Log($"[BarDiag] {barName}/Fill: type={fill?.type} fillAmount={fill?.fillAmount:F3} fillMethod={fill?.fillMethod} fillOrigin={fill?.fillOrigin} hasSprite={fill?.sprite != null} color={fill?.color}");
            if (rt != null)
                Debug.Log($"[BarDiag] {barName}/Fill RectTransform: anchorMin={rt.anchorMin} anchorMax={rt.anchorMax} sizeDelta={rt.sizeDelta} offsetMin={rt.offsetMin} offsetMax={rt.offsetMax}");
        }
    }
}
