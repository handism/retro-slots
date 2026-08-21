using NUnit.Framework;
using SlotGame.Core;
using SlotGame.Data;
using SlotGame.Utility;
using UnityEngine;

namespace SlotGame.Tests.EditMode
{
    public class BonusManagerTests
    {
        private class StubRandomGenerator : IRandomGenerator
        {
            public int NextValue { get; set; }
            public float NextFloatValue { get; set; }

            public int Next(int minValue, int maxValue)
            {
                return NextValue;
            }

            public float NextFloat()
            {
                return NextFloatValue;
            }
        }

        private BonusManager _bonusManager;
        private StubRandomGenerator _stubRandom;
        private PayoutTableData _payouts;

        [SetUp]
        public void Setup()
        {
            var go = new GameObject("BonusManager");
            _bonusManager = go.AddComponent<BonusManager>();

            _stubRandom = new StubRandomGenerator();
            // Initialize with null config since it's not used in DrawBonusReward
            _bonusManager.Initialize(_stubRandom, null);

            _payouts = ScriptableObject.CreateInstance<PayoutTableData>();
            _payouts.bonusRewards = new[]
            {
                new BonusRewardEntry { multiplier = 5,   weight = 40 }, // cumulative 40
                new BonusRewardEntry { multiplier = 10,  weight = 25 }, // cumulative 65
                new BonusRewardEntry { multiplier = 20,  weight = 15 }, // cumulative 80
                new BonusRewardEntry { multiplier = 30,  weight = 10 }, // cumulative 90
                new BonusRewardEntry { multiplier = 50,  weight = 7  }, // cumulative 97
                new BonusRewardEntry { multiplier = 100, weight = 3  }  // cumulative 100
            };
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_bonusManager.gameObject);
            Object.DestroyImmediate(_payouts);
        }

        [Test]
        public void DrawBonusReward_Roll0_ReturnsFirstReward()
        {
            _stubRandom.NextValue = 0;
            int result = _bonusManager.DrawBonusReward(_payouts);
            Assert.AreEqual(5, result);
        }

        [Test]
        public void DrawBonusReward_Roll39_ReturnsFirstReward()
        {
            _stubRandom.NextValue = 39;
            int result = _bonusManager.DrawBonusReward(_payouts);
            Assert.AreEqual(5, result);
        }

        [Test]
        public void DrawBonusReward_Roll40_ReturnsSecondReward()
        {
            _stubRandom.NextValue = 40;
            int result = _bonusManager.DrawBonusReward(_payouts);
            Assert.AreEqual(10, result);
        }

        [Test]
        public void DrawBonusReward_Roll65_ReturnsThirdReward()
        {
            _stubRandom.NextValue = 65;
            int result = _bonusManager.DrawBonusReward(_payouts);
            Assert.AreEqual(20, result);
        }

        [Test]
        public void DrawBonusReward_Roll99_ReturnsLastReward()
        {
            _stubRandom.NextValue = 99;
            int result = _bonusManager.DrawBonusReward(_payouts);
            Assert.AreEqual(100, result);
        }

        [Test]
        public void DrawBonusReward_RollOutOfBounds_ReturnsLastReward()
        {
            // The method sums up totalWeight and has a fallback "return payouts.bonusRewards[^1].multiplier"
            _stubRandom.NextValue = 1000;
            int result = _bonusManager.DrawBonusReward(_payouts);
            Assert.AreEqual(100, result);
        }
    }
}
