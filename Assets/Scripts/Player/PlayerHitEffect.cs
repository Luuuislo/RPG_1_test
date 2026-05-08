using UnityEngine;
using System.Collections;

public class PlayerHitEffect : MonoBehaviour
{
    [Header("Hit Flash")]
    public float flashDuration = 0.12f;

    [Header("Blood Particles")]
    public ParticleSystem bloodParticles;

    private SpriteRenderer sr;
    private Coroutine flashRoutine;
    private static readonly Color HitColor = new Color(1f, 0.15f, 0.15f, 1f);

    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        if (bloodParticles == null)
            bloodParticles = CreateBloodParticles();
    }

    public void Play()
    {
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(Flash());
        bloodParticles?.Play();
    }

    private IEnumerator Flash()
    {
        sr.color = HitColor;
        float half = flashDuration * 0.5f;
        yield return new WaitForSeconds(half);

        float t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            sr.color = Color.Lerp(HitColor, Color.white, t / half);
            yield return null;
        }
        sr.color = Color.white;
        flashRoutine = null;
    }

    private ParticleSystem CreateBloodParticles()
    {
        var go = new GameObject("VFX_HitBlood");
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;

        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.loop           = false;
        main.playOnAwake    = false;
        main.duration       = 0.3f;
        main.startLifetime  = new ParticleSystem.MinMaxCurve(0.25f, 0.45f);
        main.startSpeed     = new ParticleSystem.MinMaxCurve(1.8f, 3.5f);
        main.startSize      = new ParticleSystem.MinMaxCurve(0.05f, 0.14f);
        main.gravityModifier = 0.6f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // Dark red to bright red
        var startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.65f, 0.02f, 0.02f, 1f),
            new Color(0.90f, 0.10f, 0.10f, 1f)
        );
        main.startColor = startColor;

        // Burst of 6 particles at t=0
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 6, 8) });

        // Spread in all directions
        var shape = ps.shape;
        shape.enabled     = true;
        shape.shapeType   = ParticleSystemShapeType.Circle;
        shape.radius      = 0.15f;

        // Fade out over lifetime
        var colorOverLife = ps.colorOverLifetime;
        colorOverLife.enabled = true;
        var fadeGradient = new Gradient();
        fadeGradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLife.color = new ParticleSystem.MinMaxGradient(fadeGradient);

        return ps;
    }
}
