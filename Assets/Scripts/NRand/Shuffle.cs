using System.Collections.Generic;

namespace NRand
{
    public static class ShuffleExtension
    {
        public static IList<T> Shuffle<T>(this IList<T> list, IRandomGenerator generator)
        {
            int nb = list.Count;

            UniformIntDistribution d = new UniformIntDistribution(0, nb);

            for(int i = 0; i < nb; i++)
            {
                int index = d.Next(generator);
                list.Swap(i, index);
            }

            return list;
        }

        public static IList<T> Swap<T>(this IList<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
            return list;
        }
    }
}
