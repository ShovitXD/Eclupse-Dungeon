using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    [Header("Pool Identity")]
    [Tooltip("ID used to find this pool at runtime, e.g. 'Arrow' or 'EnemyProjectile'.")]
    public string poolId = "Default";

    [Header("Pool Settings")]
    [SerializeField] private PooledProjectile projectilePrefab;
    [SerializeField] private int initialSize = 32;
    [SerializeField] private Transform poolParent;

    private readonly Queue<PooledProjectile> pool = new Queue<PooledProjectile>();

    private static readonly Dictionary<string, ProjectilePool> pools =
        new Dictionary<string, ProjectilePool>();

    private void Awake()
    {
        if (!Application.isPlaying)
            return;

        if (string.IsNullOrEmpty(poolId))
        {
            Debug.LogWarning($"{name}: ProjectilePool.poolId is empty. Set a unique ID.");
        }
        else
        {
            if (pools.ContainsKey(poolId) && pools[poolId] != this)
            {
                Debug.LogWarning(
                    $"{name}: Duplicate ProjectilePool with id '{poolId}'. Overwriting previous entry.");
                pools[poolId] = this;
            }
            else
            {
                pools[poolId] = this;
            }
        }

        if (poolParent == null)
            poolParent = transform;

        Prewarm();
    }

    private void OnDestroy()
    {
        if (!Application.isPlaying)
            return;

        if (!string.IsNullOrEmpty(poolId) && pools.ContainsKey(poolId) && pools[poolId] == this)
        {
            pools.Remove(poolId);
        }
    }

    private void Prewarm()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError($"{name}: ProjectilePool has no projectilePrefab assigned.");
            return;
        }

        for (int i = 0; i < initialSize; i++)
        {
            PooledProjectile proj = CreateNew();
            if (proj == null)
                continue;

            proj.gameObject.SetActive(false);
            pool.Enqueue(proj);
        }
    }

    private PooledProjectile CreateNew()
    {
        if (projectilePrefab == null)
            return null;

        // No parent in Instantiate → parent later
        PooledProjectile proj = Instantiate(projectilePrefab);
        proj.SetPool(this);

        if (poolParent != null)
        {
            proj.transform.SetParent(poolParent, false);
        }

        return proj;
    }

    public PooledProjectile Get(Vector3 position, Quaternion rotation)
    {
        if (projectilePrefab == null)
        {
            Debug.LogError($"{name}: ProjectilePool has no projectilePrefab assigned.");
            return null;
        }

        PooledProjectile proj;

        if (pool.Count > 0)
        {
            proj = pool.Dequeue();
        }
        else
        {
            proj = CreateNew();
        }

        if (proj == null)
            return null;

        Transform t = proj.transform;
        t.SetPositionAndRotation(position, rotation);
        proj.gameObject.SetActive(true);

        return proj;
    }

    public void Return(PooledProjectile proj)
    {
        if (proj == null)
            return;

        proj.gameObject.SetActive(false);
        pool.Enqueue(proj);
    }

    public static ProjectilePool GetPool(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        ProjectilePool pool;
        pools.TryGetValue(id, out pool);
        return pool;
    }
}
