using Cysharp.Threading.Tasks;
using DG.Tweening;
using SlotGame.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotGame.View
{
    /// <summary>設定画面（BGM/SE ボリューム・コインリセット）の View。</summary>
    public class SettingsView : ModalViewBase
    {
        [SerializeField]
        private Slider bgmSlider;

        [SerializeField]
        private Slider seSlider;

        [SerializeField]
        private TMP_Text bgmValueText;

        [SerializeField]
        private TMP_Text seValueText;

        [SerializeField]
        private Button resetCoinsButton;

        [SerializeField]
        private Button closeButton;

        private Button _descriptionButton;
        private AudioManager _audioManager;

        public event System.Action<float> OnBGMVolumeChanged;
        public event System.Action<float> OnSEVolumeChanged;
        public event System.Action OnResetCoinsRequested;
        public event System.Action OnDescriptionRequested;
        public event System.Action OnCloseRequested;

        protected override void Awake()
        {
            base.Awake();
            _audioManager = FindFirstObjectByType<AudioManager>();

            bgmSlider.onValueChanged.AddListener(v =>
            {
                bgmValueText.text = $"{(int)(v * 100)}%";
                OnBGMVolumeChanged?.Invoke(v);
            });

            seSlider.onValueChanged.AddListener(v =>
            {
                seValueText.text = $"{(int)(v * 100)}%";
                OnSEVolumeChanged?.Invoke(v);
            });

            resetCoinsButton.onClick.AddListener(() =>
            {
                PlayButtonClickSe();
                OnResetCoinsRequested?.Invoke();
            });

            // ゲーム説明ボタンを動的に作成
            CreateDescriptionButton();

            closeButton.onClick.AddListener(() =>
            {
                PlayButtonClickSe();
                OnCloseRequested?.Invoke();
            });
        }

        private void OnDestroy()
        {
            if (bgmSlider != null)
                bgmSlider.onValueChanged.RemoveAllListeners();
            if (seSlider != null)
                seSlider.onValueChanged.RemoveAllListeners();
            if (resetCoinsButton != null)
                resetCoinsButton.onClick.RemoveAllListeners();
            if (closeButton != null)
                closeButton.onClick.RemoveAllListeners();
            if (_descriptionButton != null)
                _descriptionButton.onClick.RemoveAllListeners();
        }

        private void CreateDescriptionButton()
        {
            if (resetCoinsButton == null)
                return;

            var btnGo = Instantiate(resetCoinsButton.gameObject, resetCoinsButton.transform.parent);
            btnGo.name = "DescriptionButton";

            var txt = btnGo.GetComponentInChildren<TMP_Text>();
            if (txt != null)
                txt.text = "ゲーム説明";

            _descriptionButton = btnGo.GetComponent<Button>();
            _descriptionButton.onClick.RemoveAllListeners();
            _descriptionButton.onClick.AddListener(() =>
            {
                PlayButtonClickSe();
                OnDescriptionRequested?.Invoke();
            });

            // レイアウト上の位置調整（Resetボタンの隣）
            btnGo.transform.SetSiblingIndex(resetCoinsButton.transform.GetSiblingIndex() + 1);
        }

        public void SetVolumes(float bgm, float se)
        {
            bgmSlider.SetValueWithoutNotify(bgm);
            seSlider.SetValueWithoutNotify(se);
            bgmValueText.text = $"{(int)(bgm * 100)}%";
            seValueText.text = $"{(int)(se * 100)}%";
        }

        private void PlayButtonClickSe()
        {
            _audioManager ??= FindFirstObjectByType<AudioManager>();
            _audioManager?.PlaySE(SEType.ButtonClick);
        }
    }
}
