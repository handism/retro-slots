using NUnit.Framework;
using SlotGame.Model;

namespace SlotGame.Tests.EditMode
{
    [TestFixture]
    public class SaveDataTests
    {
        [Test]
        public void SaveData_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var saveData = new SaveData();

            // Assert
            Assert.AreEqual(1000, saveData.coins);
            Assert.AreEqual(10, saveData.betAmount);
            Assert.AreEqual(0.8f, saveData.bgmVolume);
            Assert.AreEqual(1.0f, saveData.seVolume);
            Assert.AreEqual(0, saveData.totalSpins);
            Assert.AreEqual(0, saveData.totalWins);
            Assert.AreEqual(0, saveData.maxWin);
            Assert.AreEqual(0, saveData.totalFreeSpinTriggers);
            Assert.AreEqual("1.0", saveData.saveVersion);
            Assert.AreEqual("", saveData.checksum);
            Assert.IsFalse(saveData.hasCompletedTutorial);
            Assert.IsFalse(saveData.isTurbo);
        }

        [Test]
        public void SaveData_Properties_CanBeModified()
        {
            // Arrange
            var saveData = new SaveData();

            // Act
            saveData.coins = 5000;
            saveData.betAmount = 50;
            saveData.bgmVolume = 0.5f;
            saveData.seVolume = 0.3f;
            saveData.totalSpins = 100;
            saveData.totalWins = 250;
            saveData.maxWin = 1000;
            saveData.totalFreeSpinTriggers = 5;
            saveData.saveVersion = "2.0";
            saveData.checksum = "test_checksum";
            saveData.hasCompletedTutorial = true;
            saveData.isTurbo = true;

            // Assert
            Assert.AreEqual(5000, saveData.coins);
            Assert.AreEqual(50, saveData.betAmount);
            Assert.AreEqual(0.5f, saveData.bgmVolume);
            Assert.AreEqual(0.3f, saveData.seVolume);
            Assert.AreEqual(100, saveData.totalSpins);
            Assert.AreEqual(250, saveData.totalWins);
            Assert.AreEqual(1000, saveData.maxWin);
            Assert.AreEqual(5, saveData.totalFreeSpinTriggers);
            Assert.AreEqual("2.0", saveData.saveVersion);
            Assert.AreEqual("test_checksum", saveData.checksum);
            Assert.IsTrue(saveData.hasCompletedTutorial);
            Assert.IsTrue(saveData.isTurbo);
        }
    }
}