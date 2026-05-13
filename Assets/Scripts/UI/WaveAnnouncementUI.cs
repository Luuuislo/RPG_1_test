using System.Collections;
using TMPro;
using UnityEngine;

// Panel de anuncio de oleada. Crear con RPGTools > Create Wave Announcement UI.
// Asignar la referencia en WaveManager > Wave Announcement.
public class WaveAnnouncementUI : MonoBehaviour
{
    [Header("Texts")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI subtitleText;

    [Header("Format")]
    [Tooltip("Usa {0} para número de oleada y {1} para total. Ej: '¡OLEADA {0}!'")]
    public string titleFormat      = "¡OLEADA {0}!";
    [Tooltip("Subtítulo por defecto si la oleada no tiene uno personalizado.")]
    public string defaultSubtitle  = "¡Defiende tu castillo!";

    [Header("Timing")]
    public float fadeInDuration  = 0.4f;
    public float displayDuration = 2.5f;
    public float fadeOutDuration = 0.8f;

    private CanvasGroup _group;
    private Coroutine   _routine;

    void Awake()
    {
        _group = GetComponent<CanvasGroup>();
        if (_group == null) _group = gameObject.AddComponent<CanvasGroup>();
        _group.alpha          = 0f;
        _group.blocksRaycasts = false;
        _group.interactable   = false;
    }

    public void Show(int waveNumber, int totalWaves, string customSubtitle = null)
    {
        if (titleText    != null)
            titleText.text    = string.Format(titleFormat, waveNumber, totalWaves);
        if (subtitleText != null)
            subtitleText.text = string.IsNullOrEmpty(customSubtitle) ? defaultSubtitle : customSubtitle;

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        float t = 0f;
        while (t < 1f) { t += Time.deltaTime / Mathf.Max(fadeInDuration, 0.01f);  _group.alpha = Mathf.Clamp01(t); yield return null; }
        yield return new WaitForSeconds(displayDuration);
        t = 1f;
        while (t > 0f) { t -= Time.deltaTime / Mathf.Max(fadeOutDuration, 0.01f); _group.alpha = Mathf.Clamp01(t); yield return null; }
        _group.alpha = 0f;
    }
}
