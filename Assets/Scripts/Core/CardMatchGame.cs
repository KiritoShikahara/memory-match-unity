using System;
using System.Collections.Generic;
using System.Linq;

namespace MemoryMatch.Core
{
    /// 1回分の対戦。外したら交代、全カードがなくなるまで続く。取得は CardMatchBoard に書き戻す。
    public sealed class CardMatchGame : IMemoryBoard
    {

        readonly CardMatchBoard board;
        readonly Card[] cards;
        readonly int[] misses;
        readonly List<Prize>[] prizes;
        readonly List<int> faceUp = new List<int>(2);

        public CardMatchGame(PlayerSlot[] players, CardMatchBoard board)
        {
            if (players == null || players.Length < 2) throw new ArgumentException("参加者は2人以上必要です", nameof(players));
            Players = players;
            this.board = board;
            misses = new int[players.Length];
            prizes = players.Select(_ => new List<Prize>()).ToArray();
            cards = Enumerable.Range(0, CardMatchBoard.CardCount).Select(i => new Card(i, (int)board.At(i))
            {
                State = board.IsTaken(i) ? CardState.Matched : CardState.FaceDown,
            }).ToArray();
            if (board.IsCleared) Phase = GamePhase.GameOver;
        }

        public event Action<int, int> CardRevealed;

        /// (プレイヤー, 景品, その手番での連続数)
        public event Action<int, Prize, int> PairMatched;

        public int Combo { get; private set; }

        public PlayerSlot[] Players { get; }
        public int CardCount => cards.Length;
        public int CurrentPlayer { get; private set; }
        public GamePhase Phase { get; private set; } = GamePhase.WaitingFirst;

        public CardState StateOf(int index) => cards[index].State;
        public int OwnerOf(int index) => cards[index].Owner;
        public int MissesOf(int player) => misses[player];
        public IReadOnlyList<Prize> PrizesOf(int player) => prizes[player];
        public int PointsOf(int player) => prizes[player].Sum(Prizes.PointsOf);

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
                board.Take(c.Index);
            }
            faceUp.Clear();
            prizes[CurrentPlayer].Add((Prize)card.PairId);
            Combo++;
            Phase = board.IsCleared ? GamePhase.GameOver : GamePhase.WaitingFirst;
            PairMatched?.Invoke(CurrentPlayer, (Prize)card.PairId, Combo);
            return true;
        }

        public void ResolveMismatch()
        {
            if (Phase != GamePhase.ShowingMismatch) return;
            foreach (var i in faceUp) cards[i].State = CardState.FaceDown;
            faceUp.Clear();
            misses[CurrentPlayer]++;
            Combo = 0;
            CurrentPlayer = (CurrentPlayer + 1) % Players.Length;
            Phase = GamePhase.WaitingFirst;
        }

        public int[] Winners()
        {
            var best = Enumerable.Range(0, Players.Length).Max(PointsOf);
            return Enumerable.Range(0, Players.Length).Where(p => PointsOf(p) == best).ToArray();
        }
    }
}
