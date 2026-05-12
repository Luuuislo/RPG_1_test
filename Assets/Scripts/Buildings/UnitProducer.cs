using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UnitEntry
{
    public string     unitName = "Unit";
    public GameObject prefab;
    public Sprite     icon;
    public int        goldCost  = 10;
    public int        woodCost  = 0;
    public int        meatCost  = 0;
    public float      trainTime = 10f;
}

// Añadir a Barracks / Archery / Monastery.
// Gestiona la cola de entrenamiento y el spawn de unidades.
public class UnitProducer : MonoBehaviour
{
    [Header("Available Units (max 3)")]
    public UnitEntry[] units = new UnitEntry[0];

    [Header("Queue")]
    public int maxQueueSize = 5;

    [Header("Spawn")]
    [Tooltip("Offset desde el centro del edificio donde aparecen las unidades")]
    public Vector3 spawnOffset = new Vector3(1.5f, -0.5f, 0f);

    // Estado público (leído por UnitProducerUI)
    public bool      IsTraining    { get; private set; }
    public float     TrainProgress { get; private set; }  // 0–1
    public UnitEntry CurrentUnit   { get; private set; }
    public int       QueueCount    => _queue.Count;

    // La UI se suscribe a este evento para refrescarse
    public System.Action onStateChanged;

    private readonly List<UnitEntry> _queue = new List<UnitEntry>();
    private BuildingLevel _bl;

    void Awake() => _bl = GetComponent<BuildingLevel>();

    // ── API pública ──────────────────────────────────────────────────────

    public bool TryEnqueue(int unitIndex)
    {
        if (unitIndex < 0 || unitIndex >= units.Length) return false;
        if (_queue.Count >= maxQueueSize) return false;

        var entry = units[unitIndex];
        var res   = PlayerResources.Instance;
        if (res == null || !res.HasEnough(entry.goldCost, entry.woodCost, entry.meatCost))
            return false;

        res.Spend(entry.goldCost, entry.woodCost, entry.meatCost);
        _queue.Add(entry);
        onStateChanged?.Invoke();

        if (!IsTraining) StartCoroutine(TrainLoop());
        return true;
    }

    // Cancela y reembolsa el último de la cola (no el que se está entrenando)
    public void CancelLast()
    {
        if (_queue.Count == 0) return;
        var entry = _queue[_queue.Count - 1];
        _queue.RemoveAt(_queue.Count - 1);

        var res = PlayerResources.Instance;
        if (res != null)
        {
            if (entry.goldCost > 0) res.AddGold(entry.goldCost);
            if (entry.woodCost > 0) res.AddWood(entry.woodCost);
            if (entry.meatCost > 0) res.AddMeat(entry.meatCost);
        }
        onStateChanged?.Invoke();
    }

    public UnitEntry[] GetQueueSnapshot() => _queue.ToArray();

    // ── Entrenamiento ────────────────────────────────────────────────────

    IEnumerator TrainLoop()
    {
        IsTraining = true;
        while (_queue.Count > 0)
        {
            CurrentUnit   = _queue[0];
            _queue.RemoveAt(0);
            TrainProgress = 0f;
            onStateChanged?.Invoke();

            float speed         = _bl != null ? Mathf.Max(_bl.currentAtkSpeed, 0.1f) : 1f;
            float effectiveTime = Mathf.Max(CurrentUnit.trainTime / speed, 3f); // mínimo 3s
            float elapsed = 0f;
            while (elapsed < effectiveTime)
            {
                elapsed       += Time.deltaTime;
                TrainProgress  = Mathf.Clamp01(elapsed / effectiveTime);
                onStateChanged?.Invoke();
                yield return null;
            }

            SpawnUnit(CurrentUnit);
            CurrentUnit   = null;
            TrainProgress = 0f;
            onStateChanged?.Invoke();
        }
        IsTraining = false;
        onStateChanged?.Invoke();
    }

    void SpawnUnit(UnitEntry entry)
    {
        if (entry.prefab == null) return;
        Instantiate(entry.prefab, transform.position + spawnOffset, Quaternion.identity);
    }
}
