using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public class RecreatePawnShopUI
{
    public static void Execute()
    {
        // 1. Borrar el manager existente
        var existing = GameObject.Find("PawnShopUIManager");
        if (existing != null)
        {
            Object.DestroyImmediate(existing);
            Debug.Log("[Recreate] PawnShopUIManager antiguo eliminado.");
        }

        // 2. Crear el nuevo UI
        CreatePawnShopUI.Create_Internal();

        // 3. Reasignar buildings
        var ui = Object.FindFirstObjectByType<PawnShopUI>();
        if (ui == null) { Debug.LogError("[Recreate] PawnShopUI no encontrado tras recrear."); return; }

        string[] paths = {
            "Assets/TowerBuilding.asset",
            "Assets/BarracksBuilding.asset",
            "Assets/MonasteryBuilding.asset",
            "Assets/ArcheryBuilding.asset",
            "Assets/HouseBuilding.asset"
        };
        var arr = new BuildingUnlockData[paths.Length];
        for (int i = 0; i < paths.Length; i++)
        {
            arr[i] = AssetDatabase.LoadAssetAtPath<BuildingUnlockData>(paths[i]);
            if (arr[i] == null) Debug.LogWarning($"[Recreate] Asset no encontrado: {paths[i]}");
        }
        ui.buildings = arr;

        // 4. Añadir BuildingUnlockManager al mismo GO si no está
        var mgr = ui.GetComponent<BuildingUnlockManager>();
        if (mgr == null) ui.gameObject.AddComponent<BuildingUnlockManager>();

        EditorUtility.SetDirty(ui.gameObject);
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[Recreate] PawnShopUI recreado y escena guardada.");
    }
}
