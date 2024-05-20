using FishNet.Object;
using UnityEngine;
using Cinemachine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class WeaponBase : NetworkBehaviour
{
    public int ammo;
    public bool isAutomatic;
    public float fireCooldown;
    bool _isBetweenShot;
    float _cooldownTimer;
    public Transform normalBulletExit;
    public Transform flippedBulletExit;
    Transform _bulletExit;
    public int minDmg;
    public int maxDmg;
    [FormerlySerializedAs("Knockback")] public float knockback;
    bool _isHolding;
    public GameObject bulletPrefab;
    public float bulletVelocity;
    public float visualRecoilForce = 1;
    CinemachineImpulseSource _impulseSource;
    public float hitShakeStrength = 1f;
    private bool _isClient;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (base.IsOwner)
        {
            _isClient = true;
        }
        _impulseSource = GetComponent<CinemachineImpulseSource>();
        _bulletExit = normalBulletExit;
    }

    public void Shoot()
    {
        if (_isBetweenShot || _isHolding && !isAutomatic) { return; }
        _isBetweenShot = true;
        Vector3 startPos = _bulletExit.position;
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

        if (Camera.main != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
            Vector3 aimingDir = (mousePos - transform.position).normalized;
            float aimingAngle = Mathf.Atan2(aimingDir.y, aimingDir.x) * Mathf.Rad2Deg;
            SpawnBulletLocal(startPos, dir, bulletID, Owner.ClientId, aimingAngle);
            SpawnBullet(startPos, dir, TimeManager.Tick, GetComponentInParent<Player>(), Owner.ClientId, bulletID, minDmg, maxDmg, aimingAngle);
        }

        _impulseSource.m_DefaultVelocity = dir.normalized;
        _impulseSource.GenerateImpulseWithForce(visualRecoilForce);
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
        Vector3 spawnPos = startPos + dir * (bulletVelocity * timeDifference);

        Bullet bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.Euler(dir)).GetComponent<Bullet>();
        bullet.Init(dir, bulletVelocity, bulletID, ownerID, newMinDmg, newMaxDmg, startPos, hitShakeStrength, angle);
    }
    
    void CheckFlip()
    {
        if (transform.eulerAngles.z < 180 && transform.eulerAngles.z > -180)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            _bulletExit = flippedBulletExit;
        }
        else
        {
            GetComponent<SpriteRenderer>().flipX = false;
            _bulletExit = normalBulletExit;
        }
    }

    private void FixedUpdate()
    {
        CheckFlip();
    }

    private void Update()
    {
        if (!_isClient) return;
        if (_isBetweenShot)
        {
            if(Input.GetMouseButton(0))
            {
                _isHolding = true;
            }
        }
        if (!Input.GetMouseButton(0))
        {
            _isHolding = false;
        }
        if(_isBetweenShot)
        {
            _cooldownTimer += Time.deltaTime;
            if(_cooldownTimer >= fireCooldown)
            {
                _isBetweenShot = false;
                _cooldownTimer = 0f;
            }
        }
    }
}
