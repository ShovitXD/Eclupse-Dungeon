using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpradeHandler : MonoBehaviour
{
    [Header("UI Text")]
    [SerializeField] private TMP_Text totalLevelText;
    [SerializeField] private TMP_Text freeLevelText;

    [Header("Level Visuals")]
    [SerializeField] private GameObject[] hpLevelImages;
    [SerializeField] private GameObject[] speedLevelImages;
    [SerializeField] private GameObject[] parryLevelImages;

    private void Awake()
    {
        ApplyLevelsFromManagerToVisuals();
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStatsChanged += HandleStatsChanged;
        }

        RefreshAll();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStatsChanged -= HandleStatsChanged;
        }
    }

    private void HandleStatsChanged()
    {
        RefreshAll();
    }

    // Button hooks
    public void UpgradeHP()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.UpgradeHP())
        {
            RefreshAll();
        }
    }

    public void UpgradeSpeed()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.UpgradeSpeed())
        {
            RefreshAll();
        }
    }

    public void UpgradeParry()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.UpgradeParry())
        {
            RefreshAll();
        }
    }

    public void ResetLevels()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.ResetAllLevels();
        RefreshAll();
    }

    // Helpers
    private void RefreshAll()
    {
        RefreshTextUI();
        ApplyLevelsFromManagerToVisuals();
    }

    private void RefreshTextUI()
    {
        if (GameManager.Instance == null) return;

        if (totalLevelText != null)
        {
            totalLevelText.text = GameManager.Instance.CurrentLevel.ToString();
        }

        if (freeLevelText != null)
        {
            freeLevelText.text = GameManager.Instance.FreeLevelPoints.ToString();
        }
    }

    private void ApplyLevelsFromManagerToVisuals()
    {
        // Image Switch for level vizualization
        if (GameManager.Instance == null)
        {
            SetLevelObjects(hpLevelImages, 0);
            SetLevelObjects(speedLevelImages, 0);
            SetLevelObjects(parryLevelImages, 0);
            return;
        }

        SetLevelObjects(hpLevelImages, GameManager.Instance.HpUpgradeLevel);
        SetLevelObjects(speedLevelImages, GameManager.Instance.SpeedUpgradeLevel);
        SetLevelObjects(parryLevelImages, GameManager.Instance.ParryUpgradeLevel);
    }

    //simply active the index equal to levle
    private void SetLevelObjects(GameObject[] objects, int level)
    {
        if (objects == null) return;

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] == null) continue;

            bool shouldBeActive = i < level;
            objects[i].SetActive(shouldBeActive);
        }
    }
}
