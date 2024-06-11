using FishNet.Object;
using FishNet.Connection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class NetworkGameManager : NetworkBehaviour
{
	[SerializeField] private int round = 0;
	public float roundTime = 0f;
	bool client = false;
	bool counting = false;
	public float minWeaponSpawnTime = 3f;
	public float maxWeaponSpawnTime = 10f;
	private float weaponSpawnTime;
	int playerCount;
	bool spawnedWeapons = false;
	public GameObject weaponPickupPrefab;
	public List<GameObject> weaponPrefabs = new List<GameObject>();
	public List<Player> players = new List<Player>();
	public float weaponSpawnPadding = .5f;
	public bool debug = false;

	public override void OnStartClient()
	{
		base.OnStartClient();

		if (base.IsOwner)
		{
			client = true;
			weaponSpawnTime = Random.Range(minWeaponSpawnTime, maxWeaponSpawnTime);
			ServerSetCounting(this, true, 0);
			ServerResetPlayerList(this, GetAllPlayers());
		}
	}

	List<Player> GetAllPlayers()
	{
		List<Player> allPlayers = new List<Player>();
		foreach(Player foundPlayer in GameObject.FindObjectsOfType<Player>())
		{
			allPlayers.Add(foundPlayer);
		}
		return allPlayers;
	}

	private void Update()
	{
		playerCount = FindObjectsOfType<Player>().Length;
		// Local Code
		if (counting)
		{
			roundTime += Time.deltaTime;
		}
		if (!client) return;
		// Server Code
		if (roundTime >= weaponSpawnTime && !spawnedWeapons)
		{
			int weaponsToSpawn = playerCount - Random.Range(0, (int)Mathf.Floor(playerCount / 2));
			for (int i = 0; i < weaponsToSpawn; i++)
			{
				GameObject chosenWeapon = weaponPrefabs[Random.Range(0, weaponPrefabs.Count - 1)];
				ServerSpawnWeaponPickup(weaponPickupPrefab, chosenWeapon);
			}
			spawnedWeapons = true;
		}
		if(debug) return;
		// Game Loop
		if(players.Count == 1)
		{
			Player roundWinner = players[0];
			PlayerSpawner roundWinnerSpawner = roundWinner.localSpawner;
			string roundWinnerName = roundWinner.localSpawner.playerName;
			ServerSetRoundsWon(roundWinnerSpawner, roundWinnerSpawner.roundsWon + 1);
			ServerKillLastPlayer(roundWinner);
			ServerClearAllItems();
			ServerRespawnAll();
		}
	}
	
	[ServerRpc]
	void ServerClearAllItems()
	{
		// Clear weapon pickups
		foreach(WeaponPickup weaponPickup in GameObject.FindObjectsOfType<WeaponPickup>())
		{
			ServerManager.Despawn(weaponPickup.gameObject, DespawnType.Destroy);
		}
		// Clear all player's weapons
		foreach(GameObject weapon in GameObject.FindGameObjectsWithTag("Weapon"))
		{
			ServerManager.Despawn(weapon, DespawnType.Destroy);
		}
	}
	
	[ServerRpc]
	void ServerSetRoundsWon(PlayerSpawner targetPlayer, int roundsWon)
	{
		SetRoundsWon(targetPlayer, roundsWon);
	}
	
	[ObserversRpc]
	void SetRoundsWon(PlayerSpawner targetPlayer, int roundsWon)
	{
		targetPlayer.roundsWon = roundsWon;	
	}
	
	[ServerRpc]
	void ServerKillLastPlayer(Player lastPlayer)
	{
		KillLastPlayer(lastPlayer.Owner ,lastPlayer);
	}
	
	[TargetRpc]
	void KillLastPlayer(NetworkConnection conn,Player lastPlayer)
	{
		lastPlayer.Die();	
	}

	[ServerRpc]
	void ServerRespawnAll()
	{
		RespawnAll();
	}

	[ObserversRpc]
	void RespawnAll()
	{
		PlayerSpawner localPlayerSpawner = null;
		foreach(PlayerSpawner playerSpawnerFound in GameObject.FindObjectsOfType<PlayerSpawner>())
		{
			if(playerSpawnerFound.client)
			{
				localPlayerSpawner = playerSpawnerFound;
			}
		}
		localPlayerSpawner.RespawnPlayer(localPlayerSpawner.Owner, localPlayerSpawner);
	}

	[ServerRpc(RequireOwnership = false)]
	public void ServerRemovePlayer(NetworkGameManager manager, Player playerToRemove)
	{
		RemovePlayer(manager, playerToRemove);
	}

	[ObserversRpc]
	void RemovePlayer(NetworkGameManager manager, Player playerToRemove)
	{
		manager.players.Remove(playerToRemove);
	}

	[ServerRpc]
	void ServerResetPlayerList(NetworkGameManager manager, List<Player> newPlayerList)
	{
		ResetPlayerList(manager, newPlayerList);
	}

	[ObserversRpc]
	void ResetPlayerList(NetworkGameManager manager, List<Player> newPlayerList)
	{
		manager.players = newPlayerList;
	}

	[ServerRpc]
	void ServerSpawnWeaponPickup(GameObject weaponPickupSpawn, GameObject weapon)
	{
		float width = Vector2.Distance(Camera.main.ScreenToWorldPoint(new Vector2(0f, 0f)), Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, 0f))) * 0.5f;
		float height = Vector2.Distance(Camera.main.ScreenToWorldPoint(new Vector2(0f, 0f)), Camera.main.ScreenToWorldPoint(new Vector2(0f, Screen.height))) * 0.5f;
		GameObject weaponSpawnPickupRef = Instantiate(weaponPickupSpawn);
		weaponSpawnPickupRef.transform.position = new Vector2(Random.Range(-width + weaponSpawnPadding, width - weaponSpawnPadding), Random.Range(-height + weaponSpawnPadding, height - weaponSpawnPadding));
		ServerManager.Spawn(weaponSpawnPickupRef);
		SetWeaponPickup(weaponSpawnPickupRef.GetComponent<WeaponPickup>(), weapon);
	}

	[ObserversRpc]
	void SetWeaponPickup(WeaponPickup pickup, GameObject weapon)
	{
		pickup.weapon = weapon;
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
