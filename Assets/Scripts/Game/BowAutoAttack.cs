// Assets/Scripts/Combat/BowAutoAttack.cs
using System.Collections;
using UnityEngine;

public class BowAutoAttack : MonoBehaviour
{
    [Header("Shooting")]
    [Tooltip("ID of the ProjectilePool to use for this bow, e.g. 'Arrow'.")]
    public string projectilePoolId = "Arrow";

    [Tooltip("Optional spawn point. If null, uses this transform.")]
    public Transform shootPoint;

    [Min(0.01f)]
    [Tooltip("Seconds between auto attacks. Also used as bow blend-shape duration.")]
    public float attackInterval = 0.8f;

    [Min(0f)]
    [Tooltip("Delay before the very first shot after enabling.")]
    public float initialDelay = 0f;

    [Header("Bow Blend Shape")]
    [Tooltip("SkinnedMeshRenderer that has the Key1 blend shape.")]
    public SkinnedMeshRenderer bowRenderer;

    [Tooltip("Index of the Key1 blend shape in this mesh.")]
    public int blendShapeIndex = 0;

    [Min(1)]
    [Tooltip("Number of steps from 0 to 100.")]
    public int blendShapeSteps = 12;

    private float _attackTimer;
    private bool _missingPoolWarned;
    private Coroutine _blendRoutine;
    private bool _blendWarned;

    private void Awake()
    {
        // Attack speed is tunable via ValueHandler; initialDelay stays inspector-controlled
        if (ValueHandler.Instance != null)
        {
            attackInterval = Mathf.Max(0.01f, ValueHandler.Instance.BowAttackInterval);
        }
    }

    private void OnEnable()
    {
        _attackTimer = Mathf.Max(0f, initialDelay);
    }

    private void Update()
    {
        _attackTimer -= Time.deltaTime;
        if (_attackTimer <= 0f)
        {
            Shoot();
            _attackTimer = Mathf.Max(0.01f, attackInterval);
        }
    }

    private void Shoot()
    {
        if (string.IsNullOrWhiteSpace(projectilePoolId))
        {
            Debug.LogWarning("BowAutoAttack: projectilePoolId is empty.");
            return;
        }

        var pool = ProjectilePool.GetPool(projectilePoolId);
        if (pool == null)
        {
            if (!_missingPoolWarned)
            {
                Debug.LogWarning($"BowAutoAttack: No ProjectilePool found with id '{projectilePoolId}'.");
                _missingPoolWarned = true;
            }
            return;
        }

        Vector3 spawnPos = shootPoint != null ? shootPoint.position : transform.position;
        Quaternion spawnRot = shootPoint != null ? shootPoint.rotation : transform.rotation;

        var proj = pool.Get(spawnPos, spawnRot);
        if (proj == null) return;

        Animation();
    }

    public void Animation()
    {
        if (bowRenderer == null)
        {
            if (!_blendWarned)
            {
                Debug.LogWarning("BowAutoAttack: bowRenderer is not assigned.");
                _blendWarned = true;
            }
            return;
        }

        var mesh = bowRenderer.sharedMesh;
        if (mesh == null || mesh.blendShapeCount == 0 || blendShapeIndex < 0 || blendShapeIndex >= mesh.blendShapeCount)
        {
            if (!_blendWarned)
            {
                Debug.LogWarning(
                    $"BowAutoAttack: Invalid blend shape index {blendShapeIndex}. " +
                    $"Mesh has {mesh?.blendShapeCount ?? 0} blend shapes.");
                _blendWarned = true;
            }
            return;
        }

        if (_blendRoutine != null)
            StopCoroutine(_blendRoutine);

        _blendRoutine = StartCoroutine(BlendShapeRoutine());
    }

    private IEnumerator BlendShapeRoutine()
    {
        if (blendShapeSteps < 1) blendShapeSteps = 1;

        // Use attackInterval as the full 0 -> 100 duration
        float totalUpTime = Mathf.Max(0.0001f, attackInterval);
        float stepTime = totalUpTime / blendShapeSteps;

        // 0 -> 100
        for (int i = 0; i <= blendShapeSteps; i++)
        {
            float t = (float)i / blendShapeSteps;    // 0..1
            float weight = Mathf.Lerp(0f, 100f, t);  // 0..100
            bowRenderer.SetBlendShapeWeight(blendShapeIndex, weight);

            yield return new WaitForSeconds(stepTime);
        }

        // back to 0 just before the next shot
        bowRenderer.SetBlendShapeWeight(blendShapeIndex, 0f);
        _blendRoutine = null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        attackInterval = Mathf.Max(0.01f, attackInterval);
        initialDelay = Mathf.Max(0f, initialDelay);
        blendShapeSteps = Mathf.Max(1, blendShapeSteps);
    }
#endif
}
