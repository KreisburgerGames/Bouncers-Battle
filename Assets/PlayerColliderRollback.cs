using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class PlayerColliderRollback : NetworkBehaviour
{
    public static Dictionary<int, PlayerColliderRollback> Players = new Dictionary<int, PlayerColliderRollback>(); //Only server based

    private List<State> _pastStates = new List<State>();
    private BoxCollider2D _capsuleCollider;
    private float _capsuleRadius, _capsuleHeight;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsServer)
            enabled = false;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        Players.Add(Owner.ClientId, this);

        if (TryGetComponent(out BoxCollider2D capsuleCollider))
            _capsuleCollider = capsuleCollider;

        _capsuleRadius = _capsuleCollider.transform.lossyScale.x;   // Define your capsule radius
        _capsuleHeight = _capsuleCollider.transform.lossyScale.y;   // Define your capsule height

        TimeManager.OnTick += OnTick;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        Players.Remove(Owner.ClientId);
    }

    private void OnTick()
    {
        if (_pastStates.Count > TimeManager.TickRate)
            _pastStates.RemoveAt(0);

        _pastStates.Add(new State() { Position = transform.position });
    }

    public bool CheckPastCollisions(Bullet bullet)
    {
        for (int i = 0; i < Mathf.Min(_pastStates.Count, bullet.pastStates.Count); i++)
        {
            Vector3 playerPosition = _pastStates[i].Position;
            Vector3 bulletPosition = bullet.pastStates[i].Position;

            float bulletRadius = bullet.transform.localScale.x; // Define your bullet radius

            Vector3 capsuleCenter = playerPosition + Vector3.up * _capsuleHeight / 2;
            Vector3 point1 = capsuleCenter + Vector3.up * ((_capsuleHeight / 2) - _capsuleRadius);
            Vector3 point2 = capsuleCenter - Vector3.up * ((_capsuleHeight / 2) - _capsuleRadius);

            Vector3 closest = ClosestPointOnLineSegment(point1, point2, bulletPosition);

            if (Vector3.Distance(closest, bulletPosition) <= _capsuleRadius + bulletRadius)
            {
                if (bullet.ownerID == OwnerId)
                    continue;

                // A collision has occurred.
                DestroyBullet(bullet.bulletID);
                return true;
            }
        }

        return false;
    }

    [ObserversRpc]
    private void DestroyBullet(int bulletID)
    {
        if (Bullet.bullets.TryGetValue(bulletID, out Bullet bullet))
            bullet.DestroyBullet();
    }

    private Vector3 ClosestPointOnLineSegment(Vector3 a, Vector3 b, Vector3 bulletPosition)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(bulletPosition - a, ab) / Vector3.Dot(ab, ab);
        return a + Mathf.Clamp01(t) * ab;
    }

    private class State
    {
        public Vector3 Position;
    }
}