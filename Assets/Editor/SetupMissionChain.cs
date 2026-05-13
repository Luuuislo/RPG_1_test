using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SetupMissionChain
{
    [MenuItem("RPGTools/Setup Mission Chain")]
    public static void Execute()
    {
        var tower    = LoadOrWarn("Assets/TowerBuilding.asset");
        var barracks = LoadOrWarn("Assets/BarracksBuilding.asset");
        var archery  = LoadOrWarn("Assets/ArcheryBuilding.asset");
        var monastery = LoadOrWarn("Assets/MonasteryBuilding.asset");

        if (tower == null || barracks == null || archery == null || monastery == null) return;

        // Create expansion slot missions (no prefab — just unlock a new tower slot)
        var exp2 = GetOrCreate("Assets/TowerExpansion2.asset",
            "2ª Torre", "La segunda torre reforzará los flancos del castillo.",
            "Construye la primera Torre para desbloquear este slot.",
            requiredLevel: 5, gold: 2000, wood: 2000, meat: 500);

        var exp3 = GetOrCreate("Assets/TowerExpansion3.asset",
            "3ª Torre", "Con tres torres el castillo puede resistir oleadas mucho más numerosas.",
            "Debes haber desbloqueado la 2ª Torre primero.",
            requiredLevel: 10, gold: 4000, wood: 3000, meat: 1500);

        var exp4 = GetOrCreate("Assets/TowerExpansion4.asset",
            "4ª Torre", "Cuatro torres garantizan la defensa total del castillo.",
            "Debes haber desbloqueado la 3ª Torre primero.",
            requiredLevel: 15, gold: 8000, wood: 6000, meat: 3000);

        // ── Quest chain prerequisites ────────────────────────────────────────
        // Main story chain: Tower → Barracks → Archery → Monastery
        tower.prerequisite     = null;
        barracks.prerequisite  = tower;
        archery.prerequisite   = barracks;
        monastery.prerequisite = archery;

        // Tower expansions are INDEPENDENT of the main chain:
        // they only require having built the previous number of towers.
        exp2.prerequisite = null;
        exp3.prerequisite = exp2;   // exp3 needs exp2 unlocked first
        exp4.prerequisite = exp3;

        // exp2 unlocks as soon as the player has 1 tower placed
        exp2.requiredBuildings = new BuildingRequirement[]
            { new BuildingRequirement { building = tower, count = 1 } };
        // exp3/exp4 chain off exp2/exp3 respectively, no extra building req needed
        exp3.requiredBuildings = null;
        exp4.requiredBuildings = null;

        // ── Tower capacity expansions ────────────────────────────────────────
        tower.maxCount           = 1;
        tower.capacityExpansions = new[] { exp2, exp3, exp4 };

        // Copy tower icon to expansion missions so they show the same icon in the shop
        exp2.icon = tower.icon;
        exp3.icon = tower.icon;
        exp4.icon = tower.icon;

        // ── Pawn dialogues ───────────────────────────────────────────────────
        tower.pawnDialogue    = "Las torres son el pilar de defensa del castillo. Sin ellas, las oleadas enemigas llegarán directamente a los muros. ¡Construye la primera torre para empezar a proteger el reino!";
        barracks.pawnDialogue = "En las barracks puedes entrenar caballeros y lanceros. Son las tropas que defenderán el castillo de las oleadas enemigas. Sin soldados, las torres solas no serán suficientes.";
        archery.pawnDialogue  = "Los arqueros son devastadores desde la distancia. Un cuartel de arqueros puede detener oleadas enteras antes de que lleguen a los muros. ¡La distancia es tu mejor aliada!";
        monastery.pawnDialogue = "Los monjes del monasterio curan a las tropas aliadas en batalla. Sin ellos, tus caballeros caerán antes de tiempo. Un ejército que se cura es un ejército invencible.";
        exp2.pawnDialogue     = "Con una segunda torre, los flancos del castillo quedarán protegidos. Los enemigos ya no podrán rodear las defensas tan fácilmente.";
        exp3.pawnDialogue     = "Tres torres en el perímetro del castillo pueden cubrir todos los ángulos de ataque. Las oleadas más grandes no serán un problema.";
        exp4.pawnDialogue     = "Cuatro torres. La defensa del castillo será casi impenetrable. Has construido algo digno de los grandes reyes.";

        foreach (var d in new[] { tower, barracks, archery, monastery, exp2, exp3, exp4 })
            EditorUtility.SetDirty(d);

        // ── Update PawnShopUI.buildings ──────────────────────────────────────
        var ui = Object.FindFirstObjectByType<PawnShopUI>();
        if (ui != null)
        {
            ui.buildings = new[] { tower, barracks, archery, monastery, exp2, exp3, exp4 };
            EditorUtility.SetDirty(ui.gameObject);
            Debug.Log("[MissionChain] PawnShopUI.buildings actualizado con 7 misiones.");
        }
        else Debug.LogWarning("[MissionChain] PawnShopUI no encontrado en la escena.");

        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[MissionChain] Cadena de misiones configurada y escena guardada.");
    }

    static BuildingUnlockData LoadOrWarn(string path)
    {
        var a = AssetDatabase.LoadAssetAtPath<BuildingUnlockData>(path);
        if (a == null) Debug.LogError($"[MissionChain] Asset no encontrado: {path}");
        return a;
    }

    static BuildingUnlockData GetOrCreate(string path,
        string name, string description, string lockedDesc,
        int requiredLevel, int gold, int wood, int meat)
    {
        var asset = AssetDatabase.LoadAssetAtPath<BuildingUnlockData>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<BuildingUnlockData>();
            AssetDatabase.CreateAsset(asset, path);
        }
        asset.buildingName       = name;
        asset.description        = description;
        asset.lockedDescription  = lockedDesc;
        asset.requiredLevel      = requiredLevel;
        asset.goldCost           = gold;
        asset.woodCost           = wood;
        asset.meatCost           = meat;
        asset.maxCount           = 0; // expansion slots have no own build limit
        return asset;
    }
}
