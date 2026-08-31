using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using SlotGame.Audio;
using SlotGame.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotGame.View
{
    /// <summary>配当テーブルを ScrollView で動的生成して表示する View。</summary>
    public class PaytableView : ModalViewBase
    {
        public const float SymbolColumnWidth = 120f;
        public const float ColumnWidth = 112f;
        public const float ColumnSpacing = 20f;
        public const float RowHeight = 60f;
        public const float RowSidePadding = 12f;
        public const float IconSize = 44f;

        [SerializeField]
        private Transform contentRoot;

        [SerializeField]
        private GameObject rowPrefab; // Image + TMP_Text × 3（3/4/5 揃え）

        [SerializeField]
        private Button closeButton;

        private AudioManager _audioManager;

        public event System.Action OnCloseRequested;

        protected override void Awake()
        {
            base.Awake();
            _audioManager = FindFirstObjectByType<AudioManager>();

            closeButton.onClick.AddListener(() =>
            {
                PlayButtonClickSe();
                OnCloseRequested?.Invoke();
            });
        }

        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
            }
        }

        public void Populate(SymbolData[] symbols, PayoutTableData payoutData)
        {
            EnsureRowPrefab();
            if (rowPrefab == null || contentRoot == null)
                return;

            // RowTemplate: childControlWidth=true にして preferredWidth で列幅を制御
            var rowHlg = rowPrefab.GetComponent<HorizontalLayoutGroup>();
            if (rowHlg != null)
                rowHlg.childControlWidth = true;

            // RowTemplate のペイアウト列幅を ColumnWidth に統一（0番目はシンボル列なのでスキップ）
            int rowColIdx = 0;
            foreach (Transform child in rowPrefab.transform)
            {
                if (rowColIdx > 0)
                {
                    var le = child.GetComponent<LayoutElement>();
                    if (le != null)
                        le.preferredWidth = ColumnWidth;
                }
                rowColIdx++;
            }

            // HeaderRow: childControlWidth=true にして同じ列幅を適用
            foreach (var hlg in GetComponentsInChildren<HorizontalLayoutGroup>(true))
            {
                if (hlg.gameObject.name != "HeaderRow")
                    continue;
                hlg.childControlWidth = true;
                int headerColIdx = 0;
                foreach (Transform child in hlg.transform)
                {
                    if (headerColIdx > 0)
                    {
                        var le = child.GetComponent<LayoutElement>();
                        if (le != null)
                            le.preferredWidth = ColumnWidth;
                        var txt = child.GetComponent<TMP_Text>();
                        if (txt != null)
                            txt.alignment = TextAlignmentOptions.Right;
                    }
                    headerColIdx++;
                }
                break;
            }

            // 既存の行を削除
            var staleRows = new List<GameObject>();
            foreach (Transform child in contentRoot)
            {
                if (child == null)
                    continue;
                if (rowPrefab != null && child.gameObject == rowPrefab)
                    continue;
                staleRows.Add(child.gameObject);
            }

            foreach (var staleRow in staleRows)
                Destroy(staleRow);

            foreach (var sym in symbols)
            {
                // Only show normal symbol payouts in the paytable UI
                if (sym.type != SymbolType.Normal)
                    continue;

                var row = Instantiate(rowPrefab, contentRoot);
                row.SetActive(true);
                row.name = $"Row_{sym.symbolName}";

                var rowRect = row.GetComponent<RectTransform>();
                if (rowRect != null)
                {
                    rowRect.localScale = Vector3.one;
                    rowRect.anchoredPosition3D = Vector3.zero;
                }

                TMP_Text text0 = null;
                TMP_Text text1 = null;
                TMP_Text text2 = null;
                int textIdx = 0;

                foreach (Transform child in row.transform)
                {
                    if (child.TryGetComponent<TMP_Text>(out var tmp))
                    {
                        if (textIdx == 0)
                            text0 = tmp;
                        else if (textIdx == 1)
                            text1 = tmp;
                        else if (textIdx == 2)
                            text2 = tmp;
                        textIdx++;
                    }
                }

                var iconTransform = row.transform.Find("SymbolCell/Icon");
                var img = iconTransform != null ? iconTransform.GetComponent<Image>() : null;

                if (img != null)
                {
                    img.sprite = sym.sprite;
                    img.preserveAspect = true;
                    img.SetNativeSize();

                    var iconRect = img.rectTransform;
                    iconRect.sizeDelta = new Vector2(IconSize, IconSize);
                }

                if (text0 != null)
                    text0.text = sym.payouts.Length > 0 ? sym.payouts[0].ToString("N0") : "-";
                if (text1 != null)
                    text1.text = sym.payouts.Length > 1 ? sym.payouts[1].ToString("N0") : "-";
                if (text2 != null)
                    text2.text = sym.payouts.Length > 2 ? sym.payouts[2].ToString("N0") : "-";
            }

            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)contentRoot);
        }

        private void EnsureRowPrefab()
        {
            if (rowPrefab != null)
                return;

            rowPrefab = CreateFallbackRowPrefab();
        }

        private GameObject CreateFallbackRowPrefab()
        {
            var row = new GameObject(
                "RuntimeRowPrefab",
                typeof(RectTransform),
                typeof(Image),
                typeof(HorizontalLayoutGroup),
                typeof(LayoutElement)
            );
            row.SetActive(false);
            row.hideFlags = HideFlags.HideAndDontSave;

            var rowImage = row.GetComponent<Image>();
            rowImage.color = new Color(1f, 1f, 1f, 0.08f);

            var rowLayout = row.GetComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = ColumnSpacing;
            rowLayout.padding = new RectOffset((int)RowSidePadding, (int)RowSidePadding, 6, 6);
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.childControlWidth = false;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;

            row.GetComponent<LayoutElement>().preferredHeight = RowHeight;

            CreateSymbolCell(row.transform);
            CreateValueText(row.transform, "Payout3");
            CreateValueText(row.transform, "Payout4");
            CreateValueText(row.transform, "Payout5");

            return row;
        }

        private static void CreateSymbolCell(Transform parent)
        {
            var cell = new GameObject("SymbolCell", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            cell.transform.SetParent(parent, false);
            cell.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.02f);
            cell.GetComponent<LayoutElement>().preferredWidth = SymbolColumnWidth;

            var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(cell.transform, false);
            var iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = Vector2.zero;
            iconRect.sizeDelta = new Vector2(IconSize, IconSize);
        }

        private static void CreateValueText(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<LayoutElement>().preferredWidth = ColumnWidth;

            var text = go.AddComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.text = "-";
            text.fontSize = 24f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(ColumnWidth, 44f);
        }

        private void PlayButtonClickSe()
        {
            _audioManager ??= FindFirstObjectByType<AudioManager>();
            _audioManager?.PlaySE(SEType.ButtonClick);
        }
    }
}
