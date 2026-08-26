using System;
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
            public int NextValueToReturn { get; set; }

            public int Next(int minValue, int maxValue)
            {
                return NextValueToReturn;
            }

            public float NextFloat()
            {
                return 0f;
            }
        }

        private PayoutTableData CreatePayoutTableData()
        {
            var pd = ScriptableObject.CreateInstance<PayoutTableData>();
            pd.bonusRewards = new[]
            {
                new BonusRewardEntry { multiplier = 5, weight = 40 },
                new BonusRewardEntry { multiplier = 10, weight = 25 },
                new BonusRewardEntry { multiplier = 20, weight = 15 },
                new BonusRewardEntry { multiplier = 30, weight = 10 },
                new BonusRewardEntry { multiplier = 50, weight = 7 },
                new BonusRewardEntry { multiplier = 100, weight = 3 },
            };
            return pd;
        }

        [Test]
        public void Initialize_AssignsDependencies()
        {
            // Create a temporary GameObject to hold the BonusManager
            var go = new GameObject();
            var manager = go.AddComponent<BonusManager>();

            var stubRandom = new StubRandomGenerator();
            var config = ScriptableObject.CreateInstance<SlotConfig>();

            manager.Initialize(stubRandom, config);

            // Since _random and _config are private, we test their effect implicitly via DrawBonusReward
            // We set NextValueToReturn to 0 which should yield the first entry multiplier (5)
            stubRandom.NextValueToReturn = 0;
            var payouts = CreatePayoutTableData();

            int reward = manager.DrawBonusReward(payouts);

            Assert.AreEqual(5, reward);

            UnityEngine.Object.DestroyImmediate(go);
            UnityEngine.Object.DestroyImmediate(config);
            UnityEngine.Object.DestroyImmediate(payouts);
        }

        [Test]
        public void DrawBonusReward_ReturnsCorrectMultiplier_BasedOnWeight()
        {
            var go = new GameObject();
            var manager = go.AddComponent<BonusManager>();
            var stubRandom = new StubRandomGenerator();
            manager.Initialize(stubRandom, null);

            var payouts = CreatePayoutTableData();

            // Total weight is 40 + 25 + 15 + 10 + 7 + 3 = 100
            // Rolls:
            // 0 - 39 -> index 0 (multiplier 5)
            // 40 - 64 -> index 1 (multiplier 10)
            // 65 - 79 -> index 2 (multiplier 20)
            // 80 - 89 -> index 3 (multiplier 30)
            // 90 - 96 -> index 4 (multiplier 50)
            // 97 - 99 -> index 5 (multiplier 100)

            // Test first bracket
            stubRandom.NextValueToReturn = 0;
            Assert.AreEqual(5, manager.DrawBonusReward(payouts));

            stubRandom.NextValueToReturn = 39;
            Assert.AreEqual(5, manager.DrawBonusReward(payouts));

            // Test second bracket
            stubRandom.NextValueToReturn = 40;
            Assert.AreEqual(10, manager.DrawBonusReward(payouts));

            // Test third bracket
            stubRandom.NextValueToReturn = 65;
            Assert.AreEqual(20, manager.DrawBonusReward(payouts));

            // Test last bracket
            stubRandom.NextValueToReturn = 97;
            Assert.AreEqual(100, manager.DrawBonusReward(payouts));

            stubRandom.NextValueToReturn = 99;
            Assert.AreEqual(100, manager.DrawBonusReward(payouts));

            UnityEngine.Object.DestroyImmediate(go);
            UnityEngine.Object.DestroyImmediate(payouts);
        }
    }
}
