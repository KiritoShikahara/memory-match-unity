using System.Collections.Generic;
using MemoryMatch.Core;
using UnityEngine;

namespace MemoryMatch.Game
{
    /// オリジナルの 12×12 ドット絵。文字グリッドをパレットで Texture2D にする。
    public static class PixelArt
    {
        static readonly Dictionary<char, Color32> Palette = new Dictionary<char, Color32>
        {
            ['K'] = new Color32(40, 30, 30, 255),
            ['Y'] = new Color32(250, 210, 60, 255),
            ['O'] = new Color32(225, 150, 40, 255),
            ['B'] = new Color32(240, 120, 170, 255),
            ['Q'] = new Color32(255, 200, 225, 255),
            ['C'] = new Color32(235, 215, 175, 255),
            ['P'] = new Color32(170, 110, 220, 255),
            ['G'] = new Color32(70, 170, 80, 255),
            ['R'] = new Color32(235, 90, 130, 255),
            ['W'] = new Color32(255, 255, 255, 255),
            ['N'] = new Color32(40, 60, 120, 255),
            ['L'] = new Color32(90, 120, 200, 255),
            ['T'] = new Color32(90, 230, 240, 255),
            ['D'] = new Color32(30, 140, 170, 255),
            ['E'] = new Color32(30, 120, 50, 255),
        };
        static readonly string[] Gem =
        {
            "............", "...KKKKKK...", "..KTTWTTDK..", ".KTTWTTTDDK.", "KKKKKKKKKKKK", ".KTTTTTTDDK.",
            "..KTTTTTDK..", "...KTTTDK...", "....KTDK....", ".....KK.....", "............", "............",
        };
        static readonly string[] Clover =
        {
            "..GGG..GGG..", ".GGGGGGGGGG.", ".GGGEGGEGGG.", "..GGGEEGGG..", ".GGGGEEGGGG.", "GGGEGEEGEGGG",
            ".GGGGGGGGGG.", "..GGG..GGG..", ".....EE.....", "......EE....", ".......EE...", "............",
        };

        static readonly string[] Coin =
        {
            "....KKKK....", "..KKYYYYKK..", ".KYYOOOOYYK.", ".KYOYYYYOYK.", "KYOYYWYYYOYK", "KYOYYWYYYOYK",
            "KYOYYWYYYOYK", "KYOYYYYYYOYK", ".KYOYYYYOYK.", ".KYYOOOOYYK.", "..KKYYYYKK..", "....KKKK....",
        };
        static readonly string[] Mushroom =
        {
            "....KKKK....", "..KKBBBBKK..", ".KBBBBBBBBK.", "KBBQBBBBQBBK", "KBBBBBBBBBBK", "KKKKKKKKKKKK",
            "...KCCCCK...", "...KCCCCK...", "...KCCCCK...", "...KCCCCK...", "...KKKKKK...", "............",
        };
        static readonly string[] Flower =
        {
            "....PPPP....", "...PPPPPP...", ".PPPPYYPPPP.", "PPPPYYYYPPPP", ".PPPPYYPPPP.", "...PPPPPP...",
            "....PPPP....", ".....GG.....", "..GG.GG.....", "...GGGG.GG..", ".....GGGG...", ".....GG.....",
        };
        static readonly string[] Star =
        {
            ".....YY.....", ".....YY.....", "....YYYY....", "YYYYYYYYYYYY", ".YYYYYYYYYY.", "..YYYYYYYY..",
            "...YYYYYY...", "...YYYYYY...", "..YYYY.YYYY.", "..YYY...YYY.", ".YY......YY.", "............",
        };
        static readonly string[] Heart =
        {
            "............", "..RRR..RRR..", ".RRWRRRRRRR.", "RRWRRRRRRRRR", "RRRRRRRRRRRR", "RRRRRRRRRRRR",
            ".RRRRRRRRRR.", "..RRRRRRRR..", "...RRRRRR...", "....RRRR....", ".....RR.....", "............",
        };
        static readonly string[] Back =
        {
            "NNNNNLLNNNNN", "NNNNLNNLNNNN", "NNNLNNNNLNNN", "NNLNNNNNNLNN", "NLNNNLLNNNLN", "LNNNLNNLNNNL",
            "LNNNLNNLNNNL", "NLNNNLLNNNLN", "NNLNNNNNNLNN", "NNNLNNNNLNNN", "NNNNLNNLNNNN", "NNNNNLLNNNNN",
        };

        static readonly Dictionary<string, Texture2D> Cache = new Dictionary<string, Texture2D>();

        public static Texture2D CardBack => Get("back", Back);

        public static Texture2D Of(Prize prize)
        {
            switch (prize)
            {
                case Prize.Mushroom: return Get("mushroom", Mushroom);
                case Prize.Flower: return Get("flower", Flower);
                case Prize.Star: return Get("star", Star);
                case Prize.OneUp: return Get("heart", Heart);
                case Prize.Gem: return Get("gem", Gem);
                case Prize.Clover: return Get("clover", Clover);
                default: return Get("coin", Coin);
            }
        }

        static Texture2D Get(string key, string[] rows)
        {
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;
            var size = rows.Length;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
                texture.SetPixel(x, size - 1 - y, Palette.TryGetValue(rows[y][x], out var c) ? c : new Color32(0, 0, 0, 0));
            texture.Apply();
            return Cache[key] = texture;
        }
    }
}
