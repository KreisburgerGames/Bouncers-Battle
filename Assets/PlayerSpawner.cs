using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Connection;
using Steamworks;
using FishNet.CodeGenerating;
using FishNet.Managing.Server;
using static Gamebuild.Feedback.GameBuildData;
using FishNet.Object.Synchronizing;
using System.Runtime.CompilerServices;
using UnityEngine.UI;

public class PlayerSpawner : NetworkBehaviour
{
    public GameObject playerToSpawn;
    public float spawnPadding = 1.0f;
    bool spawned = false;
    [SerializeField]public readonly SyncVar<ulong> playerSteamID = new SyncVar<ulong>();
    public readonly SyncVar<string> playerName = new SyncVar<string>();
    public readonly SyncVar<bool> playerReady = new SyncVar<bool>();
    bool client = false;
    bool requestListUpdate = false;
    public readonly SyncVar<int> players = new SyncVar<int>();
    GameObject playersListContent;
    public GameObject playerBarObj;
    public Vector2 offset = Vector2.zero;
    public float addPlayerOffset = 0;
    bool updating = false;
    bool infoLoading = false;

    private void Start()
    {
        SteamAPI.Init();
    }

    private void Awake()
    {
        players.Value = GameObject.FindObjectsOfType<PlayerSpawner>().Length;
        playersListContent = GameObject.FindWithTag("PlayerListContent");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            client = true;
            Init();
        }
    }

    [Rpc]
    private void Init()
    {
        playerSteamID.Value = (ulong)SteamUser.GetSteamID();
        playerName.Value = SteamFriends.GetPersonaName();
        SetReady(false);
        UpdatePlayerList();
    }

    [Rpc]
    public void ToggleReady()
    {
        playerReady.Value = !playerReady.Value;
    }

    [Rpc]
    private void SetReady(bool isReady)
    {
        playerReady.Value = isReady;
    }

    void UpdatePlayerList()
    {
        if (infoLoading) return;
        foreach (PlayerBar listObject in GameObject.FindObjectsOfType<PlayerBar>()) { if (listObject.playerId == null) { infoLoading = true; return; } }
        requestListUpdate = false;
        foreach (PlayerBar listObject in GameObject.FindObjectsOfType<PlayerBar>())
        {
            Destroy(listObject.gameObject);
        }
        int playerCount = 0;
        foreach(PlayerSpawner player in GameObject.FindObjectsOfType<PlayerSpawner>())
        {
            GameObject playerBarObjRef = Instantiate(playerBarObj) as GameObject;
            PlayerBar playerBar = playerBarObjRef.GetComponent<PlayerBar>();

            playerBar.playerName = player.playerName.Value;
            playerBar.name = player.playerName.Value;
            playerBar.playerId = player.playerSteamID.Value;
            playerBar.SetPlayerValues();

            playerBarObjRef.transform.SetParent(playersListContent.transform);
            playerBarObjRef.GetComponent<RectTransform>().SetLocalPositionAndRotation(offset + new Vector2(0, addPlayerOffset * playerCount), Quaternion.Euler(0, 0, 0));
            playerBarObjRef.transform.localScale = Vector3.one;
            playerCount++;
        }
        updating = false;
    }

    [Rpc]
    private void Update()
    {
        if(!client)
        {
            return;
        }
        if (infoLoading || requestListUpdate)
        {
            bool loaded = true;
            foreach (PlayerBar listObject in GameObject.FindObjectsOfType<PlayerBar>()) { if (listObject.playerName == null) { loaded = false; } }
            if (loaded)
            {
                UpdatePlayerList();
            }
        }
        if(players.Value != GameObject.FindObjectsOfType<PlayerSpawner>().Length && !updating)
        {
            players.Value = GameObject.FindObjectsOfType<PlayerSpawner>().Length;
            updating = true;
            requestListUpdate = true;
        }
        if (SteamMatchmaking.GetLobbyData(new CSteamID(BootstrapManager.CurrentLobbyID), "Started") == "true" && !spawned)
        {
            GameObject playerSpawned = (GameObject)Instantiate(playerToSpawn, UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex(2));    
            float width = Vector2.Distance(Camera.main.ScreenToWorldPoint(new Vector2(0f, 0f)), Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, 0f))) * 0.5f;
            float height = Vector2.Distance(Camera.main.ScreenToWorldPoint(new Vector2(0f, 0f)), Camera.main.ScreenToWorldPoint(new Vector2(0f, Screen.height))) * 0.5f;
            playerSpawned.transform.position = new Vector2(Random.Range(-width + spawnPadding, width - spawnPadding), Random.Range(-height + spawnPadding, height - spawnPadding));
            playerSpawned.SetActive(true);
            ServerManager.Spawn(playerSpawned, ownerConnection:Owner);
            if (base.IsClient)
            {
                playerToSpawn.GetComponent<PlayerMovement>().client = true;
            }
            else
            {
               playerToSpawn.GetComponent<PlayerMovement>().client = false;
            }
            spawned = true;
        }
        foreach (PlayerSpawner player in GameObject.FindObjectsOfType<PlayerSpawner>())
        {
            foreach(PlayerBar bar in GameObject.FindObjectsOfType<PlayerBar>())
            {
                if(bar.playerId == player.playerSteamID.Value)
                {
                    if (player.playerReady.Value)
                    {
                        bar.ready.color = Color.green;
                    }
                    else
                    {
                        bar.ready.color = Color.red;
                    }
                }
            }
        }
    }
}
