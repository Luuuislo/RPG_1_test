using UnityEngine;
using UnityEditor;

public class CreateLevelUpAura
{
    public static void Execute()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null) { Debug.LogError("Player not found."); return; }

        // ── VFX_LevelUpAura — columna saiyan hacia arriba ─────────────────
        Transform existingAura = player.transform.Find("VFX_LevelUpAura");
        if (existingAura != null) Object.DestroyImmediate(existingAura.gameObject);

        GameObject aura = new GameObject("VFX_LevelUpAura");
        aura.transform.SetParent(player.transform, false);
        aura.transform.localPosition = new Vector3(0f, -0.2f, 0f);

        ParticleSystem psAura = aura.AddComponent<ParticleSystem>();
        ParticleSystemRenderer psrAura = aura.GetComponent<ParticleSystemRenderer>();

        var mainA = psAura.main;
        mainA.loop             = false;
        mainA.playOnAwake      = false;
        mainA.duration         = 0.6f;
        mainA.startLifetime    = new ParticleSystem.MinMaxCurve(0.45f, 0.90f);
        mainA.startSpeed       = new ParticleSystem.MinMaxCurve(0.2f, 0.8f);
        mainA.startSize        = new ParticleSystem.MinMaxCurve(0.05f, 0.18f);
        mainA.gravityModifier  = new ParticleSystem.MinMaxCurve(-3.5f, -5.5f); // flotan hacia arriba
        mainA.simulationSpace  = ParticleSystemSimulationSpace.World;
        mainA.maxParticles     = 120;

        // Gradiente: dorado brillante → blanco → sky blue
        var gradientA = new ParticleSystem.MinMaxGradient();
        gradientA.mode = ParticleSystemGradientMode.RandomColor;
        Gradient gA = new Gradient();
        gA.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(1.00f, 0.88f, 0.10f), 0.00f), // oro brillante
                new GradientColorKey(new Color(1.00f, 1.00f, 1.00f), 0.30f), // blanco
                new GradientColorKey(new Color(0.55f, 0.85f, 1.00f), 0.60f), // sky blue
                new GradientColorKey(new Color(1.00f, 0.95f, 0.40f), 1.00f), // oro claro
            },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );
        gradientA.gradient = gA;
        mainA.startColor = gradientA;

        // Burst grande — el corazón del efecto saiyan
        var emA = psAura.emission;
        emA.enabled = true;
        emA.rateOverTime = 0f;
        emA.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0.00f, 70, 1, 1, 0f),
            new ParticleSystem.Burst(0.08f, 40, 1, 1, 0f), // segundo pulso
        });

        // Forma: anillo estrecho alrededor del cuerpo
        var shapeA = psAura.shape;
        shapeA.enabled          = true;
        shapeA.shapeType        = ParticleSystemShapeType.Circle;
        shapeA.radius           = 0.28f;
        shapeA.radiusThickness  = 0f;

        // Velocidad lateral mínima, el grav negativo los empuja hacia arriba en pantalla
        var volA = psAura.velocityOverLifetime;
        volA.enabled = true;
        volA.x = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f);
        volA.y = new ParticleSystem.MinMaxCurve(0.5f, 1.2f);
        volA.space = ParticleSystemSimulationSpace.World;

        // Fade out al final
        var colA = psAura.colorOverLifetime;
        colA.enabled = true;
        Gradient fadeA = new Gradient();
        fadeA.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.55f), new GradientAlphaKey(0f, 1f) }
        );
        colA.color = new ParticleSystem.MinMaxGradient(fadeA);

        // Size fade out
        var solA = psAura.sizeOverLifetime;
        solA.enabled = true;
        AnimationCurve scA = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.5f, 1.1f), new Keyframe(1f, 0f));
        solA.size = new ParticleSystem.MinMaxCurve(1f, scA);

        psrAura.renderMode       = ParticleSystemRenderMode.Billboard;
        psrAura.sortingLayerName = "Foreground";
        psrAura.sortingOrder     = 12;
        psrAura.material         = new Material(Shader.Find("Sprites/Default"));

        // ── VFX_LevelUpFlash — círculo de iluminación en los pies ─────────
        Transform existingFlash = player.transform.Find("VFX_LevelUpFlash");
        if (existingFlash != null) Object.DestroyImmediate(existingFlash.gameObject);

        GameObject flash = new GameObject("VFX_LevelUpFlash");
        flash.transform.SetParent(player.transform, false);
        flash.transform.localPosition = new Vector3(0f, -0.25f, 0f);

        SpriteRenderer flashSr = flash.AddComponent<SpriteRenderer>();
        flashSr.sortingLayerName = "Foreground";
        flashSr.sortingOrder     = 11;
        flashSr.color            = new Color(1f, 0.92f, 0.30f, 0f); // empieza invisible

        // El componente LevelUpFlashEffect genera su propio sprite en Awake
        flash.AddComponent<LevelUpFlashEffect>();

        // ── Marcar escena sucia ───────────────────────────────────────────
        EditorUtility.SetDirty(aura);
        EditorUtility.SetDirty(flash);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[VFX] VFX_LevelUpAura y VFX_LevelUpFlash creados correctamente.");
    }
}
