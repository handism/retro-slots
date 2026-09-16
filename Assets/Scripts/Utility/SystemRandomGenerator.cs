using System;
using System.Security.Cryptography;

namespace SlotGame.Utility
{
    /// <summary>プロダクション用乱数実装。System.Random をラップする。</summary>
    public sealed class SystemRandomGenerator : IRandomGenerator, IDisposable
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
                throw new ArgumentOutOfRangeException(nameof(minValue));
            if (minValue == maxValue)
                return minValue;

            long range = (long)maxValue - minValue;
            long maxCount = (long)uint.MaxValue + 1;
            long remainder = maxCount % range;
            long limit = maxCount - remainder - 1;

            while (true)
            {
                _rng.GetBytes(_byteBuffer);
                uint value = BitConverter.ToUInt32(_byteBuffer, 0);

                if (value <= limit)
                {
                    return (int)(minValue + (value % range));
                }
            }
        }

        public float NextFloat()
        {
            _rng.GetBytes(_byteBuffer);
            uint value = BitConverter.ToUInt32(_byteBuffer, 0);
            return (value & 0xFFFFFF) / 16777216f;
        }

        public void Dispose()
        {
            _rng?.Dispose();
        }
    }
}
