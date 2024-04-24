using FishNet.Object;
using FishNet.Connection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkGameManager : NetworkBehaviour
{
    [SerializeField] private int round = 0;
    public float roundTime = 0f;
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
        roundTime += Time.deltaTime;
        ServerUpdateVaribles(this, roundTime, round);
    }

    [ServerRpc]
    private void ServerUpdateVaribles(NetworkGameManager gameManager, float rt, int newRound)
    {
        UpdateVaribles(gameManager, rt, newRound);
    }

    [ObserversRpc]
    private void UpdateVaribles(NetworkGameManager gameManager, float rt, int newRound) 
    {
        gameManager.roundTime = rt;
        gameManager.round = newRound;
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
