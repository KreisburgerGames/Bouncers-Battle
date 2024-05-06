using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Player;
using Cinemachine;

public class WeaponBase : NetworkBehaviour
{
    public int ammo;
    public bool isAutomatic;
    public float fireCooldown;
    bool isBetweenShot;
    float cooldownTimer;
    public Transform bulletExit;
    public int minDmg;
    public int maxDmg;
    public float Knockback;
    bool isHolding = false;
    public GameObject bulletPrefab;
    public float bulletVelocity;
    Player owner;
    public float visualRecoilForce = 1;
    CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        owner = GetComponentInParent<Player>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void Shoot()
    {
        if (isBetweenShot || isHolding && !isAutomatic) { return; }
        isBetweenShot = true;
        Vector3 startPos = bulletExit.position;
        Vector3 dir = transform.up;

        bool uniqueIDFound = false;
        int bulletID = Random.Range(100000, 999999);
        while(!uniqueIDFound)
        {
            bool diff = true;
            foreach(Bullet bullet in GameObject.FindObjectsOfType<Bullet>())
            {
                if(bulletID == bullet.bulletID)
                {
                    diff = false;
                }
            }
            if( diff ) 
            { 
                uniqueIDFound= true;
            }
            else
            {
                bulletID = Random.Range(100000, 999999);
            }
        }
        SpawnBulletLocal(startPos, dir, bulletID, Owner.ClientId);
        SpawnBullet(startPos, dir, TimeManager.Tick, GetComponentInParent<Player>(), Owner.ClientId, bulletID, minDmg, maxDmg);
        impulseSource.m_DefaultVelocity = dir.normalized;
        impulseSource.GenerateImpulseWithForce(visualRecoilForce);
    }
    private void SpawnBulletLocal(Vector3 startPos, Vector3 dir, int newBulletID, int ownerID)
    {
        Bullet bullet = Instantiate(bulletPrefab, startPos, Quaternion.Euler(dir)).GetComponent<Bullet>();
        bullet.Init(dir, bulletVelocity, newBulletID, ownerID, minDmg, maxDmg, startPos);
    }

    [ServerRpc]
    private void SpawnBullet(Vector3 startPos, Vector3 dir, uint startTick, Player ownerRef, int bulletID, int ownerID, int newMinDmg, int newMaxDmg)
    {
        SpawnBulletServer(startPos, dir, startTick, ownerRef, bulletID, ownerID, newMinDmg, newMaxDmg);
        
    }

    [ObserversRpc(ExcludeOwner = true)]
    private void SpawnBulletServer(Vector3 startPos, Vector3 dir, uint startTick, Player ownerRef, int bulletID, int ownerID, int newMinDmg, int newMaxDmg)
    {
        float timeDifference = (float)(TimeManager.Tick - startTick) / TimeManager.TickRate;
        Vector3 spawnPos = startPos + dir * bulletVelocity * timeDifference;

        Bullet bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.Euler(dir)).GetComponent<Bullet>();
        bullet.Init(dir, bulletVelocity, bulletID, ownerID, newMinDmg, newMaxDmg, startPos);
    }

    private void Update()
    {
        if (isBetweenShot)
        {
            if(Input.GetMouseButton(0))
            {
                isHolding = true;
            }
        }
        if (!Input.GetMouseButton(0))
        {
            isHolding = false;
        }
        if(isBetweenShot)
        {
            cooldownTimer += Time.deltaTime;
            if(cooldownTimer >= fireCooldown)
            {
                isBetweenShot = false;
                cooldownTimer = 0f;
            }
        }
    }
}
