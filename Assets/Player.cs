using FishNet.Object;
using FishNet.Connection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.U2D;
using Cinemachine;
using UnityEditor;

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
	private CinemachineImpulseSource impulseSource;
	public float punchGlowHeightMultiplier = 1.1f;
	public float punchGlowCapsuleOffset = 0.2f;
	public float punchGlowRadiusMultipier = 1.2f;
	public float punchShakeForce = 0.5f;

	public PlayerSpawner localSpawner;

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
		if(health <= 0)
		{
			Die();
		}
	}
	public void Die()
	{
		NetworkGameManager ngm = FindFirstObjectByType<NetworkGameManager>();
		ngm.ServerRemovePlayer(ngm, this);
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
				ServerKnockbackPlayer(hitPlayer.Owner, dir, punchKB, hitPlayer.gameObject);
				SpawnPunchLineObject(origin, hitPlayer.gameObject.transform.position, punchLinePrefab, Owner, dir, punchGlowRadiusMultipier, punchGlowCapsuleOffset);
			}
		}
		else
		{
			Vector3 endPoint;
			dir.x = Mathf.Clamp(dir.x, -punchRange, punchRange);
			dir.y = Mathf.Clamp(dir.y, -punchRange, punchRange);
			endPoint = origin + (dir * punchRange);
			SpawnPunchLineObject(origin, endPoint, punchLinePrefab, Owner, dir, punchGlowRadiusMultipier, punchGlowCapsuleOffset);
		}
		impulseSource.m_DefaultVelocity = dir.normalized;
		impulseSource.GenerateImpulseWithForce(punchShakeForce);
	}

	[ServerRpc]
	private void ServerKnockbackPlayer(NetworkConnection conn, Vector3 dir, float force, GameObject obj)
	{
		KnockbackPlayer(conn, dir, force, obj);
	}

	[TargetRpc]
	private void KnockbackPlayer(NetworkConnection conn, Vector3 dir, float force, GameObject obj)
	{
		obj.GetComponent<Rigidbody2D>().AddForce(dir * force, ForceMode2D.Impulse);
	}

	[ServerRpc(RequireOwnership = false)]
	public void ServerSetHealth(int newHealth, Player player)
	{
		SetHealth(newHealth, player);
	}

	[TargetRpc]
	public void ShakeEnemyScreen(NetworkConnection conn, Vector2 shakeDir, float shakeStrength)
	{
		impulseSource.m_DefaultVelocity = shakeDir;
		impulseSource.GenerateImpulseWithForce(shakeStrength);
	}

	[ObserversRpc]
	private void SetHealth(int newHealth, Player player)
	{
		player.health = newHealth;
	}

	[ServerRpc]
	private void SpawnPunchLineObject(Vector3 startPos, Vector3 endPos, GameObject gameObject, NetworkConnection owner, Vector3 dir, float radiusM, float offset)
	{
		GameObject spawnedLine = Instantiate(gameObject);
		spawnedLine.transform.position = startPos;
		ServerManager.Spawn(spawnedLine, scene: UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex(2), ownerConnection: owner);
		float len = Vector2.Distance(startPos, endPos);
		len *= punchGlowHeightMultiplier;
		ServerSetLine(startPos, endPos, spawnedLine, len, radiusM, offset);
	}

	[ObserversRpc]
	private void ServerSetLine(Vector3 startPos, Vector3 endPos, GameObject gameObjectRef, float length, float radiusM, float offset)
	{
		LineRenderer line = gameObjectRef.GetComponent<LineRenderer>();
		float angle = Mathf.Rad2Deg * (Mathf.Atan2(endPos.y - startPos.y, endPos.x - startPos.x));
		print(startPos);
		print(endPos);
		line.gameObject.transform.localRotation = Quaternion.Euler(0, 0, angle);
		line.useWorldSpace = true;
		Vector3[] positions = new Vector3[] { startPos, endPos };
		line.SetPositions(positions);
		line.gameObject.GetComponent<CapsuleCollider>().height = length + offset;
		line.gameObject.GetComponent<CapsuleCollider>().center = new Vector3((length/2) + offset, 0, 0);
		float radius = line.gameObject.GetComponent<CapsuleCollider>().radius * radiusM;
		line.gameObject.GetComponent<CapsuleCollider>().radius = radius;
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
