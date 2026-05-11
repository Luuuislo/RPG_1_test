using System.Collections;
using UnityEngine;

// Añadir este componente al root del árbol/recurso.
// DamageReceiver lo llama automáticamente en cada hit.
public class TreeShakeEffect : MonoBehaviour
{
    [Header("Shake")]
    public float shakeDuration   = 0.40f;
    public float shakeIntensity  = 0.07f;
    [Tooltip("Oscilaciones por segundo")]
    public float shakeFrequency  = 20f;

    [Header("Leaf Particles")]
    public Color leafColorA = new Color(0.15f, 0.70f, 0.10f);
    public Color leafColorB = new Color(0.50f, 0.90f, 0.25f);
    [Range(4, 20)] public int burstCount = 8;

    private Vector3        _origin;
    private ParticleSystem _ps;

    void Awake()
    {
        _origin = transform.localPosition;
        _ps     = BuildParticleSystem();
    }

    public void Play()
    {
        StopAllCoroutines();
        transform.localPosition = _origin;
        StartCoroutine(Shake());
        _ps?.Play();
    }

    IEnumerator Shake()
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float t         = elapsed / shakeDuration;
            float intensity = shakeIntensity * (1f - t);
            float wave      = Mathf.Sin(elapsed * shakeFrequency * Mathf.PI * 2f) * intensity;
            transform.localPosition = _origin + new Vector3(wave, 0f, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = _origin;
    }

    ParticleSystem BuildParticleSystem()
    {
        var go = new GameObject("LeafBurst");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, 0.5f, 0f);

        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main             = ps.main;
        main.loop            = false;
        main.playOnAwake     = false;
        main.duration        = 0.2f;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.04f, 0.11f);
        main.startColor      = new ParticleSystem.MinMaxGradient(leafColorA, leafColorB);
        main.gravityModifier = 0.6f;
        main.maxParticles    = 20;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, burstCount) });

        var shape       = ps.shape;
        shape.enabled   = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle     = 55f;
        shape.radius    = 0.06f;
        // Cone apunta hacia arriba en local space
        shape.rotation  = new Vector3(-90f, 0f, 0f);

        var col     = ps.colorOverLifetime;
        col.enabled = true;
        var grad    = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        col.color = new ParticleSystem.MinMaxGradient(grad);

        var rend         = go.GetComponent<ParticleSystemRenderer>();
        rend.sortingOrder = 10;
        rend.material    = new Material(Shader.Find("Sprites/Default"));

        return ps;
    }
}
