using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.AI;

// RPGTools > Create Ally Unit Prefabs
// Crea Warrior, Archer, Monk y Lancer como prefabs NPC aliados completos.
public static class CreateAllyPrefabs
{
    const string SpritesBase = "Assets/Sprites/Tiny Swords (Free Pack)/Units/Blue Units";
    const string AnimOut     = "Assets/Animations/WAW";
    const string PrefabOut   = "Assets/Prefabs/CHARACTERS";

    [MenuItem("RPGTools/Create Ally Unit Prefabs")]
    static void Create()
    {
        if (!AssetDatabase.IsValidFolder(AnimOut))
            AssetDatabase.CreateFolder("Assets/Animations", "WAW");

        var arrowPrefab      = BuildArrowPrefab();
        var healEffectPrefab = BuildHealEffectPrefab();

        BuildWarrior();
        BuildLancer();
        BuildArcher(arrowPrefab);
        BuildMonk(healEffectPrefab);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Ally Prefabs] Warrior, Lancer, Archer, Monk creados en " + PrefabOut);
    }

    // ── Warrior ───────────────────────────────────────────────────────────

    static void BuildWarrior()
    {
        var ctrl = MakeController("WarriorAllyController",
            MakeClip($"{SpritesBase}/Warrior/Warrior_Idle.png",    6, 12f, true),
            MakeClip($"{SpritesBase}/Warrior/Warrior_Run.png",     6, 14f, true),
            MakeClip($"{SpritesBase}/Warrior/Warrior_Attack1.png", 4, 12f, false));

        MakePrefab("Warrior_Ally", ctrl, $"{SpritesBase}/Warrior/Warrior_Idle.png",
            go => {
                var u = go.AddComponent<AllyUnit>();
                u.maxHp = 150f; u.attackDamage = 20f; u.attackRange = 1.4f;
                u.detectRange = 8f; u.attackCooldown = 1.4f; u.moveSpeed = 3.2f;
            });
    }

    // ── Lancer ────────────────────────────────────────────────────────────

    static void BuildLancer()
    {
        var ctrl = MakeController("LancerAllyController",
            MakeClip($"{SpritesBase}/Lancer/Lancer_Idle.png",         6, 10f, true),
            MakeClip($"{SpritesBase}/Lancer/Lancer_Run.png",          6, 14f, true),
            MakeClip($"{SpritesBase}/Lancer/Lancer_Right_Attack.png", 3, 12f, false));

        MakePrefab("Lancer_Ally", ctrl, $"{SpritesBase}/Lancer/Lancer_Idle.png",
            go => {
                var u = go.AddComponent<AllyUnit>();
                u.maxHp = 120f; u.attackDamage = 18f; u.attackRange = 1.8f;
                u.detectRange = 9f; u.attackCooldown = 1.6f; u.moveSpeed = 3.5f;
            });
    }

    // ── Archer ────────────────────────────────────────────────────────────

    static void BuildArcher(GameObject arrowPrefab)
    {
        var ctrl = MakeController("ArcherAllyController",
            MakeClip($"{SpritesBase}/Archer/Archer_Idle.png",  6, 10f, true),
            MakeClip($"{SpritesBase}/Archer/Archer_Run.png",   4, 14f, true),
            MakeClip($"{SpritesBase}/Archer/Archer_Shoot.png", 8, 14f, false));

        // Cargar sprite de flecha para asignar en el Archer
        string arrowTex = $"{SpritesBase}/Archer/Arrow.png";
        AssetDatabase.ImportAsset(arrowTex, ImportAssetOptions.ForceSynchronousImport);
        Sprite arrowSprite = null;
        foreach (var a in AssetDatabase.LoadAllAssetsAtPath(arrowTex))
            if (a is Sprite s) { arrowSprite = s; break; }

        MakePrefab("Archer_Ally", ctrl, $"{SpritesBase}/Archer/Archer_Idle.png",
            go => {
                var u = go.AddComponent<AllyArcher>();
                u.maxHp = 80f; u.attackDamage = 15f; u.attackRange = 6f;
                u.detectRange = 10f; u.attackCooldown = 2f; u.moveSpeed = 3f;
                u.arrowPrefab = arrowPrefab; u.arrowSprite = arrowSprite;
                u.arrowSpeed = 10f; u.minDistance = 3f;
            });
    }

    // ── Monk ──────────────────────────────────────────────────────────────

    static void BuildMonk(GameObject healEffectPrefab)
    {
        var ctrl = MakeController("MonkAllyController",
            MakeClip($"{SpritesBase}/Monk/Idle.png", 6, 10f, true),
            MakeClip($"{SpritesBase}/Monk/Run.png",  4, 14f, true),
            MakeClip($"{SpritesBase}/Monk/Heal.png", 9, 12f, false));

        MakePrefab("Monk_Ally", ctrl, $"{SpritesBase}/Monk/Idle.png",
            go => {
                var m = go.AddComponent<AllyMonk>();
                m.maxHp = 70f; m.attackDamage = 0f; m.attackRange = 1.8f;
                m.detectRange = 0f; m.attackCooldown = 2.5f; m.moveSpeed = 2.8f;
                m.healAmount = 30f; m.healRange = 1.8f; m.healDetectRange = 10f;
                m.healEffectPrefab = healEffectPrefab;
            });
    }

    // ── AnimatorController builder ────────────────────────────────────────

    static AnimatorController MakeController(string ctrlName,
        AnimationClip idle, AnimationClip run, AnimationClip attack)
    {
        string path = $"{AnimOut}/{ctrlName}.controller";

        // Borrar el asset existente para evitar state machine corrompido
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null)
            AssetDatabase.DeleteAsset(path);

        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);

        // Parámetros
        ctrl.AddParameter("isRunning", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("DoAttack",  AnimatorControllerParameterType.Trigger);

        var sm = ctrl.layers[0].stateMachine;

        var stIdle   = sm.AddState("Idle");   stIdle.motion   = idle;
        var stRun    = sm.AddState("Run");    stRun.motion    = run;
        var stAttack = sm.AddState("Attack"); stAttack.motion = attack;

        sm.defaultState = stIdle;

        // Idle ↔ Run
        var toRun  = stIdle.AddTransition(stRun);
        toRun.hasExitTime = false; toRun.duration = 0.05f;
        toRun.AddCondition(AnimatorConditionMode.If, 0, "isRunning");

        var toIdle = stRun.AddTransition(stIdle);
        toIdle.hasExitTime = false; toIdle.duration = 0.05f;
        toIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "isRunning");

        // AnyState → Attack
        var toAtk = sm.AddAnyStateTransition(stAttack);
        toAtk.hasExitTime = false; toAtk.duration = 0.05f;
        toAtk.canTransitionToSelf = false;
        toAtk.AddCondition(AnimatorConditionMode.If, 0, "DoAttack");

        // Attack → Idle (al terminar)
        var exitAtk = stAttack.AddExitTransition();
        exitAtk.hasExitTime = true; exitAtk.exitTime = 1f;
        exitAtk.hasFixedDuration = false; exitAtk.duration = 0.05f;

        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssetIfDirty(ctrl);
        return ctrl;
    }

    // ── AnimationClip builder ─────────────────────────────────────────────

    static AnimationClip MakeClip(string texPath, int maxFrames, float fps, bool loop)
    {
        // Nombre único por textura
        string clipName = System.IO.Path.GetFileNameWithoutExtension(texPath);
        string outPath  = $"{AnimOut}/{clipName}.anim";

        var sprites = new List<Sprite>();
        foreach (var a in AssetDatabase.LoadAllAssetsAtPath(texPath))
            if (a is Sprite s) sprites.Add(s);

        if (sprites.Count == 0)
        {
            Debug.LogError($"[CreateAllyPrefabs] Sin sprites en: {texPath}");
            var empty = new AnimationClip();
            AssetDatabase.CreateAsset(empty, outPath);
            return empty;
        }

        int count = Mathf.Min(maxFrames, sprites.Count);

        var clip     = new AnimationClip();
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        var keys    = new ObjectReferenceKeyframe[count];
        float dt    = 1f / fps;
        for (int i = 0; i < count; i++)
            keys[i] = new ObjectReferenceKeyframe { time = i * dt, value = sprites[i] };

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        // Reemplazar asset existente
        if (AssetDatabase.LoadAssetAtPath<AnimationClip>(outPath) != null)
            AssetDatabase.DeleteAsset(outPath);
        AssetDatabase.CreateAsset(clip, outPath);
        return clip;
    }

    // ── Prefab builder ────────────────────────────────────────────────────

    static void MakePrefab(string prefabName, AnimatorController ctrl,
                            string idleTexPath, System.Action<GameObject> setup)
    {
        string path = $"{PrefabOut}/{prefabName}.prefab";

        Sprite first = null;
        foreach (var a in AssetDatabase.LoadAllAssetsAtPath(idleTexPath))
            if (a is Sprite s) { first = s; break; }

        var go = new GameObject(prefabName);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = first; sr.sortingOrder = 1;

        var anim = go.AddComponent<Animator>();
        anim.runtimeAnimatorController = ctrl;

        var agent = go.AddComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis   = false;
        agent.radius         = 0.28f;
        agent.height         = 1f;
        agent.speed          = 3f;
        agent.stoppingDistance = 0.1f;
        agent.angularSpeed   = 0f;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.isKinematic  = true;

        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.32f;

        setup(go);

        var hb = go.AddComponent<BuildingHealthBar>();
        hb.offset = new Vector3(0f, 0.7f, 0f);
        hb.width  = 0.8f;
        hb.height = 0.1f;

        // Guardar / sobreescribir prefab
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        Debug.Log($"[Ally Prefabs] Guardado: {path}");
    }

    // ── Arrow prefab ──────────────────────────────────────────────────────

    static GameObject BuildArrowPrefab()
    {
        string path = $"{PrefabOut}/Arrow_Ally.prefab";

        string arrowTex = $"{SpritesBase}/Archer/Arrow.png";
        AssetDatabase.ImportAsset(arrowTex, ImportAssetOptions.ForceSynchronousImport);

        Sprite arrowSpr = null;
        foreach (var a in AssetDatabase.LoadAllAssetsAtPath(arrowTex))
            if (a is Sprite s) { arrowSpr = s; break; }

        var go = new GameObject("Arrow_Ally");

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = arrowSpr;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.1f; col.isTrigger = true;

        go.AddComponent<AllyProjectile>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    // ── Heal Effect prefab ────────────────────────────────────────────────

    static GameObject BuildHealEffectPrefab()
    {
        string path     = $"{PrefabOut}/HealEffect_Ally.prefab";
        string clipPath = "Assets/Animations/CHARACTERS/Monk_Blue/Heal_effect.anim";

        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (clip == null)
        {
            Debug.LogError($"[Ally Prefabs] No se encontró: {clipPath}");
            return null;
        }

        // Controller simple: reproduce el clip una vez y sale
        string ctrlPath = $"{AnimOut}/HealEffectController.controller";
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath) != null)
            AssetDatabase.DeleteAsset(ctrlPath);
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
        var sm   = ctrl.layers[0].stateMachine;
        var st   = sm.AddState("HealEffect"); st.motion = clip;
        sm.defaultState = st;
        var exit = st.AddExitTransition();
        exit.hasExitTime = true; exit.exitTime = 1f; exit.hasFixedDuration = false; exit.duration = 0f;
        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssetIfDirty(ctrl);

        // Prefab: SpriteRenderer + Animator (el clip ya tiene los sprites referenciados)
        var go   = new GameObject("HealEffect_Ally");
        var sr   = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 2;
        var anim = go.AddComponent<Animator>();
        anim.runtimeAnimatorController = ctrl;

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        Debug.Log($"[Ally Prefabs] HealEffect guardado: {path}");
        return prefab;
    }
}
