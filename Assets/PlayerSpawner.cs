using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using FishNet.Object;
using FishNet.Connection;
using FishNet.CodeGenerating;
using FishNet.Managing.Server;
using UnityEngine.UI;
using FishNet.Object.Synchronizing;
using FishNet.Demo.AdditiveScenes;

public class PlayerSpawner : NetworkBehaviour
{
	public GameObject playerToSpawn;
	public float spawnPadding = 1.0f;
	bool spawned = false;

	public ulong playerSteamID;
	public string playerName;
	public bool playerReady;

	public bool client = false;
	bool requestListUpdate = false;
	public int players = 0;

	GameObject playersListContent;
	public GameObject playerBarPrefab;
	public Vector2 offset = Vector2.zero;
	public float addPlayerOffset = 0;

	bool updating = false;
	bool infoLoading = false;
	public bool server = false;

	public float refreshRate = 1f;
	public float refreshTimer = 0f;

	public GameObject networkGmPrefab;
	
	public int roundsWon = 0;

	void Start()
	{
		SteamAPI.Init();
	}

	public override void OnStartClient()
	{
		base.OnStartClient();

		// Check if this is the local player
		if (base.IsOwner)
		{
			client = true;
			Init();
			UpdatePlayerList();
			if(GameObject.FindObjectsOfType<PlayerSpawner>().Length == 1)
			{
				server = true;
			}
		}
		else
		{
			GetComponent<PlayerSpawner>().enabled = false;
			UpdatePlayerList();
		}
	}

	private void Awake()
	{
		playersListContent = GameObject.FindWithTag("PlayerListContent");
	}

	private void Init()
	{
		ServerSetNameAndID((ulong)SteamUser.GetSteamID(), SteamFriends.GetPersonaName(), this.gameObject);
		ServerSetReady(false, this.gameObject);
		UpdatePlayerList();
	}

	[ServerRpc]
	public void ServerToggleReady(GameObject player)
	{
		ToggleReady(player);
	}

	[ServerRpc]
	public void ServerSetNameAndID(ulong steamID, string name, GameObject player)
	{
		CmdSetNameAndID(steamID, name, player);
	}

	[ServerRpc]
	public void ServerSetReady(bool ready, GameObject player)
	{
		SetReady(ready, player);
	}

	[ObserversRpc]
	private void ToggleReady(GameObject player)
	{
		player.GetComponent<PlayerSpawner>().playerReady = !playerReady;
	}

	[ObserversRpc]
	private void CmdSetNameAndID(ulong steamID, string name, GameObject player)
	{
		player.GetComponent<PlayerSpawner>().playerSteamID = steamID;
		player.GetComponent<PlayerSpawner>().playerName = name;
	}

	[ObserversRpc]
	private void SetReady(bool isReady, GameObject player)
	{
		player.GetComponent<PlayerSpawner>().playerReady = isReady;
	}

