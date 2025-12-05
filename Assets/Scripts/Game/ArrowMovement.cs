using UnityEngine;

/// <summary>
/// Handles arrow propulsion and configures combat stats on spawn.
/// Requires a PooledProjectile on the same GameObject.
/// </summary>
[RequireComponent(typeof(PooledProjectile))]
public class ArrowMovement : MonoBehaviour
{
    [Header("Motion")]
    [Min(0f)] public float speed = 20f;
    public bool useRigidbody = true;
    public bool useGravity = false;

    [Header("Combat")]
    [Min(0f)] public float damage = 5f;
    public PooledProjectile.TargetType targetType = PooledProjectile.TargetType.Enemy;

    [Header("Lifetime")]
    [Tooltip("Seconds before auto-despawn. 0 disables.")]
    [Min(0f)] public float maxLifetime = 10f;

    private Rigidbody _rb;
    private PooledProjectile _proj;
    private float _lifeTimer;

    private void Awake()
    {
        _proj = GetComponent<PooledProjectile>();
        _rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        // Bow damage from ValueHandler (roguelike-adjustable)
        if (ValueHandler.Instance != null)
        {
            damage = Mathf.Max(0f, ValueHandler.Instance.BowDamage);
        }

        if (_proj != null)
            _proj.ConfigureCombat(damage, targetType);
        else
            Debug.LogWarning("ArrowMovement: Missing PooledProjectile.");

        _lifeTimer = 0f;

        if (useRigidbody)
        {
            if (_rb == null)
                _rb = GetComponent<Rigidbody>();

            if (_rb != null)
            {
                _rb.isKinematic = false;
                _rb.useGravity = useGravity;

                // NOTE: flipped direction here
                _rb.linearVelocity = -transform.forward * speed;

                _rb.angularVelocity = Vector3.zero;
            }
            else
            {
                Debug.LogWarning("ArrowMovement: useRigidbody enabled but no Rigidbody found.");
            }
        }
    }

    private void Update()
    {
        if (!useRigidbody)
        {
            // NOTE: flipped direction here too
            transform.position += -transform.forward * (speed * Time.deltaTime);
        }

        if (maxLifetime > 0f)
        {
            _lifeTimer += Time.deltaTime;
            if (_lifeTimer >= maxLifetime)
                Despawn();
        }
    }

    private void OnDisable()
    {
        _lifeTimer = 0f;

        if (_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
    }

    private void Despawn()
    {
        if (_proj != null)
            _proj.Despawn();
        else
            gameObject.SetActive(false);
    }
}
