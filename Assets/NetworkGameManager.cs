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
    public List<GameObject> weaponPickupPrefabs = new List<GameObject>();
    public float weaponSpawnPadding = .5f;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (base.IsOwner)
        {
            client = true;
            weaponSpawnTime = Random.Range(minWeaponSpawnTime, maxWeaponSpawnTime);
            ServerSetCounting(this, true, 0);
        }
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
                GameObject weaponPickup =weaponPickupPrefabs[Random.Range(0, weaponPickupPrefabs.Count - 1)];
                ServerSpawnWeaponPickup(weaponPickup);
            }
            spawnedWeapons = true;
        }
    }

    [ServerRpc]
    void ServerSpawnWeaponPickup(GameObject weaponPickupSpawn)
    {
        float width = Vector2.Distance(Camera.main.ScreenToWorldPoint(new Vector2(0f, 0f)), Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, 0f))) * 0.5f;
        float height = Vector2.Distance(Camera.main.ScreenToWorldPoint(new Vector2(0f, 0f)), Camera.main.ScreenToWorldPoint(new Vector2(0f, Screen.height))) * 0.5f;
        GameObject weaponSpawnPickupRef = Instantiate(weaponPickupSpawn);
        weaponSpawnPickupRef.transform.position = new Vector2(Random.Range(-width + weaponSpawnPadding, width - weaponSpawnPadding), Random.Range(-height + weaponSpawnPadding, height - weaponSpawnPadding));
        ServerManager.Spawn(weaponSpawnPickupRef);
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
