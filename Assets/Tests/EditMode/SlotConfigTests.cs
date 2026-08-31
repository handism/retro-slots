using NUnit.Framework;
using SlotGame.Model;

namespace SlotGame.Tests.EditMode
{
    [TestFixture]
    public class SlotConfigTests
    {
        [Test]
        public void SlotConfig_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            long initialCoins = 1000;
            long maxCoins = 100000;
            int[] validBetAmounts = new int[] { 10, 20, 50 };
            int reelCount = 5;
            int rowCount = 3;
            int minMatch = 3;
            int[] bonusTriggerReels = new int[] { 0, 2, 4 };
            int freeSpinMultiplier = 2;
            int maxFreeSpinAddition = 10;
            int defaultAutoSpinCount = 50;
            float defaultBgmVolume = 0.5f;
            float defaultSeVolume = 0.8f;
            float turboSpinDuration = 0.5f;
            float turboStopInterval = 0.1f;
            float normalSpinDuration = 2.0f;
            float normalStopInterval = 0.3f;
            string checksumSalt = "test_salt";

            // Act
            var config = new SlotConfig(
                initialCoins,
                maxCoins,
                validBetAmounts,
                reelCount,
                rowCount,
                minMatch,
                bonusTriggerReels,
                freeSpinMultiplier,
                maxFreeSpinAddition,
                defaultAutoSpinCount,
                defaultBgmVolume,
                defaultSeVolume,
                turboSpinDuration,
                turboStopInterval,
                normalSpinDuration,
                normalStopInterval,
                checksumSalt
            );

            // Assert
            Assert.AreEqual(initialCoins, config.InitialCoins);
            Assert.AreEqual(maxCoins, config.MaxCoins);
            Assert.AreSame(validBetAmounts, config.ValidBetAmounts);
            Assert.AreEqual(reelCount, config.ReelCount);
            Assert.AreEqual(rowCount, config.RowCount);
            Assert.AreEqual(minMatch, config.MinMatch);
            Assert.AreSame(bonusTriggerReels, config.BonusTriggerReels);
            Assert.AreEqual(freeSpinMultiplier, config.FreeSpinMultiplier);
            Assert.AreEqual(maxFreeSpinAddition, config.MaxFreeSpinAddition);
            Assert.AreEqual(defaultAutoSpinCount, config.DefaultAutoSpinCount);
            Assert.AreEqual(defaultBgmVolume, config.DefaultBgmVolume);
            Assert.AreEqual(defaultSeVolume, config.DefaultSeVolume);
            Assert.AreEqual(turboSpinDuration, config.TurboSpinDuration);
            Assert.AreEqual(turboStopInterval, config.TurboStopInterval);
            Assert.AreEqual(normalSpinDuration, config.NormalSpinDuration);
            Assert.AreEqual(normalStopInterval, config.NormalStopInterval);
            Assert.AreEqual(checksumSalt, config.ChecksumSalt);
        }

        [Test]
        public void SlotConfig_ValueEquality_WorksCorrectly()
        {
            // Arrange
            int[] validBetAmounts = new int[] { 10, 20, 50 };
            int[] bonusTriggerReels = new int[] { 0, 2, 4 };

            var config1 = new SlotConfig(
                1000, 100000, validBetAmounts, 5, 3, 3, bonusTriggerReels,
                2, 10, 50, 0.5f, 0.8f, 0.5f, 0.1f, 2.0f, 0.3f, "test_salt"
            );

            var config2 = new SlotConfig(
                1000, 100000, validBetAmounts, 5, 3, 3, bonusTriggerReels,
                2, 10, 50, 0.5f, 0.8f, 0.5f, 0.1f, 2.0f, 0.3f, "test_salt"
            );

            var config3 = new SlotConfig(
                2000, 100000, validBetAmounts, 5, 3, 3, bonusTriggerReels,
                2, 10, 50, 0.5f, 0.8f, 0.5f, 0.1f, 2.0f, 0.3f, "test_salt"
            );

            // Assert
            Assert.AreEqual(config1, config2);
            Assert.IsTrue(config1 == config2);
            Assert.AreNotEqual(config1, config3);
            Assert.IsFalse(config1 == config3);
        }

        [Test]
        public void SlotConfig_WithExpression_CreatesModifiedCopy()
        {
            // Arrange
            int[] validBetAmounts = new int[] { 10, 20, 50 };
            int[] bonusTriggerReels = new int[] { 0, 2, 4 };

            var originalConfig = new SlotConfig(
                1000, 100000, validBetAmounts, 5, 3, 3, bonusTriggerReels,
                2, 10, 50, 0.5f, 0.8f, 0.5f, 0.1f, 2.0f, 0.3f, "test_salt"
            );

            // Act
            var modifiedConfig = originalConfig with { InitialCoins = 2000, TurboSpinDuration = 0.25f };

            // Assert
            Assert.AreEqual(2000, modifiedConfig.InitialCoins);
            Assert.AreEqual(0.25f, modifiedConfig.TurboSpinDuration);
            Assert.AreEqual(100000, modifiedConfig.MaxCoins); // Unchanged
            Assert.AreSame(validBetAmounts, modifiedConfig.ValidBetAmounts); // Unchanged

            // Ensure original is not modified
            Assert.AreEqual(1000, originalConfig.InitialCoins);
            Assert.AreEqual(0.5f, originalConfig.TurboSpinDuration);
        }
    }
}