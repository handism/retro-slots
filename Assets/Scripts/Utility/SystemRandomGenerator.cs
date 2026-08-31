using System;
using System.Security.Cryptography;

namespace SlotGame.Utility
{
    /// <summary>プロダクション用乱数実装。System.Security.Cryptography.RandomNumberGenerator をラップする。</summary>
    public class SystemRandomGenerator : IRandomGenerator, IDisposable
    {
        private readonly RandomNumberGenerator _rng;
        private readonly byte[] _buffer;

        public SystemRandomGenerator()
        {
            _rng = RandomNumberGenerator.Create();
            _buffer = new byte[4];
        }

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
            // Calculate the maximum acceptable value to prevent modulo bias
            // 0x100000000L is 2^32, the number of possible uint values
            long max = 0x100000000L - (0x100000000L % range);

            while (true)
            {
                _rng.GetBytes(_buffer);
                uint val = BitConverter.ToUInt32(_buffer, 0);
                if (val < max)
                {
                    return (int)(minValue + (val % range));
                }
            }
        }

        public float NextFloat()
        {
            _rng.GetBytes(_buffer);
            uint val = BitConverter.ToUInt32(_buffer, 0);
            // Use 24 bits of precision (0xFFFFFF) and divide by 2^24 to get a float in [0, 1)
            return (val & 0xFFFFFF) / (float)(1 << 24);
        }

        public void Dispose()
        {
            _rng?.Dispose();
        }
    }
}
