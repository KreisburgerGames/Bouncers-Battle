using FishNet.Object;
using FishNet.Connection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDecayNetworked : NetworkBehaviour
{
    public float decayRate;
    public float width = 1f;
    bool client = false;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (base.IsOwner) client = true; else this.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(!client) { return; }
        width -= decayRate * Time.deltaTime;
        ServerSetAlphaChannel(gameObject, width);
        if(width <= 0f)
        {
            Despawn(this.gameObject);
        }
    }

    [ServerRpc]
    private void Despawn(GameObject line)
    {
        ServerManager.Despawn(line, DespawnType.Destroy);
    }

    [ServerRpc]
    private void ServerSetAlphaChannel(GameObject renderer, float width)
    {
        SetAlphaChannel(renderer, width);
    }

    [ObserversRpc]
    private void SetAlphaChannel(GameObject renderer, float width) 
    { 
        renderer.GetComponent<LineRenderer>().startWidth = width;
        renderer.GetComponent<LineRenderer>().endWidth = width;
    }
}
