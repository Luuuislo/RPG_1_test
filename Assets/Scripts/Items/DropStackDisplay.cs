using UnityEngine;
using TMPro;

// Attached at runtime to a dropped item when quantity > 1.
// Spawns visual copies (pile effect) and shows a quantity label above.
// All copies are children — destroyed automatically when the parent is picked up.
public class DropStackDisplay : MonoBehaviour
{
    private DropDisplaySettings s;

    public void Initialize(GameObject prefab, int quantity, DropDisplaySettings settings)
    {
        s = settings;
        Initialize(prefab, quantity);
    }

    public void Initialize(GameObject prefab, int quantity)
    {
        SpawnExtras(prefab, quantity);
        if (quantity > 1)
            CreateLabel(quantity);
    }

    void SpawnExtras(GameObject prefab, int quantity)
    {
        int max = s != null ? s.maxVisuals : 5;
        float spread = s != null ? s.pileSpread : 0.2f;
        int count = Mathf.Clamp(quantity - 1, 0, max - 1);
        for (int i = 0; i < count; i++)
        {
            Vector2 offset = Random.insideUnitCircle * spread;
            GameObject clone = Instantiate(prefab, transform);
            clone.transform.localPosition = new Vector3(offset.x, offset.y, 0f);

            foreach (Collider2D col in clone.GetComponentsInChildren<Collider2D>())
                col.enabled = false;

            DroppedItem di = clone.GetComponent<DroppedItem>();
            if (di != null) Destroy(di);
            ItemPickup ip = clone.GetComponent<ItemPickup>();
            if (ip != null) Destroy(ip);
        }
    }

    void CreateLabel(int quantity)
    {
        float height  = s != null ? s.labelHeight  : 0.5f;
        float size    = s != null ? s.fontSize      : 2.2f;
        Color color   = s != null ? s.labelColor    : Color.yellow;
        Color outline = s != null ? s.outlineColor  : Color.black;
        float outW    = s != null ? s.outlineWidth  : 0.25f;
        int   order   = s != null ? s.sortingOrder  : 20;

        GameObject label = new GameObject("DropLabel");
        label.transform.SetParent(transform);
        label.transform.localPosition = new Vector3(0f, height, 0f);

        TextMeshPro tmp = label.AddComponent<TextMeshPro>();
        tmp.text = quantity.ToString();
        tmp.fontSize = size;
        tmp.color = color;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.outlineWidth = outW;
        tmp.outlineColor = outline;

        MeshRenderer mr = label.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = order;
    }
}
