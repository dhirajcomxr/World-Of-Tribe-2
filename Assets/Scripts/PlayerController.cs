using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{

    // Components
    [Header("Components")]
    public Animator anim;
    public CharacterController controller;
    public Transform cam;

    // Movement Parameters
    [Header("Movement Parameters")]
    [SerializeField] private float speed;
    [SerializeField] private float speedBoostPercentage;
    [SerializeField] private float turnSmoothDamp;
    [SerializeField] private float gravityMultiplier;
    [SerializeField] private float jumpForce;

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

    //Multiplayer Variables
    [SerializeField] Vector2 defaultPositionRange = new Vector2(-4, 4);

    //Host
    [SerializeField] NetworkVariable<Vector3> networkPlayerInput = new NetworkVariable<Vector3>();


    //Client
    private Vector3 oldPlayerInput;



    private void Start()
    {
        if (IsOwner && IsClient)
        {
            transform.position = new Vector3(
            Random.Range(defaultPositionRange.x, defaultPositionRange.y),
            0,
            Random.Range(defaultPositionRange.x, defaultPositionRange.y)
            );
        }
    }


    private void UpdateServer()
    {
        // Only process movement for non-owner players on server
        // Owner movement is handled through ClientInput -> RPC
        if (!IsOwner)
        {
            HandleRotationAndMovement(networkPlayerInput.Value);
            HandleCharacterMovement(networkPlayerInput.Value);
        }


        // HandleRotationAndMovement(networkPlayerInput.Value);
        // HandleCharacterMovement(networkPlayerInput.Value);
        //transform.position = new Vector3(transform.position.x + leftRightPosition.Value, transform.position.y, transform.position.z + forwardBackPosition.Value);
    }

    private void ClientInput()
    {

         Vector3 thisPlayerInput = InputAction.Instance._moveAction;
        
        if (oldPlayerInput != thisPlayerInput)
        {
            oldPlayerInput = thisPlayerInput;
            
            // Update Server with new input
            UpdateClientInputToServerRpc(thisPlayerInput);
            
            // Immediate local response (client-side prediction)
            if (IsClient)
            {
                HandleRotationAndMovement(thisPlayerInput);
                HandleCharacterMovement(thisPlayerInput);
            }
        }

        // Vector3 thisPlayerInput = InputAction.Instance._moveAction;

        // if (oldPlayerInput != thisPlayerInput)
        // {
        //     oldPlayerInput = thisPlayerInput;

        //     //Update Server            
        //     UpdateClientInputToServerRpc(thisPlayerInput);
        // }

    }



    // Methods

    private void Update()
    {

        if (IsServer)
        {
            UpdateServer();
        }

        // Only owner handles input and client-side prediction
        if (IsOwner)
        {
            ClientInput();

            // Client-side prediction (immediate response)
            if (IsClient)
            {
                HandleRotationAndMovement(oldPlayerInput);
                HandleCharacterMovement(oldPlayerInput);
            }
        }
        else if (IsClient)
        {
            // Non-owner clients use server-authoritative movement
            HandleRotationAndMovement(networkPlayerInput.Value);
            HandleCharacterMovement(networkPlayerInput.Value);
        }


        // if (IsServer)
        // {
        //     UpdateServer();
        // }
        // if (IsClient && IsOwner)
        // {
        //     ClientInput();
        // }

        // //if (!IsOwner|| isDead) return;

        // HandleRotationAndMovement(networkPlayerInput.Value);
        // HandleCharacterMovement(networkPlayerInput.Value);        
        // //ApplyGravity();
        // //HandleJump();

    }

    

    private float currentVelocity; // Tracks the smoothed velocity
    private float velocityDamp;    // Reference velocity for SmoothDamp
    public float smoothTime = 0.3f; // Time to smooth the velocity changes

    private float animationSpeedModification;
    [SerializeField] float currentSpeed;
    [SerializeField] float targetVelocity;
    private void HandleCharacterMovement(Vector3 _moveAction)
    {
        Vector2 moveInput = _moveAction;
        float inputMagnitude = moveInput.magnitude;

        // Determine the target velocity based on conditions
        if (!isGrounded || CombatManager.Instance.attackCooldownTimer > 0.1f)
        {
            targetVelocity = inputMagnitude / 2;
        }
        else
        {
            if (CombatManager.Instance.isUltimateRunning)
            {
                targetVelocity = inputMagnitude * speedBoostPercentage;
            }
            else
            {
                targetVelocity = inputMagnitude;
            }

        }

        if (CombatManager.Instance.isUltimateRunning)
        {
            animationSpeedModification = 1.5f;
        }
        else
        {
            animationSpeedModification = 1;
        }

        // Smoothly transition to the target velocity
        currentVelocity = Mathf.SmoothDamp(currentVelocity, targetVelocity, ref velocityDamp, smoothTime);

        // Update the animator parameter with the smoothed velocity
        anim.SetFloat(AnimHash.Velocity, currentVelocity);
        anim.SetFloat(AnimHash.AttackSpeedModification, animationSpeedModification);

    }

    private void HandleRotationAndMovement(Vector2 moveInput)
    {

        if (moveInput.sqrMagnitude > 0.01f && !CombatManager.Instance.isSkillPerforming) // Check if there's significant input
        {
            float targetAngle = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg /*+ cam.eulerAngles.y*/;

            if (shouldRotate)
            {
                float smoothedAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, turnSmoothDamp);
                transform.rotation = Quaternion.Euler(0, smoothedAngle, 0);
            }

            _moveDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;

            Debug.Log($"ERROR {CombatManager.Instance.gameObject.name} : {CombatManager.Instance.isUltimateRunning}");

            if (!isGrounded || CombatManager.Instance.attackCooldownTimer > 0.1f || CombatManager.Instance.isStunned)
            {
                currentSpeed = speed / 2;
            }
            else
            {
                if (CombatManager.Instance.isUltimateRunning)
                {
                    currentSpeed = speed * speedBoostPercentage;
                }
                else
                {
                    currentSpeed = speed;
                }
            }

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


    [ServerRpc]
    public void UpdateClientInputToServerRpc(Vector3 input)
    {
        networkPlayerInput.Value = input;
    }
}
