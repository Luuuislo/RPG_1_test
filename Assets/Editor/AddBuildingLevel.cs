using UnityEngine;
using UnityEditor;

public class AddBuildingLevel
{
    // Maps base-prefab name prefix to its 4 evolution prefab paths (tier 2-5)
    static readonly (string prefix, string displayName)[] Buildings = new[]
    {
        ("Tower",    "Tower"),
        ("Archery",  "Archery"),
        ("Barracks", "Barracks"),
        ("House",    "House"),
        ("Monastery","Monastery"),
    };

    static readonly string[] Colors = { "Yellow", "Purple", "Red", "Black" };
    static readonly int[]    Evolutions = { 2, 3, 4, 5 };

    [MenuItem("Tools/Setup Building Levels")]
    public static void Execute()
    {
        foreach (var (prefix, displayName) in Buildings)
        {
            string basePath = $"Assets/Prefabs/BUILDS/{prefix}_Blue_Base.prefab";
            var basePrefab  = AssetDatabase.LoadAssetAtPath<GameObject>(basePath);
            if (basePrefab == null) { Debug.LogWarning($"Not found: {basePath}"); continue; }

            // Collect evolution prefabs
            var evoPrefabs = new GameObject[4];
            for (int i = 0; i < 4; i++)
            {
                string evoPath = $"Assets/Prefabs/BUILDS/{prefix}_{Colors[i]}_Base_Evolution_{Evolutions[i]}.prefab";
                evoPrefabs[i]  = AssetDatabase.LoadAssetAtPath<GameObject>(evoPath);
                if (evoPrefabs[i] == null) Debug.LogWarning($"Not found: {evoPath}");
            }

            // Patch base prefab
            PatchPrefab(basePrefab, basePath, displayName, 1, evoPrefabs);

            // Patch each evolution prefab too
            for (int i = 0; i < 4; i++)
            {
                if (evoPrefabs[i] == null) continue;
                string evoPath = $"Assets/Prefabs/BUILDS/{prefix}_{Colors[i]}_Base_Evolution_{Evolutions[i]}.prefab";
                PatchPrefab(evoPrefabs[i], evoPath, displayName, i + 2, evoPrefabs);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[AddBuildingLevel] Done — BuildingLevel added to all base and evolution prefabs.");
    }

    static void PatchPrefab(GameObject prefab, string path, string displayName, int tier, GameObject[] evoPrefabs)
    {
        using var scope = new PrefabUtility.EditPrefabContentsScope(path);
        var root = scope.prefabContentsRoot;

        var bl = root.GetComponent<BuildingLevel>();
        if (bl == null) bl = root.AddComponent<BuildingLevel>();

        bl.buildingName    = displayName;
        bl.evolutionTier   = tier;
        bl.evolutionPrefabs = evoPrefabs;
        // Leave costs and thresholds at defaults so user edits per prefab
    }
}
