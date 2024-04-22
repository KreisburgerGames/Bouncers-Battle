using FishNet.Object;
using FishNet.Connection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkGameManager : NetworkBehaviour
{
    [SerializeField] private int round = 0;

    [ServerRpc(RequireOwnership = false)]
    public void ServerSetRound(int setRound, NetworkGameManager networkGameManager)
    {
        SetRound(setRound, networkGameManager);
    }

    [ObserversRpc]
    private void SetRound(int setRound, NetworkGameManager networkGameManager)
    {
        networkGameManager.round = setRound;
    }
}
