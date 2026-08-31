using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SlotGame.Core;
using SlotGame.Data;
using SlotGame.Model;
using SlotGame.Utility;
using SlotGame.View;
using UnityEngine;

namespace SlotGame.Tests.EditMode
{
    public class BonusManagerTests
    {
        private class MockRandom : IRandomGenerator
        {
            public int Roll;
            public int Next(int min, int max) => Roll;
            public float NextFloat() => 0.5f;
        }

        private class MockSpinManager : SpinManager
        {
            public SpinResult PredefinedResult;
            public int ExecuteSpinCallCount;

            public override UniTask<SpinResult> ExecuteSpin(
                ReelStripData[] strips, PaylineData paylines, PayoutTableData payouts,
                long betAmount, CancellationToken ct, int columns = 5, int rows = 3, int minMatch = 3, int[] bonusTriggerReels = null)
            {
                ExecuteSpinCallCount++;
                return UniTask.FromResult(PredefinedResult);
            }
        }

        private class MockBonusRoundView : BonusRoundView
        {
            public int[] SelectedMultipliers = Array.Empty<int>();

            public new UniTask<int[]> WaitForSelection(int[] presetRewards, CancellationToken ct)
            {
                return UniTask.FromResult(SelectedMultipliers);
            }

            public new UniTask ShowResultAsync(int totalMultiplier, CancellationToken ct)
            {
                return UniTask.CompletedTask;
            }
        }

        private class TestableBonusManager : BonusManager
        {
            public bool LoadCalled;
            public bool UnloadCalled;
            public BonusRoundView MockView;

            protected override UniTask LoadBonusSceneAsync(CancellationToken ct)
            {
                LoadCalled = true;
                return UniTask.CompletedTask;
            }

            protected override UniTask UnloadBonusSceneAsync(CancellationToken ct)
            {
                UnloadCalled = true;
                return UniTask.CompletedTask;
            }

            protected override BonusRoundView GetBonusRoundView()
            {
                return MockView;
            }
        }

        private GameObject _go;
        private TestableBonusManager _bonusManager;
        private MockRandom _random;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject();
            _bonusManager = _go.AddComponent<TestableBonusManager>();
            _random = new MockRandom();

