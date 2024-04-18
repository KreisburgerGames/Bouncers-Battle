using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gamebuild.Feedback
{
    public class ReactionButtonController : MonoBehaviour
    {
        [Serializable]
        public enum ButtonType
        {
            Linear,
            Reaction
        }

        public ButtonType buttonType;
        public Image backgroundImage;
        public Image reactionIcon;
        public TMP_Text reactionText;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}
