using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerManager : NetworkBehaviour
{
   public CombatManager combatManager;
   public LayerMask LocalPlayerLayer;
   public LayerMask ServerPlayerLayer;

    void Start()
    {
        if(IsLocalPlayer){
            combatManager.enemyLayer = LocalPlayerLayer;
            this.gameObject.layer = 3;
            Debug.Log("Local Player Layer : " + this.gameObject.layer);
        }else{
            combatManager.enemyLayer = ServerPlayerLayer;
            this.gameObject.layer = 6;
            Debug.Log("Server Player Layer : " + this.gameObject.layer);
        }
    }
}
