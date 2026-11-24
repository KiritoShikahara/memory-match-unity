using System;
using System.Linq;

namespace MemoryMatch.Core
{
    /// 6×4 の配置と取得済みカード。対戦をまたいで持ち越す。
    public sealed class CardMatchBoard
    {
        public const int CardCount = 24;
        public const int Columns = 6;
        public const int LayoutCount = 8;

        static readonly Prize[] Shuffled =
        {
            Prize.Mushroom, Prize.Mushroom, Prize.Mushroom, Prize.Flower, Prize.Flower, Prize.Flower,
            Prize.Star, Prize.Star, Prize.Star, Prize.OneUp, Prize.OneUp,
            Prize.Coin10, Prize.Coin10, Prize.Coin20, Prize.Coin20, Prize.Gem, Prize.Gem,
            Prize.Clover, Prize.Clover, Prize.Clover, Prize.Clover,
        };
        static readonly Prize[] FixedBottom = { Prize.Mushroom, Prize.Flower, Prize.Star };

        readonly Prize[] layout;
        readonly bool[] taken = new bool[CardCount];

        CardMatchBoard(int layoutIndex)
        {
            LayoutIndex = layoutIndex;
            var head = (Prize[])Shuffled.Clone();
            var random = new Random(layoutIndex);
            for (var i = head.Length - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                (head[i], head[j]) = (head[j], head[i]);
            }
            layout = head.Concat(FixedBottom).ToArray();
        }

        public static CardMatchBoard Deal(int layoutIndex) => new CardMatchBoard(layoutIndex % LayoutCount);

        public int LayoutIndex { get; }
        public bool IsCleared => taken.All(t => t);

        public Prize At(int index) => layout[index];
        public bool IsTaken(int index) => taken[index];
        public void Take(int index) => taken[index] = true;

        /// 取り残しがあれば同じ盤面、全部取られていれば別の配置を配る。
        public CardMatchBoard Next(int seed) =>
            IsCleared ? Deal(LayoutIndex + 1 + new Random(seed).Next(LayoutCount - 1)) : this;
    }
}
