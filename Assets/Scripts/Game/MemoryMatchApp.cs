using System.Collections.Generic;
using System.Linq;
using MemoryMatch.Core;
using UnityEngine;

namespace MemoryMatch.Game
{
    /// 設定画面・盤面・HUD・入力・CPU タイマー。ルールは MemoryGame / CpuPlayer に任せる。
    public class MemoryMatchApp : MonoBehaviour
    {
        const int Pairs = 8;
        const int Columns = 4;
        const int SeatCount = 4;

        [SerializeField] float mismatchSeconds = 1.0f;
        [SerializeField] float cpuDelaySeconds = 0.6f;
        [SerializeField] float flipSeconds = 0.15f;

        static readonly string[] Glyphs = { "●", "▲", "■", "◆", "★", "♥", "♣", "♠" };
        static readonly Color[] SymbolColors =
        {
            new Color(0.90f, 0.25f, 0.25f), new Color(0.20f, 0.55f, 0.95f), new Color(0.20f, 0.70f, 0.30f),
            new Color(0.95f, 0.75f, 0.10f), new Color(0.65f, 0.35f, 0.85f), new Color(0.95f, 0.45f, 0.70f),
            new Color(0.10f, 0.70f, 0.70f), new Color(0.95f, 0.55f, 0.15f),
        };
        static readonly Color[] SeatColors =
        {
            new Color(0.35f, 0.65f, 1f), new Color(1f, 0.45f, 0.45f), new Color(0.45f, 0.85f, 0.45f), new Color(1f, 0.8f, 0.3f),
        };
        static readonly string[] LevelNames = { "弱い", "普通", "強い" };

        readonly SeatKind[] seatKinds = { SeatKind.Human, SeatKind.Cpu, SeatKind.None, SeatKind.None };
        readonly CpuLevel[] seatLevels = { CpuLevel.Normal, CpuLevel.Normal, CpuLevel.Normal, CpuLevel.Normal };

        PlayerSlot[] players;
        int[] playerSeat;
        CpuPlayer[] cpus;
        float timer;
        int cursor;
        readonly Dictionary<int, float> flipStart = new Dictionary<int, float>();

        public MemoryGame Game { get; private set; }
        public int Cursor => cursor;

        void Update()
        {
            if (Game == null) return;

            if (Game.Phase == GamePhase.ShowingMismatch)
            {
                timer += Time.deltaTime;
                if (timer >= mismatchSeconds)
                {
                    var faceUp = FaceUpIndices();
                    Game.ResolveMismatch();
                    foreach (var i in faceUp) flipStart[i] = Time.time;
                    timer = 0f;
                }
                return;
            }

            var cpu = cpus[Game.CurrentPlayer];
            if (cpu == null || Game.Phase == GamePhase.GameOver) { timer = 0f; return; }

            timer += Time.deltaTime;
            if (timer < cpuDelaySeconds) return;
            timer = 0f;
            var pick = cpu.ChooseCard(Game);
            cursor = pick;
            Flip(pick);
        }

        public void StartMatch()
        {
            var seats = Enumerable.Range(0, SeatCount).Where(s => seatKinds[s] != SeatKind.None).ToArray();
            if (seats.Length < 2) return;

            playerSeat = seats;
            players = seats.Select(s => new PlayerSlot(SeatName(s), seatKinds[s], seatLevels[s])).ToArray();
            Game = MemoryGame.Create(players, Pairs, System.Environment.TickCount);
            cpus = players.Select((p, i) => p.Kind == SeatKind.Cpu ? new CpuPlayer(p.Level, System.Environment.TickCount + i) : null).ToArray();
            foreach (var cpu in cpus.Where(c => c != null)) Game.CardRevealed += cpu.Observe;
            timer = 0f;
            cursor = 0;
            flipStart.Clear();
        }

        public void BackToSetup() => Game = null;

        public void SetSeat(int seat, SeatKind kind, CpuLevel level = CpuLevel.Normal)
        {
            seatKinds[seat] = kind;
            seatLevels[seat] = level;
        }

        bool HumanTurn => Game != null && cpus[Game.CurrentPlayer] == null;

        void Flip(int index)
        {
            if (Game.Flip(index)) flipStart[index] = Time.time;
        }

        IEnumerable<int> FaceUpIndices() =>
            Enumerable.Range(0, Game.CardCount).Where(i => Game.StateOf(i) == CardState.FaceUp).ToArray();

