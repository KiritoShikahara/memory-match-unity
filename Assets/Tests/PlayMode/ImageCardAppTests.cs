using System.Collections;
using System.IO;
using System.Linq;
using MemoryMatch.Core;
using MemoryMatch.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MemoryMatch.Tests
{
    public class ImageCardAppTests
    {
        ImageCardApp app;
        string folder;

        [SetUp]
        public void SetUp()
        {
            folder = Path.Combine(Path.GetTempPath(), "imagecard-" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            var colors = new[] { Color.red, Color.green, Color.blue };
            for (var i = 0; i < colors.Length; i++)
            {
                var texture = new Texture2D(5, 7);
                texture.SetPixels(Enumerable.Repeat(colors[i], 35).ToArray());
                File.WriteAllBytes(Path.Combine(folder, $"card{i}.png"), texture.EncodeToPNG());
                Object.Destroy(texture);
            }
            File.WriteAllText(Path.Combine(folder, "readme.txt"), "not an image");

            Time.timeScale = 20f;
            var go = new GameObject("ImageCardApp", typeof(AudioSource));
            go.SetActive(false);
            app = go.AddComponent<ImageCardApp>();
            app.LoadImages(folder);
            app.ConfirmAfterPair = false;
            go.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
            Object.Destroy(app.gameObject);
            Directory.Delete(folder, true);
        }

        [UnityTest]
        public IEnumerator TwoCpus_AllImagePairs_PlayToGameOver()
        {
            Assert.That(app.ImageCount, Is.EqualTo(3));
            app.SetSeat(0, SeatKind.Cpu, CpuLevel.Weak);
            app.SetSeat(1, SeatKind.Cpu, CpuLevel.Strong);
            app.StartMatch();
            var game = app.Game;
            Assert.That(game.CardCount, Is.EqualTo(6));

            var deadline = Time.realtimeSinceStartup + 60f;
            while (Time.realtimeSinceStartup < deadline && game.Phase != GamePhase.GameOver)
                yield return null;

            Assert.That(game.Phase, Is.EqualTo(GamePhase.GameOver));
            Assert.That(game.PairsOf(0) + game.PairsOf(1), Is.EqualTo(3));
            Assert.That(app.Fx.ParticleCount, Is.GreaterThan(0));
        }

        [UnityTest]
        public IEnumerator Preview_FollowsFlippedCard_ZoomPausesGame()
        {
            app.SetSeat(0, SeatKind.Cpu, CpuLevel.Weak);
            app.SetSeat(1, SeatKind.Cpu, CpuLevel.Weak);
            app.StartMatch();
            Assert.That(app.PreviewIndex, Is.EqualTo(-1));
            Assert.That(app.SetZoom(true), Is.False, "何もめくっていないと拡大できない");

            var deadline = Time.realtimeSinceStartup + 10f;
            while (Time.realtimeSinceStartup < deadline && app.PreviewIndex < 0) yield return null;
            Assert.That(app.Game.StateOf(app.PreviewIndex), Is.Not.EqualTo(CardState.FaceDown));

            Assert.That(app.SetZoom(true), Is.True);
            var faceUp = Enumerable.Range(0, app.Game.CardCount).Select(app.Game.StateOf).ToArray();
            var phase = app.Game.Phase;
            var until = Time.realtimeSinceStartup + 0.5f; // timeScale 20 でもゲーム内 10 秒分
            while (Time.realtimeSinceStartup < until) yield return null;

            Assert.That(app.Game.Phase, Is.EqualTo(phase));
            Assert.That(Enumerable.Range(0, app.Game.CardCount).Select(app.Game.StateOf), Is.EqualTo(faceUp));
            app.SetZoom(false);
            Assert.That(app.Zoomed, Is.False);
        }

        [UnityTest]
        public IEnumerator SecondCard_WaitsForConfirm_ThenAdvances()
        {
            app.ConfirmAfterPair = true;
            app.SetSeat(0, SeatKind.Cpu, CpuLevel.Weak);
            app.SetSeat(1, SeatKind.Cpu, CpuLevel.Weak);
            app.StartMatch();

            var deadline = Time.realtimeSinceStartup + 10f;
            while (Time.realtimeSinceStartup < deadline && !app.AwaitingConfirm) yield return null;
            Assert.That(app.AwaitingConfirm, Is.True);

            var states = Enumerable.Range(0, app.Game.CardCount).Select(app.Game.StateOf).ToArray();
            var player = app.Game.CurrentPlayer;
            var until = Time.realtimeSinceStartup + 0.5f;
            while (Time.realtimeSinceStartup < until) yield return null;
            Assert.That(Enumerable.Range(0, app.Game.CardCount).Select(app.Game.StateOf), Is.EqualTo(states), "確認まで動かない");
            Assert.That(app.Game.CurrentPlayer, Is.EqualTo(player));

            var mismatch = app.Game.Phase == GamePhase.ShowingMismatch;
            app.Confirm();
            Assert.That(app.AwaitingConfirm, Is.False);
            if (mismatch)
            {
                Assert.That(app.Game.Phase, Is.EqualTo(GamePhase.WaitingFirst));
                Assert.That(app.Game.CurrentPlayer, Is.Not.EqualTo(player));
            }
        }

        [Test]
        public void EmptyFolder_CannotStart()
        {
            var empty = Path.Combine(folder, "empty");
            Directory.CreateDirectory(empty);
            app.LoadImages(empty);
            app.SetSeat(1, SeatKind.Cpu);
            app.StartMatch();

            Assert.That(app.ImageCount, Is.EqualTo(0));
            Assert.That(app.Game, Is.Null);
        }
    }
}
