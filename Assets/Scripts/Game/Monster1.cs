using System.Collections;
using UnityEngine;

public class Monster1 : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float stopDistance = 0.5f; // horizontal distance kept from player

    [Header("Attack")]
    public float attackInterval = 0.5f;   // match Attack animation length
    public float capsuleHeight = 1.5f;    // vertical offset for the attack sphere center
    public float capsuleRadius = 0.5f;    // radius of attack sphere
    public float damage = 10f;            // editable on Monster1 in Inspector
    public Vector3 attackOffset = Vector3.zero; // offset from player center

    [Header("Attack Timing")]
    public float firstAttackDelay = 0.4f;   // delay after reaching range before first hitbox
    public float attackWindup = 0.15f;      // delay between Attack anim start and hitbox

    [Header("References")]
    [Tooltip("Player ROOT (with Rigidbody)")]
    public Transform target;              // player root transform
    public Animator animator;             // enemy animator

    [Header("Death")]
    public float deathReturnDelay = 2f;   // time to wait before returning to pool

    [Header("Debug")]
    public bool debugAttackCircle = false;   // show attack circle/sphere

    private bool isAttacking;
    private bool isDead;
    private Rigidbody rb;

    private static readonly int SprintHash = Animator.StringToHash("Sprint");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int DeathHash = Animator.StringToHash("Death");

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.constraints = rb.constraints |
                             RigidbodyConstraints.FreezeRotationX |
                             RigidbodyConstraints.FreezeRotationZ;
        }
    }

    private void OnEnable()
    {
        isDead = false;
        isAttacking = false;
        StopAllCoroutines();

        if (animator != null)
        {
            animator.SetBool(SprintHash, false);
            animator.SetBool(AttackHash, false);
            animator.SetBool(DeathHash, false);
        }
    }

    private void Start()
    {
        // Fallback: auto-find player root via PlayerHP if not assigned
        if (target == null)
        {
            PlayerHP hp = FindObjectOfType<PlayerHP>();
            if (hp != null)
                target = hp.transform.root;
        }
    }

    private void Update()
    {
        if (isDead || target == null)
            return;

        // Horizontal direction + distance (ignore height)
        Vector3 toTarget = target.position - transform.position;
        Vector3 flatToTarget = new Vector3(toTarget.x, 0f, toTarget.z);
        float distance = flatToTarget.magnitude;

        // Chase player until within stopDistance (horizontal)
        if (distance > stopDistance)
        {
            MoveTowardsTarget(flatToTarget);
            animator.SetBool(SprintHash, true);

            if (isAttacking)
            {
                StopAllCoroutines();
                isAttacking = false;
                animator.SetBool(AttackHash, false);
            }
        }
        else
        {
            animator.SetBool(SprintHash, false);

            if (!isAttacking)
                StartCoroutine(AttackLoop());
        }
    }

    private void MoveTowardsTarget(Vector3 flatToTarget)
    {
        if (flatToTarget.sqrMagnitude < 0.0001f)
            return;

        Vector3 direction = flatToTarget.normalized;

        // rotate towards player (only yaw)
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(direction),
            10f * Time.deltaTime
        );

        // move forward on XZ
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    private IEnumerator AttackLoop()
    {
        if (isDead)
            yield break;

        isAttacking = true;

        // initial delay after entering attack range
        if (firstAttackDelay > 0f)
            yield return new WaitForSeconds(firstAttackDelay);

        while (!isDead)
        {
            if (target == null)
            {
                isAttacking = false;
                yield break;
            }

            // recheck horizontal distance while attacking
            Vector3 toTarget = target.position - transform.position;
            Vector3 flatToTarget = new Vector3(toTarget.x, 0f, toTarget.z);
            float distance = flatToTarget.magnitude;

            // if player moved out of range, stop attacking
            if (distance > stopDistance + 0.1f)
            {
                isAttacking = false;
                yield break;
            }

            animator.SetBool(AttackHash, true);

            // windup between animation start and hitbox spawn
            if (attackWindup > 0f)
                yield return new WaitForSeconds(attackWindup);

            DoAttackCast();

            // remainder of the attack interval
            float remaining = Mathf.Max(0f, attackInterval - attackWindup);
            if (remaining > 0f)
                yield return new WaitForSeconds(remaining);

            animator.SetBool(AttackHash, false);
        }

        isAttacking = false;
    }

    private void DoAttackCast()
    {
        if (target == null || isDead)
            return;

        // Single sphere centered around the player (from above it looks like a circle)
        Vector3 center = target.position + attackOffset + Vector3.up * (capsuleHeight * 0.5f);

        Collider[] hits = Physics.OverlapSphere(center, capsuleRadius);

        if (hits == null || hits.Length == 0)
            return;

        // First: check if parried
        foreach (var hit in hits)
        {
            if (hit != null && hit.CompareTag("Parry"))
            {
                // Parried – no damage
                return;
            }
        }

        // If not parried: look for player
        foreach (var hit in hits)
        {
            if (hit != null && hit.CompareTag("Player"))
            {
                PlayerHP hp = hit.GetComponent<PlayerHP>();
                if (hp == null)
                    hp = hit.GetComponentInParent<PlayerHP>();

                if (hp != null)
                    hp.TakeDamage(damage);

                break;
            }
        }
    }

    public void PlayDeath()
    {
        if (isDead)
            return;

        isDead = true;
        StopAllCoroutines();
        isAttacking = false;

        if (animator != null)
        {
            animator.SetBool(SprintHash, false);
            animator.SetBool(AttackHash, false);
            animator.SetBool(DeathHash, true);
        }

        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(deathReturnDelay);

        if (animator != null)
            animator.SetBool(DeathHash, false);

        isDead = false;

        // Return to pool / disable – replace with your pool logic if needed
        gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        if (!debugAttackCircle)
            return;

        Gizmos.color = Color.red;

        Transform debugTarget = target;

        if (debugTarget == null)
        {
            PlayerHP hp = FindObjectOfType<PlayerHP>();
            if (hp != null)
                debugTarget = hp.transform.root;
        }

        Vector3 center;

        if (debugTarget != null)
            center = debugTarget.position + attackOffset + Vector3.up * (capsuleHeight * 0.5f);
        else
            center = transform.position + attackOffset + Vector3.up * (capsuleHeight * 0.5f);

        Gizmos.DrawWireSphere(center, capsuleRadius);
    }
}
