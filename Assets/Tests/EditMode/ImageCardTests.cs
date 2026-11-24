using System.Linq;
using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class ImageCardTests
    {
        [Test]
        public void Select_KeepsImagesOnly_SortedByName_Distinct()
        {
            var files = new[] { "b.PNG", "a.jpg", "notes.txt", "c.jpeg", "a.jpg", "d.gif", "e" };

            Assert.That(ImageCatalog.Select(files), Is.EqualTo(new[] { "a.jpg", "b.PNG", "c.jpeg" }));
        }

        [Test]
        public void Select_UsesFileNameForOrder()
        {
            var files = new[] { @"C:\x\z\b.jpg", @"C:\x\a\c.jpg", @"C:\x\y\a.jpg" };

            Assert.That(ImageCatalog.Select(files), Is.EqualTo(new[] { @"C:\x\y\a.jpg", @"C:\x\z\b.jpg", @"C:\x\a\c.jpg" }));
        }

        [Test]
        public void ThirtySixImages_Make72Cards_EachTwice()
        {
            var game = MemoryGame.Create(new[]
            {
                new PlayerSlot("P1", SeatKind.Human), new PlayerSlot("CPU", SeatKind.Cpu),
            }, 36, 5);

            Assert.That(game.CardCount, Is.EqualTo(72));
        }

        [TestCase(72, 1240f, 470f)]
        [TestCase(6, 1240f, 470f)]
        [TestCase(2, 400f, 300f)]
        [TestCase(100, 1000f, 500f)]
        public void Fit_AllCardsInsideArea(int count, float w, float h)
        {
            const float aspect = 0.716f, gap = 6f;
            var fit = GridLayout.Fit(count, w, h, aspect, gap);
            var rows = (count + fit.Columns - 1) / fit.Columns;

            Assert.That(fit.Columns * rows, Is.GreaterThanOrEqualTo(count));
            Assert.That(fit.Columns * fit.CardWidth + (fit.Columns - 1) * gap, Is.LessThanOrEqualTo(w + 0.01f));
            Assert.That(rows * fit.CardHeight + (rows - 1) * gap, Is.LessThanOrEqualTo(h + 0.01f));
            Assert.That(fit.CardWidth / fit.CardHeight, Is.EqualTo(aspect).Within(0.001f));
        }

        [Test]
        public void Fit_ChoosesLargestCards()
        {
            const float w = 1240f, h = 470f, aspect = 0.716f, gap = 6f;
            var best = GridLayout.Fit(72, w, h, aspect, gap);

            foreach (var cols in Enumerable.Range(1, 72))
            {
                var rows = (72 + cols - 1) / cols;
                var width = System.Math.Min((w - (cols - 1) * gap) / cols, (h - (rows - 1) * gap) / rows * aspect);
                Assert.That(best.CardWidth, Is.GreaterThanOrEqualTo(width - 0.01f), $"cols {cols}");
            }
        }
    }
}
