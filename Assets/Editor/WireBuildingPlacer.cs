using UnityEngine;
using UnityEditor;

public class WireBuildingPlacer
{
    public static void Execute()
    {
        var placer = GameObject.FindFirstObjectByType<BuildingPlacer>();
        if (placer == null) { Debug.LogError("[WireBuildingPlacer] BuildingPlacer not found in scene."); return; }

        // Order matches --Icons-- slots: Icon_Tower, Icon_Barracks, Icon_Monastery, Icon_Archery, Icon_House
        string[] paths = new string[]
        {
            "Assets/Prefabs/BUILDS/Tower_Blue_Base.prefab",
            "Assets/Prefabs/BUILDS/Barracks_Blue_Base.prefab",
            "Assets/Prefabs/BUILDS/Monastery_Blue_Base.prefab",
            "Assets/Prefabs/BUILDS/Archery_Blue_Base.prefab",
            "Assets/Prefabs/BUILDS/House_Blue_Base.prefab",
        };

        var prefabs = new GameObject[paths.Length];
        for (int i = 0; i < paths.Length; i++)
        {
            prefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
            if (prefabs[i] == null)
                Debug.LogWarning($"[WireBuildingPlacer] Not found: {paths[i]}");
        }

        var so   = new SerializedObject(placer);
        var prop = so.FindProperty("buildingPrefabs");
        prop.arraySize = prefabs.Length;
        for (int i = 0; i < prefabs.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = prefabs[i];

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(placer);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(placer.gameObject.scene);
        Debug.Log("[WireBuildingPlacer] Done — Tower, Barracks, Monastery, Archery, House wired.");
    }
}
