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

    [Header("Current Levels")]
    [SerializeField] private int currentLevel = 0;
    [SerializeField] private int hpLevel = 0;
    [SerializeField] private int speedLevel = 0;
    [SerializeField] private int parryLevel = 0;

    [Header("Base Stats & Per-Level Increments")]
    [SerializeField] private int baseHP = 10;
    [SerializeField] private int hpPerLevel = 10;

    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float moveSpeedPerLevel = 0.5f;

    [SerializeField] private int baseParry = 1;
    [SerializeField] private int parryPerLevel = 1;

    [Header("Weapon Selection (runtime only)")]
    [SerializeField] private WeaponType selectedWeapon = WeaponType.None;

    public System.Action OnStatsChanged;

    public int CurrentLevel => currentLevel;
    public int HpUpgradeLevel => hpLevel;
    public int SpeedUpgradeLevel => speedLevel;
    public int ParryUpgradeLevel => parryLevel;

    public int CurrentMaxHP => baseHP + hpLevel * hpPerLevel;
    public float CurrentMoveSpeed => baseMoveSpeed + speedLevel * moveSpeedPerLevel;
    public int CurrentParry => baseParry + parryLevel * parryPerLevel;

    public WeaponType SelectedWeapon => selectedWeapon;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadData();
        NotifyStatsChanged();
    }

    // ---------------- WEAPON (NOT PERSISTENT) ----------------

    public void SetWeapon(WeaponType weapon)
    {
        selectedWeapon = weapon;
        NotifyStatsChanged();   // no save; weapon is per-run only
    }

    // ---------------- UPGRADE API ----------------

    public bool CanUpgradeHP()
    {
        return currentLevel < MaxLevel && hpLevel < MaxHpLevel;
    }

    public bool CanUpgradeSpeed()
    {
        return currentLevel < MaxLevel && speedLevel < MaxSpeedLevel;
    }

    public bool CanUpgradeParry()
    {
        return currentLevel < MaxLevel && parryLevel < MaxParryLevel;
    }

    public bool UpgradeHP()
    {
        if (!CanUpgradeHP()) return false;

        hpLevel++;
        currentLevel++;
        AutoSaveAndBroadcast();
        return true;
    }

    public bool UpgradeSpeed()
    {
        if (!CanUpgradeSpeed()) return false;

        speedLevel++;
        currentLevel++;
        AutoSaveAndBroadcast();
        return true;
    }

    public bool UpgradeParry()
    {
        if (!CanUpgradeParry()) return false;

        parryLevel++;
        currentLevel++;
        AutoSaveAndBroadcast();
        return true;
    }

    // ---------------- SAVE / LOAD (ONLY PERMANENT UPGRADES) ----------------

    private void AutoSaveAndBroadcast()
    {
        SaveData();
        NotifyStatsChanged();
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
        }
        else
        {
            currentLevel = 0;
            hpLevel = 0;
            speedLevel = 0;
            parryLevel = 0;
        }

        currentLevel = Mathf.Clamp(currentLevel, 0, MaxLevel);
        hpLevel = Mathf.Clamp(hpLevel, 0, MaxHpLevel);
        speedLevel = Mathf.Clamp(speedLevel, 0, MaxSpeedLevel);
        parryLevel = Mathf.Clamp(parryLevel, 0, MaxParryLevel);

        // weapon is always reset per app run
        selectedWeapon = WeaponType.None;
    }
}
