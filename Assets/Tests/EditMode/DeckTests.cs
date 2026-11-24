using System.Linq;
using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class DeckTests
    {
        [Test]
        public void Create_8Pairs_Has16CardsWithEachPairIdTwice()
        {
            var ids = Deck.Create(8, 1);

            Assert.That(ids.Length, Is.EqualTo(16));
            for (var pair = 0; pair < 8; pair++)
                Assert.That(ids.Count(id => id == pair), Is.EqualTo(2), $"pair {pair}");
        }

        [Test]
        public void Create_SameSeed_SameLayout()
        {
            Assert.That(Deck.Create(8, 42), Is.EqualTo(Deck.Create(8, 42)));
        }

        [Test]
        public void Create_DifferentSeeds_ProduceSomeDifferentLayout()
        {
            var first = Deck.Create(8, 0);
            var anyDifferent = Enumerable.Range(1, 10).Any(seed => !Deck.Create(8, seed).SequenceEqual(first));
            Assert.That(anyDifferent, Is.True);
        }
    }
}
