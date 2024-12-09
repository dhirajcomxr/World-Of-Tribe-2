using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Singleton<PlayerController>
{
    // Components
    [Header("Components")]
    public Animator anim;
    public CharacterController controller;
    public Transform cam;

    // Movement Parameters
    [Header("Movement Parameters")]
    public float speed;
    public float speedBoostPercentage;
    public float turnSmoothDamp;
    public float gravityMultiplier;
    public float jumpForce;

    // Private Variables
    private float _turnSmoothVelocity;
    private float _gravity = -9.8f;
    private Vector3 _velocity;
    private Vector3 _moveDir;

    // State Flags
    [Header("State Flags")]
    public bool isDead;
    public bool isGrounded;
    public bool shouldRotate = true; // Controls whether the character rotates toward the move direction

    // Methods

    private void Update()
    {
        if (isDead) return;

        HandleCharacterMovement();
        HandleRotationAndMovement();
        ApplyGravity();
        HandleJump();
    }

    private float currentVelocity; // Tracks the smoothed velocity
    private float velocityDamp;    // Reference velocity for SmoothDamp
    public float smoothTime = 0.3f; // Time to smooth the velocity changes

    private float animationSpeedModification;

    private void HandleCharacterMovement()
    {
        Vector2 moveInput = InputAction.Instance._moveAction;
        float inputMagnitude = moveInput.magnitude;

        // Determine the target velocity based on conditions
        float targetVelocity = (!isGrounded || CombatManager.Instance.attackCooldownTimer > 0.1f)
            ? (CombatManager.Instance.isUltimateRunning ? inputMagnitude * speedBoostPercentage : inputMagnitude / 2)
            : inputMagnitude;
        animationSpeedModification = (CombatManager.Instance.isUltimateRunning) ? 1.5f : 1f;
        // Smoothly transition to the target velocity
        currentVelocity = Mathf.SmoothDamp(currentVelocity, targetVelocity, ref velocityDamp, smoothTime);

        // Update the animator parameter with the smoothed velocity
        anim.SetFloat(AnimHash.Velocity, currentVelocity);
        anim.SetFloat(AnimHash.AttackSpeedModification, animationSpeedModification);
    }

    private void HandleRotationAndMovement()
    {
        Vector2 moveInput = InputAction.Instance._moveAction;

        if (moveInput.sqrMagnitude > 0.01f && !CombatManager.Instance.isSkillPerforming) // Check if there's significant input
        {
            float targetAngle = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg + cam.eulerAngles.y;

            if (shouldRotate)
            {
                float smoothedAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, turnSmoothDamp);
                transform.rotation = Quaternion.Euler(0, smoothedAngle, 0);
            }

            _moveDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;

            float currentSpeed = (!isGrounded || CombatManager.Instance.attackCooldownTimer > 0 || CombatManager.Instance.isStunned)
      ? speed / 2
      : (CombatManager.Instance.isUltimateRunning ? speed * speedBoostPercentage : speed);

            controller.Move(_moveDir.normalized * currentSpeed * Time.deltaTime);
        }
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && _velocity.y < 0f)
        {
            _velocity.y = -1f;
        }
        else
        {
            _velocity.y += _gravity * gravityMultiplier * Time.deltaTime;
        }

        controller.Move(_velocity * Time.deltaTime);
    }

    private void HandleJump()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && InputAction.Instance._jump)
        {
            _velocity.y = jumpForce;
            anim.SetTrigger(AnimHash.jump);
            InputAction.Instance._jump = false;
        }
    }
}
