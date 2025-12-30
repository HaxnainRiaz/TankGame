using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Canvas))]
public class ModeHUD : MonoBehaviour
{
    public Text ModeText;

    private void Awake()
    {
        if (ModeText == null)
            ModeText = GetComponentInChildren<Text>();

        UpdateModeText();

        // Update on scene loads as modes may change when switching scenes.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateModeText();
    }

    private void UpdateModeText()
    {
        if (ModeText == null)
            return;

        if (GameModeManager.Instance == null)
        {
            ModeText.text = string.Empty;
            return;
        }

        switch (GameModeManager.Instance.Mode)
        {
            case GameMode.SinglePlayerAI:
                ModeText.text = "SINGLE PLAYER";
                break;
            case GameMode.CoopAI:
                ModeText.text = "CO-OP MULTIPLAYER";
                break;
            case GameMode.Versus:
            default:
                ModeText.text = "VERSUS";
                break;
        }
    }
}