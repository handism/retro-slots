using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;

namespace SlotGame.View
{
    /// <summary>
    /// タイトルシーンのUI演出（ブレス演出、光彩、アニメーション）を担当するクラス。
    /// </summary>
    public class TitleEffects : MonoBehaviour
    {
        [Header("Logo Animation")]
        [SerializeField] private RectTransform logoTransform;
        [SerializeField] private float breathingSpeed = 2f;
        [SerializeField] private float breathingAmount = 0.05f;

        [Header("Start Button Animation")]
        [SerializeField] private RectTransform startButtonTransform;
        [SerializeField] private CanvasGroup startButtonCanvasGroup;
        [SerializeField] private float pulseSpeed = 1.5f;
        [SerializeField] private float pulseMinAlpha = 0.6f;

        [Header("Background Effects")]
        [SerializeField] private Image overlayFade;
        [SerializeField] private RectTransform[] backgroundGlows;
        [SerializeField] private float glowRotationSpeed = 10f;

        [Header("Floating Symbols")]
        [SerializeField] private RectTransform[] floatingSymbols;
        [SerializeField] private float floatAmount = 20f;
        [SerializeField] private float floatSpeed = 1f;

        private void Start()
        {
            StartAnimations();
        }

        private void OnDestroy()
        {
            DOTween.Kill(this);
        }

        private void StartAnimations()
        {
            float breathHalfPeriod = Mathf.PI / breathingSpeed;
            if (logoTransform != null)
            {
                logoTransform.DOScale(1f + breathingAmount, breathHalfPeriod)
                    .From(1f - breathingAmount)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
                    .SetTarget(this);
            }

            float pulseHalfPeriod = Mathf.PI / pulseSpeed;
            if (startButtonCanvasGroup != null)
            {
                DOTween.To(() => startButtonCanvasGroup.alpha, a => startButtonCanvasGroup.alpha = a, 1f, pulseHalfPeriod)
                    .From(pulseMinAlpha)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
                    .SetTarget(this);
            }
            if (startButtonTransform != null)
            {
                startButtonTransform.DOScale(1.02f, pulseHalfPeriod)
                    .From(0.98f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
                    .SetTarget(this);
            }

            if (backgroundGlows != null)
            {
                float rotationPeriod = 360f / glowRotationSpeed;
                for (int i = 0; i < backgroundGlows.Length; i++)
                {
                    if (backgroundGlows[i] == null) continue;
                    float direction = (i % 2 == 0) ? 1f : -1f;
                    backgroundGlows[i]
                        .DORotate(new Vector3(0f, 0f, direction * 360f), rotationPeriod, RotateMode.FastBeyond360)
                        .SetLoops(-1, LoopType.Restart)
                        .SetEase(Ease.Linear)
                        .SetTarget(this);
                }
            }

            if (floatingSymbols != null)
            {
                float floatHalfPeriod = Mathf.PI / floatSpeed;
                for (int i = 0; i < floatingSymbols.Length; i++)
                {
                    if (floatingSymbols[i] == null) continue;
                    var sym = floatingSymbols[i];
                    float startY = sym.anchoredPosition.y;
                    DOTween.To(
                            () => sym.anchoredPosition.y,
                            y => sym.anchoredPosition = new Vector2(sym.anchoredPosition.x, y),
                            startY + floatAmount,
                            floatHalfPeriod)
                        .From(startY - floatAmount)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(Ease.InOutSine)
                        .SetDelay(i * floatHalfPeriod * 0.5f)
                        .SetTarget(this);
                }
            }
        }

        public async UniTask FadeOutAsync(CancellationToken ct = default)
        {
            if (overlayFade == null) return;

            overlayFade.gameObject.SetActive(true);
            await DOTween.To(
                    () => overlayFade.color.a,
                    a => overlayFade.color = new Color(0f, 0f, 0f, a),
                    1f,
                    0.5f)
                .SetEase(Ease.Linear)
                .ToUniTask(cancellationToken: ct);
        }
    }
}
