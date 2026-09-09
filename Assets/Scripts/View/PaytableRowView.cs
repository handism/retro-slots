using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SlotGame.View
{
    /// <summary>
    /// Holds pre-cached component references for a paytable row to avoid costly dynamic lookups.
    /// </summary>
    public class PaytableRowView : MonoBehaviour
    {
        public Image IconImage;
        public TMP_Text Text0;
        public TMP_Text Text1;
        public TMP_Text Text2;
    }
}
