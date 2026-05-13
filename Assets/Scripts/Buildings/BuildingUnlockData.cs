using UnityEngine;

[System.Serializable]
public class BuildingRequirement
{
    public BuildingUnlockData building;
    [Min(1)] public int count = 1;
}

[CreateAssetMenu(fileName = "BuildingUnlockData", menuName = "RPG/Building Unlock Data")]
public class BuildingUnlockData : ScriptableObject
{
    [Header("Identity")]
    public string     buildingName;
    public Sprite     icon;
    [Tooltip("Debe ser el mismo prefab que está en BuildingPlacer.buildingPrefabs")]
    public GameObject prefab;

    [Header("Build Limit")]
    [Tooltip("Máximo que se puede construir. 0 = sin límite.")]
    public int maxCount = 1;

    [Header("Quest Chain")]
    [Tooltip("Esta misión solo aparece disponible después de completar la misión anterior.")]
    public BuildingUnlockData prerequisite;
    [Tooltip("Cada misión de esta lista que esté completada permite construir 1 unidad más de este edificio.")]
    public BuildingUnlockData[] capacityExpansions;

    [Header("Unlock Requirements (lo que Pawn pide)")]
    public int                   requiredLevel = 0;
    public int                   goldCost      = 0;
    public int                   woodCost      = 0;
    public int                   meatCost      = 0;
    public KillRequirement[]     killRequirements;
    public BuildingRequirement[] requiredBuildings;

    [Header("Dialogue")]
    [TextArea(2, 4)] public string pawnDialogue     = "";
    [TextArea(2, 3)] public string description       = "";
    [TextArea(2, 3)] public string lockedDescription = "Habla con Pawn para desbloquear esta construcción.";
}
