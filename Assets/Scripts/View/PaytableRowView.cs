using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotGame.View
{
    /// <summary>
    /// Holds cached references for a Paytable row's UI elements,
    /// avoiding the need for Transform.Find and TryGetComponent loops.
    /// </summary>
    public class PaytableRowView : MonoBehaviour
    {
        public Image Icon;
        public TMP_Text Text0;
        public TMP_Text Text1;
        public TMP_Text Text2;
    }
}
