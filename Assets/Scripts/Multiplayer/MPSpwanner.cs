using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class MPSpwanner : SimulationBehaviour, IPlayerJoined
{
     public GameObject _PlayerPrefab;    
    public void PlayerJoined(PlayerRef player)
    {        
        if (player == Runner.LocalPlayer)
        {
            // _SpwanPosition[player.PlayerId - 1].spwanPosition.position
            Runner.Spawn(_PlayerPrefab, transform.position, Quaternion.identity);            
        }
    }
}
