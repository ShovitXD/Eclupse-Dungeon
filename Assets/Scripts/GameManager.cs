using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public const int MaxLevel = 12;
    public const int MaxHpLevel = 5;
    public const int MaxSpeedLevel = 5;
    public const int MaxParryLevel = 2;

    public enum WeaponType
    {
        None = 0,
        Sword = 1,
        Spear = 2,
        Bow = 3,
        Amulet = 4
    }

    [Header("Save and Load")]
    [SerializeField] private bool useSavedProgress = true;

    [Header("Current Levels")]
    [SerializeField] private int currentLevel = 0;
    [SerializeField] private int hpLevel = 0;
    [SerializeField] private int speedLevel = 0;
    [SerializeField] private int parryLevel = 0;

    [Header("Local Level Progression")]
    [SerializeField] private int baseLocalLevelsForFirstLevel = 20;
    [SerializeField] private int extraLocalLevelsPerLevel = 20;
    [SerializeField] private int localLevelProgress = 0;

    [Header("Base Stats")]
    [SerializeField] private int baseHP = 10;
    [SerializeField] private int hpPerLevel = 10;

    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float moveSpeedPerLevel = 0.5f;

    [SerializeField] private int baseParry = 1;
    [SerializeField] private int parryPerLevel = 1;

    [Header("Weapon)")]
    [SerializeField] private WeaponType selectedWeapon = WeaponType.None;

    [Header("Debug")]
    [SerializeField] private int debugTotalLevel = 0;
    [SerializeField] private int debugFreeLevelPoints = 0;

    public System.Action OnStatsChanged;

    public int CurrentLevel => currentLevel;
    public int HpUpgradeLevel => hpLevel;
    public int SpeedUpgradeLevel => speedLevel;
    public int ParryUpgradeLevel => parryLevel;

    public int FreeLevelPoints =>
        Mathf.Clamp(currentLevel - (hpLevel + speedLevel + parryLevel), 0, MaxLevel);

    public int CurrentMaxHP => baseHP + hpLevel * hpPerLevel;
    public float CurrentMoveSpeed => baseMoveSpeed + speedLevel * moveSpeedPerLevel;
    public int CurrentParry => baseParry + parryLevel * parryPerLevel;

    public WeaponType SelectedWeapon => selectedWeapon;

    // Lifecycle
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (useSavedProgress)
        {
            LoadData();
        }
        else
        {
            ClampLevels();
        }

        UpdateDebugFields();
        NotifyStatsChanged();
    }

    // Weapon
    public void SetWeapon(WeaponType weapon)
    {
        selectedWeapon = weapon;
        UpdateDebugFields();
        NotifyStatsChanged();
    }

    // Upgrade API
    public bool CanUpgradeHP()
    {
        return FreeLevelPoints > 0 && hpLevel < MaxHpLevel;
    }

    public bool CanUpgradeSpeed()
    {
        return FreeLevelPoints > 0 && speedLevel < MaxSpeedLevel;
    }

    public bool CanUpgradeParry()
    {
        return FreeLevelPoints > 0 && parryLevel < MaxParryLevel;
    }

    public bool UpgradeHP()
    {
        if (!CanUpgradeHP()) return false;

        hpLevel++;
        AutoSaveAndBroadcast();
        return true;
    }

    public bool UpgradeSpeed()
    {
        if (!CanUpgradeSpeed()) return false;

        speedLevel++;
        AutoSaveAndBroadcast();
        return true;
    }

    public bool UpgradeParry()
    {
        if (!CanUpgradeParry()) return false;

        parryLevel++;
        AutoSaveAndBroadcast();
        return true;
    }

    // Reset all levels
    public void ResetAllLevels()
    {
        currentLevel = 0;
        hpLevel = 0;
        speedLevel = 0;
        parryLevel = 0;
        localLevelProgress = 0;

        AutoSaveAndBroadcast();
    }

    // Local player level 
    public void OnLocalLevelGained()
    {
        if (currentLevel >= MaxLevel)
            return;

        localLevelProgress++;

        // Requirement grows linearly
        int requiredForNextLevel =
            baseLocalLevelsForFirstLevel
            + currentLevel * extraLocalLevelsPerLevel;

        while (currentLevel < MaxLevel && localLevelProgress >= requiredForNextLevel)
        {
            localLevelProgress -= requiredForNextLevel;

            currentLevel++;

            requiredForNextLevel =
                baseLocalLevelsForFirstLevel
                + currentLevel * extraLocalLevelsPerLevel;
        }

        AutoSaveAndBroadcast();
    }

    // Save/load account level
    private void AutoSaveAndBroadcast()
    {
        if (useSavedProgress)
        {
            SaveData();
        }

        ClampLevels();
        UpdateDebugFields();
        NotifyStatsChanged();
    }

    private void UpdateDebugFields()
    {
        debugTotalLevel = currentLevel;
        debugFreeLevelPoints = FreeLevelPoints;
    }

    private void NotifyStatsChanged()
    {
        OnStatsChanged?.Invoke();
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt("GM_Level", currentLevel);
        PlayerPrefs.SetInt("GM_HPLevel", hpLevel);
        PlayerPrefs.SetInt("GM_SpeedLevel", speedLevel);
        PlayerPrefs.SetInt("GM_ParryLevel", parryLevel);

        PlayerPrefs.SetInt("GM_LocalLevelProgress", localLevelProgress);

        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        if (PlayerPrefs.HasKey("GM_Level"))
        {
            currentLevel = PlayerPrefs.GetInt("GM_Level", 0);
            hpLevel = PlayerPrefs.GetInt("GM_HPLevel", 0);
            speedLevel = PlayerPrefs.GetInt("GM_SpeedLevel", 0);
            parryLevel = PlayerPrefs.GetInt("GM_ParryLevel", 0);

            localLevelProgress = PlayerPrefs.GetInt("GM_LocalLevelProgress", 0);
        }
        else
        {
            currentLevel = 0;
            hpLevel = 0;
            speedLevel = 0;
            parryLevel = 0;
            localLevelProgress = 0;
        }

        ClampLevels();

        if (localLevelProgress < 0)
        {
            localLevelProgress = 0;
        }

        selectedWeapon = WeaponType.None;
    }

    private void ClampLevels()
    {
        currentLevel = Mathf.Clamp(currentLevel, 0, MaxLevel);
        hpLevel = Mathf.Clamp(hpLevel, 0, MaxHpLevel);
        speedLevel = Mathf.Clamp(speedLevel, 0, MaxSpeedLevel);
        parryLevel = Mathf.Clamp(parryLevel, 0, MaxParryLevel);
    }
}
