using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerManager : MonoBehaviour
{
    public CinemachineFreeLook cinemachineFree;
    public PlayerController controller;
    public Transform rotationTarget;

    private void OnEnable()
    {
        CinemachineFreeLook localCinemachine =  Instantiate(cinemachineFree.gameObject).GetComponent<CinemachineFreeLook>();
        localCinemachine.m_Follow = this.transform;
        localCinemachine.m_LookAt = rotationTarget;
        controller.cam = FindObjectOfType<CinemachineBrain>().transform;

    }
}
