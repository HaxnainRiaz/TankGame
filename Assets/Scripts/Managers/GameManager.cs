using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int m_NumRoundsToWin = 5;
    public float m_StartDelay = 3f;
    public float m_EndDelay = 3f;
    public int m_TargetScore = 15;

    [Header("References")]
    public CameraControl m_CameraControl;
    public Text m_MessageText;
    public GameObject m_TankPrefab;
    public TankManager[] m_Tanks;

    private List<TankManager> m_AllTanks = new List<TankManager>();
    private TankManager m_PlayerTank;

    private int m_CurrentScore;
    private int m_RoundNumber;
    private WaitForSeconds m_StartWait;
    private WaitForSeconds m_EndWait;
    private TankManager m_RoundWinner;
    private TankManager m_GameWinner;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);
    }

    private void Start()
    {
        // Validate references
        if (m_TankPrefab == null) Debug.LogError("Tank prefab is missing!");
        if (m_CameraControl == null) Debug.LogError("CameraControl is missing!");
        if (m_MessageText == null) Debug.LogError("MessageText is missing!");

        // Initialize tanks
        SpawnAllTanks();
        if (GameModeManager.Instance != null &&
            GameModeManager.Instance.Mode == GameMode.SinglePlayerAI &&
            m_Tanks.Length > 0)
        {
            m_PlayerTank = m_Tanks[0];
        }

        SetCameraTargets();

        // Start main game loop
        StartCoroutine(GameLoop());
    }

    private void SpawnAllTanks()
    {
        if (m_Tanks == null || m_Tanks.Length == 0)
        {
            Debug.LogError("No tanks assigned in GameManager!");
            return;
        }

        m_AllTanks.Clear();

        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (GameModeManager.Instance != null &&
                GameModeManager.Instance.Mode == GameMode.SinglePlayerAI &&
                i == 1) // Skip player 2 in single-player mode
                continue;

            if (m_TankPrefab == null || m_Tanks[i].m_SpawnPoint == null)
            {
                Debug.LogError($"Tank prefab or spawn point missing for tank {i}");
                continue;
            }

            m_Tanks[i].m_Instance = Instantiate(m_TankPrefab,
                m_Tanks[i].m_SpawnPoint.position,
                m_Tanks[i].m_SpawnPoint.rotation);

            m_Tanks[i].m_PlayerNumber = i + 1;
            m_Tanks[i].Setup();
            m_AllTanks.Add(m_Tanks[i]);
        }
    }

    private void SetCameraTargets()
    {
        if (m_CameraControl == null) return;

        Transform[] targets = new Transform[m_AllTanks.Count];
        for (int i = 0; i < m_AllTanks.Count; i++)
        {
            if (m_AllTanks[i].m_Instance != null)
                targets[i] = m_AllTanks[i].m_Instance.transform;
        }

        m_CameraControl.m_Targets = targets;
    }

    private IEnumerator GameLoop()
    {
        while (true)
        {
            yield return StartCoroutine(RoundStarting());
            yield return StartCoroutine(RoundPlaying());
            yield return StartCoroutine(RoundEnding());

            if (m_GameWinner != null)
            {
                // Restart scene safely
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                yield break;
            }
        }
    }

    private IEnumerator RoundStarting()
    {
        ResetAllTanks();
        DisableTankControl();
        m_CurrentScore = 0;
        m_RoundNumber++;

        if (m_CameraControl != null) m_CameraControl.SetStartPositionAndSize();
        if (m_MessageText != null) m_MessageText.text = $"ROUND {m_RoundNumber}";

        yield return m_StartWait;
    }

    private IEnumerator RoundPlaying()
    {
        EnableTankControl();
        if (m_MessageText != null) m_MessageText.text = "";

        if (GameModeManager.Instance != null &&
            (GameModeManager.Instance.Mode == GameMode.SinglePlayerAI ||
             GameModeManager.Instance.Mode == GameMode.CoopAI))
        {
            while (!AreAllPlayersDead() && m_CurrentScore < m_TargetScore)
                yield return null;
        }
        else
        {
            while (!OneTankLeft())
                yield return null;
        }
    }

    private IEnumerator RoundEnding()
    {
        DisableTankControl();
        m_RoundWinner = GetRoundWinner();
        if (m_RoundWinner != null) m_RoundWinner.m_Wins++;
        m_GameWinner = GetGameWinner();

        if (m_MessageText != null) m_MessageText.text = EndMessage();

        yield return m_EndWait;
    }

    private bool OneTankLeft()
    {
        int count = 0;
        foreach (var tank in m_AllTanks)
        {
            if (tank != null && tank.m_Instance != null && tank.m_Instance.activeSelf)
                count++;
        }
        return count <= 1;
    }

    private TankManager GetRoundWinner()
    {
        foreach (var tank in m_AllTanks)
        {
            if (tank != null && tank.m_Instance != null && tank.m_Instance.activeSelf)
                return tank;
        }
        return null;
    }

    private TankManager GetGameWinner()
    {
        foreach (var tank in m_AllTanks)
        {
            if (tank != null && tank.m_Wins >= m_NumRoundsToWin)
                return tank;
        }
        return null;
    }

    private string EndMessage()
    {
        string message = "DRAW!";
        if (m_RoundWinner != null)
            message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

        if (GameModeManager.Instance != null &&
            (GameModeManager.Instance.Mode == GameMode.SinglePlayerAI ||
             GameModeManager.Instance.Mode == GameMode.CoopAI))
        {
            message = m_CurrentScore >= m_TargetScore ?
                $"MISSION ACCOMPLISHED!\nSCORE: {m_CurrentScore}" :
                $"MISSION FAILED!\nSCORE: {m_CurrentScore}";
        }

        message += "\n\n\n\n";

        foreach (var tank in m_AllTanks)
        {
            if (tank != null)
                message += tank.m_ColoredPlayerText + ": " + tank.m_Wins + " WINS\n";
        }

        if (m_GameWinner != null)
            message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";

        return message;
    }

    private bool AreAllPlayersDead()
    {
        foreach (var tank in m_Tanks)
        {
            if (tank != null && tank.m_Instance != null && tank.m_Instance.activeSelf)
                return false;
        }
        return true;
    }

    public void AddScore(int amount)
    {
        m_CurrentScore += amount;
    }

    private void ResetAllTanks()
    {
        foreach (var tank in m_AllTanks)
        {
            if (tank != null)
                tank.Reset();
        }
    }

    private void EnableTankControl()
    {
        foreach (var tank in m_AllTanks)
        {
            if (tank != null)
                tank.EnableControl();
        }
    }

    private void DisableTankControl()
    {
        foreach (var tank in m_AllTanks)
        {
            if (tank != null)
                tank.DisableControl();
        }
    }
}
