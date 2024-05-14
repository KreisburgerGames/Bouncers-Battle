using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using FishNet.Object;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEngine;
using Input = UnityEngine.Windows.Input;

public class AimRotation : NetworkBehaviour
{
    public float rotSpeed = 5f;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner)
        {
            this.enabled = false;
        }
    }

    private void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
        Vector3 aimingDir = (mousePos - transform.position).normalized;
        float aimingAngle = Mathf.Atan2(aimingDir.y, aimingDir.x) * Mathf.Rad2Deg;
        float newAngle = Mathf.LerpAngle(transform.eulerAngles.z, aimingAngle, rotSpeed * Time.deltaTime);

        transform.eulerAngles = new Vector3(0, 0, newAngle);
    }
}