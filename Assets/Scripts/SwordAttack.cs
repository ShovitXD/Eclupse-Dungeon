using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    public enum WeaponKind
    {
        Sword,
        Spear
    }

    [Header("Weapon Type")]
    public WeaponKind weaponKind = WeaponKind.Sword;

    [Header("Raycast")]
    public Transform raycastOrigin;

    [Header("vector For animation")]
    public Vector3 localStartPosition;
    public Vector3 localStartEuler;

    public Vector3 localEndPosition;
    public Vector3 localEndEuler;

    [SerializeField] private float swingDuration = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool drawDebugRay = true;
    [SerializeField] private bool drawGizmoRay = true;
    [SerializeField] private Color gizmoColor = Color.cyan;

    // Runtime stats
    private float swingInterval = 1f;
    private int weaponDamage = 10;
    private float weaponRange = 2f;

    private readonly HashSet<EnemyHealthXP> hitThisSwing = new HashSet<EnemyHealthXP>();
    private Coroutine swingLoop;

    private void Awake()
    {
        if (ValueHandler.Instance != null)
        {
            switch (weaponKind)
            {
                case WeaponKind.Sword:
                    swingInterval = Mathf.Max(0f, ValueHandler.Instance.SwordSwingInterval);
                    weaponDamage = ValueHandler.Instance.SwordDamage;
                    weaponRange = Mathf.Max(0f, ValueHandler.Instance.SwordAttackRange);
                    break;

                case WeaponKind.Spear:
                    swingInterval = Mathf.Max(0f, ValueHandler.Instance.SpearSwingInterval);
                    weaponDamage = ValueHandler.Instance.SpearDamage;
                    weaponRange = Mathf.Max(0f, ValueHandler.Instance.SpearAttackRange);
                    break;
            }
        }

        transform.localPosition = localStartPosition;
        transform.localRotation = Quaternion.Euler(localStartEuler);
    }

    private void OnEnable()
    {
        swingLoop = StartCoroutine(SwingLoop());
    }

    private void OnDisable()
    {
        if (swingLoop != null)
            StopCoroutine(swingLoop);

        transform.localPosition = localStartPosition;
        transform.localRotation = Quaternion.Euler(localStartEuler);
    }

    private IEnumerator SwingLoop()
    {
        while (true)
        {
            yield return StartCoroutine(SwingOnce());

            if (swingInterval > 0f)
                yield return new WaitForSeconds(swingInterval);
            else
                yield return null;
        }
    }

    private IEnumerator SwingOnce()
    {
        if (raycastOrigin == null)
            raycastOrigin = transform;

        hitThisSwing.Clear();

        float t = 0f;
        Quaternion startRot = Quaternion.Euler(localStartEuler);
        Quaternion endRot = Quaternion.Euler(localEndEuler);

        while (t < swingDuration)
        {
            float normalized = swingDuration > 0f ? t / swingDuration : 1f;

            transform.localPosition = Vector3.Lerp(localStartPosition, localEndPosition, normalized);
            transform.localRotation = Quaternion.Slerp(startRot, endRot, normalized);

            DoHitCheck();

            t += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = localEndPosition;
        transform.localRotation = endRot;

        transform.localPosition = localStartPosition;
        transform.localRotation = startRot;
    }

    private void DoHitCheck()
    {
        if (raycastOrigin == null) return;
        if (weaponRange <= 0f) return;
        if (weaponDamage <= 0) return;

        Ray ray = new Ray(raycastOrigin.position, raycastOrigin.forward);

        float debugDuration = 0.05f;

        if (Physics.Raycast(ray, out RaycastHit hit, weaponRange))
        {
            if (drawDebugRay)
                Debug.DrawRay(raycastOrigin.position, raycastOrigin.forward * hit.distance, Color.red, debugDuration);

            if (hit.collider.CompareTag("Enemy"))
            {
                EnemyHealthXP enemy = hit.collider.GetComponent<EnemyHealthXP>();
                if (enemy != null && !hitThisSwing.Contains(enemy))
                {
                    enemy.TakeDamage(weaponDamage);
                    hitThisSwing.Add(enemy);
                }
            }
        }
        else if (drawDebugRay)
        {
            Debug.DrawRay(raycastOrigin.position, raycastOrigin.forward * weaponRange, Color.yellow, debugDuration);
        }
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmoRay) return;

        Transform originT = raycastOrigin != null ? raycastOrigin : transform;
        if (originT == null) return;

        float range = weaponRange;

        if (range <= 0f && ValueHandler.Instance != null)
        {
            switch (weaponKind)
            {
                case WeaponKind.Sword:
                    range = ValueHandler.Instance.SwordAttackRange;
                    break;
                case WeaponKind.Spear:
                    range = ValueHandler.Instance.SpearAttackRange;
                    break;
            }
        }

        if (range <= 0f) range = 2f;

        Gizmos.color = gizmoColor;
        Gizmos.DrawRay(originT.position, originT.forward * range);
    }
}
