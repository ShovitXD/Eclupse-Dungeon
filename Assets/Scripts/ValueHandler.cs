using UnityEngine;

public class ValueHandler : MonoBehaviour
{
    public static ValueHandler Instance { get; private set; }

    [Header("Enemy XP")]
    [SerializeField] private int enemyXPValue = 5;

    [Header("XP Curve")]
    [SerializeField] private int baseXPToLevelUp = 10;
    [SerializeField] private int xpIncreasePerLocalLevel = 1;
    [SerializeField] private int xpIncreasePerPlayerLevel = 2;

    [Header("Parry")]
    [SerializeField] private float parryActiveDuration = 1f;
    [SerializeField] private float parryRechargeDuration = 2f;

    [Header("Bow")]
    [SerializeField] private float bowAttackInterval = 0.8f;

    [Header("Bow Combat")]
    [SerializeField] private float bowDamage = 5f;

    [Header("Enemy Projectile")]
    [SerializeField] private float spitProjectileSpeed = 10f;

    [Header("Sword")]
    [SerializeField] private float swordSwingInterval = 1f;
    [SerializeField] private int swordDamage = 10;
    [SerializeField] private float swordAttackRange = 2f;

    [Header("Spear")]
    [SerializeField] private float spearSwingInterval = 1.2f;
    [SerializeField] private int spearDamage = 12;
    [SerializeField] private float spearAttackRange = 3f;

    [Header("Amulet")]
    [SerializeField] private float amuletPulseInterval = 2f;
    [SerializeField] private int amuletDamage = 8;
    [SerializeField] private float amuletRadius = 4f;

    [Header("Boss Projectile")]
    [SerializeField] private float bossProjectileBaseDamage = 10f;
    [SerializeField] private float bossProjectileDamageMultiplier = 1f;
    [SerializeField] private float bossProjectileBaseSpeed = 10f;
    [SerializeField] private float bossProjectileSpeedMultiplier = 1f;

    // Properties

    public int EnemyXPValue
    {
        get => enemyXPValue;
        set => enemyXPValue = Mathf.Max(0, value);
    }

    public int BaseXPToLevelUp
    {
        get => baseXPToLevelUp;
        set => baseXPToLevelUp = Mathf.Max(1, value);
    }

    public int XPIncreasePerLocalLevel
    {
        get => xpIncreasePerLocalLevel;
        set => xpIncreasePerLocalLevel = Mathf.Max(0, value);
    }

    public int XPIncreasePerPlayerLevel
    {
        get => xpIncreasePerPlayerLevel;
        set => xpIncreasePerPlayerLevel = Mathf.Max(0, value);
    }

    public float ParryActiveDuration
    {
        get => parryActiveDuration;
        set => parryActiveDuration = Mathf.Max(0f, value);
    }

    public float ParryRechargeDuration
    {
        get => parryRechargeDuration;
        set => parryRechargeDuration = Mathf.Max(0f, value);
    }

    public float BowAttackInterval
    {
        get => bowAttackInterval;
        set => bowAttackInterval = Mathf.Max(0.01f, value);
    }

    public float BowDamage
    {
        get => bowDamage;
        set => bowDamage = Mathf.Max(0f, value);
    }

    public float SpitProjectileSpeed
    {
        get => spitProjectileSpeed;
        set => spitProjectileSpeed = Mathf.Max(0f, value);
    }

    public float SwordSwingInterval
    {
        get => swordSwingInterval;
        set => swordSwingInterval = Mathf.Max(0.01f, value);
    }

    public int SwordDamage
    {
        get => swordDamage;
        set => swordDamage = Mathf.Max(0, value);
    }

    public float SwordAttackRange
    {
        get => swordAttackRange;
        set => swordAttackRange = Mathf.Max(0f, value);
    }

    public float SpearSwingInterval
    {
        get => spearSwingInterval;
        set => spearSwingInterval = Mathf.Max(0.01f, value);
    }

    public int SpearDamage
    {
        get => spearDamage;
        set => spearDamage = Mathf.Max(0, value);
    }

    public float SpearAttackRange
    {
        get => spearAttackRange;
        set => spearAttackRange = Mathf.Max(0f, value);
    }

    public float AmuletPulseInterval
    {
        get => amuletPulseInterval;
        set => amuletPulseInterval = Mathf.Max(0.01f, value);
    }

    public int AmuletDamage
    {
        get => amuletDamage;
        set => amuletDamage = Mathf.Max(0, value);
    }

    public float AmuletRadius
    {
        get => amuletRadius;
        set => amuletRadius = Mathf.Max(0f, value);
    }

    public float BossProjectileBaseDamage
    {
        get => bossProjectileBaseDamage;
        set => bossProjectileBaseDamage = Mathf.Max(0f, value);
    }

    public float BossProjectileBaseSpeed
    {
        get => bossProjectileBaseSpeed;
        set => bossProjectileBaseSpeed = Mathf.Max(0f, value);
    }

    public float BossProjectileDamageMultiplier
    {
        get => bossProjectileDamageMultiplier;
        set => bossProjectileDamageMultiplier = Mathf.Max(0f, value);
    }

    public float BossProjectileSpeedMultiplier
    {
        get => bossProjectileSpeedMultiplier;
        set => bossProjectileSpeedMultiplier = Mathf.Max(0f, value);
    }

    public float BossProjectileDamage => bossProjectileBaseDamage * bossProjectileDamageMultiplier;
    public float BossProjectileSpeed => bossProjectileBaseSpeed * bossProjectileSpeedMultiplier;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
