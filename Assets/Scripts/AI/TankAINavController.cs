using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class TankAINavController : MonoBehaviour
{
    public float engageDistance = 40f;
    public float stopDistance = 5f;
    public float angleThreshold = 10f;
    public float shootCooldown = 1.2f;

    private NavMeshAgent agent;
    private Complete.TankShooting shooting;
    private Complete.TankHealth health;
    private Transform target;
    private float shootTimer = 0f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        shooting = GetComponent<Complete.TankShooting>();
        health = GetComponent<Complete.TankHealth>();

        // keep rotation controlled by code for aiming
        agent.updateRotation = false;
    }

    private void Start()
    {
        if (shooting != null)
            shooting.SetAIMode(true);
    }

    private float targetCheckTimer = 0f;

    private void Update()
    {
        // Periodically check/refresh target
        targetCheckTimer += Time.deltaTime;
        if (target == null || !target.gameObject.activeSelf || targetCheckTimer > 1.0f)
        {
            if (Complete.GameManager.Instance != null)
                target = Complete.GameManager.Instance.GetClosestActivePlayer(transform.position);
            targetCheckTimer = 0f;
        }

        if (target == null || !gameObject.activeSelf)
            return;

        agent.SetDestination(target.position);

        Vector3 dir = (target.position - transform.position);
        float distance = dir.magnitude;
        Vector3 dirNorm = dir.normalized;

        // Face the target smoothly
        Vector3 look = Vector3.RotateTowards(transform.forward, dirNorm, Time.deltaTime * 4f, 0f);
        transform.rotation = Quaternion.LookRotation(look);

        // Stop or move depending on distance
        if (distance <= stopDistance)
        {
            agent.isStopped = true;
        }
        else
        {
            agent.isStopped = false;
        }

        // Simple shooting logic
        shootTimer += Time.deltaTime;
        float angle = Vector3.Angle(transform.forward, dirNorm);
        if (distance <= engageDistance && angle < angleThreshold && shootTimer >= shootCooldown && shooting != null)
        {
            shooting.AIFire();
            shootTimer = 0f;
        }
    }

    // Initialize target and difficulty
    public void Initialize(Transform playerTarget, float healthMultiplier = 1f, float damageMultiplier = 1f, float attackSpeedMultiplier = 1f)
    {
        target = playerTarget;

        if (health != null)
            health.m_StartingHealth *= healthMultiplier;

        if (shooting != null)
        {
            shooting.m_DamageMultiplier = damageMultiplier;
            shooting.m_AttackSpeedMultiplier = attackSpeedMultiplier;
            shooting.SetAIMode(true);
        }
    }
}