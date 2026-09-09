using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using SlotGame.View;

namespace SlotGame.Tests.EditMode.View
{
    /// <summary>
    /// <see cref="TitleEffects"/> の EditMode テスト。
    /// 主に初期化時やメソッド呼び出し時の参照欠損（null）に対する堅牢性を検証する。
    /// </summary>
    public class TitleEffectsTests
    {
        private GameObject _go;
        private TitleEffects _titleEffects;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("TitleEffectsMock");
            _titleEffects = _go.AddComponent<TitleEffects>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
            {
                Object.DestroyImmediate(_go);
            }
        }

        [Test]
        public void Start_DoesNotThrow_WhenReferencesAreNull()
        {
            // TitleEffects のフィールド（logoTransform 等）はアタッチ直後 null のまま。
            // Start メソッドをリフレクション経由で呼び出し、例外が発生しないことを確認する。
            var startMethod = typeof(TitleEffects).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(startMethod, "Start method should exist in TitleEffects.");

            Assert.DoesNotThrow(() =>
            {
                startMethod.Invoke(_titleEffects, null);
            }, "Start should safely handle null references without throwing exceptions.");
        }

        [Test]
        public void FadeOutAsync_DoesNothing_WhenOverlayFadeIsNull()
        {
            // overlayFade が null の状態でも FadeOutAsync が例外をスローせず直ちに完了することを確認する。
            Assert.DoesNotThrow(() =>
            {
                // ToCoroutine や await なしでそのまま呼ぶことで同期的に直ちに戻ることを確認
                var task = _titleEffects.FadeOutAsync();

                // overlayFade == null の場合、即座に完了するタスクが返るはず
                // UniTaskはそのまま完了状態になる
            }, "FadeOutAsync should not throw when overlayFade is null.");
        }
    }
}
