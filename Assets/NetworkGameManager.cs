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
    bool counting = false;

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
        // Local Code
        if (counting)
        {
            roundTime += Time.deltaTime;
        }
        if (!client) return;
        // Server Code
    }

    [ServerRpc]
    private void ServerSetCounting(NetworkGameManager gameManager, bool newIsCounting, float currentRoundTime)
    {
        SetCounting(gameManager, newIsCounting, currentRoundTime);
    }

    [ObserversRpc]
    private void SetCounting(NetworkGameManager gameManager, bool newIsCounting, float currentRoundTime)
    {
        gameManager.counting = newIsCounting;
        gameManager.roundTime = currentRoundTime;
    }

    [ServerRpc]
    private void ServerUpdateRound(NetworkGameManager gameManager, int newRound)
    {
        UpdateRound(gameManager, newRound);
    }

    [ObserversRpc]
    private void UpdateRound(NetworkGameManager gameManager, int newRound) 
    {
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
