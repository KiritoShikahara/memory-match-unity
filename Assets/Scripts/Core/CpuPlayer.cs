using System;
using System.Collections.Generic;
using System.Linq;

namespace MemoryMatch.Core
{
    public sealed class CpuPlayer
    {
        readonly Dictionary<int, int> memory = new Dictionary<int, int>();
        readonly Random random;
        readonly double rememberRate;

        public CpuPlayer(CpuLevel level, int seed)
        {
            random = new Random(seed);
            rememberRate = level == CpuLevel.Weak ? 0.3 : level == CpuLevel.Normal ? 0.7 : 1.0;
        }

        public bool Knows(int index) => memory.ContainsKey(index);

        public void Observe(int index, int pairId)
        {
            if (random.NextDouble() < rememberRate) memory[index] = pairId;
        }

        public int ChooseCard(IMemoryBoard game)
        {
            var selectable = Enumerable.Range(0, game.CardCount).Where(game.IsSelectable).ToArray();
            var known = memory.Where(m => game.IsSelectable(m.Key)).ToArray();

            if (game.Phase == GamePhase.WaitingSecond)
            {
                var first = Enumerable.Range(0, game.CardCount).First(i => game.StateOf(i) == CardState.FaceUp);
                var pairId = game.VisiblePairId(first);
                foreach (var m in known)
                    if (m.Value == pairId) return m.Key;
            }
            else
            {
                var pair = known.GroupBy(m => m.Value).FirstOrDefault(g => g.Count() >= 2);
                if (pair != null) return pair.First().Key;
            }

            var unseen = selectable.Where(i => !memory.ContainsKey(i)).ToArray();
            var pool = unseen.Length > 0 ? unseen : selectable;
            return pool[random.Next(pool.Length)];
        }
    }
}
