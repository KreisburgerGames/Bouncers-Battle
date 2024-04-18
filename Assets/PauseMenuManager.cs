using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuManager : MonoBehaviour
{
    bool paused = false;
    public GameObject pauseMenu;
    public void Leave()
    {
        BootstrapManager.LeaveLobby();
        pauseMenu.SetActive(false);
    }

    public void Pause()
    {
        paused = true;
        pauseMenu.SetActive(true);
    }

    public void UnPause()
    {
        paused = false;
        pauseMenu.SetActive(false);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab)) 
        {
            if (paused)
            {
                UnPause();
            }
            else
            {
                Pause();
            }
        }
    }
}
