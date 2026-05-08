using UnityEngine;

// Add this component to any item prefab that supports quantity (gold, wood, meat, etc.)
// DamageReceiver will set the quantity when the item is dropped.
// Your pickup script should read this value and add it to the player's inventory.
public class DroppedItem : MonoBehaviour
{
    public int quantity = 1;
}
