using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [Header("Card Database")]
    public CardDefinition[] allCards;

    [Header("UI")]
    public GameObject panelRoot;
    public Button[] optionButtons;
    public TMP_Text[] optionTitleTexts;
    public TMP_Text[] optionDescriptionTexts;

    [Header("Rarity Colors")]
    public Color commonColor = Color.grey;
    public Color uncommonColor = Color.blue;
    public Color rareColor = new Color(1f, 0.84f, 0f);

    [Header("Player Control")]
    public MonoBehaviour[] extraScriptsToDisable;

    private PlayerMovement playerMovement;
    private readonly CardDefinition[] currentOptions = new CardDefinition[3];
    private bool isPanelOpen;

    private CursorLockMode previousLockState;
    private bool previousCursorVisible;

    // Singleton setup
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (panelRoot != null)
            panelRoot.SetActive(false);

        SetupButtonListeners();
    }

    private void OnEnable()
    {
        XPHandler.OnLocalLevelChanged += HandleLocalLevelChanged;
    }

    private void OnDisable()
    {
        XPHandler.OnLocalLevelChanged -= HandleLocalLevelChanged;
    }

    private void SetupButtonListeners()
    {
        if (optionButtons == null)
            return;

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i;
            if (optionButtons[i] != null)
            {
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => OnCardButtonPressed(index));
            }
        }
    }

    private void HandleLocalLevelChanged(int newLocalLevel)
    {
        if (isPanelOpen)
            return;

        OpenCardPanel();
    }

    // Card panel open flow
    private void OpenCardPanel()
    {
        List<CardDefinition> options = GenerateThreeCardOptions();
        if (options.Count == 0)
            return;

        for (int i = 0; i < currentOptions.Length; i++)
        {
            currentOptions[i] = i < options.Count ? options[i] : null;

            bool hasCard = currentOptions[i] != null;

            if (optionButtons != null && i < optionButtons.Length && optionButtons[i] != null)
                optionButtons[i].gameObject.SetActive(hasCard);

            if (hasCard)
            {
                CardDefinition card = currentOptions[i];

                if (optionTitleTexts != null && i < optionTitleTexts.Length && optionTitleTexts[i] != null)
                    optionTitleTexts[i].text = card.title;

                if (optionDescriptionTexts != null && i < optionDescriptionTexts.Length && optionDescriptionTexts[i] != null)
                    optionDescriptionTexts[i].text = card.description;

                ApplyButtonRarityColor(i, card.rarity);
            }
        }

        isPanelOpen = true;

        previousLockState = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
        SetGameplayScriptsEnabled(false);

        if (panelRoot != null)
            panelRoot.SetActive(true);
    }

    // Card panel close flow
    private void CloseCardPanel()
    {
        isPanelOpen = false;

        if (panelRoot != null)
            panelRoot.SetActive(false);

        Cursor.lockState = previousLockState;
        Cursor.visible = previousCursorVisible;

        Time.timeScale = 1f;
        SetGameplayScriptsEnabled(true);

        for (int i = 0; i < currentOptions.Length; i++)
            currentOptions[i] = null;
    }

    private void FindPlayerMovementIfNeeded()
    {
        if (playerMovement != null)
            return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
            return;

        playerMovement = playerObj.GetComponent<PlayerMovement>();
    }

    private void SetGameplayScriptsEnabled(bool enabled)
    {
        FindPlayerMovementIfNeeded();
        if (playerMovement != null)
            playerMovement.enabled = enabled;

        if (extraScriptsToDisable == null)
            return;

        foreach (var mb in extraScriptsToDisable)
        {
            if (mb != null)
                mb.enabled = enabled;
        }
    }

    private void OnCardButtonPressed(int index)
    {
        if (index < 0 || index >= currentOptions.Length)
            return;

        CardDefinition card = currentOptions[index];
        if (card == null)
            return;

        ApplyCardEffect(card);
        CloseCardPanel();
    }

    private void ApplyButtonRarityColor(int index, CardRarity rarity)
    {
        if (optionButtons == null || index >= optionButtons.Length || optionButtons[index] == null)
            return;

        Button btn = optionButtons[index];
        Color targetColor = commonColor;

        switch (rarity)
        {
            case CardRarity.Common:
                targetColor = commonColor;
                break;
            case CardRarity.Uncommon:
                targetColor = uncommonColor;
                break;
            case CardRarity.Rare:
                targetColor = rareColor;
                break;
        }

        ColorBlock colors = btn.colors;
        colors.normalColor = targetColor;
        colors.highlightedColor = targetColor;
        btn.colors = colors;
    }

    // Card options generation
    private List<CardDefinition> GenerateThreeCardOptions()
    {
        List<CardDefinition> result = new List<CardDefinition>();

        if (allCards == null || allCards.Length == 0)
            return result;

        List<CardDefinition> candidates = FilterCardsByCurrentWeapon(allCards).ToList();
        if (candidates.Count == 0)
            return result;

        List<CardDefinition> used = new List<CardDefinition>();

        for (int slot = 0; slot < 3; slot++)
        {
            CardDefinition chosen = RollSingleCardWeightedByRarity(candidates, used);
            if (chosen == null)
                break;

            result.Add(chosen);
            used.Add(chosen);
        }

        return result;
    }

    private IEnumerable<CardDefinition> FilterCardsByCurrentWeapon(IEnumerable<CardDefinition> cards)
    {
        GameManager.WeaponType currentWeapon = GameManager.WeaponType.None;
        if (GameManager.Instance != null)
        {
            currentWeapon = GameManager.Instance.SelectedWeapon;
        }

        foreach (var card in cards)
        {
            if (card == null)
                continue;

            if (card.allowedWeapons == null || card.allowedWeapons.Length == 0)
            {
                yield return card;
            }
            else
            {
                if (card.allowedWeapons.Contains(currentWeapon))
                    yield return card;
            }
        }
    }

    private CardDefinition RollSingleCardWeightedByRarity(List<CardDefinition> candidates, List<CardDefinition> used)
    {
        List<CardDefinition> available = candidates.Where(c => c != null && !used.Contains(c)).ToList();
        if (available.Count == 0)
            return null;

        float totalWeight = 0f;
        float[] weights = new float[available.Count];

        for (int i = 0; i < available.Count; i++)
        {
            CardDefinition card = available[i];
            float w = 0.6f;

            switch (card.rarity)
            {
                case CardRarity.Common:
                    w = 0.6f;
                    break;
                case CardRarity.Uncommon:
                    w = 0.3f;
                    break;
                case CardRarity.Rare:
                    w = 0.1f;
                    break;
            }

            weights[i] = w;
            totalWeight += w;
        }

        if (totalWeight <= 0f)
        {
            int idxFallback = Random.Range(0, available.Count);
            return available[idxFallback];
        }

        float r = Random.value * totalWeight;
        float cumulative = 0f;

        for (int i = 0; i < available.Count; i++)
        {
            cumulative += weights[i];
            if (r <= cumulative)
                return available[i];
        }

        return available[available.Count - 1];
    }

    // Card effect application
    private void ApplyCardEffect(CardDefinition card)
    {
        if (card == null)
            return;

        ValueHandler vh = ValueHandler.Instance;
        if (vh == null)
        {
            Debug.LogWarning($"CardManager: ValueHandler.Instance is null. Card '{card.title}' effect cannot be applied.");
            return;
        }

        float v = card.value; // direct delta

        switch (card.effectType)
        {
            case CardEffectType.EnemyXPValueAdd:
                vh.EnemyXPValue += Mathf.RoundToInt(v);
                break;
            case CardEffectType.BaseXPToLevelUpAdd:
                vh.BaseXPToLevelUp += Mathf.RoundToInt(v);
                break;
            case CardEffectType.XPIncreasePerLocalLevelAdd:
                vh.XPIncreasePerLocalLevel += Mathf.RoundToInt(v);
                break;
            case CardEffectType.XPIncreasePerPlayerLevelAdd:
                vh.XPIncreasePerPlayerLevel += Mathf.RoundToInt(v);
                break;

            case CardEffectType.ParryActiveDurationAdd:
                vh.ParryActiveDuration += v;
                break;
            case CardEffectType.ParryRechargeDurationAdd:
                vh.ParryRechargeDuration += v;
                break;

            case CardEffectType.BowAttackIntervalAdd:
                vh.BowAttackInterval += v;
                break;
            case CardEffectType.BowDamageAdd:
                vh.BowDamage += v;
                break;

            case CardEffectType.EnemyProjectileSpeedAdd:
                vh.SpitProjectileSpeed += v;
                break;

            case CardEffectType.SwordSwingIntervalAdd:
                vh.SwordSwingInterval += v;
                break;
            case CardEffectType.SwordDamageAdd:
                vh.SwordDamage += Mathf.RoundToInt(v);
                break;
            case CardEffectType.SwordAttackRangeAdd:
                vh.SwordAttackRange += v;
                break;

            case CardEffectType.SpearSwingIntervalAdd:
                vh.SpearSwingInterval += v;
                break;
            case CardEffectType.SpearDamageAdd:
                vh.SpearDamage += Mathf.RoundToInt(v);
                break;
            case CardEffectType.SpearAttackRangeAdd:
                vh.SpearAttackRange += v;
                break;

            case CardEffectType.AmuletPulseIntervalAdd:
                vh.AmuletPulseInterval += v;
                break;
            case CardEffectType.AmuletDamageAdd:
                vh.AmuletDamage += Mathf.RoundToInt(v);
                break;
            case CardEffectType.AmuletRadiusAdd:
                vh.AmuletRadius += v;
                break;

            case CardEffectType.BossProjectileBaseDamageAdd:
                vh.BossProjectileBaseDamage += v;
                break;
            case CardEffectType.BossProjectileDamageMultiplierAdd:
                vh.BossProjectileDamageMultiplier += v;
                break;
            case CardEffectType.BossProjectileBaseSpeedAdd:
                vh.BossProjectileBaseSpeed += v;
                break;
            case CardEffectType.BossProjectileSpeedMultiplierAdd:
                vh.BossProjectileSpeedMultiplier += v;
                break;

            default:
                Debug.LogWarning($"CardManager: Unhandled CardEffectType '{card.effectType}' for card '{card.title}'.");
                break;
        }
    }
}
