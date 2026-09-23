using System;
using NUnit.Framework;
using SlotGame.Utility;

namespace SlotGame.Tests.EditMode
{
    [TestFixture]
    public class SystemRandomGeneratorTests
    {
        private SystemRandomGenerator _generator;

        [SetUp]
        public void SetUp()
        {
            _generator = new SystemRandomGenerator();
        }

        [Test]
        public void Next_ReturnsValueWithinBounds()
        {
            int minValue = 0;
            int maxValue = 10;
            for (int i = 0; i < 100; i++)
            {
                int result = _generator.Next(minValue, maxValue);
                Assert.That(result, Is.GreaterThanOrEqualTo(minValue));
                Assert.That(result, Is.LessThan(maxValue));
            }
        }

        [Test]
        public void Next_WithEqualMinMax_ReturnsMin()
        {
            int minValue = 5;
            int maxValue = 5;
            int result = _generator.Next(minValue, maxValue);
            Assert.That(result, Is.EqualTo(minValue));
        }

        [Test]
        public void Next_WithMinGreaterThanMax_ThrowsArgumentOutOfRangeException()
        {
            int minValue = 10;
            int maxValue = 5;
            Assert.Throws<ArgumentOutOfRangeException>(() => _generator.Next(minValue, maxValue));
        }

        [Test]
        public void NextFloat_ReturnsValueWithinBounds()
        {
            for (int i = 0; i < 100; i++)
            {
                float result = _generator.NextFloat();
                Assert.That(result, Is.GreaterThanOrEqualTo(0.0f));
                Assert.That(result, Is.LessThan(1.0f));
            }
        }
    }
}
