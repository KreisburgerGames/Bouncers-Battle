using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace Gamebuild.Feedback
{
    public class GamebuildPopUpController : MonoBehaviour
    {


        [Header("UI Elements")]
        public TMP_Text questionTextUI;
        public TMP_InputField questionInputField;
        public GameObject LinearPanel, ReactionPanel;
        public Transform QuestionReactionPanel;

        [Header("Audio")]
        public bool playChime;
        public AudioClip chimeSound;
        public AudioSource chimeAudioSource;

        [Header("Settings")]
        public float delayBetweenCharacters = 0.01f;
        public Color iconColor;
        public Color textColor;

        private bool showInputField;
        private ReactionButtonController lastPressedReactionController;
        public Button submitButton;

        public bool customFeedbackPanelShowing;
        public GameBuildData gameBuildData;

        Action nextRunOnSubmit;
        private List<string> additionalFeedback;


        public void ShowPlayerPopUp(string questionText, GamebuildFeedback.QuestionType type, bool showInput)
        {
            SetNewQuestion(questionText, type, showInput);
            ShowPopUp();
        }
        public void ShowPlayerPopUp(string questionText, GamebuildFeedback.QuestionType type, bool showInput, List<string> addtionalFeedback)
        {
            additionalFeedback.Clear();
            this.additionalFeedback = addtionalFeedback;
            SetNewQuestion(questionText, type, showInput);
            ShowPopUp();
        }

        public void ShowPlayerPopUp(string questionText, GamebuildFeedback.QuestionType type, bool showInput, List<string> addtionalFeedback, Action runOnSubmit)
        {
            additionalFeedback.Clear();
            nextRunOnSubmit = runOnSubmit;

            this.additionalFeedback = addtionalFeedback;
            SetNewQuestion(questionText, type, showInput);
            ShowPopUp();
        }

        public void ShowPlayerPopUp(string questionText, GamebuildFeedback.QuestionType type, bool showInput, Action runOnSubmit)
        {
            nextRunOnSubmit = runOnSubmit;
            SetNewQuestion(questionText, type, showInput);
            ShowPopUp();
        }

        /// <summary>
        /// Sets the question text and type for the popup.
        /// </summary>
        public void SetNewQuestion(string question, GamebuildFeedback.QuestionType questionType, bool showInputField)
        {
            ClearQuestion();
            questionTextUI.text = question;
            this.showInputField = showInputField;

            if (questionType == GamebuildFeedback.QuestionType.Linear)
                LinearPanel.SetActive(true);
            else
                ReactionPanel.SetActive(true);
        }

        private void ClearQuestion()
        {
            ResetPanel();

            questionTextUI.text = string.Empty;
            questionInputField.text = string.Empty;
            LinearPanel.SetActive(false);
            ReactionPanel.SetActive(false);
        }

        private void HidePopUp()
        {
            StartCoroutine(HidePopUpAfterPress());
        }
        
        [Serializable]
        public class FeedbackData
        {
            public List<string> tags;
            public string feedbackText;
            public string feedbackQuestion;
            public List<string> feedbackAdditional;

        }


        public void OnSubmitPressed()
        {
            FeedbackData feedbackData = new FeedbackData
            {
                feedbackQuestion = questionTextUI.text,
                feedbackText = questionInputField.text,
                feedbackAdditional = additionalFeedback
            };
            
            List<string> tags = new List<string>();
            tags.Add(lastPressedReactionController.reactionText.text);
            feedbackData.tags = tags;

            if (nextRunOnSubmit != null)
            {
                nextRunOnSubmit?.Invoke();
            }
            string jsonFeedbackData = JsonUtility.ToJson(feedbackData);

            gameBuildData.CaptureDataAndSend(jsonFeedbackData, true);
            HidePopUp();
            
        }

        private IEnumerator HidePopUpAfterPress()
        {
            if (playChime)
            {
                chimeAudioSource.pitch = 0.8f;
                chimeAudioSource.PlayOneShot(chimeSound);
            }

            StartCoroutine(TypeText("Thanks for the feedback!"));
            yield return new WaitForSeconds(1.5f);
            GetComponent<RectTransform>().DOAnchorPosX(130f, 1f);
            customFeedbackPanelShowing = false;
        }

        private IEnumerator TypeText(string textToType)
        {
            questionTextUI.text = "";
            foreach (char character in textToType)
            {
                questionTextUI.text += character;
                yield return new WaitForSeconds(delayBetweenCharacters);
            }
        }

        void ResetPanel()
        {

            if (lastPressedReactionController != null)
            {
                lastPressedReactionController.backgroundImage.color = Color.clear;
                if(lastPressedReactionController.reactionIcon != null)
                    lastPressedReactionController.reactionIcon.color = iconColor;

                if (lastPressedReactionController.buttonType == ReactionButtonController.ButtonType.Linear)
                {
                    lastPressedReactionController.reactionText.DOColor(iconColor, .5f);
                }
                else
                {
                    lastPressedReactionController.reactionText.DOColor(textColor, .5f);
                }

                QuestionReactionPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
            }

        }

        public void ShowPopUp()
        {
            //Reset the Panel
            ResetPanel();
            gameBuildData.CaptureScreenshot();
            // Debug.Log("Screenshot taken");
            //Check if there is another panel
            customFeedbackPanelShowing = true;
            //Move it to the left


            GetComponent<RectTransform>().DOAnchorPosX(-130f, 1f);
            if (playChime)
            {
                chimeAudioSource.pitch = 1f;
                chimeAudioSource.PlayOneShot(chimeSound);
            }
        }

        /// <summary>
        /// Called when a reaction button is pressed.
        /// </summary>
        public void OnReactionButtonPressed(ReactionButtonController reactionButtonController)
        {
            if (lastPressedReactionController != null)
            {
                lastPressedReactionController.backgroundImage.DOColor(Color.clear, .5f);
                if (lastPressedReactionController.reactionIcon != null)
                    lastPressedReactionController.reactionIcon.DOColor(iconColor, .5f);

                if (reactionButtonController.buttonType == ReactionButtonController.ButtonType.Linear)
                {
                    lastPressedReactionController.reactionText.DOColor(iconColor, .5f);
                }
                else
                {
                    lastPressedReactionController.reactionText.DOColor(textColor, .5f);
                }

            }

            reactionButtonController.backgroundImage.DOColor(Color.black, .5f);
            if (lastPressedReactionController != null && lastPressedReactionController.reactionIcon != null)
                reactionButtonController.reactionIcon.DOColor(Color.white, .5f);

            reactionButtonController.reactionText.DOColor(Color.white, .5f);

            lastPressedReactionController = reactionButtonController;

            if (playChime)
            {
                chimeAudioSource.pitch = UnityEngine.Random.Range(1.1f, 1.3f);
                chimeAudioSource.PlayOneShot(chimeSound);
            }


            if (showInputField)
            {
                questionInputField.gameObject.SetActive(true);
                QuestionReactionPanel.GetComponent<RectTransform>().DOAnchorPos3DY(107.9f, 1f);
            }
            else
            {
                QuestionReactionPanel.GetComponent<RectTransform>().DOAnchorPos3DY(22.7f, 0.5f);
            }

            if (!submitButton.interactable)
            {
                submitButton.interactable = true;
            }
        }

    }

}
