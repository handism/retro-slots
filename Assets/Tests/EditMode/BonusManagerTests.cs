using NUnit.Framework;
using UnityEngine;
using SlotGame.Core;
using SlotGame.Utility;
using SlotGame.Data;

namespace SlotGame.Tests.EditMode
{
    public class BonusManagerTests
    {
        private class MockRandomGenerator : IRandomGenerator
        {
            public int NextValue { get; set; }

            public int Next(int minValue, int maxValue)
            {
                return NextValue;
            }

            public float NextFloat()
            {
                return 0f;
            }
        }

        private GameObject _go;
        private BonusManager _bonusManager;
        private MockRandomGenerator _mockRandom;
        private PayoutTableData _payouts;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject();
            _bonusManager = _go.AddComponent<BonusManager>();
            _mockRandom = new MockRandomGenerator();

            // Minimal config init
            var config = ScriptableObject.CreateInstance<SlotConfig>();
            _bonusManager.Initialize(_mockRandom, config);

            // Setup payout data
            _payouts = ScriptableObject.CreateInstance<PayoutTableData>();
            _payouts.bonusRewards = new[]
            {
                new BonusRewardEntry { multiplier = 5,   weight = 40 }, // cumulative: 40
                new BonusRewardEntry { multiplier = 10,  weight = 25 }, // cumulative: 65
                new BonusRewardEntry { multiplier = 20,  weight = 15 }, // cumulative: 80
                new BonusRewardEntry { multiplier = 30,  weight = 10 }, // cumulative: 90
                new BonusRewardEntry { multiplier = 50,  weight = 7  }, // cumulative: 97
                new BonusRewardEntry { multiplier = 100, weight = 3  }, // cumulative: 100
            };
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void DrawBonusReward_Roll0_ReturnsFirstMultiplier()
        {
            _mockRandom.NextValue = 0;
            int result = _bonusManager.DrawBonusReward(_payouts);
            Assert.AreEqual(5, result);
        }

        [Test]
        public void DrawBonusReward_RollJustBelowFirstBoundary_ReturnsFirstMultiplier()
        {
            _mockRandom.NextValue = 39;
            int result = _bonusManager.DrawBonusReward(_payouts);
            Assert.AreEqual(5, result);
        }

        [Test]
        public void DrawBonusReward_RollAtFirstBoundary_ReturnsSecondMultiplier()
        {
            _mockRandom.NextValue = 40;
            int result = _bonusManager.DrawBonusReward(_payouts);
            Assert.AreEqual(10, result);
        }

        [Test]
        public void DrawBonusReward_RollAtIntermediateBoundary_ReturnsCorrectMultiplier()
        {
            _mockRandom.NextValue = 80; // Should return the item starting at cumulative 80 (the 4th item: 30)
            int result = _bonusManager.DrawBonusReward(_payouts);
            Assert.AreEqual(30, result);
        }

        [Test]
        public void DrawBonusReward_RollMaxValidValue_ReturnsLastMultiplier()
        {
            _mockRandom.NextValue = 99;
            int result = _bonusManager.DrawBonusReward(_payouts);
            Assert.AreEqual(100, result);
        }

        [Test]
        public void DrawBonusReward_RollOutOfBounds_ReturnsLastMultiplier()
        {
            // Even though the random generator shouldn't return this based on the logic (0 to totalWeight-1),
            // testing the fallback logic just in case it's hit.
            _mockRandom.NextValue = 100;
            int result = _bonusManager.DrawBonusReward(_payouts);
            Assert.AreEqual(100, result);
        }
    }
}
