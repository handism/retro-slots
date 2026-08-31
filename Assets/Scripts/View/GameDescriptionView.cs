using Cysharp.Threading.Tasks;
using DG.Tweening;
using SlotGame.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotGame.View
{
    /// <summary>ゲーム説明モーダルを表示する View。</summary>
    public class GameDescriptionView : ModalViewBase
    {
        [SerializeField]
        private Button closeButton;

        [SerializeField]
        private TMP_Text descriptionText;

        private AudioManager _audioManager;

        public event System.Action OnCloseRequested;

        protected override void Awake()
        {
            base.Awake();
            _audioManager = FindFirstObjectByType<AudioManager>();

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() =>
                {
                    PlayButtonClickSe();
                    OnCloseRequested?.Invoke();
                });
            }
        }

        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
            }
        }

        /// <summary>
        /// インスペクター未アサイン時にランタイムで UI を構築する。
        /// UIManager.ShowGameDescription() から AddComponent 直後に呼ぶ。
        /// </summary>
        public void Setup()
        {
            _audioManager ??= FindFirstObjectByType<AudioManager>();
            if (!TryGetComponent(out _canvasGroup))
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            // フルスクリーン暗幕
            if (!gameObject.TryGetComponent<RectTransform>(out var rect))
                rect = gameObject.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            if (!gameObject.TryGetComponent<Image>(out var bg))
                bg = gameObject.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.75f);
            bg.raycastTarget = true;

            // 中央パネル
            var panel = new GameObject("Panel", typeof(RectTransform));
            panel.transform.SetParent(transform, false);
            var panelRect = (RectTransform)panel.transform;
            panelRect.sizeDelta = new Vector2(800f, 600f);
            var panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0.12f, 0.12f, 0.18f, 1f);

            // タイトル
            var titleObj = new GameObject("Title", typeof(RectTransform));
            titleObj.transform.SetParent(panel.transform, false);
            var titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
            titleTxt.rectTransform.anchorMin = new Vector2(0f, 0.88f);
            titleTxt.rectTransform.anchorMax = new Vector2(1f, 1f);
            titleTxt.rectTransform.offsetMin = Vector2.zero;
            titleTxt.rectTransform.offsetMax = Vector2.zero;
            titleTxt.font = TMP_Settings.defaultFontAsset;
            titleTxt.text = "ゲーム説明";
            titleTxt.fontSize = 34f;
            titleTxt.alignment = TextAlignmentOptions.Center;
            titleTxt.color = Color.white;

            // スクロールビュー（説明文用）
            var scrollObj = new GameObject("ScrollView", typeof(RectTransform));
            scrollObj.transform.SetParent(panel.transform, false);
            var scrollRt = (RectTransform)scrollObj.transform;
            scrollRt.anchorMin = new Vector2(0.03f, 0.15f);
            scrollRt.anchorMax = new Vector2(0.97f, 0.87f);
            scrollRt.offsetMin = Vector2.zero;
            scrollRt.offsetMax = Vector2.zero;
            var scrollImg = scrollObj.AddComponent<Image>();
            scrollImg.color = new Color(1f, 1f, 1f, 0.05f);
            var sr = scrollObj.AddComponent<ScrollRect>();

            var viewport = new GameObject("Viewport", typeof(RectTransform));
            viewport.transform.SetParent(scrollObj.transform, false);
            var vpRect = (RectTransform)viewport.transform;
            vpRect.anchorMin = Vector2.zero;
            vpRect.anchorMax = Vector2.one;
            vpRect.offsetMin = Vector2.zero;
            vpRect.offsetMax = Vector2.zero;
            var vpImg = viewport.AddComponent<Image>();
            vpImg.color = new Color(0f, 0f, 0f, 0.01f);
            var vpMask = viewport.AddComponent<Mask>();
            vpMask.showMaskGraphic = false;

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            var contentRect = (RectTransform)content.transform;
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            var csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var textObj = new GameObject("Text", typeof(RectTransform));
            textObj.transform.SetParent(content.transform, false);
            descriptionText = textObj.AddComponent<TextMeshProUGUI>();
            descriptionText.rectTransform.anchorMin = Vector2.zero;
            descriptionText.rectTransform.anchorMax = Vector2.one;
            descriptionText.rectTransform.offsetMin = new Vector2(16f, 8f);
            descriptionText.rectTransform.offsetMax = new Vector2(-16f, -8f);
            descriptionText.font = TMP_Settings.defaultFontAsset;
            descriptionText.fontSize = 26f;
            descriptionText.textWrappingMode = TextWrappingModes.Normal;
            descriptionText.color = Color.white;

            sr.content = contentRect;
            sr.viewport = vpRect;
            sr.horizontal = false;
            sr.vertical = true;
            sr.scrollSensitivity = 30f;

            // 閉じるボタン
            var closeObj = new GameObject("CloseButton", typeof(RectTransform));
            closeObj.transform.SetParent(panel.transform, false);
            var closeRt = (RectTransform)closeObj.transform;
            closeRt.anchorMin = new Vector2(0.3f, 0.03f);
            closeRt.anchorMax = new Vector2(0.7f, 0.13f);
            closeRt.offsetMin = Vector2.zero;
            closeRt.offsetMax = Vector2.zero;
            var closeImg = closeObj.AddComponent<Image>();
            closeImg.color = new Color(0.35f, 0.35f, 0.35f, 1f);
            closeButton = closeObj.AddComponent<Button>();
            closeButton.onClick.AddListener(() =>
            {
                PlayButtonClickSe();
                OnCloseRequested?.Invoke();
            });

            var closeTxtObj = new GameObject("Text", typeof(RectTransform));
            closeTxtObj.transform.SetParent(closeObj.transform, false);
            var closeTxt = closeTxtObj.AddComponent<TextMeshProUGUI>();
            closeTxt.rectTransform.anchorMin = Vector2.zero;
            closeTxt.rectTransform.anchorMax = Vector2.one;
            closeTxt.rectTransform.offsetMin = Vector2.zero;
            closeTxt.rectTransform.offsetMax = Vector2.zero;
            closeTxt.font = TMP_Settings.defaultFontAsset;
            closeTxt.text = "閉じる";
            closeTxt.color = Color.white;
            closeTxt.alignment = TextAlignmentOptions.Center;
            closeTxt.fontSize = 28f;

            gameObject.SetActive(false);
        }

        public void SetDescription(string text)
        {
            if (descriptionText != null)
                descriptionText.text = text;
        }

        private void PlayButtonClickSe()
        {
            _audioManager ??= FindFirstObjectByType<AudioManager>();
            _audioManager?.PlaySE(SEType.ButtonClick);
        }
    }
}
