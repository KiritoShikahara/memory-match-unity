using System.Collections;
using System.Linq;
using MemoryMatch.Core;
using MemoryMatch.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MemoryMatch.Tests
{
    public class CardMatchAppTests
    {
        CardMatchApp app;

        [SetUp]
        public void SetUp()
        {
            Time.timeScale = 20f;
            app = new GameObject("CardMatchApp", typeof(AudioSource)).AddComponent<CardMatchApp>();
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
            Object.Destroy(app.gameObject);
        }

        [UnityTest]
        public IEnumerator FourCpus_PlayToGameOver_ThenNextRoundKeepsTakenCards()
        {
            for (var s = 0; s < 4; s++) app.SetSeat(s, SeatKind.Cpu, (CpuLevel)(s % 3));
            app.StartMatch();

            yield return WaitForGameOver();

            var game = app.Game;
            Assert.That(game.Phase, Is.EqualTo(GamePhase.GameOver));
            Assert.That(game.Players.Length, Is.EqualTo(4));
            Assert.That(app.Board.IsCleared, Is.True, "全カードがなくなるまで続く");
            Assert.That(Enumerable.Range(0, 4).Sum(game.PointsOf), Is.EqualTo(460));
            Assert.That(app.Fx.ParticleCount, Is.GreaterThan(0), "終了時の花火");

            var board = app.Board;
            app.StartMatch();
            Assert.That(app.Board, Is.Not.SameAs(board));
            Assert.That(Enumerable.Range(0, app.Game.CardCount).All(i => app.Game.StateOf(i) == CardState.FaceDown), Is.True);
        }

        [UnityTest]
        public IEnumerator HumanVsCpu_PlayToGameOver()
        {
            app.SetSeat(0, SeatKind.Human);
            app.SetSeat(1, SeatKind.Cpu, CpuLevel.Strong);
            app.StartMatch();

            var game = app.Game;
            var humanFlips = 0;
            var deadline = Time.realtimeSinceStartup + 60f;
            while (Time.realtimeSinceStartup < deadline && game.Phase != GamePhase.GameOver)
            {
                if (game.CurrentPlayer == 0 && game.Phase != GamePhase.ShowingMismatch)
                {
                    Assert.That(game.Flip(Enumerable.Range(0, game.CardCount).First(game.IsSelectable)), Is.True);
                    humanFlips++;
                }
                yield return null;
            }

            Assert.That(game.Phase, Is.EqualTo(GamePhase.GameOver));
            Assert.That(humanFlips, Is.GreaterThan(0));
            Assert.That(game.Winners(), Is.Not.Empty);
        }

        IEnumerator WaitForGameOver()
        {
            var deadline = Time.realtimeSinceStartup + 60f;
            while (Time.realtimeSinceStartup < deadline && app.Game.Phase != GamePhase.GameOver)
                yield return null;
        }
    }
}
