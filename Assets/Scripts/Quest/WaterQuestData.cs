using UnityEngine;

[System.Serializable]
public class KillRequirement
{
    [Tooltip("Debe coincidir exactamente con el campo 'enemyType' en DamageReceiver del enemigo")]
    public string enemyType;
    public int    count = 5;
}

[CreateAssetMenu(fileName = "WaterQuestData", menuName = "RPG/Water Quest Data")]
public class WaterQuestData : ScriptableObject
{
    [Header("NPC")]
    public string npcName = "Marinero";

    [Header("Requirements")]
    public int               requiredLevel = 5;
    public int               goldCost      = 0;
    public int               woodCost      = 0;
    public int               meatCost      = 0;
    public KillRequirement[] killRequirements;

    [Header("Dialogue")]
    [TextArea(3, 5)] public string introText      = "Si deseas aventurarte en estas aguas, primero demuestra tu valía.";
    [TextArea(3, 5)] public string inProgressText = "Aún no has completado los requisitos. ¡Sigue adelante!";
    [TextArea(3, 5)] public string readyText      = "¡Lo has logrado! Ya puedes pasar. El agua te espera.";
    [TextArea(3, 5)] public string completedText  = "Ya tienes acceso al agua. ¡Buena suerte, aventurero!";
}
