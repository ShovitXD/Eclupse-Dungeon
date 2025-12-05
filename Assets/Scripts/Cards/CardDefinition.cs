using UnityEngine;
using static GameManager;

[CreateAssetMenu(menuName = "Cards/Card Definition", fileName = "Card_")]
public class CardDefinition : ScriptableObject
{
    [Header("Identity")]
    public string id;

    [Header("Presentation")]
    public string title;

    [TextArea]
    public string description;

    [Header("Rarity")]
    public CardRarity rarity = CardRarity.Common;

    [Header("Availability")]
    public WeaponType[] allowedWeapons;

    [Header("Effect")]
    public CardEffectType effectType;
    public float value = 0f;
}

public enum CardRarity
{
    Common,
    Uncommon,
    Rare
}

public enum CardEffectType
{
    EnemyXPValueAdd,
    BaseXPToLevelUpAdd,
    XPIncreasePerLocalLevelAdd,
    XPIncreasePerPlayerLevelAdd,

    ParryActiveDurationAdd,
    ParryRechargeDurationAdd,

    BowAttackIntervalAdd,
    BowDamageAdd,

    EnemyProjectileSpeedAdd,

    SwordSwingIntervalAdd,
    SwordDamageAdd,
    SwordAttackRangeAdd,

    SpearSwingIntervalAdd,
    SpearDamageAdd,
    SpearAttackRangeAdd,

    AmuletPulseIntervalAdd,
    AmuletDamageAdd,
    AmuletRadiusAdd,

    BossProjectileBaseDamageAdd,
    BossProjectileDamageMultiplierAdd,
    BossProjectileBaseSpeedAdd,
    BossProjectileSpeedMultiplierAdd
}
