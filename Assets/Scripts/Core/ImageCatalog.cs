using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MemoryMatch.Core
{
    /// フォルダ内のファイルから、カードの柄に使う画像を選ぶ。
    public static class ImageCatalog
    {
        static readonly string[] Extensions = { ".jpg", ".jpeg", ".png" };

        public static string[] Select(IEnumerable<string> paths) =>
            paths.Where(p => Extensions.Contains(Path.GetExtension(p), StringComparer.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(p => Path.GetFileName(p), StringComparer.OrdinalIgnoreCase)
                .ToArray();
    }
}
