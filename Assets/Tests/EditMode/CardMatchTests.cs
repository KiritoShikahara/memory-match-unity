using System;
using System.Collections.Generic;
using System.Linq;
using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class CardMatchTests
    {
        const int N = CardMatchBoard.CardCount;

        static PlayerSlot[] Players(int count) =>
            Enumerable.Range(0, count).Select(i => new PlayerSlot($"P{i + 1}", SeatKind.Human)).ToArray();

        static CardMatchGame NewGame(int players = 2, CardMatchBoard board = null) =>
            new CardMatchGame(Players(players), board ?? CardMatchBoard.Deal(0));

        static int[] IndicesOf(CardMatchBoard board, Prize prize) =>
            Enumerable.Range(0, N).Where(i => board.At(i) == prize).ToArray();

        static void Miss(CardMatchGame game, CardMatchBoard board)
        {
            var open = Enumerable.Range(0, N).Where(game.IsSelectable).ToArray();
            var a = open[0];
            game.Flip(a);
            game.Flip(open.First(i => board.At(i) != board.At(a)));
            game.ResolveMismatch();
        }

        static void TakeAll(CardMatchGame game, CardMatchBoard board, Prize prize)
        {
            var cards = IndicesOf(board, prize);
            for (var k = 0; k < cards.Length; k += 2)
            {
                game.Flip(cards[k]);
                game.Flip(cards[k + 1]);
            }
        }

        [Test]
        public void Layouts_EightDistinct_24Cards_WithFixedBottomAndPrizeCounts()
        {
            Assert.That(N, Is.EqualTo(24));
            Assert.That(CardMatchBoard.LayoutCount, Is.EqualTo(8));
            var expected = new Dictionary<Prize, int>
            {
                [Prize.Mushroom] = 4, [Prize.Flower] = 4, [Prize.Star] = 4, [Prize.OneUp] = 2,
                [Prize.Coin10] = 2, [Prize.Coin20] = 2, [Prize.Gem] = 2, [Prize.Clover] = 4,
            };
            var layouts = Enumerable.Range(0, 8).Select(CardMatchBoard.Deal).ToArray();
            foreach (var b in layouts)
            {
                foreach (var e in expected)
                    Assert.That(IndicesOf(b, e.Key).Length, Is.EqualTo(e.Value), e.Key.ToString());
                Assert.That(new[] { b.At(21), b.At(22), b.At(23) }, Is.EqualTo(new[] { Prize.Mushroom, Prize.Flower, Prize.Star }));
            }
            var keys = layouts.Select(b => string.Join(",", Enumerable.Range(0, N).Select(b.At))).Distinct();
            Assert.That(keys.Count(), Is.EqualTo(8));
        }

        [Test]
        public void Points_PerPrize()
        {
            Assert.That(Prizes.PointsOf(Prize.Coin10), Is.EqualTo(10));
            Assert.That(Prizes.PointsOf(Prize.Clover), Is.EqualTo(15));
            Assert.That(Prizes.PointsOf(Prize.Coin20), Is.EqualTo(20));
            Assert.That(Prizes.PointsOf(Prize.Mushroom), Is.EqualTo(30));
            Assert.That(Prizes.PointsOf(Prize.Flower), Is.EqualTo(40));
            Assert.That(Prizes.PointsOf(Prize.Star), Is.EqualTo(50));
            Assert.That(Prizes.PointsOf(Prize.Gem), Is.EqualTo(60));
            Assert.That(Prizes.PointsOf(Prize.OneUp), Is.EqualTo(100));
        }

        [Test]
        public void Constructor_FewerThanTwoPlayers_Throws()
        {
            Assert.Throws<ArgumentException>(() => new CardMatchGame(Players(1), CardMatchBoard.Deal(0)));
        }

        [Test]
        public void Start_AllFaceDown_FirstPlayer_NoPoints()
        {
            var game = NewGame();
            Assert.That(Enumerable.Range(0, N).All(i => game.StateOf(i) == CardState.FaceDown), Is.True);
            Assert.That(game.CurrentPlayer, Is.EqualTo(0));
            Assert.That(game.Phase, Is.EqualTo(GamePhase.WaitingFirst));
            Assert.That(game.PointsOf(0), Is.EqualTo(0));
            Assert.That(game.VisiblePairId(0), Is.EqualTo(-1));
        }

        [Test]
        public void Match_AwardsPrizeAndPoints_SamePlayerContinues()
        {
            var board = CardMatchBoard.Deal(0);
            var game = NewGame(board: board);
            var stars = IndicesOf(board, Prize.Star);

            game.Flip(stars[0]);
            game.Flip(stars[1]);

            Assert.That(game.StateOf(stars[0]), Is.EqualTo(CardState.Matched));
            Assert.That(game.OwnerOf(stars[0]), Is.EqualTo(0));
            Assert.That(game.PointsOf(0), Is.EqualTo(50));
            Assert.That(game.PrizesOf(0), Is.EqualTo(new[] { Prize.Star }));
            Assert.That(game.CurrentPlayer, Is.EqualTo(0));
        }

        [Test]
        public void Mismatch_CountsMiss_PassesTurn()
        {
            var board = CardMatchBoard.Deal(0);
            var game = NewGame(board: board);
            var open = Enumerable.Range(0, N).ToArray();
            var a = open[0];
            var b = open.First(i => board.At(i) != board.At(a));
            game.Flip(a);
            game.Flip(b);

            Assert.That(game.Phase, Is.EqualTo(GamePhase.ShowingMismatch));
            Assert.That(Enumerable.Range(0, N).Any(game.IsSelectable), Is.False);
            game.ResolveMismatch();

            Assert.That(game.StateOf(a), Is.EqualTo(CardState.FaceDown));
            Assert.That(game.MissesOf(0), Is.EqualTo(1));
            Assert.That(game.CurrentPlayer, Is.EqualTo(1));
        }

        [Test]
        public void ManyMisses_NeverEndGame_TurnRotates()
        {
            var board = CardMatchBoard.Deal(0);
            var game = NewGame(3, board);
            for (var i = 0; i < 9; i++) Miss(game, board);

            Assert.That(game.Phase, Is.EqualTo(GamePhase.WaitingFirst));
            Assert.That(game.CurrentPlayer, Is.EqualTo(0));
            Assert.That(game.MissesOf(0), Is.EqualTo(3));
        }

        [Test]
        public void GameEndsOnlyWhenAllCardsTaken_WinnerHasMostPoints()
        {
            var board = CardMatchBoard.Deal(3);
            var game = NewGame(2, board);
            Miss(game, board);
            Miss(game, board);
            Miss(game, board); // P2 の手番へ
            var prizes = (Prize[])Enum.GetValues(typeof(Prize));
            foreach (var prize in prizes)
            {
                Assert.That(game.Phase, Is.Not.EqualTo(GamePhase.GameOver));
                TakeAll(game, board, prize);
            }

            Assert.That(game.Phase, Is.EqualTo(GamePhase.GameOver));
            Assert.That(game.PointsOf(1), Is.EqualTo(10 + 20 + 30 * 2 + 40 * 2 + 50 * 2 + 100 + 60 + 15 * 2));
            Assert.That(game.Winners(), Is.EqualTo(new[] { 1 }));
            Assert.That(board.IsCleared, Is.True);
        }

        [Test]
        public void Combo_CountsConsecutiveMatches_ResetsOnMiss()
        {
            var board = CardMatchBoard.Deal(0);
            var game = NewGame(2, board);
            var events = new List<(int player, Prize prize, int combo)>();
            game.PairMatched += (p, prize, combo) => events.Add((p, prize, combo));

            TakeAll(game, board, Prize.Clover); // 2ペア連続
            Assert.That(game.Combo, Is.EqualTo(2));
            Miss(game, board);
            Assert.That(game.Combo, Is.EqualTo(0));
            TakeAll(game, board, Prize.OneUp);

            Assert.That(events, Is.EqualTo(new[]
            {
                (0, Prize.Clover, 1), (0, Prize.Clover, 2), (1, Prize.OneUp, 1),
            }));
        }

        [Test]
        public void Next_KeepsUnclearedBoard_DealsNewWhenCleared()
        {
            var board = CardMatchBoard.Deal(1);
            board.Take(0);
            Assert.That(board.Next(5), Is.SameAs(board));

            for (var i = 0; i < N; i++) board.Take(i);
            var fresh = board.Next(5);

            Assert.That(fresh, Is.Not.SameAs(board));
            Assert.That(fresh.LayoutIndex, Is.Not.EqualTo(board.LayoutIndex));
            Assert.That(Enumerable.Range(0, N).Any(fresh.IsTaken), Is.False);
        }

        [Test]
        public void Cpu_Strong_TakesKnownPairOnCardMatchBoard()
        {
            var board = CardMatchBoard.Deal(0);
            var game = NewGame(board: board);
            var cpu = new CpuPlayer(CpuLevel.Strong, 1);
            game.CardRevealed += cpu.Observe;
            var ups = IndicesOf(board, Prize.OneUp);
            cpu.Observe(ups[0], (int)Prize.OneUp);
            cpu.Observe(ups[1], (int)Prize.OneUp);

            game.Flip(cpu.ChooseCard(game));
            game.Flip(cpu.ChooseCard(game));

            Assert.That(game.PointsOf(0), Is.EqualTo(100));
        }
    }
}
