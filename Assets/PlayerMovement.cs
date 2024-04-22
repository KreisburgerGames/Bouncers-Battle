using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.CodeGenerating;
using UnityEngine.SceneManagement;
using Steamworks;
using UnityEngine.EventSystems;

public class PlayerMovement : NetworkBehaviour
{
    // Start is called before the first frame update

    Vector3 throwVector;
    Rigidbody2D rb;
    LineRenderer lineRenderer;
    public float throwForce;
    public bool client = true;
    [AllowMutableSyncType]
    public float bouncebackMultiplier;
    public float lineLengthMultiplier;
    public float maxDragDistance;
    public float distanceEffect;
    public float bounceEdgePadding = 0.5f;
    public float nudgeForce = 1f;
    public float nudgeVelocityThreshold = 0.5f;
    bool nudging = false;
    public GameObject leaveParticle;
    public GameObject dashParticle;
    PlayerObjectSpawner spawner;
    public Vector2 lastThrowVector;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        lineRenderer = GetComponent<LineRenderer>();
        spawner = GetComponent<PlayerObjectSpawner>();
    }

    public void LeaveLobby()
    {
        spawner.objToSpawn = leaveParticle;
        spawner.SpawnObject(spawner.objToSpawn, transform, spawner);
    }

    private void Start()
    {
        SteamAPI.Init();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if(base.IsOwner)
        {
            client = true;
        }
        else
        {
            client = false;
        }
        this.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (SteamMatchmaking.GetLobbyData(new CSteamID(BootstrapManager.CurrentLobbyID), "Started") == "false")
        {
            GetComponent<SpriteRenderer>().enabled = false;
        }
        else
        {
            GetComponent<SpriteRenderer>().enabled = true;
        }
        if (!client)
        {
            return;
        }
        float width = Vector2.Distance(Camera.main.ScreenToWorldPoint(new Vector2(0f, 0f)), Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, 0f))) * 0.5f;
        float height = Vector2.Distance(Camera.main.ScreenToWorldPoint(new Vector2(0f, 0f)), Camera.main.ScreenToWorldPoint(new Vector2(0f, Screen.height))) * 0.5f;
        if (transform.position.x <= -width + (transform.localScale.x/2) + bounceEdgePadding || transform.position.x >= width - (transform.localScale.x / 2) - bounceEdgePadding)
        {
            if(!nudging)
            {
                rb.AddForce(Vector2.left * rb.velocity.x * bouncebackMultiplier, ForceMode2D.Impulse);
            }
            if (Mathf.Abs(rb.velocity.x) <= nudgeVelocityThreshold)
            {
                nudging = true;
                if(rb.position.x < 0f)
                {
                    rb.AddForce(Vector2.right * nudgeForce, ForceMode2D.Impulse);
                }
                else
                {
                    rb.AddForce(-Vector2.right * nudgeForce, ForceMode2D.Impulse);
                }
            }
        }
        if (transform.position.y <= -height + (transform.localScale.y / 2) + bounceEdgePadding || transform.position.y >= height - (transform.localScale.y / 2) - bounceEdgePadding)
        {
            if (!nudging)
            {
                rb.AddForce(Vector2.down * rb.velocity.y * bouncebackMultiplier, ForceMode2D.Impulse);
            }
            if (Mathf.Abs(rb.velocity.y) <= nudgeVelocityThreshold)
            {
                nudging = true;
                if (rb.position.y < 0f)
                {
                    rb.AddForce(Vector2.up * nudgeForce, ForceMode2D.Impulse);
                }
                else
                {
                    rb.AddForce(-Vector2.up * nudgeForce, ForceMode2D.Impulse);
                }
            }
        }
        if(nudging)
        {
            if (transform.position.x >= -width + (transform.localScale.x / 2) + bounceEdgePadding 
                &&
                transform.position.x <= width - (transform.localScale.x / 2) - bounceEdgePadding 
                &&
                transform.position.y >= -height + (transform.localScale.y / 2) + bounceEdgePadding 
                &&
                transform.position.y <= height - (transform.localScale.y / 2) - bounceEdgePadding)
            {
                nudging = false;
            }
        }
    }

    private void OnMouseDrag()
    {
        if (client)
        {
            CalculateThrowVector();
            SetArrow();
        }
    }

    void CalculateThrowVector()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 distance = mousePos - transform.position;
        distance.x = Mathf.Clamp(distance.x, -maxDragDistance, maxDragDistance);
        distance.y = Mathf.Clamp(distance.y, -maxDragDistance, maxDragDistance);
        throwVector = -distance.normalized * 100 * (Vector2.one * (distance.magnitude * distanceEffect));
    }

    void SetArrow()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 distance = mousePos - this.transform.position;
        distance.x = Mathf.Clamp(distance.x, -maxDragDistance, maxDragDistance);
        distance.y = Mathf.Clamp(distance.y, -maxDragDistance, maxDragDistance);
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, (this.transform.position - distance) * lineLengthMultiplier);
        lineRenderer.enabled = true;
    }

    private void OnMouseUp()
    {
        if (client)
        {
            lineRenderer.enabled = false;
            Throw();
        }
    }

    void Throw()
    {
        rb.AddForce(throwVector * throwForce, ForceMode2D.Impulse);
        lastThrowVector = throwVector;
        SpawnDashParticle();
    }

    void SpawnDashParticle()
    {
        spawner.SpawnObject(dashParticle, transform, spawner, true, lastThrowVector);
    }
}
