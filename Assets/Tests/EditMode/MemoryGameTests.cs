using System;
using System.Linq;
using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class MemoryGameTests
    {
        // 位置: 0 1 2 3 4 5 6 7 / 絵柄: 0 0 1 1 2 2 3 3
        static readonly int[] Layout = { 0, 0, 1, 1, 2, 2, 3, 3 };

        static PlayerSlot[] Players(int count) =>
            Enumerable.Range(0, count).Select(i => new PlayerSlot($"P{i + 1}", SeatKind.Human)).ToArray();

        static MemoryGame NewGame(int players = 2) => new MemoryGame(Players(players), Layout);

        [Test]
        public void Create_StartsAllFaceDown_ZeroPairs_FirstPlayerTurn()
        {
            var game = MemoryGame.Create(Players(2), 8, 1);

            Assert.That(game.CardCount, Is.EqualTo(16));
            Assert.That(Enumerable.Range(0, 16).All(i => game.StateOf(i) == CardState.FaceDown), Is.True);
            Assert.That(game.PairsOf(0), Is.EqualTo(0));
            Assert.That(game.PairsOf(1), Is.EqualTo(0));
            Assert.That(game.CurrentPlayer, Is.EqualTo(0));
            Assert.That(game.Phase, Is.EqualTo(GamePhase.WaitingFirst));
        }

        [Test]
        public void Create_SameSeed_SameLayout()
        {
            var a = MemoryGame.Create(Players(2), 8, 7);
            var b = MemoryGame.Create(Players(2), 8, 7);
            for (var i = 0; i < 16; i++)
            {
                a.Flip(i);
                b.Flip(i);
                Assert.That(a.VisiblePairId(i), Is.EqualTo(b.VisiblePairId(i)));
                if (a.Phase == GamePhase.ShowingMismatch) a.ResolveMismatch();
                if (b.Phase == GamePhase.ShowingMismatch) b.ResolveMismatch();
            }
        }

        [Test]
        public void Constructor_FewerThanTwoPlayers_Throws()
        {
            Assert.Throws<ArgumentException>(() => new MemoryGame(Players(1), Layout));
        }

        [Test]
        public void VisiblePairId_FaceDown_IsHidden()
        {
            Assert.That(NewGame().VisiblePairId(0), Is.EqualTo(-1));
        }

        [Test]
        public void Flip_Match_TakesPair_SamePlayerContinues()
        {
            var game = NewGame();
            Assert.That(game.Flip(0), Is.True);
            Assert.That(game.Phase, Is.EqualTo(GamePhase.WaitingSecond));
            Assert.That(game.Flip(1), Is.True);

            Assert.That(game.StateOf(0), Is.EqualTo(CardState.Matched));
            Assert.That(game.StateOf(1), Is.EqualTo(CardState.Matched));
            Assert.That(game.OwnerOf(0), Is.EqualTo(0));
            Assert.That(game.PairsOf(0), Is.EqualTo(1));
            Assert.That(game.CurrentPlayer, Is.EqualTo(0));
            Assert.That(game.Phase, Is.EqualTo(GamePhase.WaitingFirst));
        }

        [Test]
        public void Flip_Mismatch_ShowsThenFlipsBackAndPassesTurn()
        {
            var game = NewGame();
            game.Flip(0);
            game.Flip(2);

            Assert.That(game.Phase, Is.EqualTo(GamePhase.ShowingMismatch));
            Assert.That(game.StateOf(0), Is.EqualTo(CardState.FaceUp));
            Assert.That(game.StateOf(2), Is.EqualTo(CardState.FaceUp));
            Assert.That(game.CurrentPlayer, Is.EqualTo(0));

            game.ResolveMismatch();

            Assert.That(game.StateOf(0), Is.EqualTo(CardState.FaceDown));
            Assert.That(game.StateOf(2), Is.EqualTo(CardState.FaceDown));
            Assert.That(game.PairsOf(0), Is.EqualTo(0));
            Assert.That(game.CurrentPlayer, Is.EqualTo(1));
            Assert.That(game.Phase, Is.EqualTo(GamePhase.WaitingFirst));
        }

        [Test]
        public void Flip_ThirdCardDuringMismatch_Rejected()
        {
            var game = NewGame();
            game.Flip(0);
            game.Flip(2);

            Assert.That(game.Flip(4), Is.False);
            Assert.That(game.StateOf(4), Is.EqualTo(CardState.FaceDown));
        }

        [Test]
        public void Flip_SameCardTwice_Rejected()
        {
            var game = NewGame();
            game.Flip(0);

            Assert.That(game.Flip(0), Is.False);
            Assert.That(game.Phase, Is.EqualTo(GamePhase.WaitingSecond));
        }

        [Test]
        public void Flip_MatchedCard_Rejected()
        {
            var game = NewGame();
            game.Flip(0);
            game.Flip(1);

            Assert.That(game.Flip(0), Is.False);
            Assert.That(game.IsSelectable(0), Is.False);
        }

        [Test]
        public void Flip_OutOfRange_Rejected()
        {
            Assert.That(NewGame().Flip(99), Is.False);
        }

        [Test]
        public void Turn_WrapsAroundThreePlayers()
        {
            var game = NewGame(3);
            for (var expectedNext = 1; expectedNext <= 3; expectedNext++)
            {
                game.Flip(0);
                game.Flip(2);
                game.ResolveMismatch();
                Assert.That(game.CurrentPlayer, Is.EqualTo(expectedNext % 3));
            }
        }

        [Test]
        public void CardRevealed_RaisedWithIndexAndPairId()
        {
            var game = NewGame();
            int seenIndex = -1, seenPair = -1;
            game.CardRevealed += (index, pair) => { seenIndex = index; seenPair = pair; };

            game.Flip(5);

            Assert.That(seenIndex, Is.EqualTo(5));
            Assert.That(seenPair, Is.EqualTo(2));
        }

        static void TakePair(MemoryGame game, int a, int b)
        {
            Assert.That(game.Flip(a), Is.True);
            Assert.That(game.Flip(b), Is.True);
        }

        static void Miss(MemoryGame game)
        {
            var down = Enumerable.Range(0, game.CardCount).Where(game.IsSelectable).ToArray();
            var first = down[0];
            var second = down.First(i => i != first && !IsPair(first, i));
            game.Flip(first);
            game.Flip(second);
            game.ResolveMismatch();
        }

        static bool IsPair(int a, int b) => Layout[a] == Layout[b];

        [Test]
        public void LastPair_EndsGame_WinnerHasMostPairs()
        {
            var game = NewGame();
            TakePair(game, 0, 1);
            TakePair(game, 2, 3);
            Miss(game); // P2 の手番へ
            Miss(game); // P1 の手番へ
            TakePair(game, 4, 5);
            Assert.That(game.Phase, Is.Not.EqualTo(GamePhase.GameOver));
            TakePair(game, 6, 7);

            Assert.That(game.Phase, Is.EqualTo(GamePhase.GameOver));
            Assert.That(game.Winners(), Is.EqualTo(new[] { 0 }));
        }

        [Test]
        public void Tie_AllTopPlayersAreWinners()
        {
            var game = NewGame();
            TakePair(game, 0, 1);
            TakePair(game, 2, 3);
            Miss(game);
            TakePair(game, 4, 5);
            TakePair(game, 6, 7);

            Assert.That(game.Phase, Is.EqualTo(GamePhase.GameOver));
            Assert.That(game.Winners(), Is.EqualTo(new[] { 0, 1 }));
        }

        [Test]
        public void AfterGameOver_FlipRejected()
        {
            var game = new MemoryGame(Players(2), new[] { 0, 0 });
            TakePair(game, 0, 1);

            Assert.That(game.Phase, Is.EqualTo(GamePhase.GameOver));
            Assert.That(game.Flip(0), Is.False);
        }
    }
}
