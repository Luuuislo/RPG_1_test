using UnityEngine;
using UnityEditor;

public class FixEnemyLayers
{
    public static void Execute()
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer < 0) { Debug.LogError("[FixEnemyLayers] Layer 'Player' not found."); return; }

        int mask = 1 << playerLayer;
        var enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (var e in enemies)
        {
            e.damageableLayers = mask;
            EditorUtility.SetDirty(e);
        }
        Debug.Log($"[FixEnemyLayers] Set damageableLayers=Player on {enemies.Length} enemies.");
    }
}
