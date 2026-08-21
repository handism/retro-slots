using System;
using System.Security.Cryptography;

namespace SlotGame.Utility
{
    /// <summary>プロダクション用乱数実装。RandomNumberGenerator をラップする。</summary>
    public class SystemRandomGenerator : IRandomGenerator, IDisposable
    {
        private readonly RandomNumberGenerator _rng;
        private readonly byte[] _byteBuffer = new byte[4];

        public SystemRandomGenerator()
        {
            _rng = RandomNumberGenerator.Create();
        }

        public int Next(int minValue, int maxValue)
        {
            if (minValue == maxValue)
                return minValue;
            if (minValue > maxValue)
                throw new ArgumentOutOfRangeException(nameof(minValue), "minValue must be less than or equal to maxValue.");

            long range = (long)maxValue - minValue;
            long max = (1L << 32);
            long remainder = max % range;

            while (true)
            {
                _rng.GetBytes(_byteBuffer);
                uint randVal = BitConverter.ToUInt32(_byteBuffer, 0);

                if (randVal < max - remainder)
                {
                    return (int)(minValue + (randVal % range));
                }
            }
        }

        public float NextFloat()
        {
            _rng.GetBytes(_byteBuffer);
            uint randVal = BitConverter.ToUInt32(_byteBuffer, 0);
            return (float)(randVal / (uint.MaxValue + 1.0));
        }

        public void Dispose()
        {
            _rng?.Dispose();
        }
    }
}
