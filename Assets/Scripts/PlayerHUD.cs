using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

[System.Serializable]
public struct PlayerData
{
    public TextMeshProUGUI playerNameText;
}


public class PlayerHUD : NetworkBehaviour
{
    private NetworkVariable<NetworkString> playerName = new NetworkVariable<NetworkString>();

    private NetworkVariable<int> playerLayerMask = new NetworkVariable<int>();
    private NetworkVariable<int> enemyLayerMask = new NetworkVariable<int>();

    private bool overlaySet = false;

    public PlayerData playerData;
    public TextMeshPro playerText;
    public CombatManager combatManager;

    void Start()
    {
        if(IsLocalPlayer){
            playerData = GameObject.Find("Local Player").GetComponent<PlayerHealthData>().playerData;
        }
    }


    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            playerName.Value = $"Player : {OwnerClientId}";
        }
    }
    public void SetOverlay()
    {
        playerData.playerNameText.text = playerName.Value;
        this.gameObject.name = playerName.Value;
    }
    private void Update()
    {
        if (!overlaySet && !string.IsNullOrEmpty(playerName.Value))
        {
            SetOverlay();
            overlaySet = true;
        }
    }

}


