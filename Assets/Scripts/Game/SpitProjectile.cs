using UnityEngine;

public class SpitProjectile : MonoBehaviour
{
    [SerializeField] private float defaultSpeed = 10f;
    [SerializeField] private float defaultHeightOffset = 1f;

    private float currentSpeed;
    private float heightOffset;
    private Vector3 direction;
    private bool hasDirection;

    // Per-projectile custom speed (set by spawners like Enemy2 / Boss)
    private bool hasCustomSpeed;
    private float customSpeed;

    private void OnEnable()
    {
        // Speed selection priority:
        // 1) Custom per-shot speed from spawner
        // 2) ValueHandler (global run value)
        // 3) Serialized defaultSpeed
        if (hasCustomSpeed)
        {
            currentSpeed = customSpeed;
        }
        else if (ValueHandler.Instance != null)
        {
            currentSpeed = ValueHandler.Instance.SpitProjectileSpeed;
        }
        else
        {
            currentSpeed = defaultSpeed;
        }

        heightOffset = defaultHeightOffset;

        // If spawner did not set a direction, auto-aim at player once
        if (!hasDirection)
        {
            CalculateDirectionToPlayer();
        }
    }

    private void OnDisable()
    {
        // Reset for pooling reuse
        hasDirection = false;
        direction = Vector3.zero;

        hasCustomSpeed = false;
        customSpeed = 0f;
    }

    /// <summary>
    /// Override projectile speed per shot.
    /// Call from Enemy2 / Boss right after spawning.
    /// </summary>
    public void SetSpeed(float speed)
    {
        if (speed <= 0f)
            return;

        customSpeed = speed;
        hasCustomSpeed = true;
        currentSpeed = speed;
    }

    /// <summary>
    /// Set flight direction (for patterns).
    /// If not set, projectile auto-aims at player once on spawn.
    /// </summary>
    public void SetDirection(Vector3 dir)
    {
        if (dir.sqrMagnitude < 0.0001f)
        {
            hasDirection = false;
            return;
        }

        direction = dir.normalized;
        hasDirection = true;
    }

    private void CalculateDirectionToPlayer()
    {
        PlayerHP player = FindObjectOfType<PlayerHP>();
        if (player == null)
            return;

        Vector3 targetPos = player.transform.position + Vector3.up * heightOffset;
        Vector3 toTarget = targetPos - transform.position;

        if (toTarget.sqrMagnitude < 0.0001f)
            return;

        direction = toTarget.normalized;
        hasDirection = true;
    }

    private void Update()
    {
        if (!hasDirection || currentSpeed <= 0f)
            return;

        transform.position += direction * currentSpeed * Time.deltaTime;
    }
}
