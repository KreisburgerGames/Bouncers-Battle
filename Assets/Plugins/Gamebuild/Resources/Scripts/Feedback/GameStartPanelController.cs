using System;
using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gamebuild.Feedback
{
    public class GameStartPanelController : MonoBehaviour
    {
        [TextArea(3, 10)]
        public string startingTextMessage;
        public TMP_Text StartText;
        public Button startButton;
        public CanvasGroup startScreenCanvasGroup;
        public string url;
        // Start is called before the first frame update
        void Start()
        {
            StartText.text = String.Empty;  
            ShowStartScreen();
        }

        
        // Update is called once per frame
        void Update()
        {

        }

        public void ShowStartScreen()
        {
            startScreenCanvasGroup.DOFade(1f, 1f).OnComplete(() =>
            {
                StartCoroutine(TypeText(startingTextMessage));
            });
        }

        public void OnPressStart()
        {
            startScreenCanvasGroup.DOFade(1f, 1f).OnComplete(() =>
            {
                gameObject.SetActive(false);
            });

            Application.OpenURL(url);
        }

        private IEnumerator TypeText(string textToType)
        {
            foreach (char character in textToType)
            {
                StartText.text += character;
                yield return new WaitForSeconds(0.01f);
            }
            yield return new WaitForSeconds(0.5f);
            startButton.interactable = true;
            startScreenCanvasGroup.interactable = true;
        }
    }

}
