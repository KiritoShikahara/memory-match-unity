using System.IO;
using MemoryMatch.Core;
using UnityEngine;

namespace MemoryMatch.Game
{
    /// デッキのフォルダ名を実パスにする。Editor はプロジェクト直下、ビルドは StreamingAssets。
    public static class ImageDeckPaths
    {
        public static string Resolve(string folderName) =>
            Application.isEditor
                ? Path.GetFullPath(Path.Combine(Application.dataPath, "..", folderName))
                : Path.Combine(Application.streamingAssetsPath, folderName);

        public static int CountImages(ImageDeck deck)
        {
            var folder = Resolve(deck.FolderName);
            return Directory.Exists(folder) ? ImageCatalog.Select(Directory.GetFiles(folder)).Length : 0;
        }
    }
}
