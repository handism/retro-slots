using NUnit.Framework;
using SlotGame.Model;

namespace SlotGame.Tests.EditMode
{
    [TestFixture]
    public class SessionStatsTests
    {
        [Test]
        public void SessionStats_Constructor_InitializesPropertiesCorrectly()
        {
            // Arrange & Act
            var stats = new SessionStats(
                totalSpins: 100,
                wins: 25,
                winRate: 25.0f,
                largestWin: 500,
                freeSpinTriggers: 2,
                netProfit: 1500
            );

            // Assert
            Assert.AreEqual(100, stats.TotalSpins);
            Assert.AreEqual(25, stats.Wins);
            Assert.AreEqual(25.0f, stats.WinRate);
            Assert.AreEqual(500, stats.LargestWin);
            Assert.AreEqual(2, stats.FreeSpinTriggers);
            Assert.AreEqual(1500, stats.NetProfit);
        }

        [Test]
        public void SessionStats_Constructor_HandlesZeroValues()
        {
            // Arrange & Act
            var stats = new SessionStats(0, 0, 0f, 0, 0, 0);

            // Assert
            Assert.AreEqual(0, stats.TotalSpins);
            Assert.AreEqual(0, stats.Wins);
            Assert.AreEqual(0f, stats.WinRate);
            Assert.AreEqual(0, stats.LargestWin);
            Assert.AreEqual(0, stats.FreeSpinTriggers);
            Assert.AreEqual(0, stats.NetProfit);
        }

        [Test]
        public void SessionStats_Constructor_HandlesNegativeProfit()
        {
            // Arrange & Act
            var stats = new SessionStats(
                totalSpins: 50,
                wins: 5,
                winRate: 10.0f,
                largestWin: 50,
                freeSpinTriggers: 0,
                netProfit: -1000
            );

            // Assert
            Assert.AreEqual(50, stats.TotalSpins);
            Assert.AreEqual(5, stats.Wins);
            Assert.AreEqual(10.0f, stats.WinRate);
            Assert.AreEqual(50, stats.LargestWin);
            Assert.AreEqual(0, stats.FreeSpinTriggers);
            Assert.AreEqual(-1000, stats.NetProfit);
        }
    }
}
