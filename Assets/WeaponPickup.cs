using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet;

public class WeaponPickup : NetworkBehaviour
{
    bool isCollected = false;
    public GameObject weapon;
    bool server = false;

    private void Awake()
    {
        if (InstanceFinder.IsServer)
        {
            server = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!server)
        {
            return;
        }
        if (!isCollected)
        {
            if(collision.gameObject.tag == "Player")
            {
                NetworkConnection playerCon = collision.gameObject.GetComponent<Player>().Owner;
                GameObject playerObj = collision.gameObject;
                GameObject weaponRef = Instantiate(weapon);
                weaponRef.transform.SetParent(transform, false);
                ServerSpawnWeaponToPlayer(weaponRef, playerCon, this.gameObject);
                ServerSetCollected(this);
            }
        }
    }

    [ServerRpc]
    void ServerSpawnWeaponToPlayer(GameObject weapon, NetworkConnection connection, GameObject thisRef)
    {
        ServerManager.Spawn(weapon, ownerConnection: connection);
        ServerManager.Despawn(thisRef, DespawnType.Destroy);
    }

    [ServerRpc]
    void ServerSetCollected(WeaponPickup script)
    {
        SetCollected(script);
    }

    [ObserversRpc]
    void SetCollected(WeaponPickup script) 
    { 
        isCollected = true;
    }
}