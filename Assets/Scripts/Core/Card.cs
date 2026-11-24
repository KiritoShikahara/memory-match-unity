namespace MemoryMatch.Core
{
    public enum CardState { FaceDown, FaceUp, Matched }

    public sealed class Card
    {
        public Card(int index, int pairId)
        {
            Index = index;
            PairId = pairId;
        }

        public int Index { get; }
        public int PairId { get; }
        public CardState State { get; set; } = CardState.FaceDown;
        public int Owner { get; set; } = -1;
    }
}
