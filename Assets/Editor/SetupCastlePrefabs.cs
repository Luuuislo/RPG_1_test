using UnityEngine;
using UnityEditor;

// RPGTools > Setup Castle Level System
// Añade BuildingLevel + BuildingHealthBar a los 5 prefabs del castillo
// y configura el tier, stats base y cadena de evolución.
public static class SetupCastlePrefabs
{
    private static readonly string[] Paths =
    {
        "Assets/Prefabs/BUILDS/Castle_Blue_Base.prefab",           // tier 1
        "Assets/Prefabs/BUILDS/Castle_Yellow_Base_Evolution_2.prefab", // tier 2
        "Assets/Prefabs/BUILDS/Castle_Purple_Base_Evolution_3.prefab", // tier 3
        "Assets/Prefabs/BUILDS/Castle_Red_Base_Evolution_4.prefab",    // tier 4
        "Assets/Prefabs/BUILDS/Castle_Black_Base_Evolution_5.prefab",  // tier 5
    };

    [MenuItem("RPGTools/Setup Castle Level System")]
    static void Setup()
    {
        // Load all castle prefab assets first (needed for cross-references)
        var assets = new GameObject[Paths.Length];
        for (int i = 0; i < Paths.Length; i++)
        {
            assets[i] = AssetDatabase.LoadAssetAtPath<GameObject>(Paths[i]);
            if (assets[i] == null)
                Debug.LogError($"No se encontró el prefab: {Paths[i]}");
        }

        // Evolution prefabs shared by all tiers: index = tier-1 (tier2 at [0], tier5 at [3])
        var evoRefs = new GameObject[4];
        for (int j = 0; j < 4; j++)
            evoRefs[j] = (j + 1 < assets.Length) ? assets[j + 1] : null;

        for (int i = 0; i < Paths.Length; i++)
        {
            if (assets[i] == null) continue;

            using var scope = new PrefabUtility.EditPrefabContentsScope(Paths[i]);
            var root = scope.prefabContentsRoot;

            // ── BuildingLevel ──────────────────────────────────────────────────
            var bl = root.GetComponent<BuildingLevel>() ?? root.AddComponent<BuildingLevel>();

            bl.buildingName  = "Castle";
            bl.evolutionTier = i + 1;

            // Levels & cost
            bl.currentLevel       = 1;
            bl.maxLevel           = 100;
            bl.pointsPerLevelUp   = 3;
            bl.goldCostPerLevel   = 20;
            bl.woodCostPerLevel   = 10;
            bl.meatCostPerLevel   = 0;

            // Base stats — castle: más HP que la torre, ataque moderado
            bl.baseAtk      = 10f;
            bl.baseAtkSpeed = 1f;
            bl.baseHp       = 300f;
            bl.baseRange    = 8f;

            // Gain per stat point
            bl.atkPerPoint      = 1f;
            bl.atkSpeedPerPoint = 0.02f;
            bl.hpPerPoint       = 100f;

            // Evolution thresholds & bonuses (mismo esquema que la torre)
            bl.evolutionThresholds       = new int[]   { 20, 40, 60, 80 };
            bl.evolutionBonusPercent     = new float[] { 20f, 40f, 60f, 80f };
            bl.rangeIncreasePerEvolution = 1f;

            // Cadena de evolución (todos los tiers comparten el mismo array)
            bl.evolutionPrefabs = new GameObject[4];
            for (int j = 0; j < 4; j++)
                bl.evolutionPrefabs[j] = evoRefs[j];

            // Reset runtime stats → Awake() los inicializa desde los base
            bl.currentAtk      = 0f;
            bl.currentAtkSpeed = 0f;
            bl.currentHp       = 0f;
            bl.currentRange    = 0f;
            bl.pendingPoints   = 0;
            bl.spentOnAtk      = 0;
            bl.spentOnAtkSpeed = 0;
            bl.spentOnHp       = 0;

            // ── BuildingHealthBar ──────────────────────────────────────────────
            if (root.GetComponent<BuildingHealthBar>() == null)
                root.AddComponent<BuildingHealthBar>();

            // ── BoxCollider2D para detección de clicks ─────────────────────────
            // EdgeCollider2D no detecta clics en el interior; añadimos un trigger
            // que sí funciona con Physics2D.OverlapPoint.
            bool hasBox = root.GetComponent<BoxCollider2D>() != null;
            if (!hasBox)
            {
                var box       = root.AddComponent<BoxCollider2D>();
                box.isTrigger = true;
                box.size      = new Vector2(2.2f, 2.2f);
                box.offset    = Vector2.zero;
            }

            Debug.Log($"[Castle Setup] Tier {i + 1} OK — {Paths[i]}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Castle Setup] Todos los prefabs configurados.");
    }
}
