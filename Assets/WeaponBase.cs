using System;
using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Player;
using Cinemachine;
using Random = UnityEngine.Random;

public class WeaponBase : NetworkBehaviour
{
    public int ammo;
    public bool isAutomatic;
    public float fireCooldown;
    bool isBetweenShot;
    float cooldownTimer;
    public Transform normalBulletExit;
    public Transform flippedBulletExit;
    Transform bulletExit;
    public int minDmg;
    public int maxDmg;
    public float Knockback;
    bool isHolding = false;
    public GameObject bulletPrefab;
    public float bulletVelocity;
    Player owner;
    public float visualRecoilForce = 1;
    CinemachineImpulseSource impulseSource;
    public float hitShakeStrength = 1f;
    private bool isClient = false;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (base.IsOwner)
        {
            isClient = true;
        }
        owner = GetComponentInParent<Player>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
        bulletExit = normalBulletExit;
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
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
        Vector3 aimingDir = (mousePos - transform.position).normalized;
        float aimingAngle = Mathf.Atan2(aimingDir.y, aimingDir.x) * Mathf.Rad2Deg;
        SpawnBulletLocal(startPos, dir, bulletID, Owner.ClientId, aimingAngle);
        SpawnBullet(startPos, dir, TimeManager.Tick, GetComponentInParent<Player>(), Owner.ClientId, bulletID, minDmg, maxDmg, aimingAngle);
        impulseSource.m_DefaultVelocity = dir.normalized;
        impulseSource.GenerateImpulseWithForce(visualRecoilForce);
    }
    private void SpawnBulletLocal(Vector3 startPos, Vector3 dir, int newBulletID, int ownerID, float angle)
    {
        Bullet bullet = Instantiate(bulletPrefab, startPos, Quaternion.Euler(dir)).GetComponent<Bullet>();
        bullet.Init(dir, bulletVelocity, newBulletID, ownerID, minDmg, maxDmg, startPos, hitShakeStrength, angle);
    }

    [ServerRpc]
    private void SpawnBullet(Vector3 startPos, Vector3 dir, uint startTick, Player ownerRef, int bulletID, int ownerID, int newMinDmg, int newMaxDmg, float angle)
    {
        SpawnBulletServer(startPos, dir, startTick, ownerRef, bulletID, ownerID, newMinDmg, newMaxDmg, angle);
    }

    [ObserversRpc(ExcludeOwner = true)]
    private void SpawnBulletServer(Vector3 startPos, Vector3 dir, uint startTick, Player ownerRef, int bulletID, int ownerID, int newMinDmg, int newMaxDmg, float angle)
    {
        float timeDifference = (float)(TimeManager.Tick - startTick) / TimeManager.TickRate;
        Vector3 spawnPos = startPos + dir * bulletVelocity * timeDifference;

        Bullet bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.Euler(dir)).GetComponent<Bullet>();
        bullet.Init(dir, bulletVelocity, bulletID, ownerID, newMinDmg, newMaxDmg, startPos, hitShakeStrength, angle);
    }
    
    void CheckFlip()
    {
        if (transform.eulerAngles.z < 180 && transform.eulerAngles.z > -180)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            bulletExit = flippedBulletExit;
        }
        else
        {
            GetComponent<SpriteRenderer>().flipX = false;
            bulletExit = normalBulletExit;
        }
    }

    private void FixedUpdate()
    {
        CheckFlip();
    }

    private void Update()
    {
        if (!isClient) return;
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
