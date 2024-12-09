using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : Singleton<CombatManager>
{
    // Components
    [Header("Components")]
    public PlayerController playerController;
    public Collider swordCollider;
    public Collider RotationCollider;

    // Attack Parameters
    [Header("Attack Parameters")]
    public float attackResetTimer = 1.5f;
    public float attackCooldown = 1f;
    public float rotationCooldown = 8f;
    public float ultimateCooldown = 8f;
    public int[] attackSequence = { 0, 1, 2 };
    public LayerMask enemyLayer;

    // State Flags
    [Header("State Flags")]
    public bool isStunned = false;

    // Nearby Detection
    [Header("Detection Parameters")]
    public float detectionRadius = 5f;
    private Collider[] nearbyEnemies;
    private Transform targetedEnemy;

    // Pushback Parameters
    [Header("Pushback Parameters")]
    public float pushbackForce = 5f;
    public float pushbackDuration = 0.2f;

    private Vector3 pushbackDirection;
    private float pushbackTimer = 0f;

    // Internal Variables
    public float attackCooldownTimer = 0f;
    private int currentAttackIndex = 0;
    private float rotationCooldownTimer = 0f;
    private float ultimateCooldownTimer = 0f;

    // Skill undergoing
    public bool isSkillPerforming = false;
    public bool isSkill2Performing = false;
    public bool isUltimateRunning = false;


    private void Update()
    {
        CheckNearbyEnemies();
        HandleAttack();
        HandlePushback();
        RotationStop();
        UltimateStop();
    }


    private void CheckNearbyEnemies()
    {
        nearbyEnemies = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
        targetedEnemy = nearbyEnemies.Length > 0 ? nearbyEnemies[0].transform : null;
    }

    private void RotationStop()
    {
        if (isSkill2Performing && rotationCooldownTimer>0)
        {
            rotationCooldownTimer -= Time.deltaTime;
            if (rotationCooldownTimer <= 0)
            {
                playerController.anim.SetTrigger(AnimHash.RotationStop);
                rotationCooldownTimer = 0;
                isSkillPerforming = true;
                isSkill2Performing = false;
            }
        }
    }

    private void UltimateStop()
    {
        if (isUltimateRunning && ultimateCooldownTimer>0)
        {
            ultimateCooldownTimer -= Time.deltaTime;
            if (ultimateCooldownTimer <= 0)
            {
                rotationCooldownTimer = 0;
                VFXManager.Instance.StopUltimateVFX();
                isUltimateRunning = false;
            }
        }
    }

    private void HandleAttack()
    {
        // Decrease cooldown timer
        if (attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
            if (attackCooldownTimer <= 0)
            {
                currentAttackIndex = 0;
            }
        }

        // Perform attack
        if (InputAction.Instance.meleAttack && !isSkillPerforming)
        {
            if (attackCooldownTimer <= 0.5f)
            {
                RotateTowardsTarget();
                playerController.anim.SetTrigger(AnimHash.Attack);
                playerController.anim.SetInteger(AnimHash.AttackNumber, attackSequence[currentAttackIndex]);

                currentAttackIndex = (currentAttackIndex + 1) % attackSequence.Length;
                if (isUltimateRunning) attackCooldownTimer = attackCooldown / (1.5f); else attackCooldownTimer = attackCooldown;
            }
            InputAction.Instance.meleAttack = false;
        }

        if (InputAction.Instance.skill_1 && !isSkillPerforming)
        {
            if (attackCooldownTimer <= 0.5f)
            {
                isSkillPerforming = true;
                RotateTowardsTarget();
                playerController.anim.SetTrigger(AnimHash.Attack);
                playerController.anim.SetInteger(AnimHash.AttackNumber, 3);

                attackCooldownTimer = 3;
            }
            InputAction.Instance.skill_1 = false;
        }

        if (InputAction.Instance.skill_2 && !isSkill2Performing && !isSkillPerforming)
        {
            if (attackCooldownTimer <= 0.5f)
            {
                isSkill2Performing = true;
                isSkillPerforming = true;
                RotateTowardsTarget();
                playerController.anim.SetTrigger(AnimHash.Attack);
                playerController.anim.SetInteger(AnimHash.AttackNumber, 4);
                rotationCooldownTimer = rotationCooldown;                
            }
            InputAction.Instance.skill_2 = false;
        }

        if (InputAction.Instance.skill_3 && !isSkillPerforming && !isUltimateRunning)
        {
            if (attackCooldownTimer <= 0.5f)
            {
                isSkillPerforming = true;
                isUltimateRunning = true;
                RotateTowardsTarget();
                playerController.anim.SetTrigger(AnimHash.Attack);
                playerController.anim.SetInteger(AnimHash.AttackNumber, 5);
                ultimateCooldownTimer = ultimateCooldown;                
            }
            InputAction.Instance.skill_3 = false;
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
            HandleSwordHit(other);
        }
        else if (other.CompareTag(TagHash.ENEMY))
        {
            HandleEnemyHit(other);
        }
    }

    private void HandleSwordHit(Collider other)
    {
        isStunned = true;
        LeanTween.delayedCall(0.5f, () => isStunned = false);
        playerController.anim.SetInteger(AnimHash.HitNumber, Random.Range(0, 3));
        playerController.anim.SetTrigger(AnimHash.Hit);
        ApplyPushback(transform.position - other.transform.position);
    }

    private int vfxNumber;
    private void HandleEnemyHit(Collider other)
    {
        Debug.Log("Enemy Hit");
        VFXManager.Instance.SpwanHitEffect(vfxNumber);
    }

    public void EnableCollider(int vfxNumber)
    {
        swordCollider.enabled = true;
        VFXManager.Instance.SpwanSwordVFX(vfxNumber);
        this.vfxNumber = vfxNumber;
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
}