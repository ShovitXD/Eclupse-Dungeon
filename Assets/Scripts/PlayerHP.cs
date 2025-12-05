using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHP : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text parryText;

    [Header("Parry")]
    [SerializeField] private GameObject parryObject;
    [SerializeField] private float parryActiveDuration = 1f;
    [SerializeField] private float parryRechargeDuration = 2f;
    [SerializeField] private int maxParryCharges = 1;
    [SerializeField] private int currentParryCharges = 1;

    [Header("Debug")]
    public bool debugImmortal = false;

    private bool isParrying = false;

    private void Awake()
    {
        if (GameManager.Instance != null)
        {
            maxHealth = GameManager.Instance.CurrentMaxHP;
        }

        currentHealth = maxHealth;

        if (hpSlider == null)
        {
            GameObject sliderObj = GameObject.FindWithTag("HPBar");
            if (sliderObj != null)
                hpSlider = sliderObj.GetComponent<Slider>();
        }

        if (hpSlider != null)
        {
            hpSlider.minValue = 0f;
            hpSlider.maxValue = maxHealth;
            hpSlider.value = currentHealth;
        }

        if (hpText == null)
        {
            GameObject textObj = GameObject.FindWithTag("HPText");
            if (textObj != null)
                hpText = textObj.GetComponent<TMP_Text>();
        }

        if (parryText == null)
        {
            GameObject parryTextObj = GameObject.FindWithTag("ParryText");
            if (parryTextObj != null)
                parryText = parryTextObj.GetComponent<TMP_Text>();
        }

        if (parryObject == null)
        {
            GameObject parryObj = GameObject.FindWithTag("Parry");
            if (parryObj != null)
                parryObject = parryObj;
        }

        if (parryObject != null)
            parryObject.SetActive(false);

        InitParryFromGameManager();

        if (GameManager.Instance != null)
            GameManager.Instance.OnStatsChanged += HandleStatsChanged;

        UpdateUI();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStatsChanged -= HandleStatsChanged;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryParry();
        }
    }

    private void HandleStatsChanged()
    {
        if (GameManager.Instance == null)
            return;

        maxHealth = GameManager.Instance.CurrentMaxHP;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (hpSlider != null)
            hpSlider.maxValue = maxHealth;

        UpdateParryFromGameManager();
        UpdateUI();
    }

    // Damage and healing
    public void TakeDamage(float amount)
    {
        if (amount <= 0f)
            return;

        if (debugImmortal)
            return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateUI();

        if (currentHealth <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (amount <= 0f)
            return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateUI();
    }

    // Called from Update on Space press
    public void TryParry()
    {
        if (maxParryCharges <= 0)
            return;

        if (currentParryCharges <= 0)
            return;

        if (isParrying)
            return;

        currentParryCharges--;

        StartCoroutine(ParryWindow());
        StartCoroutine(ParryRecharge());

        UpdateParryUI();
    }

    // Parry internals
    private void InitParryFromGameManager()
    {
        if (GameManager.Instance != null)
            maxParryCharges = Mathf.Max(1, GameManager.Instance.CurrentParry);

        maxParryCharges = Mathf.Max(1, maxParryCharges);
        currentParryCharges = maxParryCharges;
        UpdateParryUI();
    }

    private void UpdateParryFromGameManager()
    {
        if (GameManager.Instance == null)
            return;

        int newMax = Mathf.Max(1, GameManager.Instance.CurrentParry);
        maxParryCharges = newMax;
        currentParryCharges = Mathf.Clamp(currentParryCharges, 0, maxParryCharges);
        UpdateParryUI();
    }

    private IEnumerator ParryWindow()
    {
        isParrying = true;

        if (parryObject != null)
        {
            parryObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("ParryObject is null");
        }

        yield return new WaitForSeconds(parryActiveDuration);

        if (parryObject != null)
            parryObject.SetActive(false);

        isParrying = false;
    }

    private IEnumerator ParryRecharge()
    {
        yield return new WaitForSeconds(parryRechargeDuration);

        if (currentParryCharges < maxParryCharges)
        {
            currentParryCharges++;
            UpdateParryUI();
        }
    }

    // UI helpers
    private void UpdateUI()
    {
        if (hpSlider != null)
            hpSlider.value = currentHealth;

        if (hpText != null)
        {
            int cur = Mathf.RoundToInt(currentHealth);
            int max = Mathf.RoundToInt(maxHealth);
            hpText.text = cur + "/" + max;
        }

        UpdateParryUI();
    }

    private void UpdateParryUI()
    {
        if (parryText != null)
            parryText.text = currentParryCharges + "/" + maxParryCharges;
    }

    // Player death handling
    private void Die()
    {
        if (debugImmortal)
            return;

        if (RunSceneManager.Instance != null)
        {
            RunSceneManager.Instance.OnPlayerDied();
        }
        else
        {
            Debug.LogWarning("RunSceneManager instance is null.");
        }
    }
}
