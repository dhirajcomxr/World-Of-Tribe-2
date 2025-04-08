using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;

public class PlayerCameraController : NetworkBehaviour
{
    public Camera _MainCamera;
    public CinemachineFreeLook playerCamera;
    public Transform cameraTransform;

    public PlayerController playerController;


    void Start()
    {
        if(IsLocalPlayer){
            playerCamera = FindObjectOfType<CinemachineFreeLook>();        
            playerCamera.Follow = transform;
            playerCamera.LookAt = cameraTransform;
            _MainCamera = Camera.main;
            playerController.cam = _MainCamera.transform;
        }
    }
}
