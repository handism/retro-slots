using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace SlotGame.View
{
    /// <summary>
    /// UIポップアップの共通アニメーションを提供する基底クラス。
    /// </summary>
    public abstract class BasePopupView : MonoBehaviour
    {
        protected CanvasGroup _canvasGroup;

        /// <summary>
        /// CanvasGroup を初期化します。派生クラスの Awake() などで呼び出してください。
        /// </summary>
        protected void InitializeCanvasGroup()
        {
            if (!TryGetComponent(out _canvasGroup))
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            _canvasGroup.alpha = 0f;
        }

        public virtual async UniTask ShowAsync(System.Threading.CancellationToken ct = default)
        {
            gameObject.SetActive(true);
            transform.localScale = Vector3.one * 0.9f;

            if (_canvasGroup != null)
                _canvasGroup.alpha = 0f;

            await UniTask.WhenAll(
                DOTween
                    .To(
                        () => _canvasGroup != null ? _canvasGroup.alpha : 0f,
                        x =>
                        {
                            if (_canvasGroup != null)
                                _canvasGroup.alpha = x;
                        },
                        1f,
                        0.2f
                    )
                    .SetEase(Ease.OutQuad)
                    .ToUniTask(cancellationToken: ct),
                transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack).ToUniTask(cancellationToken: ct)
            );
        }

        public virtual async UniTask HideAsync(System.Threading.CancellationToken ct = default)
        {
            await UniTask.WhenAll(
                DOTween
                    .To(
                        () => _canvasGroup != null ? _canvasGroup.alpha : 1f,
                        x =>
                        {
                            if (_canvasGroup != null)
                                _canvasGroup.alpha = x;
                        },
                        0f,
                        0.15f
                    )
                    .SetEase(Ease.InQuad)
                    .ToUniTask(cancellationToken: ct),
                transform.DOScale(0.9f, 0.15f).SetEase(Ease.InBack).ToUniTask(cancellationToken: ct)
            );
            gameObject.SetActive(false);
        }
    }
}
