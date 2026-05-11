using UnityEngine;
using UnityEditor;

// RPGTools > Setup Unit Buildings
// Añade UnitProducer + BuildingAttack (para recibir daño) + BuildingHealthBar
// a los 15 prefabs de unidades (5 tiers × Barracks, Archery, Monastery)
// y configura la cadena de evolución y stats base.
public static class SetupUnitBuildingPrefabs
{
    // ── Paths ────────────────────────────────────────────────────────────

    static readonly string[] BarracksPaths =
    {
        "Assets/Prefabs/BUILDS/Barracks_Blue_Base.prefab",
        "Assets/Prefabs/BUILDS/Barracks_Yellow_Base_Evolution_2.prefab",
        "Assets/Prefabs/BUILDS/Barracks_Purple_Base_Evolution_3.prefab",
        "Assets/Prefabs/BUILDS/Barracks_Red_Base_Evolution_4.prefab",
        "Assets/Prefabs/BUILDS/Barracks_Black_Base_Evolution_5.prefab",
    };

    static readonly string[] ArcheryPaths =
    {
        "Assets/Prefabs/BUILDS/Archery_Blue_Base.prefab",
        "Assets/Prefabs/BUILDS/Archery_Yellow_Base_Evolution_2.prefab",
        "Assets/Prefabs/BUILDS/Archery_Purple_Base_Evolution_3.prefab",
        "Assets/Prefabs/BUILDS/Archery_Red_Base_Evolution_4.prefab",
        "Assets/Prefabs/BUILDS/Archery_Black_Base_Evolution_5.prefab",
    };

    static readonly string[] MonasteryPaths =
    {
        "Assets/Prefabs/BUILDS/Monastery_Blue_Base.prefab",
        "Assets/Prefabs/BUILDS/Monastery_Yellow_Base_Evolution_2.prefab",
        "Assets/Prefabs/BUILDS/Monastery_Purple_Base_Evolution_3.prefab",
        "Assets/Prefabs/BUILDS/Monastery_Red_Base_Evolution_4.prefab",
        "Assets/Prefabs/BUILDS/Monastery_Black_Base_Evolution_5.prefab",
    };

    // ── Unit prefabs ─────────────────────────────────────────────────────

    const string WarriorPath  = "Assets/Prefabs/CHARACTERS/Warrior_Ally.prefab";
    const string LancerPath   = "Assets/Prefabs/CHARACTERS/Lancer_Ally.prefab";
    const string ArcherPath   = "Assets/Prefabs/CHARACTERS/Archer_Ally.prefab";
    const string MonkPath     = "Assets/Prefabs/CHARACTERS/Monk_Ally.prefab";

    // ── Menu ─────────────────────────────────────────────────────────────