        string SeatName(int seat) =>
            seatKinds[seat] == SeatKind.Cpu ? $"CPU{seat + 1}（{LevelNames[(int)seatLevels[seat]]}）" : $"P{seat + 1}";

        void OnGUI()
        {
            var scale = Screen.height / 720f;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            var width = Screen.width / scale;

            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(0, 0, width, 720), Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0,
                new Color(0.10f, 0.12f, 0.18f), 0, 0);

            HandleKeys();
            if (Game == null) DrawSetup(width);
            else DrawGame(width);
        }

        void HandleKeys()
        {
            var e = Event.current;
            if (e.type != EventType.KeyDown) return;

            if (Game == null)
            {
                if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.Space) { StartMatch(); e.Use(); }
                return;
            }

            switch (e.keyCode)
            {
                case KeyCode.R: StartMatch(); break;
                case KeyCode.Escape: BackToSetup(); break;
                case KeyCode.LeftArrow: MoveCursor(-1, 0); break;
                case KeyCode.RightArrow: MoveCursor(1, 0); break;
                case KeyCode.UpArrow: MoveCursor(0, -1); break;
                case KeyCode.DownArrow: MoveCursor(0, 1); break;
                case KeyCode.Space:
                case KeyCode.Return:
                    if (HumanTurn) Flip(cursor);
                    break;
                default: return;
            }
            e.Use();
        }

        void MoveCursor(int dx, int dy)
        {
            var rows = Game.CardCount / Columns;
            var x = (cursor % Columns + dx + Columns) % Columns;
            var y = (cursor / Columns + dy + rows) % rows;
            cursor = y * Columns + x;
        }

        GUIStyle Style(int size, TextAnchor anchor = TextAnchor.MiddleCenter, FontStyle font = FontStyle.Bold)
        {
            return new GUIStyle(GUI.skin.label) { fontSize = size, alignment = anchor, fontStyle = font, wordWrap = false };
        }

        GUIStyle ButtonStyle(int size) => new GUIStyle(GUI.skin.button) { fontSize = size, fontStyle = FontStyle.Bold };

        void DrawSetup(float width)
        {
            GUI.Label(new Rect(0, 50, width, 80), "神経衰弱", Style(56));
            GUI.Label(new Rect(0, 125, width, 40), "席ごとに 人 / CPU / なし を選んでください（2席以上）", Style(22, font: FontStyle.Normal));

            var left = width / 2 - 330;
            for (var s = 0; s < SeatCount; s++)
            {
                var y = 190 + s * 80;
                FillRect(new Rect(left, y, 660, 64), new Color(0.18f, 0.21f, 0.30f));
                FillRect(new Rect(left, y, 10, 64), SeatColors[s]);
                GUI.Label(new Rect(left + 20, y, 90, 64), $"席{s + 1}", Style(28, TextAnchor.MiddleLeft));

                var kinds = new[] { "人", "CPU", "なし" };
                var current = seatKinds[s] == SeatKind.Human ? 0 : seatKinds[s] == SeatKind.Cpu ? 1 : 2;
                var chosen = GUI.Toolbar(new Rect(left + 110, y + 10, 270, 44), current, kinds, ButtonStyle(22));
                seatKinds[s] = chosen == 0 ? SeatKind.Human : chosen == 1 ? SeatKind.Cpu : SeatKind.None;

                if (seatKinds[s] == SeatKind.Cpu)
                    seatLevels[s] = (CpuLevel)GUI.Toolbar(new Rect(left + 400, y + 10, 240, 44), (int)seatLevels[s], LevelNames, ButtonStyle(20));
            }

            var count = seatKinds.Count(k => k != SeatKind.None);
            GUI.enabled = count >= 2;
            if (GUI.Button(new Rect(width / 2 - 150, 540, 300, 70), count >= 2 ? "スタート（Enter）" : "2席以上を選択", ButtonStyle(28)))
                StartMatch();
            GUI.enabled = true;
            if (GUI.Button(new Rect(20, 20, 260, 50), "◀ ゲーム選択へ", ButtonStyle(22)))
                TitleApp.BackToMenu();
            GUI.Label(new Rect(0, 640, width, 40), "操作: クリック / 矢印キー＋Space ・ R: 最初から ・ Esc: 席設定へ", Style(20, font: FontStyle.Normal));
        }

        void DrawGame(float width)
        {
            DrawHud(width);

            var rows = Game.CardCount / Columns;
            const float size = 128f, gap = 14f;
            var boardWidth = Columns * size + (Columns - 1) * gap;
            var left = (width - boardWidth) / 2;
            const float top = 130f;

            for (var i = 0; i < Game.CardCount; i++)
            {
                var rect = new Rect(left + i % Columns * (size + gap), top + i / Columns * (size + gap), size, size);
                DrawCard(i, rect);
            }

            if (Game.Phase == GamePhase.GameOver) DrawResult(width);
        }

        void DrawHud(float width)
        {
            var slot = Mathf.Min(260f, (width - 40) / players.Length);
            var left = (width - slot * players.Length) / 2;
            for (var p = 0; p < players.Length; p++)
            {
                var rect = new Rect(left + p * slot + 5, 15, slot - 10, 90);
                var active = Game.Phase != GamePhase.GameOver && Game.CurrentPlayer == p;
                var color = SeatColors[playerSeat[p]];
                FillRect(rect, active ? color * 0.75f : new Color(0.18f, 0.21f, 0.30f));
                if (active) Outline(rect, Color.white, 4);
                GUI.Label(new Rect(rect.x, rect.y + 6, rect.width, 36), (active ? "▶ " : "") + players[p].Name, Style(24));
                GUI.Label(new Rect(rect.x, rect.y + 42, rect.width, 44), $"{Game.PairsOf(p)} ペア", Style(30));
            }
        }

        void DrawCard(int index, Rect rect)
        {
            var state = Game.StateOf(index);
            var showFace = state != CardState.FaceDown;

            // めくりアニメ: 横幅を 1→0→1 に誇張して見せる
            if (flipStart.TryGetValue(index, out var start))
            {
                var t = (Time.time - start) / flipSeconds;
                if (t < 1f)
                {
                    var w = Mathf.Abs(1f - 2f * t);
                    if (t < 0.5f) showFace = !showFace;
                    rect = new Rect(rect.center.x - rect.width * w / 2, rect.y, rect.width * w, rect.height);
                }
            }

            if (Game.Phase == GamePhase.ShowingMismatch && state == CardState.FaceUp)
                rect.x += Mathf.Sin(Time.time * 60f) * 4f * Mathf.Clamp01(1f - timer / mismatchSeconds * 2f);

            if (showFace)
            {
                var pair = Game.VisiblePairId(index);
                var owner = Game.OwnerOf(index);
                FillRect(rect, state == CardState.Matched ? new Color(0.80f, 0.82f, 0.86f) : Color.white);
                if (pair >= 0)
                {
                    var style = Style(72);
                    style.normal.textColor = SymbolColors[pair];
                    GUI.Label(rect, Glyphs[pair], style);
                }
                if (owner >= 0) FillRect(new Rect(rect.x, rect.yMax - 12, rect.width, 12), SeatColors[playerSeat[owner]]);
            }
            else
            {
                FillRect(rect, new Color(0.25f, 0.35f, 0.60f));
                Outline(new Rect(rect.x + 10, rect.y + 10, rect.width - 20, rect.height - 20), new Color(0.40f, 0.52f, 0.80f), 3);
                GUI.Label(rect, "？", Style(40));
            }

            if (HumanTurn && index == cursor && Game.Phase != GamePhase.GameOver)
                Outline(new Rect(rect.x - 6, rect.y - 6, rect.width + 12, rect.height + 12), new Color(1f, 0.9f, 0.2f), 5);

            if (HumanTurn && Game.IsSelectable(index) && GUI.Button(rect, GUIContent.none, GUIStyle.none))
            {
                cursor = index;
                Flip(index);
            }
        }

        void DrawResult(float width)
        {
            var winners = Game.Winners();
            var text = winners.Length > 1
                ? "引き分け！ " + string.Join("・", winners.Select(w => players[w].Name))
                : $"{players[winners[0]].Name} の勝ち！";
            var rect = new Rect(width / 2 - 360, 300, 720, 170);
            FillRect(rect, new Color(0, 0, 0, 0.85f));
            Outline(rect, SeatColors[playerSeat[winners[0]]], 5);
            GUI.Label(new Rect(rect.x, rect.y + 20, rect.width, 70), text, Style(46));
            GUI.Label(new Rect(rect.x, rect.y + 100, rect.width, 40), "R: もう一度 ・ Esc: 席設定へ", Style(24, font: FontStyle.Normal));
        }

        static void FillRect(Rect rect, Color color)
        {
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, color, 0, 0);
        }

        static void Outline(Rect rect, Color color, float thickness)
        {
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, color, thickness, 0);
        }
    }
}
