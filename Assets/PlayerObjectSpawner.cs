using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.CodeGenerating;
using Steamworks;
using UnityEngine.UI;
using System.Linq;

public class PlayerObjectSpawner : NetworkBehaviour
{
    public GameObject objToSpawn;
    public GameObject spawnedObject;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
            GetComponent<PlayerObjectSpawner>().enabled = false;
    }

    [ServerRpc]
    public void SpawnObject(GameObject obj, Transform player, PlayerObjectSpawner script, bool dashParticle=false, Vector2 lastThrowVector=new Vector2())
    {
        GameObject spawned = Instantiate(obj, player.position, Quaternion.identity);
        if(dashParticle)
        {
            float angle = Mathf.Atan2(lastThrowVector.y, lastThrowVector.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360;
            Quaternion rot = Quaternion.Euler((angle + 180) * -1, 90, 0);
            spawned.transform.localRotation = rot;
        }
        ServerManager.Spawn(spawned);
        SetSpawnedObject(spawned, script);
    }

    [ObserversRpc]
    public void SetSpawnedObject(GameObject spawned, PlayerObjectSpawner script)
    {
        script.spawnedObject = spawned;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DespawnObject(GameObject obj)
    {
        ServerManager.Despawn(obj);
    }
}
