using FishNet.Object;
using FishNet.Connection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.U2D;
using Cinemachine;

public class Player : NetworkBehaviour
{
    public int health;
    bool client = false;
    public float punchRange;
    public int minPunchDmg;
    public int maxPunchDmg;
    public float punchKB;
    public float punchCooldown;
    float cooldownTimer;
    bool isPunchCooldown = false;
    public GameObject punchLineRenderer;
    bool isHovering;
    bool canAttack = true;
    public bool isDead = false;
    private CinemachineImpulseSource impulseSource;

    private void OnMouseEnter()
    {
        isHovering = true;
    }

    private void OnMouseExit()
    {
        if (Input.GetMouseButton(0))
        {
            canAttack = false;
        }
        isHovering = false;
    }

    private void Awake()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if(base.IsOwner)
        {
            client = true;
        }
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        if(!Input.GetMouseButton(0))
        {
            canAttack = true;
        }
        if (isPunchCooldown)
        {
            cooldownTimer += Time.deltaTime;
            if(cooldownTimer >= punchCooldown)
            {
                cooldownTimer = 0;
                isPunchCooldown = false;
            }
        }
        if (Input.GetMouseButton(0))
        {
            Attack();
            isPunchCooldown=true;
        }
    }
    public void Die()
    {
        ServerSetDead(this, true);
    }

    [ServerRpc]
    private void ServerSetDead(Player player, bool newIsDead)
    {
        SetDead(player, newIsDead);
    }

    [ObserversRpc]
    private void SetDead(Player player, bool newIsDead)
    {
        player.isDead = newIsDead;
    }

    [ServerRpc]
    public void ServerSetHealth(Player player, int newHealth)
    {
        SetHealth(player, newHealth);
    }

    [ObserversRpc]
    private void SetHealth(Player player, int newHealth)
    {
        player.health = newHealth;
    }

    void Attack()
    {
        if(isHovering || !canAttack) { return; }
        if (GetComponentInChildren<WeaponBase>() != null)
        {
            WeaponBase weapon = GetComponentInChildren<WeaponBase>();

            weapon.Shoot();
        }
        else
        {
            Punch(transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition), isPunchCooldown, punchRange, this, punchLineRenderer);
        }
    }

    [ObserversRpc]
    void SetPunchCooldown(Player player, bool newPunchCooldown)
    {
        player.isPunchCooldown = newPunchCooldown;
    }

    [ServerRpc]
    void Punch(Vector3 origin, Vector3 mousePos, bool isPunchCooldownL, float range, Player callback, GameObject punchLinePrefab)
    {
        if (isPunchCooldownL) { return; }
        RaycastHit punch;
        Vector3 dir = mousePos - origin;
        Physics.Raycast(origin, dir, out punch, range);
        callback.SetPunchCooldown(callback, true);
        if(punch.collider != null)
        {
            if (punch.collider.gameObject.tag == "Player")
            {
                Player hitPlayer = punch.collider.gameObject.GetComponent<Player>();
                hitPlayer.ServerSetHealth(hitPlayer.health - Random.Range(minPunchDmg, maxPunchDmg), hitPlayer);
                hitPlayer.GetComponent<Rigidbody2D>().AddForce(dir * punchKB, ForceMode2D.Impulse);
                SpawnPunchLineObject(origin, hitPlayer.gameObject.transform.position, punchLinePrefab);
            }
        }
        else
        {
            Vector3 endPoint;
            dir.x = Mathf.Clamp(dir.x, -punchRange, punchRange);
            dir.y = Mathf.Clamp(dir.y, -punchRange, punchRange);
            endPoint = origin + (dir * punchRange);
            SpawnPunchLineObject(origin, endPoint, punchLinePrefab);
        }
        impulseSource.m_DefaultVelocity = dir.normalized;
        impulseSource.GenerateImpulseWithForce(0.35f);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerSetHealth(int newHealth, Player player)
    {
        SetHealth(newHealth, player);
    }

    [ObserversRpc]
    private void SetHealth(int newHealth, Player player)
    {
        player.health = newHealth;
    }

    [ServerRpc]
    private void SpawnPunchLineObject(Vector3 startPos, Vector3 endPos, GameObject gameObject)
    {
        GameObject spawnedLine = Instantiate(gameObject);
        spawnedLine.transform.position = startPos;
        ServerManager.Spawn(spawnedLine, scene: UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex(2), ownerConnection: Owner);
        ServerSetLine(startPos, endPos, spawnedLine);
    }

    [ServerRpc]
    private void ServerSetLine(Vector3 startPos, Vector3 endPos, GameObject gameObject)
    {
        SetLine(startPos, endPos, gameObject);
    }

    [ObserversRpc]
    private void SetLine(Vector3 startPos, Vector3 endPos, GameObject gameObject)
    {
        LineRenderer line = gameObject.GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        Vector3[] positions = new Vector3[] { startPos, endPos };
        line.SetPositions(positions);
    }

    public class Bullet
    {
        public Transform bulletTransform;
        public Vector3 bulletDir;
        public bool launched = false;
        public Rigidbody2D BulletRb;
        public float bulletVelocity;
    }
}
