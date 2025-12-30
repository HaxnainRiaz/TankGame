using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameMode { Versus, SinglePlayerAI, CoopAI }

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance { get; private set; }

    public GameMode Mode { get; private set; } = GameMode.Versus;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadVersusMode()
    {
        Mode = GameMode.Versus;
        LoadNextScene();
    }

    public void LoadSinglePlayerAIMode()
    {
        Mode = GameMode.SinglePlayerAI;
        LoadNextScene();
    }

    public void LoadCoopAIMode()
    {
        Mode = GameMode.CoopAI;
        LoadNextScene();
    }

    public void LoadNextScene()
    {
        // Assumes Mode selection screen is index 0 and BattleGround is index 1 (or next)
        // If current scene is already the battleground (e.g. restart), reload it.
        // But usually Menu -> Game.
        int current = SceneManager.GetActiveScene().buildIndex;
        // If we are in Menu (index 0 usually), go to 1.
        // If we want to support flexible build settings, checking by name might be safer, but index is standard.
        // Let's assume BattleGround is the NEXT scene in Build Settings.
        // If we are essentially restarting, we might just load the Game scene directly.
        // For now, load index 1 if we are at 0, or just load the game scene by name "Main" or similar if known? 
        // User didn't give scene names. Let's stick to buildIndex + 1 logic or specific check.
        // Safest default for "Menu -> Game" transition:
        int next = current + 1;
        if (next < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(next);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Reload current if no next
    }
}
