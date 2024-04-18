using FishNet.Managing.Scened;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using UnityEngine;
using System.Linq;
using Steamworks;
using UnityEngine.EventSystems;

public class BootstrapNetworkManager : NetworkBehaviour
{
    private static BootstrapNetworkManager instance;

    private void Start()
    {
        SteamAPI.Init();
    }

    private void Awake()
    {
        instance = this;
    }

    [Rpc]
    public static void ChangeNetworkScene(string sceneName, string[] scenesToClose)
    {
        instance.CloseScenes(scenesToClose);

        SceneLoadData sld = new SceneLoadData(sceneName);
        NetworkConnection[] conns = instance.ServerManager.Clients.Values.ToArray();
        instance.SceneManager.LoadConnectionScenes(conns, sld);
    }

    [ServerRpc(RequireOwnership = false)]
    void CloseScenes(string[] scenesToClose)
    {
        CloseScenesObserver(scenesToClose);
    }

    [ObserversRpc]
    void CloseScenesObserver(string[] scenesToClose)
    {
        foreach (var scene in scenesToClose)
        {
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
        }
    }
    [Rpc]
    public static void ShowAllPlayers()
    {
        foreach(PlayerMovement player in GameObject.FindObjectsOfType<PlayerMovement>())
        {
            player.gameObject.SetActive(true);
        }
    }
}
