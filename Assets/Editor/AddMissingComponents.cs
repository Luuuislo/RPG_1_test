using UnityEngine;
using UnityEditor;

public class AddMissingComponents
{
    public static void Execute()
    {
        var playerGo = GameObject.Find("---CHARACTERS---/Player");
        if (playerGo == null) { Debug.LogError("[AddMissing] Player not found."); return; }

        // PlayerExperience
        if (playerGo.GetComponent<PlayerExperience>() == null)
        {
            playerGo.AddComponent<PlayerExperience>();
            Debug.Log("[AddMissing] Added PlayerExperience.");
        }
        else Debug.Log("[AddMissing] PlayerExperience already present.");

        // PlayerHitEffect
        if (playerGo.GetComponent<PlayerHitEffect>() == null)
        {
            playerGo.AddComponent<PlayerHitEffect>();
            Debug.Log("[AddMissing] Added PlayerHitEffect.");
        }
        else Debug.Log("[AddMissing] PlayerHitEffect already present.");

        EditorUtility.SetDirty(playerGo);
        Debug.Log("[AddMissing] Done.");
    }
}
