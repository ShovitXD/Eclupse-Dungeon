using UnityEngine;

public class PooledProjectile : MonoBehaviour
{
    public enum TargetType
    {
        Enemy,
        Player
    }

    [Header("Lifetime")]
    [Tooltip("Seconds before the projectile auto-despawns if it does not hit anything.")]
    public float lifeTime = 2f;

    private ProjectilePool pool;
    private float damage;
    private TargetType targetType;

    public float Damage => damage;
    public TargetType Target => targetType;

    public void SetPool(ProjectilePool pool)
    {
        this.pool = pool;
    }

    private void OnEnable()
    {
        if (lifeTime > 0f)
        {
            CancelInvoke();
            Invoke(nameof(Despawn), lifeTime);
        }
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    /// <summary>
    /// Who this projectile damages and by how much.
    /// Movement is handled by ArrowMovement / SpitProjectile.
    /// </summary>
    public void ConfigureCombat(float damage, TargetType targetType)
    {
        this.damage = damage;
        this.targetType = targetType;
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other);
    }

    private void HandleHit(Collider other)
    {
        if (targetType == TargetType.Enemy)
        {
            // Projectile from player → damage enemies
            EnemyHealthXP enemyHP =
                other.GetComponent<EnemyHealthXP>() ??
                other.GetComponentInParent<EnemyHealthXP>();

            if (enemyHP != null)
            {
                enemyHP.TakeDamage(Mathf.RoundToInt(damage));
                Despawn();
            }
        }
        else if (targetType == TargetType.Player)
        {
            // Projectile from enemy → damage player
            PlayerHP playerHP =
                other.GetComponent<PlayerHP>() ??
                other.GetComponentInParent<PlayerHP>();

            if (playerHP != null)
            {
                playerHP.TakeDamage(damage);
                Despawn();
            }
        }
    }

    public void Despawn()
    {
        if (pool != null)
            pool.Return(this);
        else
            gameObject.SetActive(false);
    }
}
