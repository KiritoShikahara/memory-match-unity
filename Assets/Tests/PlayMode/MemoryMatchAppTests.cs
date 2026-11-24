using System.Collections;
using System.Linq;
using MemoryMatch.Core;
using MemoryMatch.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MemoryMatch.Tests
{
    public class MemoryMatchAppTests
    {
        MemoryMatchApp app;

        [SetUp]
        public void SetUp()
        {
            Time.timeScale = 20f;
            app = new GameObject("MemoryMatchApp").AddComponent<MemoryMatchApp>();
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
            Object.Destroy(app.gameObject);
        }

        [UnityTest]
        public IEnumerator FourCpus_PlayToGameOver()
        {
            app.SetSeat(0, SeatKind.Cpu, CpuLevel.Weak);
            app.SetSeat(1, SeatKind.Cpu, CpuLevel.Normal);
            app.SetSeat(2, SeatKind.Cpu, CpuLevel.Strong);
            app.SetSeat(3, SeatKind.Cpu, CpuLevel.Normal);
            app.StartMatch();

            yield return WaitForGameOver();

            AssertFinished(4);
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
                    var pick = Enumerable.Range(0, game.CardCount).First(game.IsSelectable);
                    Assert.That(game.Flip(pick), Is.True);
                    humanFlips++;
                }
                yield return null;
            }

            AssertFinished(2);
            Assert.That(humanFlips, Is.GreaterThan(0));
        }

        IEnumerator WaitForGameOver()
        {
            var deadline = Time.realtimeSinceStartup + 60f;
            while (Time.realtimeSinceStartup < deadline && app.Game.Phase != GamePhase.GameOver)
                yield return null;
        }

        void AssertFinished(int players)
        {
            var game = app.Game;
            Assert.That(game.Phase, Is.EqualTo(GamePhase.GameOver));
            Assert.That(game.Players.Length, Is.EqualTo(players));
            Assert.That(Enumerable.Range(0, players).Sum(game.PairsOf), Is.EqualTo(8));
            Assert.That(game.Winners(), Is.Not.Empty);
        }
    }
}
