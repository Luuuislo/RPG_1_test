using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class LevelUpFlashEffect : MonoBehaviour
{
    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite  = CreateGlowSprite();
        sr.enabled = false;
    }

    public void Play() => StartCoroutine(FlashRoutine());

    private IEnumerator FlashRoutine()
    {
        sr.enabled = true;
        transform.localScale = Vector3.zero;

        // Expand rápido
        yield return Animate(0.10f, t =>
        {
            float s = Mathf.Lerp(0f, 1.6f, EaseOut(t));
            transform.localScale = new Vector3(s, s, 1f);
            sr.color = new Color(1f, 0.92f, 0.30f, Mathf.Lerp(0f, 0.90f, t));
        });

        // Pulse hacia afuera + fade
        yield return Animate(0.40f, t =>
        {
            float s = Mathf.Lerp(1.6f, 4.0f, EaseOut(t));
            float a = Mathf.Lerp(0.90f, 0f, EaseIn(t));
            transform.localScale = new Vector3(s, s, 1f);
            sr.color = new Color(1f, 0.96f, 0.55f, a);
        });

        sr.enabled = false;
        transform.localScale = Vector3.one;
    }

    private IEnumerator Animate(float duration, System.Action<float> onStep)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            onStep(Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
    }

    // Genera un círculo con glow suave en runtime (no requiere sprite externo)
    private static Sprite CreateGlowSprite()
    {
        const int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float c = size / 2f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(c, c));
            float t = Mathf.Clamp01(1f - d / c);
            float a = Mathf.Pow(t, 1.1f);
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    private static float EaseOut(float t) => 1f - (1f - t) * (1f - t);
    private static float EaseIn(float t)  => t * t;
}
