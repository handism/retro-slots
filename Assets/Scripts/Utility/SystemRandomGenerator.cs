using System;
using System.Security.Cryptography;

namespace SlotGame.Utility
{
    /// <summary>プロダクション用乱数実装。System.Security.Cryptography.RandomNumberGenerator をラップする。</summary>
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
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minValue),
                    "minValue must be less than or equal to maxValue"
                );
            }
            if (minValue == maxValue)
            {
                return minValue;
            }

            long range = (long)maxValue - minValue;
            long max = (1L << 32);
            long remainder = max % range;
            long limit = max - remainder;

            while (true)
            {
                _rng.GetBytes(_byteBuffer);
                uint val = BitConverter.ToUInt32(_byteBuffer, 0);

                if (val < limit)
                {
                    return (int)(minValue + (val % range));
                }
            }
        }

        public float NextFloat()
        {
            _rng.GetBytes(_byteBuffer);
            uint val = BitConverter.ToUInt32(_byteBuffer, 0) & 0x00FFFFFF;
            return val / 16777216f;
        }

        public void Dispose()
        {
            _rng?.Dispose();
        }
    }
}
