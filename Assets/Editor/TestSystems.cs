using UnityEngine;

public class TestSystems
{
    public static void TestXp()
    {
        var exp = Object.FindFirstObjectByType<PlayerExperience>();
        if (exp == null) { Debug.LogError("[Test] PlayerExperience not found."); return; }
        exp.GainXp(50);
        Debug.Log($"[Test] +50 XP → CurrentXp={exp.CurrentXp}, Level={exp.Level}, XpToNext={exp.XpToNextLevel}");
    }

    public static void TestDamage()
    {
        var player = Object.FindFirstObjectByType<Player>();
        if (player == null) { Debug.LogError("[Test] Player not found."); return; }
        int before = player.GetCurrentHealth();
        player.TakeDamage(25);
        int after = player.GetCurrentHealth();
        Debug.Log($"[Test] TakeDamage(25): {before} → {after}");
    }

    // Spam damage so the floating text is visible in screenshots
    public static void SpamDamage()
    {
        var player = Object.FindFirstObjectByType<Player>();
        if (player == null) return;
        for (int i = 0; i < 8; i++)
            player.TakeDamage(Random.Range(5, 30));
    }
}
