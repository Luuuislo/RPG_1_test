using UnityEngine;
using TMPro;

public class FloatingDamageText : MonoBehaviour
{
    public float floatSpeed   = 2.5f;
    public float fadeDuration = 3f;
    public Color textColor    = new Color(1f, 0.15f, 0.15f);

    private TextMeshPro tmp;
    private float elapsed;

    void Awake() => tmp = GetComponent<TextMeshPro>();

    public void Setup(int damage)
    {
        tmp.text         = damage.ToString();
        tmp.color        = textColor;
        tmp.outlineWidth = 0.25f;
        tmp.outlineColor = new Color32(60, 0, 0, 255);
        Destroy(gameObject, fadeDuration);
    }

    public void Setup(int amount, Color color)
    {
        tmp.text         = amount.ToString();
        tmp.color        = color;
        tmp.outlineWidth = 0.2f;
        tmp.outlineColor = new Color32(0, 60, 0, 255);
        Destroy(gameObject, fadeDuration);
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float t = elapsed / fadeDuration;
        transform.position += Vector3.up * floatSpeed * (1f - t * 0.7f) * Time.deltaTime;
        tmp.alpha = Mathf.Lerp(1f, 0f, t);
    }
}
