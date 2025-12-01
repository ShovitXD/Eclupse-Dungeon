using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DungeonCreator dungeonCreator;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private GameObject enemyType1Prefab;
    [SerializeField] private GameObject enemyType2Prefab;

    [Header("Player & Boss Spawn")]
    [SerializeField] private float playerSpawnYOffset = 0.5f;
    [SerializeField] private float bossSpawnYOffset = 0.5f;

    [Header("Enemy Spawn Settings")]
    [SerializeField] private int enemyType1PoolSize = 20;
    [SerializeField] private int enemyType2PoolSize = 20;
    [SerializeField] private int initialEnemyType1Count = 5;
    [SerializeField] private int initialEnemyType2Count = 5;

    [Tooltip("Min distance from player to spawn normal enemies.")]
    [SerializeField] private float minSpawnDistance = 10f;

    [Tooltip("Max distance from player to spawn normal enemies.")]
    [SerializeField] private float maxSpawnDistance = 20f;

    [Tooltip("Height above candidate point from which we raycast down to find floor.")]
    [SerializeField] private float spawnRaycastHeight = 10f;

    [Tooltip("Vertical offset above the floor hit point for enemies.")]
    [SerializeField] private float spawnYOffset = 0.5f;

    [SerializeField] private int maxSpawnPositionTries = 10;

    [Tooltip("Layer(s) used for floor colliders.")]
    [SerializeField] private LayerMask groundLayer;

    private readonly List<GameObject> enemyType1Pool = new List<GameObject>();
    private readonly List<GameObject> enemyType2Pool = new List<GameObject>();

    private Transform playerInstance;
    private Transform bossInstance;

    private void Start()
    {
        StartCoroutine(InitializeRoutine());
    }

    private IEnumerator InitializeRoutine()
    {
        // Wait one frame so DungeonCreator has time to generate the dungeon
        yield return null;

        if (dungeonCreator == null)
        {
            dungeonCreator = FindObjectOfType<DungeonCreator>();
        }

        if (dungeonCreator == null)
        {
            Debug.LogWarning("Spawner: DungeonCreator reference missing.");
            yield break;
        }

        SpawnPlayer();
        SpawnBoss();
        InitializePools();
        SpawnInitialEnemies();
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null) return;

        Vector3 spawnPos = dungeonCreator.smallestRoomCenter + Vector3.up * playerSpawnYOffset;
        playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity).transform;
    }

    private void SpawnBoss()
    {
        if (bossPrefab == null) return;

        Vector3 spawnPos = dungeonCreator.largestRoomCenter + Vector3.up * bossSpawnYOffset;
        bossInstance = Instantiate(bossPrefab, spawnPos, Quaternion.identity).transform;
    }

    private void InitializePools()
    {
        if (enemyType1Prefab != null)
        {
            for (int i = 0; i < enemyType1PoolSize; i++)
            {
                GameObject obj = Instantiate(enemyType1Prefab, transform);
                obj.SetActive(false);
                enemyType1Pool.Add(obj);
            }
        }

        if (enemyType2Prefab != null)
        {
            for (int i = 0; i < enemyType2PoolSize; i++)
            {
                GameObject obj = Instantiate(enemyType2Prefab, transform);
                obj.SetActive(false);
                enemyType2Pool.Add(obj);
            }
        }
    }

    private void SpawnInitialEnemies()
    {
        for (int i = 0; i < initialEnemyType1Count; i++)
        {
            SpawnEnemyFromPool(enemyType1Pool);
        }

        for (int i = 0; i < initialEnemyType2Count; i++)
        {
            SpawnEnemyFromPool(enemyType2Pool);
        }
    }

    private void SpawnEnemyFromPool(List<GameObject> pool)
    {
        if (pool == null || pool.Count == 0 || playerInstance == null) return;

        GameObject enemy = GetInactiveFromPool(pool);
        if (enemy == null) return;

        Vector3? pos = GetValidSpawnPositionAroundPlayer();
        if (!pos.HasValue) return;

        enemy.transform.position = pos.Value;
        enemy.transform.rotation = Quaternion.identity;
        enemy.SetActive(true);
    }

    private GameObject GetInactiveFromPool(List<GameObject> pool)
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeSelf)
            {
                return pool[i];
            }
        }
        return null;
    }

    private Vector3? GetValidSpawnPositionAroundPlayer()
    {
        if (playerInstance == null) return null;

        for (int attempt = 0; attempt < maxSpawnPositionTries; attempt++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float distance = Random.Range(minSpawnDistance, maxSpawnDistance);

            Vector3 flatOffset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;
            Vector3 rayOrigin = playerInstance.position + flatOffset + Vector3.up * spawnRaycastHeight;

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, spawnRaycastHeight * 2f, groundLayer))
            {
                // Only above floor
                return hit.point + Vector3.up * spawnYOffset;
            }
        }

        return null;
    }

    // Public helpers if you want to trigger extra spawns later
    public void SpawnEnemyType1()
    {
        SpawnEnemyFromPool(enemyType1Pool);
    }

    public void SpawnEnemyType2()
    {
        SpawnEnemyFromPool(enemyType2Pool);
    }
} 