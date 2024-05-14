using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet;
using UnityEngine.UI;

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
                Transform parent = collision.gameObject.GetComponentInChildren<AimRotation>().gameObject.transform;
                ServerSpawnWeaponToPlayer(weapon, playerCon, this.gameObject, parent);
                ServerSetCollected(this);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void ServerSpawnWeaponToPlayer(GameObject weapon, NetworkConnection connection, GameObject thisRef, Transform setParent)
    {
        GameObject weaponRef = Instantiate(weapon);
        weaponRef.transform.SetParent(setParent, false);
        ServerManager.Spawn(weaponRef, ownerConnection: connection);
        ServerManager.Despawn(thisRef, DespawnType.Destroy);
    }

    [ServerRpc(RequireOwnership = false)]
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