using UnityEngine;
using UnityEditor;

public class FixSceneConfig
{
    public static void Execute()
    {
        // --- Find Player layer ---
        var playerGo = GameObject.FindGameObjectWithTag("Player");
        if (playerGo == null) { Debug.LogError("[FixSceneConfig] No GameObject tagged 'Player' found."); return; }

        int playerLayer = playerGo.layer;
        int playerMask  = 1 << playerLayer;
        Debug.Log($"[FixSceneConfig] Player is on layer {playerLayer} ({LayerMask.LayerToName(playerLayer)}).");

        // --- Fix Enemy.damageableLayers ---
        int enemiesFixed = 0;
        foreach (var enemy in Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None))
        {
            int current = enemy.damageableLayers.value;
            if ((current & playerMask) == 0)
            {
                enemy.damageableLayers = current | playerMask;
                EditorUtility.SetDirty(enemy);
                enemiesFixed++;
                Debug.Log($"[FixSceneConfig] Added Player layer to Enemy: {enemy.name}");
            }
        }
        Debug.Log($"[FixSceneConfig] damageableLayers fixed on {enemiesFixed} enemies.");

        // --- Fix DamageReceiver.xpReward ---
        int xpFixed = 0;
        foreach (var dr in Object.FindObjectsByType<DamageReceiver>(FindObjectsSortMode.None))
        {
            if (dr.xpReward <= 0)
            {
                dr.xpReward = 10;
                EditorUtility.SetDirty(dr);
                xpFixed++;
                Debug.Log($"[FixSceneConfig] Set xpReward=10 on: {dr.name}");
            }
        }
        Debug.Log($"[FixSceneConfig] xpReward fixed on {xpFixed} DamageReceivers.");

        Debug.Log("[FixSceneConfig] Done. Save the scene (Ctrl+S).");
    }
}
