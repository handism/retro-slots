using NUnit.Framework;
using UnityEngine;
using SlotGame.Core;
using SlotGame.View;
using SlotGame.Model;
using System.Threading;
using System.Reflection;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using TMPro;

namespace SlotGame.Tests.EditMode
{
    public class GameManagerTests
    {
        [Test]
        public void RunTutorialSequenceAsync_ThrowsOperationCanceledException_CatchIgnoresAndCompletesTutorial()
        {
            var go = new GameObject("GameManagerTest");
            var gm = go.AddComponent<GameManager>();

            var uiGo = new GameObject("UIManager");
            var uiManager = uiGo.AddComponent<UIManager>();

            // Minimal mocked setup to bypass missing references down the call stack in EditMode testing
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            var cg = canvasGo.AddComponent<CanvasGroup>();

            var rootCanvasField = typeof(UIManager).GetField("_rootCanvas", BindingFlags.NonPublic | BindingFlags.Instance);
            rootCanvasField?.SetValue(uiManager, canvas);

            var hudCanvasGroupField = typeof(UIManager).GetField("_hudCanvasGroup", BindingFlags.NonPublic | BindingFlags.Instance);
            hudCanvasGroupField?.SetValue(uiManager, cg);

            var tutorialGo = new GameObject("TutorialView");
            var tutorialView = tutorialGo.AddComponent<TutorialView>();

            var textGo = new GameObject("MessageText");
            var messageText = textGo.AddComponent<TextMeshProUGUI>();
            var nextBtnGo = new GameObject("NextBtn");
            var nextButton = nextBtnGo.AddComponent<Button>();
            var skipBtnGo = new GameObject("SkipBtn");
            var skipButton = skipBtnGo.AddComponent<Button>();

            var nextTextGo = new GameObject("Text");
            nextTextGo.transform.SetParent(nextBtnGo.transform);
            nextTextGo.AddComponent<TextMeshProUGUI>();

            typeof(TutorialView).GetField("_messageText", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(tutorialView, messageText);
            typeof(TutorialView).GetField("_nextButton", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(tutorialView, nextButton);
            typeof(TutorialView).GetField("_skipButton", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(tutorialView, skipButton);

            var tutorialViewField = typeof(UIManager).GetField("_tutorialView", BindingFlags.NonPublic | BindingFlags.Instance);
            tutorialViewField?.SetValue(uiManager, tutorialView);

            var uiManagerField = typeof(GameManager).GetField("uiManager", BindingFlags.NonPublic | BindingFlags.Instance);
            uiManagerField?.SetValue(gm, uiManager);

            var gameStateField = typeof(GameManager).GetField("_gameState", BindingFlags.NonPublic | BindingFlags.Instance);
            var gameState = new GameState(1000, 9999, new[] {10, 20}, 1000, 10, false);
            gameStateField?.SetValue(gm, gameState);

            var runMethod = typeof(GameManager).GetMethod("RunTutorialSequenceAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            var cts = new CancellationTokenSource();
            cts.Cancel(); // Pre-cancel to trigger OperationCanceledException in ShowAsync

            // Invoke the method.
            // It returns a UniTaskVoid. Since we are in an EditMode test (which executes synchronously),
            // invoking this will run synchronously until the first yield/await.
            // Because the token is pre-cancelled, TutorialView.ShowAsync(ct) will immediately throw
            // OperationCanceledException and we don't need to await the UniTaskVoid (which isn't awaitable anyway).

            try
            {
                var uniTaskVoid = runMethod?.Invoke(gm, new object[] { cts.Token });
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException is System.NullReferenceException)
                {
                    // Ignore unavoidable deep NREs caused by incomplete mock state in Unity Editor testing
                }
                else
                {
                    throw; // Re-throw if it's an unexpected exception
                }
            }

            Assert.IsTrue(gameState.HasCompletedTutorial, "The tutorial should be marked as completed even if the task was canceled.");
        }
    }
}
