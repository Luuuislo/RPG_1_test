using UnityEditor;
using UnityEngine;

public class SetupUnlockData
{
    public static void Execute()
    {
        var placer = Object.FindFirstObjectByType<BuildingPlacer>();
        if (placer == null) { Debug.LogError("[Setup] BuildingPlacer no encontrado"); return; }

        var paths = new[]
        {
            "Assets/TowerBuilding.asset",
            "Assets/BarracksBuilding.asset",
            "Assets/MonasteryBuilding.asset",
            "Assets/ArcheryBuilding.asset",
            "Assets/HouseBuilding.asset"
        };

        var data = new BuildingUnlockData[paths.Length];
        for (int i = 0; i < paths.Length; i++)
        {
            data[i] = AssetDatabase.LoadAssetAtPath<BuildingUnlockData>(paths[i]);
            if (data[i] == null) Debug.LogWarning($"[Setup] No encontrado: {paths[i]}");
            else Debug.Log($"[Setup] [{i}] {data[i].buildingName} OK");
        }

        placer.unlockData = data;
        EditorUtility.SetDirty(placer);
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[Setup] unlockData asignado y escena guardada.");
    }
}
