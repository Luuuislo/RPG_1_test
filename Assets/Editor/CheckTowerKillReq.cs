using UnityEditor;
using UnityEngine;

public static class CheckTowerKillReq
{
    public static void Execute()
    {
        var tower = AssetDatabase.LoadAssetAtPath<BuildingUnlockData>("Assets/TowerBuilding.asset");
        if (tower == null) { Debug.LogError("TowerBuilding.asset no encontrado"); return; }
        if (tower.killRequirements == null || tower.killRequirements.Length == 0)
        { Debug.Log("[TowerKill] Sin kill requirements"); return; }
        foreach (var req in tower.killRequirements)
            Debug.Log($"[TowerKill] enemyType='{req.enemyType}' count={req.count}");
    }
}
