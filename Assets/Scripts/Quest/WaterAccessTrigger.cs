using System.Collections;
using TMPro;
using UnityEngine;

// Añadir al GameObject Trigger_Acceso_Agua.
// El Collider2D debe ser SÓLIDO (no trigger) para bloquear al player.
// Al desbloquear se desactiva el collider y desaparece el visual.
public class WaterAccessTrigger : MonoBehaviour
{
    [Tooltip("Collider2D sólido (no trigger) que bloquea el paso. Si está vacío usa el del mismo GO.")]
    public Collider2D barrierCollider;

    [Tooltip("Objeto visual opcional que desaparece al desbloquear (puerta, cadena, etc.)")]
    public GameObject barrierVisual;

    [Tooltip("Mensaje mostrado cuando el player intenta pasar sin la misión completada")]
    public string blockMessage = "Solo el digno pasará...";

    private bool        _unlocked;
    private TextMeshPro _msgText;
    private Coroutine   _msgCoroutine;
    private float       _nextMessageTime;

    void Awake()
    {
        if (barrierCollider == null)
            barrierCollider = GetComponent<Collider2D>();

        // Crea el texto flotante encima de la barrera (se auto-oculta)
        var go              = new GameObject("BlockMessage");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, 1.8f, 0f);
        _msgText            = go.AddComponent<TextMeshPro>();
        _msgText.text       = blockMessage;
        _msgText.fontSize   = 3.5f;
        _msgText.alignment  = TextAlignmentOptions.Center;
        _msgText.color      = new Color(1f, 0.85f, 0.3f, 0f);
        _msgText.outlineWidth = 0.3f;
        _msgText.outlineColor = new Color32(0, 0, 0, 220);
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(5f, 1.2f);
        go.SetActive(false);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (_unlocked) return;
        if (!col.gameObject.CompareTag("Player")) return;
        if (Time.time < _nextMessageTime) return;

        _nextMessageTime = Time.time + 3f;
        if (_msgCoroutine != null) StopCoroutine(_msgCoroutine);
        _msgCoroutine = StartCoroutine(FadeMessage());
    }

    IEnumerator FadeMessage()
    {
        _msgText.gameObject.SetActive(true);
        float t = 0f;
        while (t < 1f) { t += Time.deltaTime * 4f; SetAlpha(t); yield return null; }
        yield return new WaitForSeconds(2.5f);
        while (t > 0f) { t -= Time.deltaTime * 2f; SetAlpha(t); yield return null; }
        _msgText.gameObject.SetActive(false);
    }

    void SetAlpha(float a) =>
        _msgText.color = new Color(1f, 0.85f, 0.3f, Mathf.Clamp01(a));

    public void UnlockAccess()
    {
        if (_unlocked) return;
        _unlocked = true;
        if (barrierCollider != null) barrierCollider.enabled = false;
        if (barrierVisual   != null) barrierVisual.SetActive(false);
        if (_msgCoroutine   != null) StopCoroutine(_msgCoroutine);
        if (_msgText        != null) _msgText.gameObject.SetActive(false);
    }
}
