using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SlotGame.Core;
using SlotGame.Data;
using SlotGame.Model;
using SlotGame.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace SlotGame.Tests.PlayMode
{
    public class SpinManagerExceptionTests
    {
        private class MockRandom : IRandomGenerator
        {
            public int[] Values;
            public int Index;
            public int Next(int min, int max) => Values[Index++ % Values.Length];
            public float NextFloat() => 0.5f;
        }

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            if (GameContextInitializer.Instance != null)
                UnityEngine.Object.Destroy(GameContextInitializer.Instance.gameObject);

            while (GameContextInitializer.Instance != null)
                yield return null;

            var go = new GameObject("[GameContextInitializer]");
            go.AddComponent<GameContextInitializer>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (GameContextInitializer.Instance != null)
                UnityEngine.Object.Destroy(GameContextInitializer.Instance.gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ExecuteSpin_WhenCanceled_StopsAllSpinningReelsAndRethrows() => UniTask.ToCoroutine(async () =>
        {
            // --- Setup ---
            var mockRandom = new MockRandom { Values = new[] { 0, 0, 0, 0, 0 } };
            GameContextInitializer.Instance.Provide(
                new GameState(1000, 9_999_999, new[] { 10, 20, 50, 100 }, 1000, 10),
                new SaveDataManager(),
                mockRandom,
                new SaveData { coins = 1000, betAmount = 10 });

            await SceneManager.LoadSceneAsync("Main", LoadSceneMode.Single);

            var spinManager = GameObject.FindFirstObjectByType<SpinManager>();
            var gameManager = GameObject.FindFirstObjectByType<GameManager>();

            // Extract required properties from GameManager via reflection to pass to ExecuteSpin
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            var reelStrips = (ReelStripData[])typeof(GameManager).GetField("reelStrips", flags).GetValue(gameManager);
            var paylineData = (PaylineData)typeof(GameManager).GetField("paylineData", flags).GetValue(gameManager);
            var payoutData = (PayoutTableData)typeof(GameManager).GetField("payoutData", flags).GetValue(gameManager);

            var config = GameContext.SaveDataManager.Config;

            var cts = new CancellationTokenSource();

            // --- Execute ---
            var task = spinManager.ExecuteSpin(
                reelStrips, paylineData, payoutData, 10, cts.Token,
                config.ReelCount, config.RowCount, config.MinMatch,
                config.BonusTriggerReels);

            // Wait a little bit to ensure all reels start spinning
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f));

            bool anySpinning = false;
            foreach (var reel in spinManager.Reels)
            {
                if (reel.IsSpinning) anySpinning = true;
            }
            Assert.IsTrue(anySpinning, "Reels should be spinning before cancellation");

            // Cancel the token
            cts.Cancel();

            bool threwException = false;
            try
            {
                await task;
            }
            catch (OperationCanceledException)
            {
                threwException = true;
            }

            // --- Verify ---
            Assert.IsTrue(threwException, "ExecuteSpin should rethrow OperationCanceledException");

            // Verify that all reels are stopped as part of the catch block in SpinManager
            foreach (var reel in spinManager.Reels)
            {
                Assert.IsFalse(reel.IsSpinning, "Reel should be stopped after cancellation");
            }
        });
    }
}
