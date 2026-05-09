using UnityEngine;

public class TestFloatingText
{
    public static void Execute()
    {
        var player = Object.FindFirstObjectByType<Player>();
        if (player == null) { Debug.LogError("[TestFloat] Player not found."); return; }

        // Check if prefab loaded
        var prefab = Resources.Load<GameObject>("FloatingDamageText");
        Debug.Log(prefab != null
            ? $"[TestFloat] Prefab loaded OK: {prefab.name}"
            : "[TestFloat] ERROR: Resources.Load returned null!");

        if (prefab == null) return;

        // Spawn manually above player
        Vector3 pos = player.transform.position + new Vector3(0, 1f, 0);
        var go = Object.Instantiate(prefab, pos, Quaternion.identity);
        var fdt = go.GetComponent<FloatingDamageText>();
        if (fdt != null) fdt.Setup(25);
        Debug.Log($"[TestFloat] Spawned at {pos}");
    }
}
