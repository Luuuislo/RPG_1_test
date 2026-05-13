using UnityEngine;
using UnityEditor;

public static class SetupWaves
{
    public static void Execute()
    {
        var wm = GameObject.Find("WaveManager")?.GetComponent<WaveManager>();
        if (wm == null) { Debug.LogError("[SetupWaves] WaveManager not found."); return; }

        // Load prefabs
        var goblin   = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/TorchGoblinNormal.prefab");
        var gnoll    = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/GnollEnemy.prefab");
        var thief    = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Thief.prefab");
        var skeletor = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Skeletor.prefab");
        var shaman   = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/HexShaman.prefab");
        var minotaur = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Minotaur.prefab");
        var troll    = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/TrollBoss.prefab");

        wm.waves = new WaveData[]
        {
            // Oleada 1 — Reconocimiento
            new WaveData
            {
                waveName        = "Oleada 1 — Reconocimiento",
                delayBeforeWave = 3f,
                enemies = new EnemySpawnEntry[]
                {
                    new EnemySpawnEntry { prefab = goblin, count = 4, healthMultiplier = 1f,  damageMultiplier = 1f  },
                }
            },
            // Oleada 2 — Avanzada
            new WaveData
            {
                waveName        = "Oleada 2 — Avanzada",
                delayBeforeWave = 8f,
                enemies = new EnemySpawnEntry[]
                {
                    new EnemySpawnEntry { prefab = goblin, count = 3, healthMultiplier = 1.2f, damageMultiplier = 1.1f },
                    new EnemySpawnEntry { prefab = gnoll,  count = 3, healthMultiplier = 1.2f, damageMultiplier = 1.1f },
                }
            },
            // Oleada 3 — Asalto
            new WaveData
            {
                waveName        = "Oleada 3 — Asalto",
                delayBeforeWave = 10f,
                enemies = new EnemySpawnEntry[]
                {
                    new EnemySpawnEntry { prefab = thief,    count = 4, healthMultiplier = 1.5f, damageMultiplier = 1.3f },
                    new EnemySpawnEntry { prefab = gnoll,    count = 3, healthMultiplier = 1.5f, damageMultiplier = 1.3f },
                }
            },
            // Oleada 4 — Ejército
            new WaveData
            {
                waveName        = "Oleada 4 — Ejercito",
                delayBeforeWave = 12f,
                enemies = new EnemySpawnEntry[]
                {
                    new EnemySpawnEntry { prefab = skeletor, count = 4, healthMultiplier = 2f,   damageMultiplier = 1.5f },
                    new EnemySpawnEntry { prefab = shaman,   count = 3, healthMultiplier = 2f,   damageMultiplier = 1.5f },
                    new EnemySpawnEntry { prefab = gnoll,    count = 3, healthMultiplier = 2f,   damageMultiplier = 1.5f },
                }
            },
            // Oleada 5 — El Jefe
            new WaveData
            {
                waveName        = "Oleada 5 — El Jefe",
                delayBeforeWave = 15f,
                enemies = new EnemySpawnEntry[]
                {
                    new EnemySpawnEntry { prefab = troll,    count = 1, healthMultiplier = 4f,   damageMultiplier = 2f  },
                    new EnemySpawnEntry { prefab = minotaur, count = 2, healthMultiplier = 3f,   damageMultiplier = 1.8f },
                    new EnemySpawnEntry { prefab = skeletor, count = 4, healthMultiplier = 2.5f, damageMultiplier = 1.6f },
                }
            },
        };

        wm.autoStart             = true;
        wm.spawnIntervalSeconds  = 0.25f;

        EditorUtility.SetDirty(wm);
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);

        Debug.Log("[SetupWaves] 5 oleadas configuradas y escena guardada.");
    }
}
