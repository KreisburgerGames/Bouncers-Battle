using HeathenEngineering.UX;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering.Universal;

public class VignetteHeartbeat : MonoBehaviour
{
    [Header("General")]
    public Volume volume;
    UnityEngine.Rendering.Universal.Vignette vignette;
    public List<MainMenuManager.Menu> menusToHeartbeat;
    public float swapDeadzone = 0.5f;
    [Header("Fade In")]
    public float fadeInSpeed;
    public float fadeInIntensityGoal;
    public float fadeInDelay;
    [Header("Fade Out")]
    public float fadeOutSpeed;
    public float fadeOutDelay;
    float fadeOutIntensityGoal;

    float fadeInTimer = 0f;
    float fadeOutTimer = 0f;

    public bool active;

    bool fadeIn = true;
    // Start is called before the first frame update
    private void Awake()
    {
        volume.profile.TryGet<UnityEngine.Rendering.Universal.Vignette>(out vignette);
        fadeOutIntensityGoal = vignette.intensity.value;
    }

    // Update is called once per frame
    void Update()
    {
        active = false;
        foreach (MainMenuManager.Menu menu in menusToHeartbeat)
        {
            if(MainMenuManager.GetMenu() == menu)
            {
                active = true;
            }
        }
        if(!active)
        {
            vignette.intensity.value = fadeOutIntensityGoal;
            return;
        }
        if(fadeIn)
        {
            if (fadeInTimer >= fadeInDelay)
            {
                vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, fadeInIntensityGoal, fadeInSpeed * Time.deltaTime);
                if(vignette.intensity.value >= fadeInIntensityGoal - swapDeadzone)
                {
                    fadeIn = false;
                    fadeOutTimer = 0f;
                }
            }
            else
            {
                fadeInTimer += Time.deltaTime;
            }
        }
        else
        {
            if (fadeOutTimer >= fadeOutDelay)
            {
                vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, fadeOutIntensityGoal, fadeOutSpeed * Time.deltaTime);
                if (vignette.intensity.value <= fadeOutIntensityGoal + swapDeadzone)
                {
                    fadeIn = true;
                    fadeInTimer = 0f;
                }
            }
            else
            {
                fadeOutTimer += Time.deltaTime;
            }
        }
    }
}
