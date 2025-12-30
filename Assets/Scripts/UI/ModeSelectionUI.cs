using UnityEngine;

public class ModeSelectionUI : MonoBehaviour
{
    public void OnSinglePlayerClicked()
    {
        GameModeManager.Instance.LoadSinglePlayerAIMode();
    }

    public void OnMultiplayerClicked()
    {
        GameModeManager.Instance.LoadVersusMode();
    }
}
