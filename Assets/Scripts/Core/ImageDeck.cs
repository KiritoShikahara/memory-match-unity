using System.Collections.Generic;
using System.Linq;

namespace MemoryMatch.Core
{
    /// ユーザーが画像を置くフォルダと、そのモードのタイトル。
    public sealed class ImageDeck
    {
        public ImageDeck(string title, string folderName)
        {
            Title = title;
            FolderName = folderName;
        }

        public string Title { get; }
        public string FolderName { get; }
    }

    public static class ImageDecks
    {
        public static readonly IReadOnlyList<ImageDeck> All = new[]
        {
            new ImageDeck("クロック神経衰弱", "clock"),
            new ImageDeck("ヘブンズゲート神経衰弱", "hevensgate"),
        };

        public static ImageDeck Default => All[0];

        public static ImageDeck Find(string folderName) =>
            All.FirstOrDefault(d => d.FolderName == folderName) ?? Default;
    }
}
