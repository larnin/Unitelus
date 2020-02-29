using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRand
{
    public class DefaultRandomGenerator : IRandomGenerator
    {
        System.Random rand;

        public DefaultRandomGenerator()
        {
            rand = new System.Random();
        }

        public DefaultRandomGenerator(Int32 seed)
        {
            rand = new System.Random(seed);
        }

        public ulong Max()
        {
            return Int32.MaxValue;
        }

        public ulong Min()
        {
            return 0;
        }

        public ulong Next()
        {
            return (ulong)rand.Next();
        }
    }
}
