using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace SlotGame.View
{
    public class AutoSpinButtonRef : MonoBehaviour
    {
        public Button Button = null!;
        public TMP_Text Text = null!;
        public RectTransform RectTransform = null!;
        public EventTrigger EventTrigger = null!;
    }
}
