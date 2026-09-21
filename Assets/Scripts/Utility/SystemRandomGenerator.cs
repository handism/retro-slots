using System;

namespace SlotGame.Utility
{
    /// <summary>プロダクション用乱数実装。System.Random をラップする。</summary>
    public class SystemRandomGenerator : IRandomGenerator, IDisposable
    {
        private readonly System.Security.Cryptography.RandomNumberGenerator _rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        private readonly byte[] _buffer = new byte[4];

        public int Next(int minValue, int maxValue)
        {
            if (minValue >= maxValue)
                throw new ArgumentOutOfRangeException(nameof(minValue), "minValue must be less than maxValue");

            long range = (long)maxValue - minValue;
            long max = (1L << 32) - (1L << 32) % range;

            while (true)
            {
                _rng.GetBytes(_buffer);
                uint rand = BitConverter.ToUInt32(_buffer, 0);
                if (rand < max)
                    return (int)(minValue + (rand % range));
            }
        }

        public float NextFloat()
        {
            _rng.GetBytes(_buffer);
            uint rand = BitConverter.ToUInt32(_buffer, 0);
            return (rand & 0xFFFFFF) / 16777216f;
        }

        public void Dispose()
        {
            _rng?.Dispose();
        }
    }
}
