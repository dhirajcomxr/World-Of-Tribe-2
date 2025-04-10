using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

[System.Serializable]
public struct PlayerData
{
    public TextMeshProUGUI playerNameText;
}


public class PlayerHUD : NetworkBehaviour
{
    [Networked] private string playerName {get; set;}
    [Networked] private int playerLayerMask{ get; set;}
    [Networked] private int enemyLayerMask{ get; set;}

    private bool overlaySet = false;

    public PlayerData playerData;
    public TextMeshPro playerText;
    public CombatManager combatManager;

    void Start()
    {
        if(Runner.IsServer){
            playerData = GameObject.Find("Local Player").GetComponent<PlayerHealthData>().playerData;
        }
    }

     public override void Spawned()
    {
        // Use this instead of Start / Awake for NetworkObjects
         if (Runner.IsServer)
        {
            playerName = $"Player : {Runner.UserId}";
        }
    }
    // public override void OnNetworkSpawn()
    // {
    //     if (IsServer)
    //     {
    //         playerName = $"Player : {OwnerClientId}";
    //     }
    // }
    public void SetOverlay()
    {
        playerData.playerNameText.text = playerName;
        this.gameObject.name = playerName;
    }
    private void Update()
    {
        if (!overlaySet && !string.IsNullOrEmpty(playerName))
        {
            SetOverlay();
            overlaySet = true;
        }
    }

}


