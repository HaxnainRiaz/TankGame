using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject m_PausePanel;
    public GameObject m_MatchEndPanel;
    public GameObject m_RoundInfoPanel;
    public GameObject m_HUDPanel;
    public GameObject m_ConfirmationPanel;

    [Header("UI Text & Info (Drop GameObjects here)")]
    public GameObject m_MessageText;
    public GameObject m_RoundWinnerText;
    public GameObject m_GameWinnerText;
    public GameObject m_WaveCounterText;
    public GameObject m_ScoreText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        HideAllPanels();
        ShowPanel(m_HUDPanel);
    }

    public void ShowPanel(GameObject panel)
    {
        if (panel != null) panel.SetActive(true);
    }

    public void HidePanel(GameObject panel)
    {
        if (panel != null) panel.SetActive(false);
    }

    public void HideAllPanels()
    {
        HidePanel(m_PausePanel);
        HidePanel(m_MatchEndPanel);
        HidePanel(m_RoundInfoPanel);
        HidePanel(m_HUDPanel);
        HidePanel(m_ConfirmationPanel);
    }

    public void TogglePause(bool isPaused)
    {
        if (isPaused)
        {
            ShowPanel(m_PausePanel);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            HidePanel(m_PausePanel);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void UpdateHUD(int score, int target, int wave = -1)
    {
        SetText(m_ScoreText, $"Score: {score}/{target}");
        if (wave != -1) SetText(m_WaveCounterText, $"Wave: {wave}");
    }

    public void SetMessage(string message)
    {
        SetText(m_MessageText, message);
    }

    public void SetText(GameObject obj, string text)
    {
        if (obj == null) return;
        
        // Try Legacy Text
        var t = obj.GetComponent<UnityEngine.UI.Text>();
        if (t != null) { t.text = text; return; }

        // Try TextMeshPro (using reflection to avoid compile errors if package is missing)
        var tmp = obj.GetComponent("TextMeshProUGUI");
        if (tmp != null) {
            var prop = tmp.GetType().GetProperty("text");
            if (prop != null) prop.SetValue(tmp, text);
        }
    }

    public void OnResumeClicked()
    {
        GameManager.Instance.TogglePause();
    }

    public void OnRestartClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnQuitToMenuClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
