#nullable enable
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SlotGame.View
{
    [RequireComponent(typeof(RectTransform))]
    public class AutoSpinButtonRef : MonoBehaviour
    {
        public Button Button = null!;
        public TMP_Text? Text;
        public RectTransform RectTransform = null!;
        public EventTrigger? EventTrigger;
    }
}
