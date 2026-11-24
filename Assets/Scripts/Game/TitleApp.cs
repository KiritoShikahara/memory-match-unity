using System.Linq;
using MemoryMatch.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MemoryMatch.Game
{
    /// タイトルとゲーム選択。画像デッキの選択はシーンをまたいで SelectedDeck に残す。
    public class TitleApp : MonoBehaviour
    {
        public static ImageDeck SelectedDeck = ImageDecks.Default;
        public static bool SkipTitle;

        int[] imageCounts;
        bool showingSelect;

        public bool ShowingSelect => showingSelect;

        public static void ChooseDeck(ImageDeck deck)
        {
            SelectedDeck = deck;
            SceneManager.LoadScene("ImageCard");
        }

        public static void BackToMenu()
        {
            SkipTitle = true;
            SceneManager.LoadScene("Title");
        }

        void Awake()
        {
            showingSelect = SkipTitle;
            imageCounts = ImageDecks.All.Select(ImageDeckPaths.CountImages).ToArray();
        }

        void OnGUI()
        {
            var scale = Screen.height / 720f;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            var width = Screen.width / scale;
            Fill(new Rect(0, 0, width, 720), new Color(0.05f, 0.07f, 0.16f));
            if (showingSelect) DrawSelect(width);
            else DrawTitle(width);
        }

        void DrawTitle(float width)
        {
            for (var i = 0; i < 14; i++)
            {
                var x = Mathf.Repeat(i * 97f + Time.time * (20f + i % 4 * 9f), width + 120f) - 60f;
                var y = Mathf.Repeat(i * 151f + Time.time * (40f + i % 3 * 15f), 840f) - 120f;
                GUIUtility.RotateAroundPivot(Mathf.Sin(Time.time + i) * 25f, new Vector2(x + 30, y + 42));
                GUI.DrawTexture(new Rect(x, y, 60, 84), PixelArt.CardBack);
                GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.height / 720f, Screen.height / 720f, 1f));
            }

            Fill(new Rect(width / 2 - 420, 200, 840, 200), new Color(0, 0, 0, 0.92f));
            var title = Style(68);
            title.normal.textColor = Color.Lerp(Color.white, new Color(1f, 0.85f, 0.3f), Mathf.PingPong(Time.time, 1f));
            GUI.Label(new Rect(0, 220, width, 100), "神経衰弱コレクション", title);
            GUI.Label(new Rect(0, 320, width, 50), "いろんなカードで みんなと対戦！", Style(26, FontStyle.Normal));
            if (Mathf.Repeat(Time.time, 1.2f) < 0.8f)
                GUI.Label(new Rect(0, 520, width, 50), "クリック / Enter でスタート", Style(30));

            var e = Event.current;
            if (e.type == EventType.MouseDown || (e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.Space)))
            {
                showingSelect = true;
                e.Use();
            }
        }

        void DrawSelect(float width)
        {
            GUI.Label(new Rect(0, 30, width, 80), "どのゲームであそぶ？", Style(50));
            var left = width / 2 - 400;
            var y = 140f;

            if (Choice(new Rect(left, y, 800, 100), "神経衰弱（4×4）", "16枚・いちばんシンプル", true))
                SceneManager.LoadScene("MemoryMatch");
            y += 120;
            if (Choice(new Rect(left, y, 800, 100), "ペアめくりパーティ", "24枚・景品とポイント・派手な演出", true))
                SceneManager.LoadScene("CardMatch");
            for (var i = 0; i < ImageDecks.All.Count; i++)
            {
                y += 120;
                var deck = ImageDecks.All[i];
                var count = imageCounts[i];
                var sub = count > 0 ? $"{count} 種類 × 2枚 = {count * 2} 枚" : $"画像がありません（{deck.FolderName}/）";
                if (Choice(new Rect(left, y, 800, 100), deck.Title, sub, count > 0)) ChooseDeck(deck);
            }

            if (GUI.Button(new Rect(20, 20, 220, 50), "◀ タイトルへ", new GUIStyle(GUI.skin.button) { fontSize = 22, fontStyle = FontStyle.Bold }))
                showingSelect = false;
        }

        static bool Choice(Rect rect, string title, string sub, bool enabled)
        {
            GUI.enabled = enabled;
            var hover = enabled && rect.Contains(Event.current.mousePosition);
            Fill(rect, hover ? new Color(0.25f, 0.35f, 0.65f) : new Color(0.12f, 0.17f, 0.32f));
            GUI.DrawTexture(new Rect(rect.x + 16, rect.y + 12, 54, 76), PixelArt.CardBack);
            GUI.Label(new Rect(rect.x + 90, rect.y + 8, rect.width - 100, 52), title, Style(34, FontStyle.Bold, TextAnchor.MiddleLeft));
            GUI.Label(new Rect(rect.x + 90, rect.y + 56, rect.width - 100, 36), sub, Style(20, FontStyle.Normal, TextAnchor.MiddleLeft));
            var clicked = GUI.Button(rect, GUIContent.none, GUIStyle.none);
            GUI.enabled = true;
            return clicked && enabled;
        }

        static GUIStyle Style(int size, FontStyle font = FontStyle.Bold, TextAnchor anchor = TextAnchor.MiddleCenter) =>
            new GUIStyle(GUI.skin.label) { fontSize = size, fontStyle = font, alignment = anchor, wordWrap = false };

        static void Fill(Rect rect, Color color) =>
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, color, 0, 0);
    }
}