            var config = ScriptableObject.CreateInstance<GameConfigData>();
            config.slotConfig = new SlotConfig { ReelCount = 5, RowCount = 3, MinMatch = 3, MaxFreeSpinAddition = 20, BonusTriggerReels = new[] { 0, 2, 4 } };
            _bonusManager.Initialize(_random, config.slotConfig);
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) UnityEngine.Object.DestroyImmediate(_go);
        }

        [Test]
        public void DrawBonusReward_UsesRandomAndWeightsCorrectly()
        {
            var payouts = ScriptableObject.CreateInstance<PayoutTableData>();
            payouts.bonusRewards = new[]
            {
                new BonusRewardEntry { multiplier = 2, weight = 10 },
                new BonusRewardEntry { multiplier = 5, weight = 20 },
                new BonusRewardEntry { multiplier = 10, weight = 70 }
            };

            // total weight = 100.
            _random.Roll = 5; // < 10 -> first entry
            var result1 = _bonusManager.DrawBonusReward(payouts);
            Assert.AreEqual(2, result1);

            _random.Roll = 15; // 10 <= 15 < 30 -> second entry
            var result2 = _bonusManager.DrawBonusReward(payouts);
            Assert.AreEqual(5, result2);

            _random.Roll = 50; // 30 <= 50 < 100 -> third entry
            var result3 = _bonusManager.DrawBonusReward(payouts);
            Assert.AreEqual(10, result3);
        }

        [Test]
        public void RunFreeSpins_UpdatesGameStateAndCallsOnSpin()
        {
            var spinManagerGo = new GameObject();
            var mockSpinManager = spinManagerGo.AddComponent<MockSpinManager>();
            mockSpinManager.PredefinedResult = new SpinResult(Array.Empty<SymbolData>(), Array.Empty<LineWin>(), 0, false, 0, 100);

            var spinManagerField = typeof(BonusManager).GetField("spinManager", BindingFlags.NonPublic | BindingFlags.Instance);
            spinManagerField.SetValue(_bonusManager, mockSpinManager);

            var state = new GameState(100, 1000, new[] { 10 }, 10, 0);
            var payouts = ScriptableObject.CreateInstance<PayoutTableData>();
            payouts.freeSpinMultiplier = 2;

            int onSpinCallCount = 0;
            Func<SpinResult, long, UniTask> onSpin = (res, win) =>
            {
                onSpinCallCount++;
                return UniTask.CompletedTask;
            };

            var task = _bonusManager.RunFreeSpins(state, 1, Array.Empty<ReelStripData>(), null, payouts, onSpin, CancellationToken.None);

            // Execute synchronous parts
            task.AsTask().Wait();

            Assert.AreEqual(0, state.FreeSpinCount, "Free spin should be consumed");
            Assert.AreEqual(100 + (100 * 2), state.Coins, "Win amount should be multiplied by freeSpinMultiplier");
            Assert.AreEqual(1, onSpinCallCount, "onSpin should be called for each free spin");
            Assert.AreEqual(1, mockSpinManager.ExecuteSpinCallCount, "ExecuteSpin should be called");

            UnityEngine.Object.DestroyImmediate(spinManagerGo);
        }

        [Test]
        public void RunFreeSpins_WithScatter_AddsExtraFreeSpins()
        {
            var spinManagerGo = new GameObject();
            var mockSpinManager = spinManagerGo.AddComponent<MockSpinManager>();
            mockSpinManager.PredefinedResult = new SpinResult(Array.Empty<SymbolData>(), Array.Empty<LineWin>(), 0, true, 3, 0);

            var spinManagerField = typeof(BonusManager).GetField("spinManager", BindingFlags.NonPublic | BindingFlags.Instance);
            spinManagerField.SetValue(_bonusManager, mockSpinManager);

            var state = new GameState(100, 1000, new[] { 10 }, 10, 0);
            var payouts = ScriptableObject.CreateInstance<PayoutTableData>();
            payouts.freeSpinRewards = new[] { new FreeSpinReward { scatterCount = 3, extraSpins = 5 } };

            int onSpinCallCount = 0;
            Func<SpinResult, long, UniTask> onSpin = (res, win) =>
            {
                onSpinCallCount++;
                // Stop it from retriggering infinitely since it returns the exact same result every time
                if (onSpinCallCount >= 2)
                {
                    var extraCount = state.FreeSpinCount;
                    for(int i = 0; i < extraCount; i++) state.ConsumeFreeSpin();
                }
                return UniTask.CompletedTask;
            };

            var task = _bonusManager.RunFreeSpins(state, 1, Array.Empty<ReelStripData>(), null, payouts, onSpin, CancellationToken.None);
            task.AsTask().Wait();

            Assert.AreEqual(0, state.FreeSpinCount);
            Assert.GreaterOrEqual(mockSpinManager.ExecuteSpinCallCount, 2, "Should execute extra spins");

            UnityEngine.Object.DestroyImmediate(spinManagerGo);
        }

        [Test]
        public void RunBonusRound_CalculatesWinAmountCorrectly()
        {
            var mockViewGo = new GameObject();
            var mockView = mockViewGo.AddComponent<MockBonusRoundView>();
            mockView.SelectedMultipliers = new[] { 2, 3, 5 }; // Total 10

            _bonusManager.MockView = mockView;

            var payouts = ScriptableObject.CreateInstance<PayoutTableData>();
            payouts.bonusRewards = new[] { new BonusRewardEntry { multiplier = 1, weight = 1 } };

            var task = _bonusManager.RunBonusRound(10, payouts, CancellationToken.None);
            var winAmount = task.AsTask().GetAwaiter().GetResult();

            Assert.IsTrue(_bonusManager.LoadCalled, "LoadBonusSceneAsync should be called");
            Assert.IsTrue(_bonusManager.UnloadCalled, "UnloadBonusSceneAsync should be called");
            Assert.AreEqual(100, winAmount, "Win amount should be betAmount (10) * totalMultiplier (10)");

            UnityEngine.Object.DestroyImmediate(mockViewGo);
        }
    }
}
