using UnityEngine;

public class PlayerResources : MonoBehaviour
{
    public static PlayerResources Instance { get; private set; }

    private int gold;
    private int wood;
    private int meat;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddGold(int amount) { gold += amount; UiManager.Instance?.UpdateGold(gold); }
    public void AddWood(int amount) { wood += amount; UiManager.Instance?.UpdateWood(wood); }
    public void AddMeat(int amount) { meat += amount; UiManager.Instance?.UpdateMeat(meat); }

    public bool HasEnough(int g, int w, int m) => gold >= g && wood >= w && meat >= m;

    public bool Spend(int g, int w, int m)
    {
        if (!HasEnough(g, w, m)) return false;
        gold -= g; wood -= w; meat -= m;
        UiManager.Instance?.UpdateGold(gold);
        UiManager.Instance?.UpdateWood(wood);
        UiManager.Instance?.UpdateMeat(meat);
        return true;
    }

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
