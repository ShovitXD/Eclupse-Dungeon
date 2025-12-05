using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmuletAttack : MonoBehaviour
{
    [Header("Center")]
    public Transform centerTransform;

    [Header("Pulse Settings")]
    [SerializeField] private float pulseActiveDuration = 0.1f;

    [Header("Debug")]
    [SerializeField] private bool drawGizmoRadius = true;
    [SerializeField] private Color gizmoColor = Color.magenta;

    private float pulseInterval = 2f;
    private int pulseDamage = 8;
    private float pulseRadius = 4f;

    private Coroutine pulseLoop;
    private readonly HashSet<EnemyHealthXP> hitThisPulse = new HashSet<EnemyHealthXP>();

    private void Awake()
    {
        if (ValueHandler.Instance != null)
        {
            pulseInterval = Mathf.Max(0f, ValueHandler.Instance.AmuletPulseInterval);
            pulseDamage = ValueHandler.Instance.AmuletDamage;
            pulseRadius = Mathf.Max(0f, ValueHandler.Instance.AmuletRadius);
        }

        if (centerTransform == null)
        {
            centerTransform = transform;
        }
    }

    private void OnEnable()
    {
        pulseLoop = StartCoroutine(PulseLoop());
    }

    private void OnDisable()
    {
        if (pulseLoop != null)
            StopCoroutine(pulseLoop);
    }

    private IEnumerator PulseLoop()
    {
        while (true)
        {
            yield return StartCoroutine(DoPulse());

            if (pulseInterval > 0f)
                yield return new WaitForSeconds(pulseInterval);
            else
                yield return null;
        }
    }

    private IEnumerator DoPulse()
    {
        if (centerTransform == null)
            yield break;

        hitThisPulse.Clear();

        float t = 0f;

        // Puls stay active for a period but only hits an enemy once.
        while (t < pulseActiveDuration)
        {
            DoHitCheck();
            t += Time.deltaTime;
            yield return null;
        }
    }

    private void DoHitCheck()
    {
        if (centerTransform == null) return;
        if (pulseRadius <= 0f) return;
        if (pulseDamage <= 0) return;

        Vector3 center = centerTransform.position;

        // Area around the player is checked with OverlapSphere.
        Collider[] hits = Physics.OverlapSphere(center, pulseRadius);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider col = hits[i];
            if (!col.CompareTag("Enemy")) continue;

            EnemyHealthXP enemy = col.GetComponent<EnemyHealthXP>();
            if (enemy == null) continue;

            // Damage 1 guy only once
            if (hitThisPulse.Contains(enemy)) continue;

            enemy.TakeDamage(pulseDamage);
            hitThisPulse.Add(enemy);
        }
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmoRadius) return;

        Transform c = centerTransform != null ? centerTransform : transform;
        if (c == null) return;

        float radius = pulseRadius;

        if (radius <= 0f && ValueHandler.Instance != null)
        {
            radius = ValueHandler.Instance.AmuletRadius;
        }

        if (radius <= 0f) radius = 1f;

        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(c.position, radius);
    }
}
