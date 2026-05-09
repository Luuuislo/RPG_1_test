using UnityEngine;
using UnityEditor;

public class CreateLevelUpParticles
{
    public static void Execute()
    {
        // Find the Player in the scene
        GameObject player = GameObject.Find("Player");
        if (player == null) { Debug.LogError("Player not found in scene."); return; }

        // Remove existing VFX if re-running
        Transform existing = player.transform.Find("VFX_LevelUp");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        // Create particle system object as child of Player
        GameObject vfx = new GameObject("VFX_LevelUp");
        vfx.transform.SetParent(player.transform, false);
        vfx.transform.localPosition = Vector3.zero;

        ParticleSystem ps = vfx.AddComponent<ParticleSystem>();
        ParticleSystemRenderer psr = vfx.GetComponent<ParticleSystemRenderer>();

        // ── Main module ─────────────────────────────────────────────────
        var main = ps.main;
        main.loop             = false;
        main.playOnAwake      = false;
        main.duration         = 0.8f;
        main.startLifetime    = new ParticleSystem.MinMaxCurve(0.5f, 1.1f);
        main.startSpeed       = new ParticleSystem.MinMaxCurve(1.8f, 4.5f);
        main.startSize        = new ParticleSystem.MinMaxCurve(0.04f, 0.14f);
        main.gravityModifier  = new ParticleSystem.MinMaxCurve(-0.4f, -0.9f); // float upward
        main.simulationSpace  = ParticleSystemSimulationSpace.World;
        main.maxParticles     = 80;

        // Random color from gradient (gold / sky-blue / white)
        var gradient = new ParticleSystem.MinMaxGradient();
        gradient.mode = ParticleSystemGradientMode.RandomColor;
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(1.00f, 0.85f, 0.10f), 0.00f),  // gold
                new GradientColorKey(new Color(1.00f, 1.00f, 1.00f), 0.35f),  // white
                new GradientColorKey(new Color(0.40f, 0.78f, 1.00f), 0.65f),  // sky blue
                new GradientColorKey(new Color(1.00f, 0.92f, 0.40f), 1.00f),  // light gold
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f),
            }
        );
        gradient.gradient = g;
        main.startColor = gradient;

        // ── Emission: burst ───────────────────────────────────────────
        var emission = ps.emission;
        emission.enabled    = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, 55, 1, 1, 0f),
        });

        // ── Shape: circle around player ───────────────────────────────
        var shape = ps.shape;
        shape.enabled       = true;
        shape.shapeType     = ParticleSystemShapeType.Circle;
        shape.radius        = 0.35f;
        shape.radiusThickness = 0f;

        // ── Velocity over lifetime: upward drift + spread ─────────────
        var vol = ps.velocityOverLifetime;
        vol.enabled = true;
        vol.x = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
        vol.y = new ParticleSystem.MinMaxCurve(0.8f, 2.2f);
        vol.space = ParticleSystemSimulationSpace.World;

        // ── Size over lifetime: shrink out ────────────────────────────
        var sol = ps.sizeOverLifetime;
        sol.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(0.7f, 0.85f),
            new Keyframe(1f, 0f)
        );
        sol.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // ── Color over lifetime: fade out at end ──────────────────────
        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient fadeOut = new Gradient();
        fadeOut.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f),
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0.0f),
                new GradientAlphaKey(1f, 0.6f),
                new GradientAlphaKey(0f, 1.0f),
            }
        );
        col.color = new ParticleSystem.MinMaxGradient(fadeOut);

        // ── Renderer: use default particle material ───────────────────
        psr.renderMode  = ParticleSystemRenderMode.Billboard;
        psr.sortingLayerName = "Foreground"; // adjust if your layers differ
        psr.sortingOrder = 10;

        Material defaultParticle = new Material(Shader.Find("Particles/Standard Unlit"));
        if (defaultParticle != null && defaultParticle.shader.name != "Hidden/InternalErrorShader")
        {
            psr.material = defaultParticle;
        }
        else
        {
            // Fallback: use Sprites/Default
            Material fallback = new Material(Shader.Find("Sprites/Default"));
            psr.material = fallback;
        }

        // Mark scene dirty
        EditorUtility.SetDirty(vfx);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[VFX_LevelUp] Particle system created successfully on Player.");
    }
}
