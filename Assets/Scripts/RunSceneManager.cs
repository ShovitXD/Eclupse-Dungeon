using UnityEngine;
using UnityEngine.SceneManagement;

public class RunSceneManager : MonoBehaviour
{
    public static RunSceneManager Instance { get; private set; }

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
    }

    private void OnEnable()
    {
        EnemyHealthXP.OnEnemyDied += HandleEnemyDied;
    }

    private void OnDisable()
    {
        EnemyHealthXP.OnEnemyDied -= HandleEnemyDied;
    }

    // Player death
    public void OnPlayerDied()
    {
        if (XPHandler.Instance != null)
        {
            XPHandler.Instance.ResetRun();
        }

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    // Boss death 
    private void HandleEnemyDied(EnemyHealthXP enemy)
    {
        if (enemy == null)
            return;

        // Boss detection by searching for BossBulletHell
        BossBulletHell boss = enemy.GetComponentInParent<BossBulletHell>();
        if (boss != null)
        {
            OnBossDefeated();
        }
    }

    private void OnBossDefeated()
    {
        if (XPHandler.Instance != null)
        {
            XPHandler.Instance.ResetRun();
        }

        Time.timeScale = 1f;

        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(sceneIndex, LoadSceneMode.Single);
    }
}
