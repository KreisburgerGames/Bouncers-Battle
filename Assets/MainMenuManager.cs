using FishNet.Object;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    private static MainMenuManager instance;

    [SerializeField] private GameObject menuScreen;
    [SerializeField] private GameObject lobbyScreen;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private TMP_InputField lobbyInput;
    [SerializeField] private TMP_Text lobbyTitle, lobbyIDText;
    [SerializeField] private Button startGameButton;
    [SerializeField] private TMP_Text toggleReadyText;
    private PlayerSpawner localPlayer;
    bool canFindPlayer = true;
    public float findPlayerLeaveBuffer = 1f;
    float findPlayerLeaveBufferTimer = 0f;
    bool localPlayerFound = false;

    private void Awake()
    {
        instance = this;
    }
    private void TryFindLocalPlayer()
    {
        foreach (PlayerSpawner player in FindObjectsOfType<PlayerSpawner>())
        {
            if (player.playerSteamID == (ulong)SteamUser.GetSteamID())
            {
                localPlayer = player;
                localPlayerFound = true;
            }
        }
    }

    private void Update()
    {
        if(BootstrapManager.joinedByID)
        {
            BootstrapManager.joinedByID = false;
            CloseAllScreens();
            OpenLobbyScreen();
        }
        if (BootstrapManager.failedJoinByID)
        {
            CloseAllScreens();
            OpenMainMenu();
            BootstrapManager.failedJoinByID = false;
        }
        if (FindFirstObjectByType<PlayerSpawner>() == null)
        {
            return;
        }
        if (!localPlayerFound && canFindPlayer)
        {
            TryFindLocalPlayer();
        }
        if (!canFindPlayer)
        {
            findPlayerLeaveBufferTimer += Time.deltaTime;
            if(findPlayerLeaveBufferTimer <= findPlayerLeaveBuffer)
            {
                canFindPlayer = true;
                findPlayerLeaveBufferTimer = 0f;
            }
        }
        CheckReady();
    }

    void CheckReady()
    { 
        bool canStart = true;
        foreach (PlayerSpawner player in FindObjectsOfType<PlayerSpawner>())
        {
            if (!player.playerReady)
            {
                canStart = false;
            }
        }
        if (canStart)
        {
            startGameButton.interactable = true;
        }
        else
        {
            startGameButton.interactable = false;
        }
    }

    private void Start()
    {
        CloseAllScreens();
        OpenMainMenu();
        SteamAPI.Init();
    }

    public void CreateLobby()
    {
        LoadingScreen();
        BootstrapManager.CreateLobby();
    }

    public void ToggleReady()
    {
        localPlayer.ServerToggleReady(localPlayer.gameObject);
        if (!localPlayer.playerReady)
        {
            toggleReadyText.text = "Unready";
        }
        else
        {
            toggleReadyText.text = "Ready";
        }
    }
    
    public static void LobbyEntered(string lobbyName, bool isHost)
    {
        instance.LoadingScreen();
        instance.lobbyTitle.text = lobbyName;
        instance.lobbyIDText.text = BootstrapManager.CurrentLobbyID.ToString();
        instance.startGameButton.gameObject.SetActive(isHost);
        instance.CloseAllScreens();
        instance.OpenLobbyScreen();
    }

    public void OpenMainMenu()
    {
        menuScreen.SetActive(true);
    }

    public void LoadingScreen()
    {
        CloseAllScreens();
        loadingScreen.SetActive(true);
    }

    public void OpenLobbyScreen()
    {
        lobbyScreen.SetActive(true);
    }

    public void CloseAllScreens()
    {
        menuScreen.SetActive(false);
        lobbyScreen.SetActive(false);
        loadingScreen.SetActive(false);
    }

    public void LeaveLobby()
    {
        BootstrapManager.LeaveLobby();
        if (SteamMatchmaking.GetLobbyData((CSteamID)BootstrapManager.CurrentLobbyID, "Started") == "false")
        {
            foreach (PlayerBar listObject in GameObject.FindObjectsOfType<PlayerBar>())
            {
                Destroy(listObject.gameObject);
            }
        }
        lobbyTitle.text = "Loading...";
        lobbyIDText.text = "Loading...";
        toggleReadyText.text = "Ready";
        canFindPlayer = false;
        startGameButton.gameObject.SetActive(false);
        localPlayer = null;
        localPlayerFound = false;
        CloseAllScreens();
        OpenMainMenu();
    }

    public void JoinLobby()
    {
        if(lobbyInput.text == string.Empty)
        {
            return;
        }
        LoadingScreen();
        CSteamID steamID = new CSteamID(Convert.ToUInt64(lobbyInput.text));
        BootstrapManager.JoinByID(steamID);
    }

    public void StartGame()
    {
        CloseAllScreens();
        string[] scenesToClose = new string[] { "Menu" };
        BootstrapNetworkManager.ChangeNetworkScene("Game", scenesToClose);
        SteamMatchmaking.SetLobbyData(new CSteamID(BootstrapManager.CurrentLobbyID), "Started", "true");
        BootstrapNetworkManager.ShowAllPlayers();
    }
}
