using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [ServerRpc]
    public void ServerSetVisible(GameObject obj)
    {
        SetVisible(obj);
    }

    [ObserversRpc]
    public void SetVisible(GameObject obj)
    {
        obj.GetComponent<SpriteRenderer>().enabled = true;
    }
}
