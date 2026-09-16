using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SlotGame.Core;
using SlotGame.Data;
using SlotGame.Model;
using SlotGame.Utility;
using UnityEngine;
using UnityEngine.TestTools;

namespace SlotGame.Tests.PlayMode
{
    public class BonusManagerTests
    {
        private class MockRandom : IRandomGenerator
        {
            public int[] Values;
            public int Index;
            public int Next(int min, int max)
            {
                if (Values != null && Values.Length > 0)
                {
                    return Values[Index++ % Values.Length];
                }
                return min;
            }
            public float NextFloat() => 0.5f;
        }

        private class MockSpinManager : SpinManager
        {
            public Queue<SpinResult> Results = new Queue<SpinResult>();

            public override UniTask<SpinResult> ExecuteSpin(
                ReelStripData[] strips,
                PaylineData paylines,
                PayoutTableData payouts,
                int betAmount,
                CancellationToken ct,
                int reelCount = 5,
                int rowCount = 3,
                int minMatch = 3,
                int[] bonusReels = null)
            {
                ct.ThrowIfCancellationRequested();
                if (Results.Count > 0)
                    return UniTask.FromResult(Results.Dequeue());

                return UniTask.FromResult(new SpinResult(null, null, 0, false, 0));
            }
        }

        private GameObject _bonusManagerGo;
        private BonusManager _bonusManager;
        private MockSpinManager _mockSpinManager;
        private GameState _gameState;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _bonusManagerGo = new GameObject("BonusManager");
            _bonusManager = _bonusManagerGo.AddComponent<BonusManager>();

            _mockSpinManager = _bonusManagerGo.AddComponent<MockSpinManager>();

            var spinManagerField = typeof(BonusManager).GetField("spinManager", BindingFlags.NonPublic | BindingFlags.Instance);
            spinManagerField.SetValue(_bonusManager, _mockSpinManager);

            _bonusManager.Initialize(new MockRandom(), new SlotConfig(1000, 9999999, new[] { 10 }, 5, 3, 3, new[] {0,2,4}, 2, 20, 100, 1f, 1f, 0f, 0f, 0f, 0f, "salt"));

            _gameState = new GameState(1000, 9999999, new[] { 10, 20, 50, 100 }, 1000, 10);

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_bonusManagerGo != null)
                UnityEngine.Object.Destroy(_bonusManagerGo);

            yield return null;
        }

        [UnityTest]
        public IEnumerator RunFreeSpins_ExecutesCorrectNumberOfSpins() => UniTask.ToCoroutine(async () =>
        {
            int spinCount = 3;
            int actualSpins = 0;

            for (int i = 0; i < spinCount; i++)
                _mockSpinManager.Results.Enqueue(new SpinResult(null, null, 0, false, 0));

            await _bonusManager.RunFreeSpins(
                _gameState,
                spinCount,
                new ReelStripData[0],
                ScriptableObject.CreateInstance<PaylineData>(),
                ScriptableObject.CreateInstance<PayoutTableData>(),
                (result, win) =>
                {
                    actualSpins++;
                    return UniTask.CompletedTask;
                },
                CancellationToken.None
            );

            Assert.AreEqual(0, _gameState.FreeSpins, "Free spins should be exhausted.");
            Assert.AreEqual(spinCount, actualSpins, "onSpin should be called exactly spinCount times.");
        });

        [UnityTest]
        public IEnumerator RunFreeSpins_WithScatter_AddsExtraSpins() => UniTask.ToCoroutine(async () =>
        {
            int initialCount = 1;
            int actualSpins = 0;

            // First spin triggers a scatter. The scatter grants additional spins.
            _mockSpinManager.Results.Enqueue(new SpinResult(null, null, 0, true, 3)); // Scatter count 3

            var payouts = ScriptableObject.CreateInstance<PayoutTableData>();
            payouts.freeSpinRewards = new[] { new FreeSpinReward { scatterCount = 3, extraSpins = 5 } };

            for (int i = 0; i < 5; i++)
                _mockSpinManager.Results.Enqueue(new SpinResult(null, null, 0, false, 0)); // Subsequent non-scatter spins

            await _bonusManager.RunFreeSpins(
                _gameState,
                initialCount,
                new ReelStripData[0],
                ScriptableObject.CreateInstance<PaylineData>(),
                payouts,
                (result, win) =>
                {
                    actualSpins++;
                    return UniTask.CompletedTask;
                },
                CancellationToken.None
            );

            Assert.AreEqual(0, _gameState.FreeSpins, "Free spins should be exhausted.");
            Assert.AreEqual(initialCount + 5, actualSpins, "Total spins should include extra spins from scatter.");
        });

        [UnityTest]
        public IEnumerator RunFreeSpins_CalculatesWinCorrectly() => UniTask.ToCoroutine(async () =>
        {
            int spinCount = 1;

            var payouts = ScriptableObject.CreateInstance<PayoutTableData>();
            payouts.freeSpinMultiplier = 3; // 3x multiplier

            long baseWin = 100;
            _mockSpinManager.Results.Enqueue(new SpinResult(null, null, baseWin, false, 0));

            long actualFreeSpinWin = 0;

            await _bonusManager.RunFreeSpins(
                _gameState,
                spinCount,
                new ReelStripData[0],
                ScriptableObject.CreateInstance<PaylineData>(),
                payouts,
                (result, win) =>
                {
                    actualFreeSpinWin = win;
                    return UniTask.CompletedTask;
                },
                CancellationToken.None
            );

            Assert.AreEqual(baseWin * 3, actualFreeSpinWin, "Win should be multiplied by freeSpinMultiplier.");
            Assert.AreEqual(1000 + (baseWin * 3), _gameState.Coins, "Coins should be updated with multiplied win.");
        });

        [UnityTest]
        public IEnumerator RunFreeSpins_Cancellation_ThrowsException() => UniTask.ToCoroutine(async () =>
        {
            int spinCount = 5;

            for (int i = 0; i < spinCount; i++)
                _mockSpinManager.Results.Enqueue(new SpinResult(null, null, 0, false, 0));

            var cts = new CancellationTokenSource();

            bool threwException = false;

            try
            {
                await _bonusManager.RunFreeSpins(
                    _gameState,
                    spinCount,
                    new ReelStripData[0],
                    ScriptableObject.CreateInstance<PaylineData>(),
                    ScriptableObject.CreateInstance<PayoutTableData>(),
                    (result, win) =>
                    {
                        cts.Cancel(); // Cancel on the first spin
                        return UniTask.CompletedTask;
                    },
                    cts.Token
                );
            }
            catch (OperationCanceledException)
            {
                threwException = true;
            }

            Assert.IsTrue(threwException, "RunFreeSpins should throw OperationCanceledException when canceled.");
        });
    }
}