using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public enum ResourceType { Gold, Wood, Meat }

    public ResourceType resourceType;
    public int quantity = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerResources pr = other.GetComponentInParent<PlayerResources>();
        if (pr == null) return;

        switch (resourceType)
        {
            case ResourceType.Gold: pr.AddGold(quantity); break;
            case ResourceType.Wood: pr.AddWood(quantity); break;
            case ResourceType.Meat: pr.AddMeat(quantity); break;
        }

        Destroy(gameObject);
    }
}
