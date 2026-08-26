using System.Collections.Generic;
using NUnit.Framework;
using SlotGame.Model;

namespace SlotGame.Tests.EditMode
{
    public class SpinResultTests
    {
        [Test]
        public void SymbolPosition_Properties_AreAssignedCorrectly()
        {
            var position = new SymbolPosition(2, 1);
            Assert.AreEqual(2, position.Reel);
            Assert.AreEqual(1, position.Row);
        }

        [Test]
        public void LineWin_Properties_AreAssignedCorrectly()
        {
            var lineWin = new LineWin(3, 10, 4, 500);
            Assert.AreEqual(3, lineWin.LineIndex);
            Assert.AreEqual(10, lineWin.SymbolId);
            Assert.AreEqual(4, lineWin.MatchCount);
            Assert.AreEqual(500, lineWin.WinAmount);
        }

        [Test]
        public void SpinResult_Properties_AreAssignedCorrectly()
        {
            var stoppedSymbols = new int[,]
            {
                { 1, 2, 3 },
                { 4, 5, 6 },
                { 7, 8, 9 },
                { 10, 11, 12 },
                { 13, 14, 15 },
            };
            var lineWins = new List<LineWin> { new LineWin(0, 1, 3, 100) };
            var scatterPositions = new List<SymbolPosition> { new SymbolPosition(0, 0), new SymbolPosition(1, 1) };
            var bonusPositions = new List<SymbolPosition> { new SymbolPosition(2, 2) };

            var result = new SpinResult(
                stoppedSymbols,
                lineWins,
                true,
                2,
                scatterPositions,
                false,
                bonusPositions,
                1000
            );

            Assert.AreSame(stoppedSymbols, result.StoppedSymbolIds);
            Assert.AreSame(lineWins, result.LineWins);
            Assert.IsTrue(result.HasScatter);
            Assert.AreEqual(2, result.ScatterCount);
            Assert.AreSame(scatterPositions, result.ScatterPositions);
            Assert.IsFalse(result.HasBonusCondition);
            Assert.AreSame(bonusPositions, result.BonusPositions);
            Assert.AreEqual(1000, result.TotalWinAmount);
        }
    }
}
