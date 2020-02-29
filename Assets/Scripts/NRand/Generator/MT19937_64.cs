using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRand
{
    class MT19937_64 : IRandomGenerator
    {
        private const int w = 64;
        private const ulong n = 312;
        private const ulong m = 156;
        private const ulong r = 31;
        private const ulong a = 0xB5026F5AA96619E9;
        private const int u = 29;
        private const ulong d = 0x5555555555555555;
        private const int s = 17;
        private const ulong b = 0x71D67FFFEDA60000;
        private const int t = 37;
        private const ulong c = 0xFFF7EEE000000000;
        private const int l = 43;
        private const ulong f = 6364136223846793005;

        public const ulong lower_mask = 0x7FFFFFFF;
        public const ulong upper_mask = ~lower_mask;

        private ulong[] MT = new ulong[n];
        private ulong index = n + 1;

        public MT19937_64()
        {
            seed_mt((ulong)DateTime.Now.Ticks);
        }

        public MT19937_64(ulong seed)
        {
            seed_mt(seed);
        }

        private void seed_mt(ulong seed)
        {
            index = n;
            MT[0] = seed;

            for (ulong i = 1; i < n; ++i)
            {
                MT[i] = (f * (MT[i - 1] ^ (MT[i - 1] >> (w - 2))) + i);
            }
        }

        private ulong extract_number()
        {
            if (index >= n)
                twist();

            ulong y = MT[index];
            y = y ^ ((y >> u) & d);
            y = y ^ ((y << s) & b);
            y = y ^ ((y << t) & c);
            y = y ^ (y >> l);

            ++index;

            return y;
        }

        private void twist()
        {
            for (ulong i = 0; i < n; ++i)
            {
                ulong x = (MT[i] & upper_mask) + (MT[(i + 1) % n] & lower_mask);
                ulong xA = x >> 1;

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
            return ulong.MaxValue;
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
