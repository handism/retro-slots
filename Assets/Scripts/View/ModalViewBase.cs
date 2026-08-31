using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace SlotGame.View
{
    /// <summary>Base class for modal views sharing common open/close animations.</summary>
    public abstract class ModalViewBase : MonoBehaviour
    {
        protected CanvasGroup _canvasGroup;

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
        }

        public virtual async UniTask ShowAsync(System.Threading.CancellationToken ct = default)
        {
            gameObject.SetActive(true);
            transform.localScale = Vector3.one * 0.9f;
            _canvasGroup.alpha = 0f;

            await UniTask.WhenAll(
                DOTween
                    .To(() => _canvasGroup.alpha, x => _canvasGroup.alpha = x, 1f, 0.2f)
                    .SetEase(Ease.OutQuad)
                    .ToUniTask(cancellationToken: ct),
                transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack).ToUniTask(cancellationToken: ct)
            );
        }

        public virtual async UniTask HideAsync(System.Threading.CancellationToken ct = default)
        {
            await UniTask.WhenAll(
                DOTween
                    .To(() => _canvasGroup.alpha, x => _canvasGroup.alpha = x, 0f, 0.15f)
                    .SetEase(Ease.InQuad)
                    .ToUniTask(cancellationToken: ct),
                transform.DOScale(0.9f, 0.15f).SetEase(Ease.InBack).ToUniTask(cancellationToken: ct)
            );
            gameObject.SetActive(false);
        }
    }
}
