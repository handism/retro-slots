using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotGame.View
{
    /// <summary>
    /// Component attached to a paytable row to cache UI references and avoid runtime GC allocations.
    /// </summary>
    public class PaytableRowView : MonoBehaviour
    {
        public Image IconImage;
        public TMP_Text[] PayoutTexts;

        private bool _isInitialized = false;

        public void Initialize()
        {
            if (_isInitialized) return;

            PayoutTexts = GetComponentsInChildren<TMP_Text>(true)
                .Where(t => t.transform.parent == transform)
                .OrderBy(t => t.transform.GetSiblingIndex())
                .ToArray();

            var iconTransform = transform.Find("SymbolCell/Icon");
            IconImage = iconTransform != null ? iconTransform.GetComponent<Image>() : null;

            _isInitialized = true;
        }
    }
}
