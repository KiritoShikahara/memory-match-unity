using System.Collections;
using MemoryMatch.Core;
using MemoryMatch.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace MemoryMatch.Tests
{
    public class TitleAppTests
    {
        [TearDown]
        public void TearDown()
        {
            TitleApp.SelectedDeck = ImageDecks.Default;
            TitleApp.SkipTitle = false;
        }

        [UnityTest]
        public IEnumerator ChooseDeck_OpensImageModeWithThatDeck()
        {
            TitleApp.ChooseDeck(ImageDecks.Find("hevensgate"));
            yield return null;
            yield return null;

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("ImageCard"));
            var app = Object.FindAnyObjectByType<ImageCardApp>();
            Assert.That(app.Deck.FolderName, Is.EqualTo("hevensgate"));
            Assert.That(System.IO.Path.GetFileName(app.Folder), Is.EqualTo("hevensgate"));
        }

        [UnityTest]
        public IEnumerator BackToMenu_ShowsSelectScreenNotTitle()
        {
            TitleApp.BackToMenu();
            yield return null;
            yield return null;

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Title"));
            Assert.That(Object.FindAnyObjectByType<TitleApp>().ShowingSelect, Is.True);
        }

        [Test]
        public void TitleScene_IsFirstInBuild()
        {
            Assert.That(SceneUtility.GetScenePathByBuildIndex(0), Does.EndWith("Title.unity"));
        }
    }
}
