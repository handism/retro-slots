using NUnit.Framework;
using SlotGame.Model;
using System.Collections.Generic;

namespace SlotGame.Tests.EditMode
{
    [TestFixture]
    public class SpinResultTests
    {
        [Test]
        public void SymbolPosition_ConstructorAndProperties_AreCorrect()
        {
            // Arrange & Act
            var pos = new SymbolPosition(1, 2);

            // Assert
            Assert.AreEqual(1, pos.Reel);
            Assert.AreEqual(2, pos.Row);
        }

        [Test]
        public void SymbolPosition_Equality_WorksCorrectly()
        {
            // Arrange
            var pos1 = new SymbolPosition(2, 3);
            var pos2 = new SymbolPosition(2, 3);
            var pos3 = new SymbolPosition(1, 3);

            // Assert
            Assert.AreEqual(pos1, pos2);
            Assert.IsTrue(pos1 == pos2);
            Assert.AreNotEqual(pos1, pos3);
            Assert.IsFalse(pos1 == pos3);
        }

        [Test]
        public void LineWin_ConstructorAndProperties_AreCorrect()
        {
            // Arrange & Act
            var win = new LineWin(5, 1, 3, 1500L);

            // Assert
            Assert.AreEqual(5, win.LineIndex);
            Assert.AreEqual(1, win.SymbolId);
            Assert.AreEqual(3, win.MatchCount);
            Assert.AreEqual(1500L, win.WinAmount);
        }

        [Test]
        public void LineWin_Equality_WorksCorrectly()
        {
            // Arrange
            var win1 = new LineWin(1, 2, 4, 1000L);
            var win2 = new LineWin(1, 2, 4, 1000L);
            var win3 = new LineWin(2, 2, 4, 1000L);

            // Assert
            Assert.AreEqual(win1, win2);
            Assert.IsTrue(win1 == win2);
            Assert.AreNotEqual(win1, win3);
            Assert.IsFalse(win1 == win3);
        }

        [Test]
        public void SpinResult_ConstructorAndProperties_AreCorrect()
        {
            // Arrange
            int[,] symbols = new int[5, 3] {
                { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 }, { 10, 11, 12 }, { 13, 14, 15 }
            };
            var lineWins = new List<LineWin> { new LineWin(0, 1, 3, 500L) };
            var scatterPositions = new List<SymbolPosition> { new SymbolPosition(1, 1) };
            var bonusPositions = new List<SymbolPosition> { new SymbolPosition(0, 0), new SymbolPosition(2, 2) };

            // Act
            var result = new SpinResult(
                StoppedSymbolIds: symbols,
                LineWins: lineWins,
                HasScatter: true,
                ScatterCount: 1,
                ScatterPositions: scatterPositions,
                HasBonusCondition: true,
                BonusPositions: bonusPositions,
                TotalWinAmount: 1000L
            );

            // Assert
            Assert.AreSame(symbols, result.StoppedSymbolIds); // Reference equality for arrays
            Assert.AreSame(lineWins, result.LineWins);
            Assert.IsTrue(result.HasScatter);
            Assert.AreEqual(1, result.ScatterCount);
            Assert.AreSame(scatterPositions, result.ScatterPositions);
            Assert.IsTrue(result.HasBonusCondition);
            Assert.AreSame(bonusPositions, result.BonusPositions);
            Assert.AreEqual(1000L, result.TotalWinAmount);
        }

        [Test]
        public void SpinResult_Equality_WorksCorrectlyForValueTypesAndReferences()
        {
            // Arrange
            int[,] symbols = new int[5, 3];
            var lineWins = new List<LineWin>();
            var scatterPositions = new List<SymbolPosition>();
            var bonusPositions = new List<SymbolPosition>();

            var result1 = new SpinResult(
                symbols, lineWins, false, 0, scatterPositions, false, bonusPositions, 0L
            );

            var result2 = new SpinResult(
                symbols, lineWins, false, 0, scatterPositions, false, bonusPositions, 0L
            );

            var result3 = new SpinResult(
                symbols, lineWins, false, 0, scatterPositions, false, bonusPositions, 100L
            );

            var differentSymbols = new int[5, 3];
            var result4 = new SpinResult(
                differentSymbols, lineWins, false, 0, scatterPositions, false, bonusPositions, 0L
            );

            // Assert
            // Records use reference equality for array and List members
            Assert.AreEqual(result1, result2);
            Assert.AreNotEqual(result1, result3); // Different value type property
            Assert.AreNotEqual(result1, result4); // Different array reference
        }
    }
}
