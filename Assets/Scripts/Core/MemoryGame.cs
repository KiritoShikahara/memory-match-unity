using System;
using System.Collections.Generic;
using System.Linq;

namespace MemoryMatch.Core
{
    public enum GamePhase { WaitingFirst, WaitingSecond, ShowingMismatch, GameOver }

    public sealed class MemoryGame : IMemoryBoard
    {
        readonly Card[] cards;
        readonly int[] pairs;
        readonly List<int> faceUp = new List<int>(2);

        public MemoryGame(PlayerSlot[] players, int[] layout)
        {
            if (players == null || players.Length < 2) throw new ArgumentException("参加者は2人以上必要です", nameof(players));
            Players = players;
            pairs = new int[players.Length];
            cards = layout.Select((pairId, i) => new Card(i, pairId)).ToArray();
        }

        public static MemoryGame Create(PlayerSlot[] players, int pairCount, int seed) =>
            new MemoryGame(players, Deck.Create(pairCount, seed));

        public event Action<int, int> CardRevealed;

        public PlayerSlot[] Players { get; }
        public int CardCount => cards.Length;
        public int CurrentPlayer { get; private set; }
        public GamePhase Phase { get; private set; } = GamePhase.WaitingFirst;

        public CardState StateOf(int index) => cards[index].State;
        public int OwnerOf(int index) => cards[index].Owner;
        public int PairsOf(int player) => pairs[player];

        public int VisiblePairId(int index) =>
            cards[index].State == CardState.FaceDown ? -1 : cards[index].PairId;

        public bool IsSelectable(int index) =>
            index >= 0 && index < cards.Length && cards[index].State == CardState.FaceDown &&
            (Phase == GamePhase.WaitingFirst || Phase == GamePhase.WaitingSecond);

        public bool Flip(int index)
        {
            if (!IsSelectable(index)) return false;

            var card = cards[index];
            card.State = CardState.FaceUp;
            faceUp.Add(index);
            CardRevealed?.Invoke(index, card.PairId);

            if (Phase == GamePhase.WaitingFirst)
            {
                Phase = GamePhase.WaitingSecond;
                return true;
            }

            var first = cards[faceUp[0]];
            if (first.PairId != card.PairId)
            {
                Phase = GamePhase.ShowingMismatch;
                return true;
            }

            foreach (var c in new[] { first, card })
            {
                c.State = CardState.Matched;
                c.Owner = CurrentPlayer;
            }
            faceUp.Clear();
            pairs[CurrentPlayer]++;
            Phase = cards.All(c => c.State == CardState.Matched) ? GamePhase.GameOver : GamePhase.WaitingFirst;
            return true;
        }

        public void ResolveMismatch()
        {
            if (Phase != GamePhase.ShowingMismatch) return;
            foreach (var i in faceUp) cards[i].State = CardState.FaceDown;
            faceUp.Clear();
            CurrentPlayer = (CurrentPlayer + 1) % Players.Length;
            Phase = GamePhase.WaitingFirst;
        }

        public int[] Winners()
        {
            var best = pairs.Max();
            return Enumerable.Range(0, pairs.Length).Where(p => pairs[p] == best).ToArray();
        }
    }
}
