using UnityEngine;

public class MenuBtn : MonoBehaviour
{
    [SerializeField] private GameObject uiPanel1,uiPannelPlay;

    // Enable a UI panel
    public void TogglePanel()
    {
        if (uiPanel1 != null)
        {
            uiPanel1.SetActive(!uiPanel1.activeSelf);
            Debug.Log("Panel toggled");
        }
    }
    public void TogglePanelPlay()
    {
        if (uiPannelPlay != null)
        {
            uiPannelPlay.SetActive(!uiPannelPlay.activeSelf);
        }
    }

    // Quit the game
    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
