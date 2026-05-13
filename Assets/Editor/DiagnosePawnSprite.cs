using UnityEditor;
using UnityEngine;

public static class DiagnosePawnSprite
{
    public static void Execute()
    {
        var pawn = GameObject.Find("Pawn_NPC_Base");
        if (pawn == null) { Debug.LogError("[Diag] Pawn_NPC_Base not found."); return; }

        var sr = pawn.GetComponent<SpriteRenderer>();
        if (sr == null) { Debug.LogError("[Diag] No SpriteRenderer on Pawn."); return; }

        Debug.Log($"[Diag] SpriteRenderer — sprite={sr.sprite?.name ?? "NULL"} " +
                  $"enabled={sr.enabled} color={sr.color} " +
                  $"sortingLayer={sr.sortingLayerName} order={sr.sortingOrder} " +
                  $"GO active={pawn.activeInHierarchy}");

        var anim = pawn.GetComponent<Animator>();
        if (anim != null)
            Debug.Log($"[Diag] Animator — enabled={anim.enabled} " +
                      $"controller={(anim.runtimeAnimatorController != null ? anim.runtimeAnimatorController.name : "NULL")}");
    }
}
