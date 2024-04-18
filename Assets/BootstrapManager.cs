using FishNet.Managing;
using FishNet.Managing.Server;
using FishNet.Object;
using FishNet.Transporting;
using FishySteamworks;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class BootstrapManager : MonoBehaviour
{

    private static BootstrapManager instance;
    public static GameObject leaveParticle;
    private static PlayerMovement myPlayer;

    private void Awake()
    {
        instance = this;
    }

    private void OnApplicationQuit()
    {
        LeaveLobby();
    }

    [SerializeField] private string menuName = "Menu";
    [SerializeField] public NetworkManager networkManager;
    [SerializeField] public FishySteamworks.FishySteamworks fishySteamworks;

    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    protected Callback<LobbyEnter_t> LobbyEntered;

    public static ulong CurrentLobbyID;


    private void Start()
    {
        SteamAPI.Init();
        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(menuName, LoadSceneMode.Additive);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if(callback.m_eResult != EResult.k_EResultOK)
        {
            return;
        }
        CurrentLobbyID = callback.m_ulSteamIDLobby;
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), "HostAddress", SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), "Name", SteamFriends.GetPersonaName().ToString() + "'s Lobby");
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), "Started", "false");
        fishySteamworks.SetClientAddress(SteamUser.GetSteamID().ToString());
        fishySteamworks.StartConnection(true);
    }
    
    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        CurrentLobbyID = callback.m_ulSteamIDLobby;

        fishySteamworks.SetClientAddress(SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "HostAddress"));
        fishySteamworks.StartConnection(false);

        if (networkManager.IsServer)
        {
            MainMenuManager.LobbyEntered(SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "Name"), true);
        }
        else
        {
            MainMenuManager.LobbyEntered(SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "Name"), false);
        }
    }

    public static void CreateLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 4);
    }

    public static void HostLeave()
    {
        instance.fishySteamworks.StopConnection(false);
        SaveLeave();
    }

    public static void JoinByID(CSteamID steamID)
    {
        if(SteamMatchmaking.RequestLobbyData(steamID))
        {
            SteamMatchmaking.JoinLobby(steamID);
        }
    }
    
    public static void LeaveLobby()
    {
        foreach (PlayerMovement player in GameObject.FindObjectsOfType<PlayerMovement>())
        {
            if (player.client == true)
            {
                myPlayer = player;
            }
        }
        
        if(SteamMatchmaking.GetLobbyData(new CSteamID(BootstrapManager.CurrentLobbyID), "Started") == "true")
        {
            myPlayer.LeaveLobby();
        }

        instance.fishySteamworks.StopConnection(false);
        if (instance.networkManager.IsServer)
        {
            SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), "HostAddress", null);
            instance.fishySteamworks.StopConnection(true);
        }

        SaveLeave();
    }

    public static void SaveLeave()
    {
        instance.fishySteamworks.SetClientAddress("");
        SteamMatchmaking.LeaveLobby(new CSteamID(CurrentLobbyID));
        if (SteamMatchmaking.GetLobbyData(new CSteamID(BootstrapManager.CurrentLobbyID), "Started") == "true")
        {
            CurrentLobbyID = 0;
            SceneManager.LoadScene(instance.menuName, LoadSceneMode.Additive);
            SceneManager.UnloadSceneAsync("Game");
        }
        else
        {
            CurrentLobbyID = 0;
        }
    }
}
