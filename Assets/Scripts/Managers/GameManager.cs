using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum GameState { Starting, Playing, Paused, RoundEnding, MatchEnding }

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int m_NumRoundsToWin = 5;
    public float m_StartDelay = 3f;
    public float m_EndDelay = 3f;
    public int m_TargetScore = 15;

    [Header("References")]
    public CameraControl m_CameraControl;
    public GameObject m_TankPrefab;
    public TankManager[] m_Tanks;

    private List<TankManager> m_AllTanks = new List<TankManager>();
    private int m_CurrentScore;
    private int m_RoundNumber;
    private WaitForSeconds m_StartWait;
    private WaitForSeconds m_EndWait;
    private TankManager m_RoundWinner;
    private TankManager m_GameWinner;

    public GameState m_State { get; private set; }
    private GameState m_LastState;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);
    }

    private void Start()
    {
        SpawnAllTanks();
        SetCameraTargets();
        StartCoroutine(GameLoop());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (m_State == GameState.Paused)
        {
            m_State = m_LastState;
            Time.timeScale = 1f;
            UIManager.Instance.TogglePause(false);
            if (m_State == GameState.Playing) EnableTankControl();
        }
        else if (m_State == GameState.Playing || m_State == GameState.Starting)
        {
            m_LastState = m_State;
            m_State = GameState.Paused;
            Time.timeScale = 0f;
            DisableTankControl();
            UIManager.Instance.TogglePause(true);
        }
    }

    private IEnumerator GameLoop()
    {
        while (m_GameWinner == null)
        {
            yield return StartCoroutine(RoundStarting());
            yield return StartCoroutine(RoundPlaying());
            yield return StartCoroutine(RoundEnding());
        }

        // Match ended
        m_State = GameState.MatchEnding;
        UIManager.Instance.ShowPanel(UIManager.Instance.m_MatchEndPanel);
    }

    private IEnumerator RoundStarting()
    {
        m_State = GameState.Starting;
        ResetAllTanks();
        DisableTankControl();
        m_RoundNumber++;

        if (m_CameraControl != null) m_CameraControl.SetStartPositionAndSize();
        
        // Show Round Start UI
        UIManager.Instance.SetMessage($"ROUND {m_RoundNumber}");

        yield return m_StartWait;
    }

    private IEnumerator RoundPlaying()
    {
        m_State = GameState.Playing;
        EnableTankControl();
        
        UIManager.Instance.SetMessage("");

        // Wait until goal met or players dead
        bool isAIGame = GameModeManager.Instance != null && 
                       (GameModeManager.Instance.Mode == GameMode.SinglePlayerAI || 
                        GameModeManager.Instance.Mode == GameMode.CoopAI);

        while (true)
        {
            if (m_State == GameState.Paused)
            {
                yield return null;
                continue;
            }

            if (isAIGame)
            {
                if (AreAllPlayersDead()) break;
                if (m_CurrentScore >= m_TargetScore) break;
            }
            else
            {
                if (OneTankLeft()) break;
            }

            yield return null;
        }
    }

    private IEnumerator RoundEnding()
    {
        m_State = GameState.RoundEnding;
        DisableTankControl();

        m_RoundWinner = GetRoundWinner();
        if (m_RoundWinner != null) m_RoundWinner.m_Wins++;

        m_GameWinner = GetGameWinner();
        UIManager.Instance.SetMessage(EndMessage());

        yield return m_EndWait;
    }

    private void SpawnAllTanks()
    {
        m_AllTanks.Clear();
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            // Logic for skipping player 2 in Single Player
            if (GameModeManager.Instance != null && GameModeManager.Instance.Mode == GameMode.SinglePlayerAI && i == 1)
                continue;

            m_Tanks[i].m_Instance = Instantiate(m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation);
            m_Tanks[i].m_PlayerNumber = i + 1;
            m_Tanks[i].Setup();
            m_AllTanks.Add(m_Tanks[i]);
        }
    }

    private void SetCameraTargets()
    {
        Transform[] targets = new Transform[m_AllTanks.Count];
        for (int i = 0; i < m_AllTanks.Count; i++)
            targets[i] = m_AllTanks[i].m_Instance.transform;
        m_CameraControl.m_Targets = targets;
    }

    private bool OneTankLeft()
    {
        int count = 0;
        foreach (var tank in m_AllTanks)
            if (tank.m_Instance.activeSelf) count++;
        return count <= 1;
    }

    private TankManager GetRoundWinner()
    {
        foreach (var tank in m_AllTanks)
            if (tank.m_Instance.activeSelf) return tank;
        return null;
    }

    private TankManager GetGameWinner()
    {
        foreach (var tank in m_AllTanks)
            if (tank.m_Wins >= m_NumRoundsToWin) return tank;
        return null;
    }

    private bool AreAllPlayersDead()
    {
        foreach (var tank in m_Tanks)
            if (tank != null && tank.m_Instance != null && tank.m_Instance.activeSelf) return false;
        return true;
    }

    private string EndMessage()
    {
        string message = "DRAW!";
        if (m_RoundWinner != null) message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";
        if (m_State == GameState.MatchEnding && m_GameWinner != null) message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";
        
        return message;
    }

    public void AddScore(int amount)
    {
        m_CurrentScore += amount;
        UIManager.Instance.UpdateHUD(m_CurrentScore, m_TargetScore);
    }

    public void ResetAllTanks() { foreach (var tank in m_AllTanks) tank.Reset(); }
    public void EnableTankControl() { foreach (var tank in m_AllTanks) tank.EnableControl(); }
    public void DisableTankControl() { foreach (var tank in m_AllTanks) tank.DisableControl(); }

    // This allows EnemySpawner to register AI tanks if needed
    public TankManager RegisterAdditionalTank(GameObject go, Color color, Transform spawn)
    {
        // For simplicity in this demo, we'll just track the score. 
        // A full implementation would add the AI to m_AllTanks for camera tracking.
        return null; 
    }
}

