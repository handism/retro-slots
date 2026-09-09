using NUnit.Framework;
using SlotGame.Model;

namespace SlotGame.Tests.EditMode
{
    public class SessionStatsTests
    {
        [Test]
        public void Constructor_AssignsPropertiesCorrectly_HappyPath()
        {
            var stats = new SessionStats(
                totalSpins: 100,
                wins: 50,
                winRate: 50.0f,
                largestWin: 1000,
                freeSpinTriggers: 5,
                netProfit: 200
            );

            Assert.AreEqual(100, stats.TotalSpins);
            Assert.AreEqual(50, stats.Wins);
            Assert.AreEqual(50.0f, stats.WinRate);
            Assert.AreEqual(1000, stats.LargestWin);
            Assert.AreEqual(5, stats.FreeSpinTriggers);
            Assert.AreEqual(200, stats.NetProfit);
        }

        [Test]
        public void Constructor_AssignsPropertiesCorrectly_EdgeCases()
        {
            var stats = new SessionStats(
                totalSpins: 0,
                wins: 0,
                winRate: 0.0f,
                largestWin: 0,
                freeSpinTriggers: 0,
                netProfit: -999999
            );

            Assert.AreEqual(0, stats.TotalSpins);
            Assert.AreEqual(0, stats.Wins);
            Assert.AreEqual(0.0f, stats.WinRate);
            Assert.AreEqual(0, stats.LargestWin);
            Assert.AreEqual(0, stats.FreeSpinTriggers);
            Assert.AreEqual(-999999, stats.NetProfit);
        }
    }
}
