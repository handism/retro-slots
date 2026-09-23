using System;
using System.Reflection;
using NUnit.Framework;
using SlotGame.Core;
using SlotGame.Data;
using SlotGame.Utility;
using UnityEngine;

namespace SlotGame.Tests.EditMode.Core
{
    public class BonusManagerTests
    {
        private class MockRandom : IRandomGenerator
        {
            public int ValueToReturn;
            public int MinCalled;
            public int MaxCalled;

            public int Next(int min, int max)
            {
                MinCalled = min;
                MaxCalled = max;
                return ValueToReturn;
            }

            public float NextFloat() => 0f;
        }

        private BonusManager _bonusManager;
        private MockRandom _mockRandom;
        private PayoutTableData _payoutData;

        [SetUp]
        public void Setup()
        {
            var go = new GameObject("BonusManager");
            _bonusManager = go.AddComponent<BonusManager>();
            _mockRandom = new MockRandom();

            // Initialize bonus manager
            _bonusManager.Initialize(_mockRandom, null);

            // Create PayoutTableData
            _payoutData = ScriptableObject.CreateInstance<PayoutTableData>();
            _payoutData.bonusRewards = new BonusRewardEntry[]
            {
                new BonusRewardEntry { multiplier = 5, weight = 50 },
                new BonusRewardEntry { multiplier = 10, weight = 30 },
                new BonusRewardEntry { multiplier = 20, weight = 20 }
            };
        }

        [TearDown]
        public void Teardown()
        {
            if (_bonusManager != null)
            {
                UnityEngine.Object.DestroyImmediate(_bonusManager.gameObject);
            }
            if (_payoutData != null)
            {
                UnityEngine.Object.DestroyImmediate(_payoutData);
            }
        }

        [Test]
        public void DrawBonusReward_RollHitsFirstEntry_ReturnsFirstMultiplier()
        {
            // Total weight is 100.
            // 50, 30, 20
            // Roll = 0 to 49 -> 5
            _mockRandom.ValueToReturn = 25;

            var result = _bonusManager.DrawBonusReward(_payoutData);

            Assert.AreEqual(5, result);
            Assert.AreEqual(0, _mockRandom.MinCalled);
            Assert.AreEqual(100, _mockRandom.MaxCalled);
        }

        [Test]
        public void DrawBonusReward_RollHitsSecondEntry_ReturnsSecondMultiplier()
        {
            // Roll = 50 to 79 -> 10
            _mockRandom.ValueToReturn = 50;

            var result = _bonusManager.DrawBonusReward(_payoutData);

            Assert.AreEqual(10, result);
        }

        [Test]
        public void DrawBonusReward_RollHitsThirdEntry_ReturnsThirdMultiplier()
        {
            // Roll = 80 to 99 -> 20
            _mockRandom.ValueToReturn = 99;

            var result = _bonusManager.DrawBonusReward(_payoutData);

            Assert.AreEqual(20, result);
        }

        [Test]
        public void DrawBonusReward_RollExceedsWeight_ReturnsLastMultiplier()
        {
            // Technically shouldn't happen if Next(min, max) works correctly,
            // but the method has a fallback `return payouts.bonusRewards[^1].multiplier;`
            _mockRandom.ValueToReturn = 100;

            var result = _bonusManager.DrawBonusReward(_payoutData);

            Assert.AreEqual(20, result);
        }
    }
}
