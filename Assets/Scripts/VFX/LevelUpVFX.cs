using System.Collections;
using UnityEngine;
using TMPro;

// Efecto de level up completamente autónomo. Llámalo con:
//   LevelUpVFX.Spawn(transform.position);
// Se auto-destruye al terminar.
public class LevelUpVFX : MonoBehaviour
{
    public static void Spawn(Vector3 worldPosition)
    {
        var go = new GameObject("LevelUpVFX");
        go.transform.position = worldPosition;
        go.AddComponent<LevelUpVFX>().Boot();
    }

    private const int Segments = 36;

    private LineRenderer   _ringA;
    private LineRenderer   _ringB;
    private ParticleSystem _sparks;

    void Boot()
    {
        _ringA  = MakeRing("RingA", 0.10f, new Color(1f, 0.95f, 0.15f, 1f));
        _ringB  = MakeRing("RingB", 0.055f, new Color(1f, 0.65f, 0.05f, 1f));
        _sparks = MakeSparks();

        _sparks.Play();
        StartCoroutine(AnimateRing(_ringA, 0f,    1.8f, 0.10f, 0.55f));
        StartCoroutine(AnimateRing(_ringB, 0.08f, 2.7f, 0.055f, 0.72f));
        StartCoroutine(AnimateLabel());
        StartCoroutine(SelfDestruct(2.2f));
    }

    // ── Ring ─────────────────────────────────────────────────────────────

    LineRenderer MakeRing(string name, float width, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);

        var lr             = go.AddComponent<LineRenderer>();
        lr.useWorldSpace   = true;
        lr.loop            = true;
        lr.positionCount   = Segments;
        lr.widthMultiplier = width;
        lr.material        = new Material(Shader.Find("Sprites/Default")) { color = color };
        lr.sortingOrder    = 15;
        return lr;
    }

    IEnumerator AnimateRing(LineRenderer ring, float delay, float maxR, float startWidth, float duration)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        Color baseColor = ring.material.color;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float r = Mathf.Lerp(0.05f, maxR, EaseOut(t));
            float a = 1f - t * t;

            ring.material.color    = new Color(baseColor.r, baseColor.g, baseColor.b, a);
            ring.widthMultiplier   = Mathf.Lerp(startWidth, startWidth * 0.15f, t);
            SetRingPositions(ring, r);

            elapsed += Time.deltaTime;
            yield return null;
        }
        ring.gameObject.SetActive(false);
    }

    void SetRingPositions(LineRenderer ring, float r)
    {
        Vector3 center = transform.position;
        for (int i = 0; i < Segments; i++)
        {
            float angle = (float)i / Segments * Mathf.PI * 2f;
            ring.SetPosition(i, center + new Vector3(
                Mathf.Cos(angle) * r,
                Mathf.Sin(angle) * r * 0.55f, // aplanado = perspectiva isométrica
                0f));
        }
    }

    // ── Sparks ───────────────────────────────────────────────────────────

    ParticleSystem MakeSparks()
    {
        var go = new GameObject("Sparks");
        go.transform.SetParent(transform, false);

        var ps           = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main             = ps.main;
        main.loop            = false;
        main.playOnAwake     = false;
        main.duration        = 0.3f;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.9f, 1.6f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(2.5f, 5.5f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.06f, 0.18f);
        main.startColor      = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.95f, 0.2f), new Color(1f, 0.55f, 0.05f));
        main.gravityModifier = -0.25f;  // flotan un poco hacia arriba
        main.maxParticles    = 50;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 30, 45) });

        var shape       = ps.shape;
        shape.enabled   = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius    = 0.25f;

        var col     = ps.colorOverLifetime;
        col.enabled = true;
        var grad    = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        col.color = new ParticleSystem.MinMaxGradient(grad);

        var rend          = go.GetComponent<ParticleSystemRenderer>();
        rend.sortingOrder = 16;
        rend.material     = new Material(Shader.Find("Sprites/Default"));

        return ps;
    }

    // ── "LEVEL UP!" label ────────────────────────────────────────────────

    IEnumerator AnimateLabel()
    {
        var go = new GameObject("Label");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, 0.4f, 0f);

        var tmp            = go.AddComponent<TextMeshPro>();
        tmp.text           = "LEVEL UP!";
        tmp.fontSize       = 3.8f;
        tmp.fontStyle      = FontStyles.Bold;
        tmp.alignment      = TextAlignmentOptions.Center;
        tmp.color          = new Color(1f, 0.95f, 0.15f);
        tmp.outlineWidth   = 0.28f;
        tmp.outlineColor   = new Color32(110, 50, 0, 255);
        tmp.sortingOrder   = 20;

        // Pop-in
        float scaleIn = 0.18f, t0 = 0f;
        go.transform.localScale = Vector3.zero;
        while (t0 < scaleIn)
        {
            t0 += Time.deltaTime;
            float s = EaseOutBack(Mathf.Clamp01(t0 / scaleIn));
            go.transform.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        go.transform.localScale = Vector3.one;

        // Float up + fade
        float duration = 1.8f, elapsed = 0f;
        Vector3 start = go.transform.position;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            go.transform.position = start + new Vector3(0f, 1.6f * EaseOut(t), 0f);
            tmp.alpha = t < 0.45f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.45f) / 0.55f);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    IEnumerator SelfDestruct(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    static float EaseOut(float t)     => 1f - (1f - t) * (1f - t);
    static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
