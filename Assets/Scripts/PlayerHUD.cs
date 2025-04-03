using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;


public class PlayerHUD : NetworkBehaviour
{
    private NetworkVariable<NetworkString> playerName = new NetworkVariable<NetworkString>();
    
    private NetworkVariable<int> playerLayerMask = new NetworkVariable<int>();
    private NetworkVariable<int> enemyLayerMask = new NetworkVariable<int>();

    private bool overlaySet = false;

    public TextMeshPro playerText;
    public CombatManager combatManager;
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            playerName.Value = $"Player : {OwnerClientId}";
        }
    }
    public void SetOverlay()
    {
        playerText.text = playerName.Value;
        this.gameObject.name = playerName.Value;
    }
    private void Update()
    {
        if(!overlaySet && !string.IsNullOrEmpty(playerName.Value))
        {
            SetOverlay();
            overlaySet = true;
        }
    }

}


