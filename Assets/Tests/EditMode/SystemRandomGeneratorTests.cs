using System;
using NUnit.Framework;
using SlotGame.Utility;

namespace SlotGame.Tests.EditMode
{
    public class SystemRandomGeneratorTests
    {
        private SystemRandomGenerator _generator;

        [SetUp]
        public void Setup()
        {
            _generator = new SystemRandomGenerator();
        }

        [Test]
        public void Next_NormalRange_ReturnsValueWithinRange()
        {
            // Arrange
            int min = 0;
            int max = 10;
            bool valueInRange = true;

            // Act & Assert
            for (int i = 0; i < 100; i++)
            {
                int result = _generator.Next(min, max);
                if (result < min || result >= max)
                {
                    valueInRange = false;
                    break;
                }
            }

            Assert.IsTrue(valueInRange, "Generated values should be within the specified range.");
        }

        [Test]
        public void Next_MinEqualsMax_ReturnsMin()
        {
            // Arrange
            int min = 5;
            int max = 5;

            // Act
            int result = _generator.Next(min, max);

            // Assert
            Assert.AreEqual(min, result);
        }

        [Test]
        public void Next_MinGreaterThanMax_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            int min = 10;
            int max = 5;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => _generator.Next(min, max));
        }

    }
}
