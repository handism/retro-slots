using System;
using System.Security.Cryptography;

namespace SlotGame.Utility
{
    /// <summary>プロダクション用乱数実装。System.Security.Cryptography.RandomNumberGenerator を使用する。</summary>
    public class SystemRandomGenerator : IRandomGenerator, IDisposable
    {
        private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
        private readonly byte[] _buffer = new byte[4];

        public int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue)
                throw new ArgumentOutOfRangeException(
                    nameof(minValue),
                    "minValue must be less than or equal to maxValue"
                );
            if (minValue == maxValue)
                return minValue;

            long range = (long)maxValue - minValue;
            long max = 1L << 32;
            long remainder = max % range;
            long limit = max - remainder;

            uint value;
            do
            {
                _rng.GetBytes(_buffer);
                value = BitConverter.ToUInt32(_buffer, 0);
            } while (value >= limit);

            return minValue + (int)(value % range);
        }

        public float NextFloat()
        {
            _rng.GetBytes(_buffer);
            uint value = BitConverter.ToUInt32(_buffer, 0);
            return (float)(value & 0xFFFFFF) / (1 << 24);
        }

        public void Dispose()
        {
            _rng?.Dispose();
        }
    }
}
