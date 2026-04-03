#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using SlotGame.Data;
using UnityEngine;
using UnityEngine.UI;

namespace SlotGame.View
{
    /// <summary>シンボル 1 個の表示を担う View（オブジェクトプール管理対象）。</summary>
    [RequireComponent(typeof(Image))]
    public class SymbolView : MonoBehaviour
    {
        private const string IdleStateName = "Idle";

        private Image?         _image;
        private Animator?      _animator;
        private int           _symbolId;
        private AnimationClip? _winAnim;
        private Tween?         _pulseTween;

        private void Awake()
        {
            _image    = GetComponent<Image>();
            _animator = GetComponent<Animator>();
        }

        public int SymbolId => _symbolId;

        public void SetSymbol(SymbolData data)
        {
            _symbolId    = data.symbolId;
            if (_image != null)
            {
                _image.sprite = data.sprite;
                _image.enabled = true;
            }
            _winAnim     = data.winAnim;
        }

        public void SetSymbolId(int id) => _symbolId = id;

        public void SetHighlighted(bool highlighted)
        {
            _image ??= GetComponent<Image>();
            if (_image != null)
            {
                _image.color = highlighted ? Color.white : new Color(1f, 1f, 1f, 0.3f);
            }
        }

        public void ResetHighlight()
        {
            _image ??= GetComponent<Image>();
            if (_image != null)
            {
                _image.color = Color.white;
            }
            PlayIdleAnimation();
        }

        /// <summary>
        /// 当選演出（Animator または パルス）を開始する。
        /// 演出の完了を待機する。
        /// </summary>
        public async UniTask PlayWinPresentation(CancellationToken ct)
        {
            if (_animator != null && _winAnim != null)
            {
                // 専用アニメーションがある場合はそちらを優先
                await PlayWinAnimationClip(_winAnim, ct);
            }
            else
            {
                // ない場合はパルス演出（無限ループ）
                PlayPulseAnimation();
            }
        }

        /// <summary>パルスアニメーション（拡縮繰り返し）を開始する。</summary>
        public void PlayPulseAnimation()
        {
            // If this object has been destroyed (Unity overloads ==), bail out.
            if (this == null) return;
            StopPulseAnimation();
            try
            {
                _pulseTween = transform.DOScale(1.2f, 0.5f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            }
            catch (MissingReferenceException)
            {
                // transform was destroyed during teardown - ignore
                _pulseTween = null;
            }
        }

        /// <summary>パルスアニメーションを停止する。</summary>
        public void StopPulseAnimation()
        {
            if (_pulseTween != null && _pulseTween.IsActive())
            {
                try { _pulseTween.Kill(); } catch { }
            }
            _pulseTween = null;

            // Avoid accessing transform if the Unity object is already destroyed.
            if (this == null) return;
            try
            {
                transform.localScale = Vector3.one;
            }
            catch (MissingReferenceException)
            {
                // destroyed during teardown
            }
        }

        /// <summary>アイドル状態のアニメーションを再生する（当選演出の停止用）。</summary>
        public void PlayIdleAnimation()
        {
            if (this == null) return;
            StopPulseAnimation();
            if (_animator != null)
            {
                try { _animator.Play(IdleStateName, 0, 0f); } catch { }
            }
        }

        /// <summary>当選アニメーションを再生して完了を待機する。</summary>
        private async UniTask PlayWinAnimationClip(AnimationClip clip, CancellationToken ct)
        {
            if (_animator == null || clip == null) return;
            _animator.Play(clip.name);
            await UniTask.Delay((int)(clip.length * 1000), cancellationToken: ct);
        }

        private void OnDestroy()
        {
            StopPulseAnimation();
        }
    }
}
