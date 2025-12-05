using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BossBulletHell : MonoBehaviour
{
    private enum PatternType
    {
        AimedSpread,
        RotatingRing
    }

    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float minRange = 5f;
    public float maxRange = 12f;

    [Header("Projectiles / Pooling")]
    [Tooltip("ID of the ProjectilePool to use, e.g. 'EnemyProjectile'.")]
    public string projectilePoolId = "EnemyProjectile";
    [Tooltip("Mouths where bullets spawn from.")]
    public Transform[] mouths;

    [Header("Patterns - General")]
    [Tooltip("Seconds between shots within a given pattern.")]
    public float fireInterval = 0.5f;
    [Tooltip("Seconds before switching to another pattern.")]
    public float patternSwitchInterval = 6f;

    [Header("Pattern: Aimed Spread")]
    [Tooltip("Projectiles per mouth per shot in aimed spread.")]
    public int aimedProjectilesPerMouth = 5;
    [Tooltip("Total cone spread (degrees) for aimed spread.")]
    public float aimedSpreadAngle = 40f;

    [Header("Pattern: Rotating Ring")]
    [Tooltip("Total bullets in the ring (shared across all mouths).")]
    public int ringBulletCount = 24;
    [Tooltip("How fast the ring rotates (degrees per shot).")]
    public float ringRotateStep = 15f;

    [Header("Death")]
    public float deathReturnDelay = 3f;

    [Header("References")]
    public Animator animator;
    public Transform debugTarget;

    private static readonly int RunHash = Animator.StringToHash("Run");
    private static readonly int SpitHash = Animator.StringToHash("Spit");
    private static readonly int DeadHash = Animator.StringToHash("Dead");

    private Transform target;
    private bool isDead;

    private float nextFireTime;
    private float nextPatternSwitchTime;
    private float ringAngleOffset;
    private PatternType currentPattern;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        isDead = false;

        if (animator != null)
        {
            animator.SetBool(RunHash, false);
            animator.SetBool(SpitHash, false);
            animator.SetBool(DeadHash, false);
        }

        AcquireTarget();

        currentPattern = PatternType.AimedSpread;
        nextFireTime = Time.time + fireInterval;
        nextPatternSwitchTime = Time.time + patternSwitchInterval;
        ringAngleOffset = 0f;
    }

    private void Update()
    {
        if (isDead)
            return;

        if (target == null)
            AcquireTarget();

        if (target == null)
            return;

        HandleMovementAndAttacks();
    }

    private void HandleMovementAndAttacks()
    {
        Vector3 toTarget = target.position - transform.position;
        Vector3 flatToTarget = new Vector3(toTarget.x, 0f, toTarget.z);
        float distance = flatToTarget.magnitude;

        if (distance < minRange)
        {
            MoveInDirection(-flatToTarget);

            if (animator != null)
            {
                animator.SetBool(RunHash, true);
                animator.SetBool(SpitHash, false);
            }
        }
        else if (distance > maxRange)
        {
            MoveInDirection(flatToTarget);

            if (animator != null)
            {
                animator.SetBool(RunHash, true);
                animator.SetBool(SpitHash, false);
            }
        }
        else
        {
            if (animator != null)
            {
                animator.SetBool(RunHash, false);
                animator.SetBool(SpitHash, true);
            }

            FaceTarget();

            if (Time.time >= nextFireTime)
            {
                FireCurrentPattern();
                nextFireTime = Time.time + fireInterval;
            }

            if (Time.time >= nextPatternSwitchTime)
            {
                SwitchPattern();
                nextPatternSwitchTime = Time.time + patternSwitchInterval;
            }
        }
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

    private void SwitchPattern()
    {
        currentPattern = (currentPattern == PatternType.AimedSpread)
            ? PatternType.RotatingRing
            : PatternType.AimedSpread;

        ringAngleOffset = 0f;
    }

    private void FireCurrentPattern()
    {
        if (mouths == null || mouths.Length == 0)
            return;

        switch (currentPattern)
        {
            case PatternType.AimedSpread:
                FireAimedSpread();
                break;
            case PatternType.RotatingRing:
                FireRotatingRing();
                break;
        }
    }

    private void FireAimedSpread()
    {
        if (target == null)
            return;

        if (aimedProjectilesPerMouth <= 0)
            aimedProjectilesPerMouth = 1;

        foreach (Transform mouth in mouths)
        {
            if (mouth == null)
                continue;

            Vector3 toTarget = target.position - mouth.position;
            Vector3 flatToTarget = new Vector3(toTarget.x, 0f, toTarget.z);
            if (flatToTarget.sqrMagnitude < 0.0001f)
                continue;

            Vector3 baseDir = flatToTarget.normalized;

            float step = (aimedProjectilesPerMouth > 1)
                ? aimedSpreadAngle / (aimedProjectilesPerMouth - 1)
                : 0f;

            float startAngle = -aimedSpreadAngle * 0.5f;

            for (int i = 0; i < aimedProjectilesPerMouth; i++)
            {
                float angle = startAngle + step * i;
                Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
                Vector3 dir = rot * baseDir;

                SpawnProjectile(mouth.position, dir);
            }
        }
    }

    private void FireRotatingRing()
    {
        if (ringBulletCount <= 0)
            ringBulletCount = 8;

        float angleStep = 360f / ringBulletCount;

        for (int i = 0; i < ringBulletCount; i++)
        {
            float angle = ringAngleOffset + i * angleStep;
            float rad = angle * Mathf.Deg2Rad;

            Vector3 dir = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad));

            foreach (Transform mouth in mouths)
            {
                if (mouth == null)
                    continue;

                SpawnProjectile(mouth.position, dir);
            }
        }

        ringAngleOffset += ringRotateStep;
    }

    private void SpawnProjectile(Vector3 spawnPos, Vector3 direction)
    {
        ProjectilePool pool = ProjectilePool.GetPool(projectilePoolId);
        if (pool == null)
        {
            Debug.LogWarning($"BossBulletHell: No ProjectilePool found with id '{projectilePoolId}'.");
            return;
        }

        Quaternion spawnRot = Quaternion.LookRotation(
            direction == Vector3.zero ? transform.forward : direction
        );

        PooledProjectile proj = pool.Get(spawnPos, spawnRot);
        if (proj == null)
            return;

        float damage = 10f;
        float speed = 10f;

        if (ValueHandler.Instance != null)
        {
            damage = ValueHandler.Instance.BossProjectileDamage;
            speed = ValueHandler.Instance.BossProjectileSpeed;
        }

        proj.ConfigureCombat(
            damage,
            PooledProjectile.TargetType.Player
        );

        SpitProjectile move = proj.GetComponent<SpitProjectile>();
        if (move != null)
        {
            move.SetSpeed(speed);
            move.SetDirection(direction);
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

        gameObject.SetActive(false);
    }
}
