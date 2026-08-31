using Cysharp.Threading.Tasks;
using DG.Tweening;
using SlotGame.Audio;
using SlotGame.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotGame.View
{
    /// <summary>セッション統計パネルの View。</summary>
    public class StatsView : BasePopupView
    {
        [SerializeField]
        private TMP_Text totalSpinsText;

        [SerializeField]
        private TMP_Text winsText;

        [SerializeField]
        private TMP_Text winRateText;

        [SerializeField]
        private TMP_Text largestWinText;

        [SerializeField]
        private TMP_Text freeSpinTriggersText;

        [SerializeField]
        private TMP_Text netProfitText;

        [SerializeField]
        private Button closeButton;

        private AudioManager _audioManager;

        public event System.Action OnCloseRequested;

        private void Awake()
        {
            _audioManager = FindFirstObjectByType<AudioManager>();
            InitializeCanvasGroup();

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

        /// <summary>統計値を画面に反映する。</summary>
        public void UpdateDisplay(in SessionStats stats)
        {
            if (totalSpinsText != null)
                totalSpinsText.text = stats.TotalSpins.ToString();

            if (winsText != null)
                winsText.text = stats.Wins.ToString();

            if (winRateText != null)
                winRateText.text = $"{stats.WinRate:F1}%";

            if (largestWinText != null)
                largestWinText.text = stats.LargestWin.ToString();

            if (freeSpinTriggersText != null)
                freeSpinTriggersText.text = stats.FreeSpinTriggers.ToString();

            if (netProfitText != null)
            {
                string sign = stats.NetProfit >= 0 ? "+" : "";
                netProfitText.text = $"{sign}{stats.NetProfit}";
                netProfitText.color = stats.NetProfit >= 0 ? new Color(0.2f, 1f, 0.4f) : new Color(1f, 0.35f, 0.35f);
            }
        }

        private void PlayButtonClickSe()
        {
            _audioManager ??= FindFirstObjectByType<AudioManager>();
            _audioManager?.PlaySE(SEType.ButtonClick);
        }
    }
}
