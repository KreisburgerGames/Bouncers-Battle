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
    
    public void Shoot()
    {
        if (isBetweenShot || isHolding && !isAutomatic) { return; }
        RaycastHit shot;
        Physics.Raycast(bulletExit.position, bulletExit.forward, out shot, range);
        isBetweenShot = true;
        if (shot.collider.gameObject.tag == "Player")
        {
            Player hitPlayer = shot.collider.gameObject.GetComponent<Player>();
            hitPlayer.ServerSetHealth(hitPlayer.health - Random.Range(minDmg, maxDmg), hitPlayer);
            hitPlayer.GetComponent<Rigidbody2D>().AddForce(bulletExit.forward * Knockback, ForceMode2D.Impulse);
        }
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
