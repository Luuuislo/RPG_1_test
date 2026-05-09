using UnityEngine;
using UnityEngine.UI;

public class FixBarSprites
{
    public static void Run()
    {
        var sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        if (sprite == null)
        {
            // Fallback: create a plain white 4x4 sprite
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var pixels = new Color[16];
            for (int i = 0; i < 16; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 100f);
            Debug.Log("[FixBars] Built-in UISprite not found, created white sprite as fallback");
        }
        else
        {
            Debug.Log($"[FixBars] Found built-in sprite: {sprite.name}");
        }

        foreach (var barName in new[] { "HPBar", "XPBar" })
        {
            var go = GameObject.Find(barName);
            if (go == null) { Debug.LogError($"[FixBars] {barName} not found"); continue; }

            var bgImg   = go.transform.Find("Background")?.GetComponent<Image>();
            var fillImg = go.transform.Find("Fill")?.GetComponent<Image>();

            if (bgImg != null)
            {
                bgImg.sprite = sprite;
                Debug.Log($"[FixBars] {barName}/Background sprite assigned");
            }
            if (fillImg != null)
            {
                fillImg.sprite     = sprite;
                fillImg.type       = Image.Type.Filled;
                fillImg.fillMethod = Image.FillMethod.Horizontal;
                fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
                Debug.Log($"[FixBars] {barName}/Fill sprite assigned — type={fillImg.type} fillAmount={fillImg.fillAmount:F3}");
            }
        }
    }
}
