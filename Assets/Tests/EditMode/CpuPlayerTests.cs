using System.Linq;
using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class CpuPlayerTests
    {
        // 位置: 0 1 2 3 4 5 6 7 / 絵柄: 0 1 2 3 0 1 2 3
        static readonly int[] Layout = { 0, 1, 2, 3, 0, 1, 2, 3 };

        static MemoryGame NewGame() => new MemoryGame(new[]
        {
            new PlayerSlot("CPU", SeatKind.Cpu, CpuLevel.Strong),
            new PlayerSlot("P2", SeatKind.Human),
        }, Layout);

        [Test]
        public void Strong_KnowsBothCardsOfPair_TakesThatPair()
        {
            var game = NewGame();
            var cpu = new CpuPlayer(CpuLevel.Strong, 1);
            cpu.Observe(1, 1);
            cpu.Observe(5, 1);

            var first = cpu.ChooseCard(game);
            Assert.That(first, Is.EqualTo(1).Or.EqualTo(5));
            game.Flip(first);
            cpu.Observe(first, game.VisiblePairId(first));
            var second = cpu.ChooseCard(game);
            game.Flip(second);

            Assert.That(game.PairsOf(0), Is.EqualTo(1));
        }

        [Test]
        public void Strong_KnowsPartnerOfFirstCard_PicksPartner()
        {
            var game = NewGame();
            var cpu = new CpuPlayer(CpuLevel.Strong, 1);
            cpu.Observe(6, 2);

            game.Flip(2);
            cpu.Observe(2, game.VisiblePairId(2));

            Assert.That(cpu.ChooseCard(game), Is.EqualTo(6));
        }

        [Test]
        public void ChooseCard_AlwaysSelectable()
        {
            for (var seed = 0; seed < 50; seed++)
            {
                var game = NewGame();
                game.Flip(0);
                game.Flip(4); // 取得済み
                game.Flip(1); // 表
                var cpu = new CpuPlayer(CpuLevel.Weak, seed);
                var pick = cpu.ChooseCard(game);
                Assert.That(game.IsSelectable(pick), Is.True, $"seed {seed} pick {pick}");
            }
        }

        [Test]
        public void ChooseCard_PrefersUnseenCardsWhenNoKnownPair()
        {
            var game = NewGame();
            var cpu = new CpuPlayer(CpuLevel.Strong, 3);
            cpu.Observe(0, 0);
            cpu.Observe(1, 1);
            cpu.Observe(2, 2);

            var pick = cpu.ChooseCard(game);

            Assert.That(pick, Is.InRange(3, 7));
        }

        [Test]
        public void Unobserved_IsUnknown()
        {
            var cpu = new CpuPlayer(CpuLevel.Strong, 1);
            Assert.That(cpu.Knows(3), Is.False);
        }

        [Test]
        public void MatchedCards_AreForgottenAndNeverPicked()
        {
            var game = NewGame();
            var cpu = new CpuPlayer(CpuLevel.Strong, 1);
            game.CardRevealed += cpu.Observe;
            game.Flip(0);
            game.Flip(4); // CPU(席1)がペア取得、手番は継続

            for (var i = 0; i < 20; i++)
                Assert.That(cpu.ChooseCard(game), Is.Not.EqualTo(0).And.Not.EqualTo(4));
        }

        [TestCase(CpuLevel.Weak, 0.3)]
        [TestCase(CpuLevel.Normal, 0.7)]
        [TestCase(CpuLevel.Strong, 1.0)]
        public void Observe_RemembersAtLevelRate(CpuLevel level, double rate)
        {
            const int n = 1000;
            var cpu = new CpuPlayer(level, 123);
            for (var i = 0; i < n; i++) cpu.Observe(i, 0);

            var known = Enumerable.Range(0, n).Count(cpu.Knows) / (double)n;

            Assert.That(known, Is.EqualTo(rate).Within(0.06));
        }
    }
}
