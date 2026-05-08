using UnityEngine;

public class PlayerResources : MonoBehaviour
{
    private int gold;
    private int wood;
    private int meat;

    // Called by ItemPickup (dropped items with quantity)
    public void AddGold(int amount) { gold += amount; UiManager.Instance?.UpdateGold(gold); }
    public void AddWood(int amount) { wood += amount; UiManager.Instance?.UpdateWood(wood); }
    public void AddMeat(int amount) { meat += amount; UiManager.Instance?.UpdateMeat(meat); }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        // ItemPickup handles its own pickup and quantity — don't intercept
        if (other.GetComponent<ItemPickup>() != null) return;

        int qty = 1;
        DroppedItem dropped = other.GetComponent<DroppedItem>();
        if (dropped != null) qty = dropped.quantity;

        if (other.CompareTag("Gold_Resource"))      { AddGold(qty); Destroy(other.gameObject); }
        else if (other.CompareTag("Wood_Resource")) { AddWood(qty); Destroy(other.gameObject); }
        else if (other.CompareTag("Meat_Resource")) { AddMeat(qty); Destroy(other.gameObject); }
    }
}