	void UpdatePlayerList()
	{
		if (infoLoading) return;
		requestListUpdate = false;

		// Clear existing player bars
		foreach (Transform child in playersListContent.transform)
		{
			Destroy(child.gameObject);
		}

		int playerCount = 0;
		// Instantiate player bars for each player
		foreach (PlayerSpawner player in GameObject.FindObjectsOfType<PlayerSpawner>())
		{
			GameObject playerBarObjRef = Instantiate(playerBarPrefab, playersListContent.transform);
			PlayerBar playerBar = playerBarObjRef.GetComponent<PlayerBar>();

			playerBar.playerName = player.playerName;
			playerBar.playerId = player.playerSteamID;
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
		if (!client)
		{
			return;
		}
		refreshTimer += Time.deltaTime;
		if (refreshTimer > refreshRate)
		{
			requestListUpdate = true;
		}

		if (infoLoading || requestListUpdate)
		{
			bool loaded = true;
			foreach (PlayerBar listObject in GameObject.FindObjectsOfType<PlayerBar>()) { if (listObject.playerName == null) { loaded = false; } }
			if (loaded)
			{
				bool alreadySet = true;
				foreach (PlayerBar bar in GameObject.FindObjectsOfType<PlayerBar>())
				{
					if (bar.playerNameText.text == "Loading..." || bar.playerNameText.text == "" || bar.playerNameText.text == "Name")
					{
						alreadySet = false;
					}
				}
				if (!alreadySet)
				{
					UpdatePlayerList();
				}
			}
		}
		// Update player ready status color
		foreach (PlayerSpawner player in GameObject.FindObjectsOfType<PlayerSpawner>())
		{
			foreach(PlayerBar playerBar in GameObject.FindObjectsOfType<PlayerBar>())
			{
				if(playerBar.playerId == player.playerSteamID)
				{
					if (player.playerReady)
					{
						playerBar.ready.color = Color.green;
					}
					else
					{
						playerBar.ready.color = Color.red;
					}
				}
			}
		}
		if (players != GameObject.FindObjectsOfType<PlayerSpawner>().Length && !updating)
		{
			players = GameObject.FindObjectsOfType<PlayerSpawner>().Length;
			ServerSetNameAndID((ulong)SteamUser.GetSteamID(), SteamFriends.GetPersonaName(), this.gameObject);
			ServerSetReady(playerReady, this.gameObject);
			updating = true;
			requestListUpdate = true;
		}
		if (SteamMatchmaking.GetLobbyData(new CSteamID(BootstrapManager.CurrentLobbyID), "Started") == "true" && !spawned)
		{
			SpawnPlayer(this.Owner, this, networkGmPrefab, FindFirstObjectByType<MainMenuManager>().testing);
			spawned = true;
		}
	}

	[ServerRpc]
	public void SpawnPlayer(NetworkConnection owner, PlayerSpawner spawner, GameObject networkGmPrefabNetwork, bool testing = false)
	{
		GameObject playerSpawned = (GameObject)Instantiate(playerToSpawn, UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex(2));
		float width = Vector2.Distance(Camera.main.ScreenToWorldPoint(new Vector2(0f, 0f)), Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, 0f))) * 0.5f;
		float height = Vector2.Distance(Camera.main.ScreenToWorldPoint(new Vector2(0f, 0f)), Camera.main.ScreenToWorldPoint(new Vector2(0f, Screen.height))) * 0.5f;
		playerSpawned.transform.position = new Vector2(Random.Range(-width + spawnPadding, width - spawnPadding), Random.Range(-height + spawnPadding, height - spawnPadding));
		playerSpawned.SetActive(true);
		ServerManager.Spawn(playerSpawned, ownerConnection: owner);
		SetPlayerSpawner(playerSpawned.GetComponent<Player>(), spawner);
		if (spawner.server)
		{
			GameObject networkGameManagerObj = Instantiate(networkGmPrefabNetwork);
			ServerManager.Spawn(networkGameManagerObj, ownerConnection: spawner.Owner, scene: UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex(2));
			if(testing)
			{
				networkGameManagerObj.GetComponent<NetworkGameManager>().debug = true;
			}
		}
	}

	[ObserversRpc]
	void SetPlayerSpawner(Player playerToSet, PlayerSpawner playerSpawnerRef)
	{
		playerToSet.localSpawner = playerSpawnerRef;
	}

	[ServerRpc]
	public void RespawnPlayer(NetworkConnection owner, PlayerSpawner spawner)
	{
		GameObject playerSpawned = (GameObject)Instantiate(playerToSpawn, UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex(2));
		float width = Vector2.Distance(Camera.main.ScreenToWorldPoint(new Vector2(0f, 0f)), Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, 0f))) * 0.5f;
		float height = Vector2.Distance(Camera.main.ScreenToWorldPoint(new Vector2(0f, 0f)), Camera.main.ScreenToWorldPoint(new Vector2(0f, Screen.height))) * 0.5f;
		playerSpawned.transform.position = new Vector2(Random.Range(-width + spawnPadding, width - spawnPadding), Random.Range(-height + spawnPadding, height - spawnPadding));
		playerSpawned.SetActive(true);
		ServerManager.Spawn(playerSpawned, ownerConnection: owner);
		SetPlayerSpawner(playerSpawned.GetComponent<Player>(), spawner);
	}
}

