using FishNet;
using FishNet.Connection;
using FishNet.Managing.Server;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int minDmg;
    public int maxDmg;
    public GameObject bulletDestroyParticle;
    public int bulletID;
    public int ownerID;
    public float bulletVelocity;
    Vector3 dir;
    Rigidbody2D rb;
    bool launched = false;
    public float hitShakeStrength;

    public static Dictionary<int, Bullet> bullets = new Dictionary<int, Bullet>();
    public List<State> pastStates = new List<State>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (InstanceFinder.IsServer)
        {
            InstanceFinder.TimeManager.OnTick += OnTick;
        }
    }

    private void OnTick()
    {
        if(pastStates.Count > InstanceFinder.TimeManager.TickRate)
        {
            pastStates.RemoveAt(0);
        }

        pastStates.Add(new State() { position = transform.position });

        foreach(var player in PlayerColliderRollback.Players.Values)
        {
            if(Vector2.Distance(transform.position, player.transform.position) > 3f)
            {
                continue;
            }

            if (player.CheckPastCollisions(this))
            {
                ShakeEnemyScreen(player.Owner, dir, hitShakeStrength);
                DestroyBullet();
            }
        }
    }

    void ShakeEnemyScreen(NetworkConnection conn, Vector2 shakeDir, float shakeStrength)
    {
        FindFirstObjectByType<Player>().ShakeEnemyScreen(conn, shakeDir, shakeStrength);
    }

    public void DestroyBullet()
    {
        if (InstanceFinder.IsServer)
        {
            InstanceFinder.TimeManager.OnTick -= OnTick;
        }

        GameObject particleRef = Instantiate(bulletDestroyParticle);
        particleRef.transform.position = transform.position;

        bullets.Remove(bulletID);
        Destroy(gameObject);
    }

    public void Init(Vector3 newDir, float newBulletVelocity, int newBulletID, int newOwnerID, int newMinDmg, int newMaxDmg, Vector2 startPos, float newHitShakeStrength)
    {
        dir = newDir; bulletVelocity = newBulletVelocity; bulletID = newBulletID; ownerID = newOwnerID; minDmg = newMinDmg; maxDmg = newMaxDmg; transform.position = startPos; hitShakeStrength = newHitShakeStrength;
        bullets.Add(bulletID, this);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Barrier")
        {
            DestroyBullet();
        }
    }

    private void Update()
    {
        if (!launched)
        {
            rb.AddForce(dir * bulletVelocity, ForceMode2D.Impulse);
            launched = true;
        }
    }

    public class State
    {
        public Vector2 position;
    }
}
