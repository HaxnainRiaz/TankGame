using UnityEngine;

[RequireComponent(typeof(Complete.TankMovement), typeof(Complete.TankShooting), typeof(Complete.TankHealth))]
public class TankAIController : MonoBehaviour
{
    public Transform target;
    public float engageDistance = 40f;
    public float stopDistance = 5f;
    public float angleThreshold = 10f;
    public float shootCooldown = 1.5f;

    private Complete.TankMovement movement;
    private Complete.TankShooting shooting;
    private Complete.TankHealth health;
    private float shootTimer = 0f;

    private void Awake()
    {
        movement = GetComponent<Complete.TankMovement>();
        shooting = GetComponent<Complete.TankShooting>();
        health = GetComponent<Complete.TankHealth>();
    }

    private void Start()
    {
        movement.SetAIMode(true);
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

        Vector3 dir = (target.position - transform.position);
        float distance = dir.magnitude;
        Vector3 dirNorm = dir.normalized;

        // Determine forward movement
        float move = 0f;
        if (distance > stopDistance)
            move = 1f;

        // Determine turning
        float angle = Vector3.SignedAngle(transform.forward, dirNorm, Vector3.up);
        float turn = Mathf.Clamp(angle / 90f, -1f, 1f);

        // Apply movement to TankMovement
        movement.ApplyAIMovement(move, turn);

        // Shooting decision
        shootTimer += Time.deltaTime;
        if (distance <= engageDistance && Mathf.Abs(angle) < angleThreshold && shootTimer >= shootCooldown)
        {
            shooting.AIFire();
            shootTimer = 0f;
        }
    }

    // Initialize target and difficulty options
    public void Initialize(Transform playerTarget, float healthMultiplier = 1f, float damageMultiplier = 1f, float attackSpeedMultiplier = 1f)
    {
        target = playerTarget;
        
        // Ensure references
        if (movement == null) movement = GetComponent<Complete.TankMovement>();
        if (shooting == null) shooting = GetComponent<Complete.TankShooting>();

        if (movement != null) movement.SetAIMode(true);

        // Adjust health
        Complete.TankHealth th = GetComponent<Complete.TankHealth>();
        if (th != null)
            th.m_StartingHealth *= healthMultiplier;

        Complete.TankShooting ts = GetComponent<Complete.TankShooting>();
        if (ts != null)
        {
            ts.m_DamageMultiplier = damageMultiplier;
            ts.m_AttackSpeedMultiplier = attackSpeedMultiplier;
            ts.SetAIMode(true);
        }
    }
}