    [MenuItem("RPGTools/Setup Unit Buildings")]
    static void Setup()
    {
        var warrior = AssetDatabase.LoadAssetAtPath<GameObject>(WarriorPath);
        var lancer  = AssetDatabase.LoadAssetAtPath<GameObject>(LancerPath);
        var archer  = AssetDatabase.LoadAssetAtPath<GameObject>(ArcherPath);
        var monk    = AssetDatabase.LoadAssetAtPath<GameObject>(MonkPath);

        // Barracks: produce Warrior y Lancer (2 slots)
        SetupGroup(BarracksPaths,  "Barracks",  200f, 15, 8,
                   new[] { warrior, lancer }, new[] { "Warrior", "Lancer" });

        // Archery: produce Archer
        SetupGroup(ArcheryPaths,   "Archery",   150f, 12, 8,
                   new[] { archer }, new[] { "Archer" });

        // Monastery: produce Monk
        SetupGroup(MonasteryPaths, "Monastery", 120f, 18, 10,
                   new[] { monk }, new[] { "Monk" });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Unit Buildings] Setup completo. Recuerda asignar los iconos de unidad en el Inspector.");
    }

    // ── Setup por grupo ──────────────────────────────────────────────────

    static void SetupGroup(string[] paths, string name, float baseHp,
                           int goldCost, int woodCost, GameObject[] unitPrefabs,
                           string[] unitNames = null)
    {
        var assets = new GameObject[paths.Length];
        for (int i = 0; i < paths.Length; i++)
        {
            assets[i] = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
            if (assets[i] == null) Debug.LogError($"No encontrado: {paths[i]}");
        }

        // Evolution refs array (todos los tiers apuntan al mismo array)
        var evoRefs = new GameObject[4];
        for (int j = 0; j < 4; j++)
            evoRefs[j] = j + 1 < assets.Length ? assets[j + 1] : null;

        for (int i = 0; i < paths.Length; i++)
        {
            if (assets[i] == null) continue;

            using var scope = new PrefabUtility.EditPrefabContentsScope(paths[i]);
            var root = scope.prefabContentsRoot;

            // ── BuildingLevel ─────────────────────────────────────────
            var bl = root.GetComponent<BuildingLevel>() ?? root.AddComponent<BuildingLevel>();
            bl.buildingName      = name;
            bl.evolutionTier     = i + 1;
            bl.currentLevel      = 1;
            bl.maxLevel          = 100;
            bl.pointsPerLevelUp  = 3;
            bl.goldCostPerLevel  = goldCost;
            bl.woodCostPerLevel  = woodCost;
            bl.meatCostPerLevel  = 0;

            // Sin ataque; HP escala con nivel
            bl.baseAtk      = 0f;
            bl.baseAtkSpeed = 0f;
            bl.baseHp       = baseHp;
            bl.baseRange    = 0f;

            bl.atkPerPoint      = 0f;
            bl.atkSpeedPerPoint = 0f;
            bl.hpPerPoint       = 50f;  // invest points → más HP del edificio

            bl.evolutionThresholds       = new int[]   { 20, 40, 60, 80 };
            bl.evolutionBonusPercent     = new float[] { 20f, 40f, 60f, 80f };
            bl.rangeIncreasePerEvolution = 0f;

            bl.evolutionPrefabs = new GameObject[4];
            for (int j = 0; j < 4; j++) bl.evolutionPrefabs[j] = evoRefs[j];

            bl.currentAtk = bl.currentAtkSpeed = bl.currentHp = bl.currentRange = 0f;
            bl.pendingPoints = bl.spentOnAtk = bl.spentOnAtkSpeed = bl.spentOnHp = 0;

            // ── BuildingAttack (recibir daño de enemigos, sin arrowPrefab) ──
            var ba = root.GetComponent<BuildingAttack>() ?? root.AddComponent<BuildingAttack>();
            ba.attackDamage   = 0;
            ba.attackRange    = 0f;   // no atacar
            ba.attackCooldown = 999f;
            ba.maxHealth      = Mathf.RoundToInt(baseHp);
            // arrowPrefab queda null → nunca dispara

            // ── BuildingHealthBar ─────────────────────────────────────
            if (root.GetComponent<BuildingHealthBar>() == null)
                root.AddComponent<BuildingHealthBar>();

            // ── UnitProducer ──────────────────────────────────────────
            var up = root.GetComponent<UnitProducer>() ?? root.AddComponent<UnitProducer>();
            up.maxQueueSize = 5;
            up.spawnOffset  = new Vector3(1.5f, -0.5f, 0f);

            // Configura las entradas de unidades desde los prefabs cargados
            up.units = new UnitEntry[unitPrefabs.Length];
            for (int u = 0; u < unitPrefabs.Length; u++)
            {
                up.units[u] = new UnitEntry
                {
                    unitName = (unitNames != null && u < unitNames.Length) ? unitNames[u]
                             : unitPrefabs[u] != null ? unitPrefabs[u].name : "Unit",
                    prefab   = unitPrefabs[u],
                    goldCost = goldCost,
                    woodCost = woodCost / 2,
                    trainTime = 6f
                };
            }

            Debug.Log($"[{name}] Tier {i + 1} OK");
        }
    }
}
