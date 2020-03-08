using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRand
{
    class RandomHash : IRandomGenerator
    {
        int m_seed;
        ulong m_value;

        public RandomHash(Int32 seed)
        {
            m_seed = seed;

            m_value = Hash((ulong)m_seed);
        }

        public ulong Min()
        {
            return ulong.MinValue;
        }

        public ulong Max()
        {
            return ulong.MaxValue;
        }

        public RandomHash Set(params int[] values)
        {
            m_value = Hash((ulong)m_seed);
            foreach (var v in values)
                m_value = Hash(m_value * 37 + (ulong)v);

            return this;
        }

        public RandomHash Set(params float[] values)
        {
            m_value = Hash((ulong)m_seed);
            foreach (var v in values)
                m_value = Hash(m_value * 37 + Cast.Reinterpret<float, ulong>(v));

            return this;
        }

        public ulong Next()
        {
            return m_value;
        }

        static ulong Hash(ulong value)
        {
            value += 1UL;
            value ^= value >> 33;
            value *= 0xff51afd7ed558ccdUL;
            value ^= value >> 33;
            value *= 0xc4ceb9fe1a85ec53UL;
            value ^= value >> 33;
            return value;
        }
    }
}
