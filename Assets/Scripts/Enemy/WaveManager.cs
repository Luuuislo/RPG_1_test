using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

[System.Serializable]
public class EnemySpawnEntry
{
    public GameObject prefab;
    public int        count            = 3;
    [Range(0.5f, 10f)] public float healthMultiplier = 1f;
    [Range(0.5f, 10f)] public float damageMultiplier = 1f;
}

[System.Serializable]
public class WaveData
{
    public string            waveName        = "Wave";
    public EnemySpawnEntry[] enemies;
    [Min(0f)] public float   delayBeforeWave = 5f;
}

public class WaveManager : MonoBehaviour
{
    [Header("Waves")]
    public WaveData[] waves = new WaveData[5];

    [Header("Spawn Points")]
    [Tooltip("Dejar vacío para auto-detectar todos los WaveSpawnPoint de la escena.")]
    public WaveSpawnPoint[] spawnPoints;

    [Header("Settings")]
    public bool autoStart             = false;
    public float spawnIntervalSeconds = 0.25f;

    [Header("UI (opcional)")]
    public TextMeshProUGUI waveLabel;
    public TextMeshProUGUI statusLabel;

    [Header("Events")]
    public UnityEvent onAllWavesCleared;

    public int  CurrentWave    { get; private set; } = -1;
    public bool IsRunning      { get; private set; }
    public bool WavesCompleted { get; private set; }

    int   _aliveCount;
    bool  _waveActive;
    float _countdownTimer;
    bool  _inCountdown;

    void Start()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            spawnPoints = FindObjectsByType<WaveSpawnPoint>(FindObjectsSortMode.None);

        if (autoStart) StartWaves();
    }

    void Update()
    {
        if (!_inCountdown) return;

        _countdownTimer -= Time.deltaTime;
        SetStatus($"Próxima oleada en {Mathf.CeilToInt(_countdownTimer)}s");

        if (_countdownTimer <= 0f)
        {
            _inCountdown = false;
            StartCoroutine(SpawnWave(CurrentWave));
        }
    }

    public void StartWaves()
    {
        if (IsRunning || waves == null || waves.Length == 0) return;
        IsRunning   = true;
        CurrentWave = 0;
        LaunchWave(0);
    }

    void LaunchWave(int index)
    {
        if (index >= waves.Length)
        {
            IsRunning      = false;
            WavesCompleted = true;
            SetLabel("¡Victoria!");
            SetStatus("Todas las oleadas completadas");
            onAllWavesCleared?.Invoke();
            return;
        }

        float delay = waves[index].delayBeforeWave;
        SetLabel($"Oleada {index + 1} / {waves.Length}");

        if (delay <= 0f)
            StartCoroutine(SpawnWave(index));
        else
        {
            _countdownTimer = delay;
            _inCountdown    = true;
            SetStatus($"Próxima oleada en {Mathf.CeilToInt(delay)}s");
        }
    }

    IEnumerator SpawnWave(int index)
    {
        WaveData wave = waves[index];
        SetLabel($"Oleada {index + 1} / {waves.Length}");
        SetStatus($"¡{wave.waveName}!");

        // Count total alive before spawning anything
        _aliveCount = 0;
        foreach (var entry in wave.enemies)
            if (entry?.prefab != null) _aliveCount += entry.count;

        if (_aliveCount == 0) { AdvanceWave(); yield break; }

        _waveActive = true;

        foreach (var entry in wave.enemies)
        {
            if (entry?.prefab == null) continue;
            for (int i = 0; i < entry.count; i++)
            {
                SpawnEnemy(entry);
                yield return new WaitForSeconds(spawnIntervalSeconds);
            }
        }

        SetStatus($"Enemigos restantes: {_aliveCount}");
    }

    void SpawnEnemy(EnemySpawnEntry entry)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[WaveManager] No hay WaveSpawnPoints en la escena.");
            return;
        }

        Transform pt = spawnPoints[Random.Range(0, spawnPoints.Length)].transform;
        GameObject go = Instantiate(entry.prefab, pt.position, Quaternion.identity);

        var dr = go.GetComponentInChildren<DamageReceiver>();
        if (dr != null)
        {
            dr.maxHealth        = Mathf.Max(1, Mathf.RoundToInt(dr.maxHealth * entry.healthMultiplier));
            dr.respawnAfterDeath = false;
            dr.onDeath          += OnEnemyDied;
        }

        var enemy = go.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.attackDamage = Mathf.Max(1, Mathf.RoundToInt(enemy.attackDamage * entry.damageMultiplier));
            enemy.ActivateAsWaveEnemy();
        }
    }

    void OnEnemyDied()
    {
        _aliveCount = Mathf.Max(0, _aliveCount - 1);
        if (_waveActive)
            SetStatus($"Enemigos restantes: {_aliveCount}");

        if (_aliveCount == 0 && _waveActive)
            AdvanceWave();
    }

    void AdvanceWave()
    {
        _waveActive = false;
        SetStatus("¡Oleada completada!");
        CurrentWave++;
        LaunchWave(CurrentWave);
    }

    void SetLabel(string text)  { if (waveLabel   != null) waveLabel.text   = text; }
    void SetStatus(string text) { if (statusLabel  != null) statusLabel.text = text; }
}
