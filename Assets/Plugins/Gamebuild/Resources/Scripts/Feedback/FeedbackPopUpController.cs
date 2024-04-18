using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace Gamebuild.Feedback
{
    public class FeedbackPopUpController : MonoBehaviour
    {
        // Public Fields
        public GamebuildFeedback gamebuildFeedback;
        public TMP_InputField inputField;
        public TMP_Text feedbackTitleText;
        public Button submitButton;
        public GameBuildData gameBuildData;
        public Color iconColor;
        public Color textColor;

        // Private Fields
        private ReactionButtonController lastPressedReactionController;
        public bool feedbackPanelShowing;

        public void ShowPopUp()
        {
            ResetPanel();
            gameBuildData.CaptureScreenshot();
            feedbackPanelShowing = true;
            
            

            MovePanel(-130f);
            PlayChime(1f);
        }

        public void HidePopUp()
        {
            MovePanel(130f);
            PlayChime(1f);
        }

        private void ResetPanel()
        {
            feedbackTitleText.text = "Feedback";
            inputField.text = string.Empty;

            if (lastPressedReactionController != null)
            {
                ResetReactionButton(lastPressedReactionController);
            }
        }

        public void OnFeedbackTypePressed(ReactionButtonController reactionButtonController)
        {
            if (lastPressedReactionController != null)
            {
                ResetReactionButton(lastPressedReactionController);
            }

            SetReactionButton(reactionButtonController);
            lastPressedReactionController = reactionButtonController;

            PlayChime(UnityEngine.Random.Range(1.1f, 1.3f));

            submitButton.interactable = true;
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
                feedbackText = inputField.text
            };
            List<string> tags = new List<string>();
            tags.Add(lastPressedReactionController.reactionText.text);
            feedbackData.tags = tags;
            string jsonFeedbackData = JsonUtility.ToJson(feedbackData);

            gameBuildData.CaptureDataAndSend(jsonFeedbackData, true);

            HidePopUpAfterSubmit();
        }

        private void HidePopUpAfterSubmit()
        {
            StartCoroutine(HidePopUpAfterPress());
        }

        private IEnumerator HidePopUpAfterPress()
        {
            PlayChime(0.8f);
            StartCoroutine(TypeText("Thank you for the feedback!"));
            yield return new WaitForSeconds(1.5f);
            MovePanel(130f);
            feedbackPanelShowing = false;
        }

        private IEnumerator TypeText(string textToType)
        {
            feedbackTitleText.text = "";
            foreach (char character in textToType)
            {
                feedbackTitleText.text += character;
                yield return new WaitForSeconds(0.03f);
            }
        }

        private void PlayChime(float pitch)
        {
            if (gamebuildFeedback.playChime)
            {
                gamebuildFeedback.chimeAudioSource.pitch = pitch;
                gamebuildFeedback.chimeAudioSource.PlayOneShot(gamebuildFeedback.chimeSound);
            }
        }

        private void MovePanel(float position)
        {
            GetComponent<RectTransform>().DOAnchorPosX(position, 1f);
        }

        private void ResetReactionButton(ReactionButtonController controller)
        {
            controller.backgroundImage.DOColor(Color.clear, .5f);
            controller.reactionIcon.DOColor(iconColor, .5f);
            controller.reactionText.DOColor(textColor, .5f);
        }

        private void SetReactionButton(ReactionButtonController controller)
        {
            controller.backgroundImage.DOColor(Color.black, .5f);
            controller.reactionIcon.DOColor(Color.white, .5f);
            controller.reactionText.DOColor(Color.white, .5f);
        }
    }
}
