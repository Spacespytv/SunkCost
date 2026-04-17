using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
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

    [Header("Enemy Catalog")]
    [SerializeField] private EnemySpawnSettings[] enemyPool;

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(delay);

            SpawnRandomEnemy();
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
        if (mole != null)
        {
            mole.InitializeFromSpawner(settings.isCeilingEnemy);
        }
    }
}