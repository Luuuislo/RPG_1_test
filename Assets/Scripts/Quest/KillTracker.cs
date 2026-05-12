using System.Collections.Generic;
using UnityEngine;

public class KillTracker : MonoBehaviour
{
    public static KillTracker Instance { get; private set; }

    private readonly Dictionary<string, int> _kills = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    // Called by DamageReceiver on enemy death. enemyType matches KillRequirement.enemyType.
    public static void RegisterKill(string enemyType)
    {
        if (string.IsNullOrEmpty(enemyType)) return;

        if (Instance == null)
        {
            var go = new GameObject("KillTracker");
            go.AddComponent<KillTracker>();
        }

        Instance._kills.TryGetValue(enemyType, out int count);
        Instance._kills[enemyType] = count + 1;

        WaterQuestUI.Instance?.RefreshRequirements();
    }

    public int GetCount(string enemyType)
    {
        if (string.IsNullOrEmpty(enemyType)) return 0;
        _kills.TryGetValue(enemyType, out int c);
        return c;
    }
}
