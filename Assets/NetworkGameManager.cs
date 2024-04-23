using FishNet.Object;
using FishNet.Connection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkGameManager : NetworkBehaviour
{
    [SerializeField] private int round = 0;
    bool client = false;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (base.IsOwner)
        {
            client = true;
        }
    }

    private void Update()
    {
        if (!client) return;
    }

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
