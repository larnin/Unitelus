using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRand
{
    public class StaticRandomGenerator<Gen> : IRandomGenerator where Gen : IRandomGenerator, new()
    {
        static Gen _instance;

        public StaticRandomGenerator()
        {
            if (_instance == null)
                _instance = new Gen();
        }

        public ulong Max()
        {
            return _instance.Max();
        }

        public ulong Min()
        {
            return _instance.Min();
        }

        public ulong Next()
        {
            return _instance.Next();
        }
    }
}
