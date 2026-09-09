using System.Reflection;
using NUnit.Framework;
using SlotGame.Core;
using SlotGame.View;

namespace SlotGame.Tests.EditMode
{
    [TestFixture]
    public class GameManagerTests
    {
        private WinLevel InvokeCalcWinLevel(long amount, int betAmount)
        {
            var method = typeof(GameManager).GetMethod("CalcWinLevel", BindingFlags.NonPublic | BindingFlags.Static);
            return (WinLevel)method.Invoke(null, new object[] { amount, betAmount });
        }

        [Test]
        public void CalcWinLevel_Tests()
        {
            var method = typeof(GameManager).GetMethod("CalcWinLevel", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method);
        }

        [TestCase(0, 10, ExpectedResult = WinLevel.Small)]
        [TestCase(149, 10, ExpectedResult = WinLevel.Small)]
        [TestCase(150, 10, ExpectedResult = WinLevel.Big)]
        [TestCase(299, 10, ExpectedResult = WinLevel.Big)]
        [TestCase(300, 10, ExpectedResult = WinLevel.Mega)]
        [TestCase(499, 10, ExpectedResult = WinLevel.Mega)]
        [TestCase(500, 10, ExpectedResult = WinLevel.Epic)]
        [TestCase(1000, 10, ExpectedResult = WinLevel.Epic)]
        [TestCase(100, 0, ExpectedResult = WinLevel.Small)]
        [TestCase(100, -5, ExpectedResult = WinLevel.Small)]
        public WinLevel CalcWinLevel_ReturnsCorrectLevel(long amount, int betAmount)
        {
            return InvokeCalcWinLevel(amount, betAmount);
        }
    }
}
