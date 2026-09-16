using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using SlotGame.View;
using SlotGame.Data;
using Cysharp.Threading.Tasks;

namespace SlotGame.Tests.PlayMode.View
{
    public class UIManagerTests
    {
        // Use a mock to verify it actually gets called
        private class MockWinPopupView : WinPopupView
        {
            public bool ShowCalled { get; private set; }
            public long LastAmount { get; private set; }
            public WinLevel LastLevel { get; private set; }

            public override UniTask Show(long amount, WinLevel level, System.Threading.CancellationToken ct)
            {
                ShowCalled = true;
                LastAmount = amount;
                LastLevel = level;
                return UniTask.CompletedTask;
            }
        }

        private GameObject _uiManagerGo;
        private UIManager _uiManager;
        private GameObject _winPopupGo;
        private MockWinPopupView _winPopup;

        [SetUp]
        public void SetUp()
        {
            _uiManagerGo = new GameObject("UIManager");
            _uiManager = _uiManagerGo.AddComponent<UIManager>();

            _winPopupGo = new GameObject("WinPopup");
            _winPopup = _winPopupGo.AddComponent<MockWinPopupView>();

            var winPopupField = typeof(UIManager).GetField("winPopup", BindingFlags.NonPublic | BindingFlags.Instance);
            winPopupField.SetValue(_uiManager, _winPopup);
        }

        [TearDown]
        public void TearDown()
        {
            if (_winPopupGo != null) Object.Destroy(_winPopupGo);
            if (_uiManagerGo != null) Object.Destroy(_uiManagerGo);
        }

        [UnityTest]
        public IEnumerator ShowWinAmount_CallsWinPopupShow_WithCorrectParameters() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            long expectedAmount = 500;
            var expectedLevel = WinLevel.Big;

            // Act
            await _uiManager.ShowWinAmount(expectedAmount, expectedLevel);

            // Assert
            Assert.IsTrue(_winPopup.ShowCalled, "Show should be called on winPopup");
            Assert.AreEqual(expectedAmount, _winPopup.LastAmount, "Amount should be passed correctly");
            Assert.AreEqual(expectedLevel, _winPopup.LastLevel, "WinLevel should be passed correctly");
        });

        [UnityTest]
        public IEnumerator ShowWinAmount_WhenWinPopupIsNull_DoesNotThrow() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            var uiManagerGo = new GameObject("UIManagerNoPopup");
            var uiManager = uiManagerGo.AddComponent<UIManager>();
            // winPopup is null by default

            bool didNotThrow = true;

            // Act
            try
            {
                await uiManager.ShowWinAmount(100, WinLevel.Small);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception thrown: {e}");
                didNotThrow = false;
            }

            // Assert
            Assert.IsTrue(didNotThrow, "ShowWinAmount should not throw when winPopup is null");

            Object.Destroy(uiManagerGo);
        });
    }
}
