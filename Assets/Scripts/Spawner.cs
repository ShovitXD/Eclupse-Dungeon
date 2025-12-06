using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DungeonCreator dungeonCreator;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private GameObject enemyType1Prefab;
    [SerializeField] private GameObject enemyType2Prefab;

    [Header("Weapon Spawning")]
    [SerializeField] private GameObject swordWeaponPrefab;
    [SerializeField] private GameObject spearWeaponPrefab;
    [SerializeField] private GameObject bowWeaponPrefab;
    [SerializeField] private GameObject amuletWeaponPrefab;

    [Header("Weapon Offsets")]
    [SerializeField] private Vector3 swordLocalScale = Vector3.one;
    [SerializeField] private Vector3 spearLocalScale = Vector3.one;

    [SerializeField] private Vector3 bowLocalPosOffset = new Vector3(0f, 1f, 1f);
    [SerializeField] private Vector3 bowLocalEulerOffset = Vector3.zero;
    [SerializeField] private Vector3 bowLocalScale = Vector3.one;

    [SerializeField] private Vector3 amuletLocalPosOffset = new Vector3(0f, 1f, 1f);
    [SerializeField] private Vector3 amuletLocalEulerOffset = Vector3.zero;
    [SerializeField] private Vector3 amuletLocalScale = Vector3.one;

    [Header("Player & Boss Spawn")]
    [SerializeField] private float playerSpawnYOffset = 0.5f;
    [SerializeField] private float bossSpawnYOffset = 0.5f;

    [Header("Enemy Spawn Settings")]
    [SerializeField] private int enemyType1PoolSize = 20;
    [SerializeField] private int enemyType2PoolSize = 20;

    [Tooltip("Minimum enemy on scrren at a time")]
    [SerializeField] private int initialEnemyType1Count = 5;
    [SerializeField] private int initialEnemyType2Count = 5;

    [Header("Enemy Spawn Scaling")]
    [SerializeField] private int localLevelStepForExtraEnemy = 2;
    [SerializeField] private float minSpawnDistance = 10f;
    [SerializeField] private float maxSpawnDistance = 20f;
    [SerializeField] private float spawnRaycastHeight = 10f;
    [SerializeField] private float spawnYOffset = 0.5f;
    [SerializeField] private int maxSpawnPositionTries = 10;
    [SerializeField] private LayerMask groundLayer;

    private readonly List<GameObject> enemyType1Pool = new List<GameObject>();
    private readonly List<GameObject> enemyType2Pool = new List<GameObject>();

    private Transform playerInstance;
    private Transform bossInstance;
    private Transform currentWeaponInstance;

    // Lifecycle
    private void OnEnable()
    {
        XPHandler.OnLocalLevelChanged += HandleLocalLevelChanged;
    }

    private void OnDisable()
    {
        XPHandler.OnLocalLevelChanged -= HandleLocalLevelChanged;
    }

    private void Start()
    {
        StartCoroutine(InitializeRoutine());
    }

    // One frame delay so dungeon generation finishes before spawning enemies
    private IEnumerator InitializeRoutine()
    {
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

    private void Update()
    {
        int localLevel = 0;

        if (XPHandler.Instance != null)
        {
            localLevel = XPHandler.Instance.LocalLevel;
        }

        UpdateEnemiesForLocalLevel(localLevel);
    }

    // Player and boss spawning
    private void SpawnPlayer()
    {
        if (playerPrefab == null) return;

        Vector3 spawnPos = dungeonCreator.smallestRoomCenter + Vector3.up * playerSpawnYOffset;
        playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity).transform;

        AttachSelectedWeaponToPlayer();
    }

    private void SpawnBoss()
    {
        if (bossPrefab == null) return;

        Vector3 spawnPos = dungeonCreator.largestRoomCenter + Vector3.up * bossSpawnYOffset;
        bossInstance = Instantiate(bossPrefab, spawnPos, Quaternion.identity).transform;
    }

    private void SpawnInitialEnemies()
    {
        int initialLocalLevel = 0;

        if (XPHandler.Instance != null)
        {
            initialLocalLevel = XPHandler.Instance.LocalLevel;
        }

        if (initialLocalLevel < 0)
        {
            initialLocalLevel = 0;
        }

        UpdateEnemiesForLocalLevel(initialLocalLevel);
    }

    private void HandleLocalLevelChanged(int newLocalLevel)
    {
        UpdateEnemiesForLocalLevel(newLocalLevel);
    }

    // Enemy count management based on local level
    private void UpdateEnemiesForLocalLevel(int localLevel)
    {
        if (localLevel < 0)
        {
            localLevel = 0;
        }

        // atleast 5 of each enemy, or the inspector value if higher
        int minType1 = Mathf.Max(initialEnemyType1Count, 5);
        int minType2 = Mathf.Max(initialEnemyType2Count, 5);

        int extraEnemiesPerType = 0;

        if (localLevelStepForExtraEnemy > 0)
        {
            // Every 'localLevelStepForExtraEnemy' levels will adds one extra enemy of both type
            extraEnemiesPerType = localLevel / localLevelStepForExtraEnemy;
        }

        int targetType1 = minType1 + extraEnemiesPerType;
        int targetType2 = minType2 + extraEnemiesPerType;

        int activeType1 = CountActiveEnemies(enemyType1Pool);
        int activeType2 = CountActiveEnemies(enemyType2Pool);

        int toSpawnType1 = Mathf.Max(0, targetType1 - activeType1);
        int toSpawnType2 = Mathf.Max(0, targetType2 - activeType2);

        for (int i = 0; i < toSpawnType1; i++)
        {
            SpawnEnemyFromPool(enemyType1Pool);
        }

        for (int i = 0; i < toSpawnType2; i++)
        {
            SpawnEnemyFromPool(enemyType2Pool);
        }
    }

    private int CountActiveEnemies(List<GameObject> pool)
    {
        if (pool == null)
        {
            return 0;
        }

        int count = 0;

        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i] != null && pool[i].activeSelf)
            {
                count++;
            }
        }

        return count;
    }

    // Weapon selection and attachment
    private void AttachSelectedWeaponToPlayer()
    {
        if (playerInstance == null)
            return;

        if (GameManager.Instance == null)
        {
            Debug.Log("No Weapons");
            return;
        }

        GameManager.WeaponType selected = GameManager.Instance.SelectedWeapon;
        if (selected == GameManager.WeaponType.None)
        {
            Debug.Log("No Weapons");
            return;
        }

        GameObject weaponPrefab = GetWeaponPrefab(selected);
        if (weaponPrefab == null)
        {
            Debug.Log("No Weapons");
            return;
        }

        if (currentWeaponInstance != null)
        {
            Destroy(currentWeaponInstance.gameObject);
        }

        Transform weaponTransform = Instantiate(weaponPrefab, playerInstance).transform;

        Vector3 posOffset = Vector3.zero;
        Vector3 rotOffset = Vector3.zero;
        Vector3 scaleOffset = Vector3.one;

        switch (selected)
        {
            case GameManager.WeaponType.Sword:
                scaleOffset = swordLocalScale;
                break;
            case GameManager.WeaponType.Spear:
                scaleOffset = spearLocalScale;
                break;
            case GameManager.WeaponType.Bow:
                posOffset = bowLocalPosOffset;
                rotOffset = bowLocalEulerOffset;
                scaleOffset = bowLocalScale;
                break;
            case GameManager.WeaponType.Amulet:
                posOffset = amuletLocalPosOffset;
                rotOffset = amuletLocalEulerOffset;
                scaleOffset = amuletLocalScale;
                break;
        }

        weaponTransform.localPosition = posOffset;
        weaponTransform.localRotation = Quaternion.Euler(rotOffset);
        weaponTransform.localScale = scaleOffset;

        currentWeaponInstance = weaponTransform;
    }

    private GameObject GetWeaponPrefab(GameManager.WeaponType weaponType)
    {
        switch (weaponType)
        {
            case GameManager.WeaponType.Sword:
                return swordWeaponPrefab;
            case GameManager.WeaponType.Spear:
                return spearWeaponPrefab;
            case GameManager.WeaponType.Bow:
                return bowWeaponPrefab;
            case GameManager.WeaponType.Amulet:
                return amuletWeaponPrefab;
            default:
                return null;
        }
    }
    // Enemy pool setup
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

    // Enemy pooling and spawn helpers
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

        for (int i = 0; i < maxSpawnPositionTries; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere;
            randomDirection.y = 0f;
            randomDirection.Normalize();

            float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance);
            Vector3 candidatePoint = playerInstance.position + randomDirection * randomDistance;

            Vector3 rayOrigin = candidatePoint + Vector3.up * spawnRaycastHeight;
            Ray ray = new Ray(rayOrigin, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hit, spawnRaycastHeight * 2f, groundLayer))
            {
                Vector3 finalPosition = hit.point + Vector3.up * spawnYOffset;
                return finalPosition;
            }
        }

        return null;
    }

    public void SpawnEnemyType1()
    {
        SpawnEnemyFromPool(enemyType1Pool);
    }

    public void SpawnEnemyType2()
    {
        SpawnEnemyFromPool(enemyType2Pool);
    }
}
