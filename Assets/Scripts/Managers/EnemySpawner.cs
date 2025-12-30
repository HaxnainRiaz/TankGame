using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Complete; // For GameManager and TankManager

public class EnemySpawner : MonoBehaviour
{
    public GameObject m_TankPrefab;
    public Transform[] m_SpawnPoints;
    public float m_SpawnInterval = 5f;

    [Header("AI Difficulty")]
    public float m_HealthMultiplier = 1.5f;
    public float m_DamageMultiplier = 1.7f;
    public float m_AttackSpeedMultiplier = 1.3f;

    [Header("AI Options")]
    public bool m_UseNavMeshAI = false;

    private Complete.GameManager gameManager;

    private void Start()
    {
        gameManager = Object.FindAnyObjectByType<Complete.GameManager>();

        if (GameModeManager.Instance != null && (GameModeManager.Instance.Mode == GameMode.SinglePlayerAI || GameModeManager.Instance.Mode == GameMode.CoopAI))
        {
            SpawnAllEnemies();
        }
    }

    // Removed Random SpawnLoop to satisfy user request for enemies at all points.
    private void SpawnAllEnemies()
    {
        if (m_SpawnPoints == null || m_TankPrefab == null || gameManager == null)
            return;

        Debug.Log("Spawning Enemies. Count: " + m_SpawnPoints.Length);
        for (int i = 0; i < m_SpawnPoints.Length; i++)
        {
            if (m_SpawnPoints[i] == null) continue;

            Transform sp = m_SpawnPoints[i];
            SpawnOneAt(sp);
        }
    }

    private void SpawnOneAt(Transform sp)
    {
        GameObject go = Instantiate(m_TankPrefab, sp.position, sp.rotation) as GameObject;
        go.SetActive(true); // Ensure it is active

        // Prefer NavMesh-based AI if enabled and present on the prefab
        if (m_UseNavMeshAI)
        {
            var navAI = go.GetComponent<TankAINavController>();
            if (navAI != null)
            {
                navAI.Initialize(null, m_HealthMultiplier, m_DamageMultiplier, m_AttackSpeedMultiplier);
            }
            else
            {
                var ai = go.GetComponent<TankAIController>();
                if (ai == null) ai = go.AddComponent<TankAIController>(); // Auto-add AI if missing
                ai.Initialize(null, m_HealthMultiplier, m_DamageMultiplier, m_AttackSpeedMultiplier);
            }
        }
        else
        {
            // Configure classic steering AI
            var ai = go.GetComponent<TankAIController>();
            if (ai == null) ai = go.AddComponent<TankAIController>(); // Auto-add AI if missing
            ai.Initialize(null, m_HealthMultiplier, m_DamageMultiplier, m_AttackSpeedMultiplier);
        }

        // Register with GameManager 
        Color aiColor = Color.red; 
        // Iterate through colors if we had a list, but red is fine for enemies.
        
        var tm = gameManager.RegisterAdditionalTank(go, aiColor, sp);

        // Ensure shooting uses the damage multiplier
        var ts = go.GetComponent<TankShooting>();
        if (ts != null)
        {
            ts.m_DamageMultiplier = m_DamageMultiplier;
            ts.m_AttackSpeedMultiplier = m_AttackSpeedMultiplier;
            ts.SetAIMode(true);
        }
    }


}
