using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostDisconnectManager : MonoBehaviour
{
    private void Update()
    {
        try
        {
            if (SteamMatchmaking.GetLobbyData(new CSteamID(BootstrapManager.CurrentLobbyID), "HostAddress")  == null || GameObject.FindFirstObjectByType<PlayerSpawner>() == null)
            {
                BootstrapManager.HostLeave();
            }
        }
        catch
        {
            BootstrapManager.HostLeave();
        }
    }
}
