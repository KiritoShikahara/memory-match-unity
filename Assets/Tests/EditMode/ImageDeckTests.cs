using System.Linq;
using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class ImageDeckTests
    {
        [Test]
        public void All_HasClockAndHeavensGate_UniqueFolders()
        {
            var folders = ImageDecks.All.Select(d => d.FolderName).ToArray();

            Assert.That(folders, Does.Contain("clock"));
            Assert.That(folders, Does.Contain("hevensgate"));
            Assert.That(folders.Distinct().Count(), Is.EqualTo(folders.Length));
            Assert.That(ImageDecks.All.All(d => !string.IsNullOrWhiteSpace(d.Title)), Is.True);
        }

        [Test]
        public void Find_ByFolder_ReturnsDeck_UnknownReturnsDefault()
        {
            Assert.That(ImageDecks.Find("hevensgate").Title, Is.EqualTo("ヘブンズゲート神経衰弱"));
            Assert.That(ImageDecks.Find("nope"), Is.SameAs(ImageDecks.Default));
            Assert.That(ImageDecks.Default.FolderName, Is.EqualTo("clock"));
        }
    }
}
