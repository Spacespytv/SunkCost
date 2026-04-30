using System.Collections;
using System.Collections.Generic; 
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    [System.Serializable]
    public class EnemySpawnSettings
    {
        public string name;
        public GameObject prefab;
        public float spawnHeight;
        public bool isCeilingEnemy;

        [Header("Progression")]
        public int unlockLayer = 1; 
        [Range(1, 100)] public int spawnWeight = 10; 
    }

    [Header("Spawn Points")]
    [SerializeField] private Transform leftSpawnPos;
    [SerializeField] private Transform rightSpawnPos;

    [Header("Spawn Timing")]
    [SerializeField] private float minSpawnDelay = 1.5f;
    [SerializeField] private float maxSpawnDelay = 4f;

    [Header("Scaling Settings")]
    [SerializeField] private float minScale = 0.1f;
    [SerializeField] private float maxScale = 0.3f;
    [SerializeField] private float absoluteMinDelay = 0.4f;

    [Header("Live Debug Values (Read Only)")]
    [SerializeField] private float currentMin;
    [SerializeField] private float currentMax;
    [SerializeField] private int enemiesUnlocked;

    [Header("Enemy Catalog")]
    [SerializeField] private EnemySpawnSettings[] enemyPool;

    private Coroutine spawnLoop;
    private bool isPaused = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        RestartSpawner();
    }

    public void RestartSpawner()
    {
        if (spawnLoop != null) StopCoroutine(spawnLoop);
        isPaused = false;
        spawnLoop = StartCoroutine(SpawnRoutine());
    }

    public void SetPause(bool state)
    {
        isPaused = state;
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            int layer = GameplayManager.Instance != null ? GameplayManager.Instance.currentLayer : 1;

            float minReduction = (layer - 1) * minScale;
            float maxReduction = (layer - 1) * maxScale;

            currentMin = Mathf.Max(minSpawnDelay - minReduction, absoluteMinDelay);
            currentMax = Mathf.Max(maxSpawnDelay - maxReduction, absoluteMinDelay + 0.2f);

            if (currentMax < currentMin) currentMax = currentMin + 0.1f;

            float delay = Random.Range(currentMin, currentMax);
            yield return new WaitForSeconds(delay);

            if (!isPaused)
            {
                SpawnRandomEnemy(layer);
            }
        }
    }

    private void SpawnRandomEnemy(int currentLayer)
    {
        if (enemyPool.Length == 0) return;

        List<EnemySpawnSettings> availableEnemies = new List<EnemySpawnSettings>();
        int totalWeight = 0;

        foreach (var enemy in enemyPool)
        {
            if (currentLayer >= enemy.unlockLayer)
            {
                availableEnemies.Add(enemy);
                totalWeight += enemy.spawnWeight;
            }
        }

        if (availableEnemies.Count == 0) return;
        enemiesUnlocked = availableEnemies.Count;

        EnemySpawnSettings settings = null;
        int roll = Random.Range(0, totalWeight);
        int cursor = 0;

        foreach (var enemy in availableEnemies)
        {
            cursor += enemy.spawnWeight;
            if (roll < cursor)
            {
                settings = enemy;
                break;
            }
        }

        if (settings == null) settings = availableEnemies[0];

        bool spawnOnLeft = Random.value > 0.5f;
        float spawnX = spawnOnLeft ? leftSpawnPos.position.x : rightSpawnPos.position.x;

        Vector3 spawnPos = new Vector3(spawnX, settings.spawnHeight, 0);
        GameObject newEnemy = Instantiate(settings.prefab, spawnPos, Quaternion.identity);

        float facingDir = spawnOnLeft ? 1f : -1f;
        float yScale = settings.isCeilingEnemy ? -1f : 1f;
        newEnemy.transform.localScale = new Vector3(facingDir, yScale, 1);

        MoleEnemy mole = newEnemy.GetComponent<MoleEnemy>();
        if (mole != null) mole.InitializeFromSpawner(settings.isCeilingEnemy);
    }
}