using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CombatManager : Singleton<CombatManager>
{
    // === Components ===
    [Header("Components")]
    public VFXManager vFXManager;
    public PlayerController playerController; // Reference to the player controller
    public Collider swordCollider; // Sword collider for detecting hits
    public Collider rotationCollider; // Collider for rotation skill detection
    public Collider[] bodyCollider; //

    // === Attack Parameters ===
    [Header("Attack Parameters")]
    public float attackResetTimer = 1.5f; // Time to reset attack combo
    public float attackCooldown = 1f; // Cooldown between consecutive attacks
    public float rotationCooldown = 8f; // Cooldown for rotation skill
    public float ultimateCooldown = 8f; // Cooldown for ultimate skill
    public int[] attackSequence = { 0, 1, 2 }; // Sequence of attack animations
    public LayerMask enemyLayer; // Layer for enemy detection

    // === State Flags ===
    [Header("State Flags")]
    public bool isStunned = false; // Indicates if the player is stunned

    // === Nearby Detection ===
    [Header("Detection Parameters")]
    public float detectionRadius = 5f; // Radius for detecting nearby enemies
    private Collider[] nearbyEnemies; // Array of detected enemies
    private Transform targetedEnemy; // Currently targeted enemy

    // === Pushback Parameters ===
    [Header("Pushback Parameters")]
    public float pushbackForce = 5f; // Force applied during pushback
    public float pushbackDuration = 0.2f; // Duration of the pushback effect

    private Vector3 pushbackDirection; // Direction of pushback
    private float pushbackTimer = 0f; // Timer for pushback duration

    // === Internal Variables ===
    public float attackCooldownTimer = 0f; // Timer for attack cooldown
    private int currentAttackIndex = 0; // Index for the current attack in the sequence
    private float rotationCooldownTimer = 0f; // Timer for rotation cooldown
    private float ultimateCooldownTimer = 0f; // Timer for ultimate cooldown

    // === Skill States ===
    public bool isSkillPerforming = false; // Indicates if any skill is being performed
    public bool isSkill2Performing = false; // Indicates if the rotation skill is being performed
    public bool isUltimateRunning = false; // Indicates if the ultimate skill is active

    // === Multiplayer Variables ===
    [SerializeField] private NetworkVariable<float> networkAttackCooldownTimer = new NetworkVariable<float>();
    [SerializeField] NetworkVariable<bool> networkMeleAttack = new NetworkVariable<bool>();
    [SerializeField] NetworkVariable<bool> networkSkill_1 = new NetworkVariable<bool>();
    [SerializeField] NetworkVariable<bool> networkSkill_2 = new NetworkVariable<bool>();
    [SerializeField] NetworkVariable<bool> networkSkill_3 = new NetworkVariable<bool>();

    //Client-side input tracking
    private bool oldMeleAttack;
    private bool oldSkill_1;
    private bool oldSkill_2;
    private bool oldSkill_3;

    private void Update()
    {
        // Server-specific updates
        if (IsServer)
        {
            UpdateServer();
        }

        // Client-specific input handling
        if (IsOwner && IsClient)
        {
            HandleClientInput();
        }
        attackCooldownTimer = networkAttackCooldownTimer.Value;
        // Core gameplay logic
        CheckNearbyEnemies();
        HandlePushback();
        HandleRotationStop();
        HandleUltimateStop();
    }

    private void UpdateServer()
    {
        if (networkAttackCooldownTimer.Value > 0)
        {
            networkAttackCooldownTimer.Value -= Time.deltaTime;
            if (networkAttackCooldownTimer.Value <= 0)
            {
                currentAttackIndex = 0; // Reset attack sequence
            }
        }

        HandleAttack(networkMeleAttack.Value, networkSkill_1.Value, networkSkill_2.Value, networkSkill_3.Value);
    }

    private void HandleClientInput()
    {
        bool localMeleAttack = InputAction.Instance.meleAttack;
        bool localSkill1 = InputAction.Instance.skill_1;
        bool localSkill2 = InputAction.Instance.skill_2;
        bool localSkill3 = InputAction.Instance.skill_3;

        if (localMeleAttack != oldMeleAttack || localSkill1 != oldSkill_1 || localSkill2 != oldSkill_2 || localSkill3 != oldSkill_3)
        {
            oldMeleAttack = localMeleAttack;
            oldSkill_1 = localSkill1;
            oldSkill_2 = localSkill2;
            oldSkill_3 = localSkill3;

            UpdateCombatInputServerRpc(localMeleAttack, localSkill1, localSkill2, localSkill3);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateCombatInputServerRpc(bool meleAttack, bool skill1, bool skill2, bool skill3)
    {
        networkMeleAttack.Value = meleAttack;
        networkSkill_1.Value = skill1;
        networkSkill_2.Value = skill2;
        networkSkill_3.Value = skill3;
    }

    private void HandleAttack(bool meleAttack, bool skill1, bool skill2, bool skill3)
    {
     /*   if (attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
            if (attackCooldownTimer <= 0)
            {
                currentAttackIndex = 0; // Reset attack sequence
            }
        }*/

        if (meleAttack && !networkSkill_1.Value)
        {
            if (networkAttackCooldownTimer.Value <= 0.5f)
            {
                PerformAttackServerRpc(currentAttackIndex);
                currentAttackIndex = (currentAttackIndex + 1) % attackSequence.Length;
                networkAttackCooldownTimer.Value = isUltimateRunning ? attackCooldown / 1.5f : attackCooldown;
            }
            InputAction.Instance.meleAttack = false;
        }

        if (skill1 && !isSkillPerforming && !isSkill2Performing && !isUltimateRunning)
        {
            PerformSkillServerRpc(3, 3f);
            InputAction.Instance.skill_1 = false;
        }

        if (skill2 && !isSkillPerforming && !isSkill2Performing && !isUltimateRunning)
        {
            PerformSkillServerRpc(4, rotationCooldown);
            isSkill2Performing = true;
            rotationCooldownTimer = rotationCooldown;
            InputAction.Instance.skill_2 = false;
        }

        if (skill3 && !isSkillPerforming && !isSkill2Performing && !isUltimateRunning)
        {
            PerformSkillServerRpc(5, ultimateCooldown);
            isUltimateRunning = true;
            ultimateCooldownTimer = ultimateCooldown;
            InputAction.Instance.skill_3 = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PerformAttackServerRpc(int attackIndex)
    {
        PerformAttackClientRpc(attackIndex);
    }

    [ClientRpc]
    private void PerformAttackClientRpc(int attackIndex)
    {
        PerformAttack(attackIndex);
    }

    private void PerformAttack(int attackIndex)
    {
        RotateTowardsTarget();
        playerController.anim.SetTrigger(AnimHash.Attack);
        playerController.anim.SetInteger(AnimHash.AttackNumber, attackIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PerformSkillServerRpc(int skillIndex, float cooldown)
    {
        PerformSkillClientRpc(skillIndex, cooldown);
    }

    [ClientRpc]
    private void PerformSkillClientRpc(int skillIndex, float cooldown)
    {
        PerformSkill(skillIndex, cooldown);
    }

    private void PerformSkill(int skillIndex, float cooldown)
    {
        isSkillPerforming = true;
        RotateTowardsTarget();
        playerController.anim.SetTrigger(AnimHash.Attack);
        playerController.anim.SetInteger(AnimHash.AttackNumber, skillIndex);

        // Only set attackCooldownTimer for non-ultimate skills
        if (skillIndex != 5) // Assuming skillIndex 5 corresponds to the ultimate skill
        {
            networkAttackCooldownTimer.Value = cooldown;
        }
    }

    private void RotateTowardsTarget()
    {
        if (targetedEnemy != null)
        {
            Vector3 lookDirection = new Vector3(targetedEnemy.position.x, transform.position.y, targetedEnemy.position.z);
            transform.LookAt(lookDirection);
        }
    }

    private void HandlePushback()
    {
        if (pushbackTimer > 0)
        {
            playerController.controller.Move(pushbackDirection * pushbackForce * Time.deltaTime);
            pushbackTimer -= Time.deltaTime;
        }
    }

    private void CheckNearbyEnemies()
    {
        nearbyEnemies = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
        targetedEnemy = nearbyEnemies.Length > 0 ? nearbyEnemies[0].transform : null;
    }

    private void HandleRotationStop()
    {
        if (isSkill2Performing && rotationCooldownTimer > 0)
        {
            rotationCooldownTimer -= Time.deltaTime;
            if (rotationCooldownTimer <= 0)
            {
                StopRotationSkillClientRpc();
                rotationCooldownTimer = 0;
                isSkill2Performing = false;
            }
        }
    }

    [ClientRpc]
    private void StopRotationSkillClientRpc()
    {
        playerController.anim.SetTrigger(AnimHash.RotationStop);
    }

    private void HandleUltimateStop()
    {
        if (isUltimateRunning && ultimateCooldownTimer > 0)
        {
            ultimateCooldownTimer -= Time.deltaTime;
            if (ultimateCooldownTimer <= 0)
            {
                StopUltimateClientRpc();
                isUltimateRunning = false;
            }
        }
    }

    [ClientRpc]
    private void StopUltimateClientRpc()
    {
        VFXManager.Instance.StopUltimateVFX();
    }

    public void ApplyPushback(Vector3 direction, bool halveDuration = false)
    {
        pushbackDirection = direction.normalized;
        pushbackDirection.y = 0;
        pushbackTimer = halveDuration ? pushbackDuration / 2 : pushbackDuration;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(TagHash.SWORD))
        {
            HandleSwordHitServerRpc(other.transform.position);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void HandleSwordHitServerRpc(Vector3 hitPosition)
    {
        HandleSwordHitClientRpc(hitPosition);
    }

    [ClientRpc]
    private void HandleSwordHitClientRpc(Vector3 hitPosition)
    {
        isStunned = true;
        LeanTween.delayedCall(0.5f, () => isStunned = false);
        playerController.anim.SetInteger(AnimHash.HitNumber, Random.Range(0, 3));
        playerController.anim.SetTrigger(AnimHash.Hit);
        ApplyPushback(transform.position - hitPosition);
    }

    public void EnableCollider(int vfxNumber)
    {
        swordCollider.enabled = true;
        vFXManager.SpwanSwordVFX(vfxNumber);
    }

    public void DisableCollider()
    {
        swordCollider.enabled = false;
    }

    public void ResetAttackTimer(float time)
    {
        attackCooldown = time;
    }

    public void ResetSkillAction()
    {
        isSkillPerforming = false;
    }

    public void SpwanTimerAttackVFX()
    {
        vFXManager.SpwanTimerAttackVFX();
    }
}
