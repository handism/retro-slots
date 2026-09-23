using NUnit.Framework;
using SlotGame.Utility;

namespace SlotGame.Tests.EditMode.Utility
{
    public class SeededRandomGeneratorTests
    {
        [Test]
        public void Next_WithSameSeed_ReturnsIdenticalValues()
        {
            var seed = 12345;
            var generator1 = new SeededRandomGenerator(seed);
            var generator2 = new SeededRandomGenerator(seed);

            for (int i = 0; i < 100; i++)
            {
                Assert.AreEqual(generator1.Next(0, 100), generator2.Next(0, 100));
            }
        }

        [Test]
        public void Next_WithDifferentSeeds_ReturnsDifferentValues()
        {
            var generator1 = new SeededRandomGenerator(12345);
            var generator2 = new SeededRandomGenerator(54321);

            bool differenceFound = false;
            for (int i = 0; i < 100; i++)
            {
                if (generator1.Next(0, 100) != generator2.Next(0, 100))
                {
                    differenceFound = true;
                    break;
                }
            }

            Assert.IsTrue(differenceFound, "Generators with different seeds should produce different sequences.");
        }

        [Test]
        public void Next_ReturnsValueWithinBounds()
        {
            var generator = new SeededRandomGenerator(12345);
            int min = 10;
            int max = 20;

            for (int i = 0; i < 1000; i++)
            {
                int value = generator.Next(min, max);
                Assert.GreaterOrEqual(value, min);
                Assert.Less(value, max);
            }
        }

        [Test]
        public void NextFloat_WithSameSeed_ReturnsIdenticalValues()
        {
            var seed = 12345;
            var generator1 = new SeededRandomGenerator(seed);
            var generator2 = new SeededRandomGenerator(seed);

            for (int i = 0; i < 100; i++)
            {
                Assert.AreEqual(generator1.NextFloat(), generator2.NextFloat());
            }
        }

        [Test]
        public void NextFloat_ReturnsValueBetweenZeroAndOne()
        {
            var generator = new SeededRandomGenerator(12345);

            for (int i = 0; i < 1000; i++)
            {
                float value = generator.NextFloat();
                Assert.GreaterOrEqual(value, 0.0f);
                Assert.Less(value, 1.0f);
            }
        }
    }
}
