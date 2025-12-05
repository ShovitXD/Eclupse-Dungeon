using System;
using UnityEngine;

public class EnemyHealthXP : MonoBehaviour
{
    // Global event death notification
    public static event Action<EnemyHealthXP> OnEnemyDied;

    [Header("Stats")]
    [SerializeField] private int maxHP = 10;
    [SerializeField] private int xpValue = 5;

    [Header("Runtime")]
    [SerializeField] private int currentHP;

    [Header("Cull Settings")]
    [SerializeField] private bool enableCullByDistance = true;
    [SerializeField] private float cullDistanceFromPlayer = 50f;
    [SerializeField] private Transform player;

    public int MaxHP => maxHP;
    public int CurrentHP => currentHP;
    public int XpValue => xpValue;

    // Setup
    private void Awake()
    {
        if (ValueHandler.Instance != null)
        {
            xpValue = ValueHandler.Instance.EnemyXPValue;
        }
    }

    private void OnEnable()
    {
        currentHP = maxHP;
    }

    private void Start()
    {
        TryFindPlayerIfNull();
    }

    // Per frame cull check
    private void Update()
    {
        if (!enableCullByDistance) return;

        if (player == null)
        {
            TryFindPlayerIfNull();
            if (player == null) return;
        }

        Vector3 diff = transform.position - player.position;
        float sqrDist = diff.sqrMagnitude;
        float cullDistSqr = cullDistanceFromPlayer * cullDistanceFromPlayer;

        if (sqrDist > cullDistSqr)
        {
            CullWithoutXP();
        }
    }

    private void TryFindPlayerIfNull()
    {
        if (player != null) return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    // Damage entry point
    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        if (currentHP <= 0) return;

        currentHP -= amount;
        Debug.Log($"[EnemyHealthXP] {gameObject.name} took {amount} damage. HP now: {currentHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void KillInstant()
    {
        if (currentHP <= 0) return;
        currentHP = 0;
        Die();
    }

    // Death logic that gives xp
    private void Die()
    {
        Debug.Log($"[EnemyHealthXP] {gameObject.name} died. Grant XP: {xpValue}");

        OnEnemyDied?.Invoke(this);

        gameObject.SetActive(false);
    }

    // Cull logic without xp 
    private void CullWithoutXP()
    {
        currentHP = maxHP;
        gameObject.SetActive(false);
    }
}
