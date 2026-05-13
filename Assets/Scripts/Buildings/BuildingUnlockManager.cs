using System.Collections.Generic;
using UnityEngine;

public class BuildingUnlockManager : MonoBehaviour
{
    public static BuildingUnlockManager Instance { get; private set; }

    private class BuildingState { public bool unlocked; public int placed; }

    private readonly Dictionary<BuildingUnlockData, BuildingState> _states
        = new Dictionary<BuildingUnlockData, BuildingState>();

    private PlayerExperience _playerExp;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start() => _playerExp = FindFirstObjectByType<PlayerExperience>();

    // ── State helpers ─────────────────────────────────────────────────────

    BuildingState GetOrCreate(BuildingUnlockData data)
    {
        if (!_states.TryGetValue(data, out var s))
            _states[data] = s = new BuildingState();
        return s;
    }

    public bool IsUnlocked(BuildingUnlockData data)
        => data != null && GetOrCreate(data).unlocked;

    public int GetPlacedCount(BuildingUnlockData data)
        => data != null ? GetOrCreate(data).placed : 0;

    // Cuántas unidades están permitidas (base + expansiones desbloqueadas)
    public int GetAllowedCount(BuildingUnlockData data)
    {
        if (data == null) return 0;
        int allowed = data.maxCount > 0 ? data.maxCount : 99;
        if (data.capacityExpansions != null)
            foreach (var exp in data.capacityExpansions)
                if (exp != null && IsUnlocked(exp)) allowed++;
        return allowed;
    }

    public bool CanBuild(BuildingUnlockData data)
    {
        if (data == null) return true;
        var s = GetOrCreate(data);
        if (!s.unlocked) return false;
        if (data.maxCount > 0 && s.placed >= GetAllowedCount(data)) return false;
        return true;
    }

    // Tiene misiones disponibles para desbloquear (para el indicador "!")
    public bool HasAvailableMissions(BuildingUnlockData[] allMissions)
    {
        if (allMissions == null) return false;
        foreach (var m in allMissions)
            if (m != null && !IsUnlocked(m) && MeetsUnlockRequirements(m)) return true;
        return false;
    }

    // Hay misiones pendientes en la cadena (solo comprueba prerequisito, ignora coste)
    // — usado por PawnIndicator para que "!" no parpadee con los recursos
    public bool HasPendingMissions(BuildingUnlockData[] allMissions)
    {
        if (allMissions == null) return false;
        foreach (var m in allMissions)
        {
            if (m == null || IsUnlocked(m)) continue;
            if (m.prerequisite == null || IsUnlocked(m.prerequisite)) return true;
        }
        return false;
    }

    // ── Requirement check ─────────────────────────────────────────────────

    public bool MeetsUnlockRequirements(BuildingUnlockData data)
    {
        if (data == null) return true;
        if (_playerExp == null) _playerExp = FindFirstObjectByType<PlayerExperience>();

        // Misión previa debe estar completada (desbloqueada)
        if (data.prerequisite != null && !IsUnlocked(data.prerequisite)) return false;

        if (data.requiredLevel > 0 && (_playerExp == null || _playerExp.Level < data.requiredLevel))
            return false;

        var res = PlayerResources.Instance;
        if (res != null && !res.HasEnough(data.goldCost, data.woodCost, data.meatCost))
            return false;

        var kt = KillTracker.Instance;
        if (data.killRequirements != null)
            foreach (var req in data.killRequirements)
                if (kt == null || kt.GetCount(req.enemyType) < req.count) return false;

        if (data.requiredBuildings != null)
            foreach (var req in data.requiredBuildings)
                if (req.building != null && GetPlacedCount(req.building) < req.count) return false;

        return true;
    }

    // ── Actions ───────────────────────────────────────────────────────────

    public bool Unlock(BuildingUnlockData data)
    {
        if (data == null || IsUnlocked(data) || !MeetsUnlockRequirements(data)) return false;
        PlayerResources.Instance?.Spend(data.goldCost, data.woodCost, data.meatCost);
        GetOrCreate(data).unlocked = true;
        FindFirstObjectByType<BuildingPlacer>()?.RefreshBuildButtons();
        PawnShopUI.Instance?.RefreshCards();
        return true;
    }

    public void RegisterPlaced(BuildingUnlockData data)
    {
        if (data == null) return;
        GetOrCreate(data).placed++;
        PawnShopUI.Instance?.RefreshCards();
        FindFirstObjectByType<BuildingPlacer>()?.RefreshBuildButtons();
    }
}
