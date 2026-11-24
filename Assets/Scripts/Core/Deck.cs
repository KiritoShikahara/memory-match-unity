using System;

namespace MemoryMatch.Core
{
    public static class Deck
    {
        public static int[] Create(int pairs, int seed)
        {
            var ids = new int[pairs * 2];
            for (var i = 0; i < ids.Length; i++) ids[i] = i / 2;

            var random = new Random(seed);
            for (var i = ids.Length - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                (ids[i], ids[j]) = (ids[j], ids[i]);
            }
            return ids;
        }
    }
}
