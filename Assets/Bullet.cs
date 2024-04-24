using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    bool client = false;
    public int minDmg;
    public int maxDmg;
    public GameObject bulletDestroyParticle;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if(base.IsOwner)
        {
            client = true;
        }
    }

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

    [ServerRpc]
    public void Hit(GameObject particle, Vector2 pos)
    {
        GameObject particleRef = Instantiate(particle);
        particleRef.transform.position = pos;
        ServerManager.Spawn(particleRef);
        ServerManager.Despawn(this.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!client)
        {
            return;
        }
        if(collision.gameObject.tag == "Player")
        {
            Player player = collision.gameObject.GetComponent<Player>();
            player.ServerSetHealth(player, player.health - Random.Range(minDmg, maxDmg));
            if(player.health <= 0)
            {
                player.Die();
            }
        }
        Hit(bulletDestroyParticle, transform.position);
    }
}
