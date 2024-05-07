using FishNet.Object;
using FishNet.Connection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using System;
using Cinemachine;

public class LineDecayNetworked : NetworkBehaviour
{
    public float decayRate;
    public float bloomDecayRate;
    public float bloomColorDecayRate;
    public float width = 1f;
    Bloom bloom;
    float minBloom;

    private void Awake()
    {
        GetComponent<Volume>().profile.TryGet<Bloom>(out bloom);
        FindFirstObjectByType<CinemachineVirtualCamera>().gameObject.GetComponent<Volume>().profile.TryGet<Bloom>(out Bloom mainBloom);
        minBloom = mainBloom.intensity.value;
    }

    // Update is called once per frame
    void Update()
    {
        width -= decayRate * Time.deltaTime;
        GetComponent<LineRenderer>().startWidth = width;
        GetComponent<LineRenderer>().endWidth = width;
        float intensity = bloom.intensity.value;
        intensity = Mathf.Lerp(intensity, minBloom, bloomDecayRate);
        bloom.intensity.Override(intensity);
        Color.RGBToHSV(bloom.tint.value, out float h, out float currentS, out float v);
        currentS -= bloomColorDecayRate * Time.deltaTime;
        Color newColor = Color.HSVToRGB(h, currentS, v);
        bloom.tint.value = newColor;
        if (width <= 0f)
        {
            Destroy(this.gameObject);
        }
    }
}
