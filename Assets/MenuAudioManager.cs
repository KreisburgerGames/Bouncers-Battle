using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuAudioManager : MonoBehaviour
{
    public VignetteHeartbeat vh;
    AudioSource audio;

    private void Start()
    {
        audio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (vh.active)
        {
            audio.enabled = true;
        }
        else
        {
            audio.enabled = false;
        }
    }
}
