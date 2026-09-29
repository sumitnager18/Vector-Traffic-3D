using System;
namespace VectorTraffic3D.Generation
{
    public sealed class DeterministicRandom
    {
        private uint _state;
        public uint State => _state;
        public DeterministicRandom(int seed)
        {
            _state = unchecked((uint)seed);
            if (_state == 0) _state = 0x6D2B79F5u;
        }
        public uint NextUInt()
        {
            uint z = _state += 0x6D2B79F5u;
            z = (z ^ (z >> 15)) * (z | 1u);
            z ^= z + ((z ^ (z >> 7)) * (z | 61u));
            return z ^ (z >> 14);
        }
        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive) throw new ArgumentException("Invalid random range.");
            return minInclusive + (int)(NextUInt() % (uint)(maxExclusive - minInclusive));
        }
        public bool NextBool() => (NextUInt() & 1u) != 0;
        public T Pick<T>(System.Collections.Generic.IReadOnlyList<T> items)
        {
            if (items == null || items.Count == 0) throw new ArgumentException("Cannot pick from empty collection.");
            return items[NextInt(0, items.Count)];
        }
    }
}
