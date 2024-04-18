using FishNet.Managing.Scened;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemHandler : MonoBehaviour
{
    string lastScene;
    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        lastScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }

    private void Awake()
    {
        if (lastScene != UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
        {
            lastScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (GameObject.FindFirstObjectByType<EventSystem>() == null)
            {
                print("yes");
                GameObject eventSystem = Instantiate(new GameObject(), null, true);
                DontDestroyOnLoad(eventSystem);
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
                eventSystem.AddComponent<BaseInput>();
            }
        }
    }
}
