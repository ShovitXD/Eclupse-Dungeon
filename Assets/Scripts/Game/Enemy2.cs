using System.Collections;
using UnityEngine;

public class Enemy2 : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float minRange = 3f;   // if player is closer than this, run away
    public float maxRange = 8f;   // if player is farther than this, move closer

    [Header("Attack")]
    public float attackInterval = 3.0f;              // seconds between spits
    [Tooltip("ID of the ProjectilePool to use for this enemy, e.g. 'EnemyProjectile'.")]
    public string projectilePoolId = "EnemyProjectile";
    public Transform projectileSpawnPoint;
    public float projectileDamage = 10f;

    [Header("Attack Speed")]
    [Tooltip("Projectile speed just for Enemy2. Cards can modify this without touching the boss.")]
    public float projectileSpeed = 10f;
    [Tooltip("If true, initialize projectileSpeed from ValueHandler.SpitProjectileSpeed on Awake.")]
    public bool initSpeedFromValueHandler = true;

    [Header("Death")]
    public float deathReturnDelay = 2f;

    [Header("References")]
    public Animator animator;
    public Transform debugTarget; // optional, for testing without real player

    private static readonly int RunHash = Animator.StringToHash("Run");
    private static readonly int SpitHash = Animator.StringToHash("Spit");
    private static readonly int DeadHash = Animator.StringToHash("Dead");

    private Transform target;
    private bool isDead;

    // simple timestamp-based cooldown
    private float nextAttackTime;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (initSpeedFromValueHandler && ValueHandler.Instance != null)
        {
            projectileSpeed = ValueHandler.Instance.SpitProjectileSpeed;
        }
    }

    private void OnEnable()
    {
        isDead = false;

        // allow immediate attack as soon as we are in the ideal band
        nextAttackTime = Time.time;

        if (animator != null)
        {
            animator.SetBool(RunHash, false);
            animator.SetBool(SpitHash, false);
            animator.SetBool(DeadHash, false);
        }

        AcquireTarget();
    }

    private void Update()
    {
        if (isDead)
            return;

        if (target == null)
            AcquireTarget();

        if (target == null)
            return;

        Vector3 toTarget = target.position - transform.position;
        Vector3 flatToTarget = new Vector3(toTarget.x, 0f, toTarget.z);
        float distance = flatToTarget.magnitude;

        if (distance < minRange)
        {
            // too close – run away
            MoveInDirection(-flatToTarget);

            if (animator != null)
            {
                animator.SetBool(RunHash, true);
                animator.SetBool(SpitHash, false);
            }
        }
        else if (distance > maxRange)
        {
            // too far – move closer
            MoveInDirection(flatToTarget);

            if (animator != null)
            {
                animator.SetBool(RunHash, true);
                animator.SetBool(SpitHash, false);
            }
        }
        else
        {
            // ideal band – stand and spit
            if (animator != null)
            {
                animator.SetBool(RunHash, false);
                animator.SetBool(SpitHash, true);
            }

            FaceTarget();
            TryAttack();
        }
    }

    private void TryAttack()
    {
        if (attackInterval <= 0f)
            attackInterval = 0.1f;

        if (Time.time < nextAttackTime)
            return;

        SpawnProjectile();
        nextAttackTime = Time.time + attackInterval;
    }

    private void AcquireTarget()
    {
        if (debugTarget != null)
        {
            target = debugTarget;
            return;
        }

        PlayerHP hp = FindObjectOfType<PlayerHP>();
        if (hp != null)
            target = hp.transform.root;
    }

    private void MoveInDirection(Vector3 dir)
    {
        if (dir.sqrMagnitude < 0.0001f)
            return;

        Vector3 direction = dir.normalized;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(direction),
            10f * Time.deltaTime
        );

        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    private void FaceTarget()
    {
        if (target == null)
            return;

        Vector3 toTarget = target.position - transform.position;
        Vector3 flatToTarget = new Vector3(toTarget.x, 0f, toTarget.z);

        if (flatToTarget.sqrMagnitude < 0.0001f)
            return;

        Vector3 dir = flatToTarget.normalized;
        transform.rotation = Quaternion.LookRotation(dir);
    }

    private void SpawnProjectile()
    {
        ProjectilePool pool = ProjectilePool.GetPool(projectilePoolId);
        if (pool == null)
        {
            Debug.LogWarning($"Enemy2: No ProjectilePool found with id '{projectilePoolId}'.");
            return;
        }

        Vector3 spawnPos = (projectileSpawnPoint != null)
            ? projectileSpawnPoint.position
            : transform.position;

        Quaternion spawnRot = transform.rotation;

        PooledProjectile proj = pool.Get(spawnPos, spawnRot);
        if (proj == null)
            return;

        // Who this projectile damages and how much
        proj.ConfigureCombat(
            projectileDamage,
            PooledProjectile.TargetType.Player);

        // Movement: Enemy2 gets its own speed, direction still auto-aims at player
        SpitProjectile move = proj.GetComponent<SpitProjectile>();
        if (move != null)
        {
            move.SetSpeed(projectileSpeed);
            // Do NOT call SetDirection, so it auto-aims at player in OnEnable()
        }
    }

    public void PlayDeath()
    {
        if (isDead)
            return;

        isDead = true;

        if (animator != null)
        {
            animator.SetBool(RunHash, false);
            animator.SetBool(SpitHash, false);
            animator.SetBool(DeadHash, true);
        }

        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(deathReturnDelay);

        if (animator != null)
            animator.SetBool(DeadHash, false);

        isDead = false;

        // Enemy2 is pooled/disabled by other systems
        gameObject.SetActive(false);
    }
}
