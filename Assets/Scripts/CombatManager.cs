using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CombatManager : Singleton<CombatManager>
{
    // === Components ===
    [Header("Components")]
    public VFXManager vFXManager;
    public PlayerController playerController;
    public Collider swordCollider;
    public Collider rotationCollider;
    public Collider[] bodyCollider;

    // === Attack Parameters ===
    [Header("Attack Parameters")]
    public float attackResetTimer = 1.5f;
    public float attackCooldown = 1f;
    public float rotationCooldown = 8f;
    public float ultimateCooldown = 8f;
    public int[] attackSequence = { 0, 1, 2 };
    public LayerMask enemyLayer;

    // === State Flags ===
    [Header("State Flags")]
    public bool isStunned = false;

    // === Nearby Detection ===
    [Header("Detection Parameters")]
    public float detectionRadius = 5f;
    private Collider[] nearbyEnemies;
    private Transform targetedEnemy;

    // === Pushback Parameters ===
    [Header("Pushback Parameters")]
    public float pushbackForce = 5f;
    public float pushbackDuration = 0.2f;

    private Vector3 pushbackDirection;
    private float pushbackTimer = 0f;

    // === Internal Variables ===
    public float attackCooldownTimer = 0f;
    private int currentAttackIndex = 0;
    private float rotationCooldownTimer = 0f;
    private float ultimateCooldownTimer = 0f;

    // === Skill States ===
    public bool isSkillPerforming = false;
    public bool isSkill2Performing = false;
    public bool isUltimateRunning = false;

    // === Multiplayer Variables ===
    [SerializeField] private NetworkVariable<float> networkAttackCooldownTimer = new NetworkVariable<float>();
    [SerializeField] private NetworkVariable<bool> networkMeleAttack = new NetworkVariable<bool>();
    [SerializeField] private NetworkVariable<bool> networkSkill_1 = new NetworkVariable<bool>();
    [SerializeField] private NetworkVariable<bool> networkSkill_2 = new NetworkVariable<bool>();
    [SerializeField] private NetworkVariable<bool> networkSkill_3 = new NetworkVariable<bool>();

    // Client-side input tracking
    private bool oldMeleAttack;
    private bool oldSkill_1;
    private bool oldSkill_2;
    private bool oldSkill_3;

    // Cache frequently used values
    private WaitForSeconds stunDelay;
    private const float ultimateAttackSpeedMultiplier = 1.5f;

    private void Awake()
    {
        // Cache reusable WaitForSeconds to avoid garbage collection
        stunDelay = new WaitForSeconds(0.5f);
    }

    private void Update()
    {
        // Only update on the server or client as appropriate
        if (IsServer)
        {
            UpdateServer();
        }

        // Handle input only for the local player
        if (IsOwner && IsClient)
        {
            HandleClientInput();
        }

        // Sync the attack cooldown timer from network
        attackCooldownTimer = networkAttackCooldownTimer.Value;

        // Update combat systems
        CheckNearbyEnemies();
        HandlePushback();
        
        // Only check skill timers if skills are active
        if (isSkill2Performing)
        {
            HandleRotationStop();
        }
        
        if (isUltimateRunning)
        {
            HandleUltimateStop();
        }
    }

    /// <summary>
    /// Server-side update loop
    /// </summary>
    private void UpdateServer()
    {
        // Update attack cooldown and reset sequence if needed
        if (networkAttackCooldownTimer.Value > 0)
        {
            networkAttackCooldownTimer.Value -= Time.deltaTime;
            if (networkAttackCooldownTimer.Value <= 0)
            {
                currentAttackIndex = 0;
            }
        }

        // Process combat inputs
        HandleAttack(networkMeleAttack.Value, networkSkill_1.Value, networkSkill_2.Value, networkSkill_3.Value);
    }

    /// <summary>
    /// Handles client input and sends to server
    /// </summary>
    private void HandleClientInput()
    {
        // Get current input states
        bool localMeleAttack = InputAction.Instance.meleAttack;
        bool localSkill1 = InputAction.Instance.skill_1;
        bool localSkill2 = InputAction.Instance.skill_2;
        bool localSkill3 = InputAction.Instance.skill_3;

        // Only send RPC if input state changed
        if (localMeleAttack != oldMeleAttack || 
            localSkill1 != oldSkill_1 || 
            localSkill2 != oldSkill_2 || 
            localSkill3 != oldSkill_3)
        {
            oldMeleAttack = localMeleAttack;
            oldSkill_1 = localSkill1;
            oldSkill_2 = localSkill2;
            oldSkill_3 = localSkill3;

            UpdateCombatInputServerRpc(localMeleAttack, localSkill1, localSkill2, localSkill3);
        }
    }

    /// <summary>
    /// Processes all combat actions
    /// </summary>
    private void HandleAttack(bool meleAttack, bool skill1, bool skill2, bool skill3)
    {
        // Handle melee attack
        if (meleAttack && !networkSkill_1.Value && networkAttackCooldownTimer.Value <= 0.5f)
        {
            PerformAttackServerRpc(currentAttackIndex);
            currentAttackIndex = (currentAttackIndex + 1) % attackSequence.Length;
            networkAttackCooldownTimer.Value = isUltimateRunning ? 
                attackCooldown / ultimateAttackSpeedMultiplier : 
                attackCooldown;
            InputAction.Instance.meleAttack = false;
        }

        // Handle skill 1
        if (skill1 && !isSkillPerforming && !isSkill2Performing && !isUltimateRunning)
        {
            PerformSkillServerRpc(3, 3f);
            InputAction.Instance.skill_1 = false;
        }

        // Handle skill 2 (rotation)
        if (skill2 && !isSkillPerforming && !isSkill2Performing && !isUltimateRunning)
        {
            PerformSkillServerRpc(4, rotationCooldown);
            isSkill2Performing = true;
            rotationCooldownTimer = rotationCooldown;
            InputAction.Instance.skill_2 = false;
        }

        // Handle ultimate skill
        if (skill3 && !isSkillPerforming && !isSkill2Performing && !isUltimateRunning)
        {
            PerformSkillServerRpc(5, ultimateCooldown);
            isUltimateRunning = true;
            ultimateCooldownTimer = ultimateCooldown;
            InputAction.Instance.skill_3 = false;
        }
    }

    #region Network RPC Methods

    [ServerRpc(RequireOwnership = false)]
    private void UpdateCombatInputServerRpc(bool meleAttack, bool skill1, bool skill2, bool skill3)
    {
        networkMeleAttack.Value = meleAttack;
        networkSkill_1.Value = skill1;
        networkSkill_2.Value = skill2;
        networkSkill_3.Value = skill3;
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

    [ClientRpc]
    private void StopRotationSkillClientRpc()
    {
        playerController.anim.SetTrigger(AnimHash.RotationStop);
    }

    [ClientRpc]
    private void StopUltimateClientRpc()
    {
        VFXManager.Instance.StopUltimateVFX();
    }

    [ServerRpc(RequireOwnership = false)]
    private void HandleSwordHitServerRpc(Vector3 hitPosition)
    {
        HandleSwordHitClientRpc(hitPosition);
    }

    [ClientRpc]
    private void HandleSwordHitClientRpc(Vector3 hitPosition)
    {
        StartCoroutine(ProcessHit(hitPosition));
    }

    #endregion

    #region Combat Actions

    /// <summary>
    /// Executes a melee attack with the given index
    /// </summary>
    private void PerformAttack(int attackIndex)
    {
        RotateTowardsTarget();
        playerController.anim.SetTrigger(AnimHash.Attack);
        playerController.anim.SetInteger(AnimHash.AttackNumber, attackIndex);
    }

    /// <summary>
    /// Executes a skill with the given index
    /// </summary>
    private void PerformSkill(int skillIndex, float cooldown)
    {
        isSkillPerforming = true;
        RotateTowardsTarget();
        playerController.anim.SetTrigger(AnimHash.Attack);
        playerController.anim.SetInteger(AnimHash.AttackNumber, skillIndex);

        if (skillIndex != 5) // Skip cooldown for ultimate skill
        {
            networkAttackCooldownTimer.Value = cooldown;
        }
    }

    /// <summary>
    /// Rotates player towards the current target
    /// </summary>
    private void RotateTowardsTarget()
    {
        if (targetedEnemy != null)
        {
            Vector3 lookDirection = new Vector3(
                targetedEnemy.position.x, 
                transform.position.y, 
                targetedEnemy.position.z
            );
            transform.LookAt(lookDirection);
        }
    }

    /// <summary>
    /// Applies pushback effect to the player
    /// </summary>
    public void ApplyPushback(Vector3 direction, bool halveDuration = false)
    {
        pushbackDirection = direction.normalized;
        pushbackDirection.y = 0;
        pushbackTimer = halveDuration ? pushbackDuration / 2 : pushbackDuration;
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Handles pushback movement if active
    /// </summary>
    private void HandlePushback()
    {
        if (pushbackTimer > 0)
        {
            playerController.controller.Move(pushbackDirection * pushbackForce * Time.deltaTime);
            pushbackTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Checks for nearby enemies and updates target
    /// </summary>
    private void CheckNearbyEnemies()
    {
        nearbyEnemies = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
        targetedEnemy = nearbyEnemies.Length > 0 ? nearbyEnemies[0].transform : null;
    }

    /// <summary>
    /// Handles rotation skill cooldown and stop
    /// </summary>
    private void HandleRotationStop()
    {
        rotationCooldownTimer -= Time.deltaTime;
        if (rotationCooldownTimer <= 0)
        {
            StopRotationSkillClientRpc();
            rotationCooldownTimer = 0;
            isSkill2Performing = false;
        }
    }

    /// <summary>
    /// Handles ultimate skill cooldown and stop
    /// </summary>
    private void HandleUltimateStop()
    {
        ultimateCooldownTimer -= Time.deltaTime;
        if (ultimateCooldownTimer <= 0)
        {
            StopUltimateClientRpc();
            isUltimateRunning = false;
        }
    }

    /// <summary>
    /// Processes hit reaction and pushback
    /// </summary>
    private IEnumerator ProcessHit(Vector3 hitPosition)
    {
        isStunned = true;
        playerController.anim.SetInteger(AnimHash.HitNumber, Random.Range(0, 3));
        playerController.anim.SetTrigger(AnimHash.Hit);
        ApplyPushback(transform.position - hitPosition);

        yield return stunDelay;
        isStunned = false;
    }

    #endregion

    #region Collider and VFX Control

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(TagHash.SWORD))
        {
            HandleSwordHitServerRpc(other.transform.position);
        }
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

    public void SpwanTimerAttackVFX()
    {
        vFXManager.SpwanTimerAttackVFX();
    }

    #endregion

    #region State Management

    public void ResetAttackTimer(float time)
    {
        attackCooldown = time;
    }

    public void ResetSkillAction()
    {
        isSkillPerforming = false;
    }

    #endregion
}