using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Optional: Ensure cursor is visible if returning from game
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void OnVersusClicked()
    {
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.LoadVersusMode();
        }
    }

    public void OnSinglePlayerAIClicked()
    {
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.LoadSinglePlayerAIMode();
        }
    }

    public void OnCoopAIClicked()
    {
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.LoadCoopAIMode();
        }
    }

    public void OnExitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
