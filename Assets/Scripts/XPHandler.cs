using UnityEngine;
using TMPro;

public class XPHandler : MonoBehaviour
{
    public static XPHandler Instance { get; private set; }

    public static System.Action<int> OnLocalLevelChanged;

    [Header("UI")]
    [SerializeField] private TMP_Text xpText;
    [SerializeField] private TMP_Text localLevelText;

    [Header("XP stuff")]
    [SerializeField] private int baseXPToLevelUp = 10;
    [SerializeField] private int xpIncreasePerLocalLevel = 1;
    [SerializeField] private int xpIncreasePerPlayerLevel = 2;

    [Header("Other Things")]
    [SerializeField] private int currentXP = 0;
    [SerializeField] private int localLevel = 0;
    [SerializeField] private int xpToNextLocalLevel = 0;

    public int CurrentXP => currentXP;
    public int LocalLevel => localLevel;
    public int XPToNextLocalLevel => xpToNextLocalLevel;

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

    private void OnEnable()
    {
        // Sub to enemy death for XP 
        EnemyHealthXP.OnEnemyDied += HandleEnemyDied;
    }

    private void OnDisable()
    {
        EnemyHealthXP.OnEnemyDied -= HandleEnemyDied;
    }

    private void Start()
    {
        RecalculateXPToNextLevel();
        UpdateUI();
    }

    private void HandleEnemyDied(EnemyHealthXP enemy)
    {
        if (enemy == null) return;
        AddXP(enemy.XpValue);
    }

    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        currentXP += amount;

        // Repeat if XP gives mroe than one level
        while (true)
        {
            if (xpToNextLocalLevel <= 0)
            {
                RecalculateXPToNextLevel();
            }

            if (currentXP < xpToNextLocalLevel)
                break;

            currentXP -= xpToNextLocalLevel;
            localLevel++;

            OnLocalLevelChanged?.Invoke(localLevel);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnLocalLevelGained();
            }

            RecalculateXPToNextLevel();
        }

        UpdateUI();
    }

    //Make it difficult to level up as local level and player level increase
    private void RecalculateXPToNextLevel()
    {
        int playerLevel = 0;

        if (GameManager.Instance != null)
        {
            playerLevel = GameManager.Instance.CurrentLevel;
        }

        xpToNextLocalLevel =
            baseXPToLevelUp
            + localLevel * xpIncreasePerLocalLevel
            + playerLevel * xpIncreasePerPlayerLevel;

        if (xpToNextLocalLevel < 1)
        {
            xpToNextLocalLevel = 1;
        }
    }

    private void UpdateUI()
    {
        if (xpText != null)
        {
            xpText.text = $"{currentXP}/{xpToNextLocalLevel}";
        }

        if (localLevelText != null)
        {
            localLevelText.text = localLevel.ToString();
        }
    }

    // Reset run level and XP 
    public void ResetRun()
    {
        currentXP = 0;
        localLevel = 0;
        RecalculateXPToNextLevel();
        UpdateUI();
    }
}
