using NUnit.Framework;
using SlotGame.Utility;

namespace SlotGame.Tests.EditMode
{
    public class SeededRandomGeneratorTests
    {
        [Test]
        public void SameSeed_ProducesSameSequence()
        {
            var gen1 = new SeededRandomGenerator(12345);
            var gen2 = new SeededRandomGenerator(12345);

            for (int i = 0; i < 100; i++)
            {
                int val1 = gen1.Next(0, 1000);
                int val2 = gen2.Next(0, 1000);
                Assert.AreEqual(val1, val2);
            }
        }

        [Test]
        public void Next_ReturnsValuesWithinRange()
        {
            var gen = new SeededRandomGenerator(12345);
            int min = 10;
            int max = 20;

            for (int i = 0; i < 1000; i++)
            {
                int val = gen.Next(min, max);
                Assert.GreaterOrEqual(val, min);
                Assert.Less(val, max);
            }
        }

        [Test]
        public void DifferentSeed_ProducesDifferentSequence()
        {
            var gen1 = new SeededRandomGenerator(12345);
            var gen2 = new SeededRandomGenerator(54321);

            bool isDifferent = false;
            for (int i = 0; i < 10; i++)
            {
                if (gen1.Next(0, 1000) != gen2.Next(0, 1000))
                {
                    isDifferent = true;
                    break;
                }
            }

            Assert.IsTrue(isDifferent);
        }
    }
}
