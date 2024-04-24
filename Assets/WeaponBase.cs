using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : NetworkBehaviour
{
    public int ammo;
    public bool isAutomatic;
    public float fireCooldown;
    bool isBetweenShot;
    float cooldownTimer;
    public float range;
    public Transform bulletExit;
    public int minDmg;
    public int maxDmg;
    public float Knockback;
    bool isHolding = false;
    public GameObject bulletPrfab;
    public float bulletVelocity;

    public void Shoot()
    {
        if (isBetweenShot || isHolding && !isAutomatic) { return; }
        isBetweenShot = true;
        ServerSpawnBullet(bulletPrfab, bulletExit.localRotation, bulletExit.position, GetComponentInParent<NetworkObject>().Owner, bulletVelocity);
    }

    [ServerRpc]
    public void ServerSpawnBullet(GameObject bullet, Quaternion direction, Vector3 spawnPoint, NetworkConnection owner, float velocity)
    {
        GameObject bulletRef = Instantiate(bullet);
        bulletRef.transform.localRotation = direction;
        bulletRef.transform.position = spawnPoint;
        bulletRef.GetComponent<Rigidbody2D>().AddForce(bulletRef.transform.up * velocity, ForceMode2D.Impulse);
        ServerManager.Spawn(bulletRef, scene: UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex(2), ownerConnection: owner);
        SetBullet(bulletRef, minDmg, maxDmg);
    }

    [ObserversRpc]
    private void SetBullet(GameObject bulletToSet, int newMinDmg, int newMaxDmg)
    {
        Bullet script = bulletToSet.GetComponent<Bullet>();
        script.minDmg = newMinDmg;
        script.maxDmg = newMaxDmg;
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
