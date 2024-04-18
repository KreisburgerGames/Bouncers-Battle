using DG.Tweening;
using System.Collections;
using UnityEngine;


namespace Gamebuild.Feedback
{
    public class ReminderController : MonoBehaviour
    {
        [Header("Reminder Settings")]
        public bool ReminderEnabled;
        public float showReminderEvery;


        public RectTransform bellIcon;
        public AudioClip bellSound;
        public bool playBellSound = false;
        float reminderTimer;

        GamebuildFeedback gamebuildFeedback;
        // Start is called before the first frame update
        void Start()
        {
            reminderTimer = showReminderEvery;
        }

        private void Update()
        {
            if (!ReminderEnabled)
                return;

            if (reminderTimer > 0)
            {
                reminderTimer -= Time.deltaTime;
            }
            else
            {
                ShowPopUp();
                reminderTimer = showReminderEvery;
            }
        }



        public void ShowPopUp()
        {
            if (gamebuildFeedback.panelShowing)
                return;
            //Check if there is another panel
            //Move it to the left
            StartCoroutine(ShowPopUpCoroutine());


        }

        private IEnumerator ShowPopUpCoroutine()
        {
            RectTransform x = GetComponent<RectTransform>();
            x.DOAnchorPosX(-130f, 1f).OnComplete(() =>
            {
                if (playBellSound)
                {
                    gamebuildFeedback.chimeAudioSource.pitch = 1f;
                    gamebuildFeedback.chimeAudioSource.PlayOneShot(bellSound);
                }

                bellIcon.DOShakeRotation(1f, new Vector3(0f, 0f, 10f), 6, 90, true, ShakeRandomnessMode.Harmonic);
            });




            yield return new WaitForSeconds(4f);
            GetComponent<RectTransform>().DOAnchorPosX(130f, 1f);
        }
    }
}