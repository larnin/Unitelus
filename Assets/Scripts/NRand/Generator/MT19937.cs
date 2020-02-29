using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRand
{
    class MT19937 : IRandomGenerator
    {
        private const int w = 32;
        private const uint n = 624;
        private const uint m = 397;
        private const uint r = 31;
        private const uint a = 0x9908b0df;
        private const int u = 11;
        private const uint d = 0xffffffff;
        private const int s = 7;
        private const uint b = 0x9d2c5680;
        private const int t = 15;
        private const uint c = 0xefc60000;
        private const int l = 18;
        private const uint f = 1812433253;

        public const uint lower_mask = 0x7FFFFFFF;
        public const uint upper_mask = ~lower_mask;

        private uint[] MT = new uint[n];
        private uint index = n + 1;

        public MT19937()
        {
            seed_mt((uint)DateTime.Now.Ticks);
        }

        public MT19937(uint seed)
        {
            seed_mt(seed);
        }

        private void seed_mt(uint seed)
        {
            index = n;
            MT[0] = seed;

            for (uint i = 1; i < n; ++i)
            {
                MT[i] = (f * (MT[i - 1] ^ (MT[i - 1] >> (w - 2))) + i);
            }
        }

        private uint extract_number()
        {
            if (index >= n)
                twist();

            uint y = MT[index];
            y = y ^ ((y >> u) & d);
            y = y ^ ((y << s) & b);
            y = y ^ ((y << t) & c);
            y = y ^ (y >> l);

            ++index;

            return y;
        }

        private void twist()
        {
            for (uint i = 0; i < n; ++i)
            {
                uint x = (MT[i] & upper_mask) + (MT[(i + 1) % n] & lower_mask);
                uint xA = x >> 1;

                if (x % 2 != 0)
                {
                    xA = xA ^ a;
                }

                MT[i] = MT[(i + m) % n] ^ xA;
            }

            index = 0;
        }

        public ulong Max()
        {
            return uint.MaxValue;
        }

        public ulong Min()
        {
            return 0;
        }

        public ulong Next()
        {
            return extract_number();
        }
    }
}
