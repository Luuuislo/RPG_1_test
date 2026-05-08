using UnityEngine;

[CreateAssetMenu(fileName = "DropDisplaySettings", menuName = "RPG/Drop Display Settings")]
public class DropDisplaySettings : ScriptableObject
{
    [Header("Pile")]
    public int maxVisuals = 5;
    public float pileSpread = 0.2f;

    [Header("Label")]
    public float labelHeight = 0.5f;
    public float fontSize = 2.2f;
    public Color labelColor = Color.yellow;
    public Color outlineColor = Color.black;
    [Range(0f, 1f)] public float outlineWidth = 0.25f;
    public int sortingOrder = 20;
}