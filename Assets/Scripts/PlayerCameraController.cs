using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cinemachine;
using Fusion;

public class PlayerCameraController : NetworkBehaviour
{
    public Camera _MainCamera;
    public CinemachineFreeLook playerCamera;
    public Transform cameraTransform;

    public PlayerController playerController;


    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            playerCamera = FindObjectOfType<CinemachineFreeLook>();
            playerCamera.Follow = transform;
            playerCamera.LookAt = cameraTransform;
            _MainCamera = Camera.main;
            playerController.cam = _MainCamera.transform;
        }
    }
}
