using System.Collections;
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
                SpawnRandomEnemy();
            }
        }
    }

    private void SpawnRandomEnemy()
    {
        if (enemyPool.Length == 0) return;

        EnemySpawnSettings settings = enemyPool[Random.Range(0, enemyPool.Length)];
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