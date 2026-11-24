using System.Collections.Generic;
using System.Linq;
using MemoryMatch.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MemoryMatch.Game
{
    /// 「ペアめくりパーティ」の画面。ルールは CardMatchGame / CardMatchBoard / CpuPlayer、演出は Celebration / SoundFx に任せる。
    [RequireComponent(typeof(AudioSource))]
    public class CardMatchApp : MonoBehaviour
    {
        const int SeatCount = 4;
        const int Columns = CardMatchBoard.Columns;
        const float CardW = 84f, CardH = 110f, Gap = 12f, GridTop = 122f;

        [SerializeField] float mismatchSeconds = 1.0f;
        [SerializeField] float cpuDelaySeconds = 0.6f;
        [SerializeField] float flipSeconds = 0.15f;
        [SerializeField] bool startOnPlay;

        static readonly Color[] SeatColors =
        {
            new Color(0.35f, 0.65f, 1f), new Color(1f, 0.45f, 0.45f), new Color(0.45f, 0.85f, 0.45f), new Color(1f, 0.8f, 0.3f),
        };
        static readonly string[] LevelNames = { "弱い", "普通", "強い" };
        static readonly string[] PrizeNames = { "10コイン", "20コイン", "キノコ", "フラワー", "スター", "1UP", "ジェム", "クローバー" };
        static readonly Color[] ComboColors =
        {
            Color.white, new Color(1f, 0.9f, 0.3f), new Color(1f, 0.6f, 0.2f), new Color(1f, 0.35f, 0.6f), new Color(0.6f, 0.5f, 1f),
        };

        readonly SeatKind[] seatKinds = { SeatKind.Human, SeatKind.Cpu, SeatKind.None, SeatKind.None };
        readonly CpuLevel[] seatLevels = { CpuLevel.Normal, CpuLevel.Normal, CpuLevel.Normal, CpuLevel.Normal };
        readonly Dictionary<int, float> flipStart = new Dictionary<int, float>();
        readonly Celebration fx = new Celebration();

        CardMatchBoard board;
        SoundFx sound;
        PlayerSlot[] players;
        int[] playerSeat;
        CpuPlayer[] cpus;
        float timer;
        float mismatchStart;
        float fireworkTimer;
        float width = 1280f;
        int cursor;
        int lastFlipped;

        public CardMatchGame Game { get; private set; }
        public CardMatchBoard Board => board;
        public Celebration Fx => fx;

        void Awake()
        {
            board = CardMatchBoard.Deal(Random.Range(0, CardMatchBoard.LayoutCount));
            sound = new SoundFx(GetComponent<AudioSource>());
        }

        void Start()
        {
            if (startOnPlay) StartMatch();
        }

        public void SetSeat(int seat, SeatKind kind, CpuLevel level = CpuLevel.Normal)
        {
            seatKinds[seat] = kind;
            seatLevels[seat] = level;
        }

        public void StartMatch()
        {
            var seats = Enumerable.Range(0, SeatCount).Where(s => seatKinds[s] != SeatKind.None).ToArray();
            if (seats.Length < 2) return;

            board = board.Next(System.Environment.TickCount);
            playerSeat = seats;
            players = seats.Select(s => new PlayerSlot(SeatName(s), seatKinds[s], seatLevels[s])).ToArray();
            Game = new CardMatchGame(players, board);
            cpus = players.Select((p, i) => p.Kind == SeatKind.Cpu ? new CpuPlayer(p.Level, System.Environment.TickCount + i) : null).ToArray();
            foreach (var cpu in cpus.Where(c => c != null)) Game.CardRevealed += cpu.Observe;
            Game.PairMatched += OnPairMatched;
            timer = 0f;
            cursor = 0;
            flipStart.Clear();
            fx.Clear();
        }

        void OnPairMatched(int player, Prize prize, int combo)
        {
            var at = CardRect(lastFlipped).center;
            var points = Prizes.PointsOf(prize);
            fx.Burst(at, 25 + combo * 15);
            fx.Text($"+{points}", at, 38, new Color(1f, 0.95f, 0.4f));
            fx.Shake(0.25f + combo * 0.12f);
            if (combo >= 2)
                fx.Text($"{combo}れんさ" + new string('！', Mathf.Min(combo, 5)), new Vector2(width / 2, 330),
                    44 + Mathf.Min(combo, 6) * 10, ComboColors[Mathf.Min(combo, ComboColors.Length) - 1]);
            if (points >= 60)
            {
                fx.Flash(new Color(1f, 0.9f, 0.4f));
                fx.Text($"{PrizeNames[(int)prize]} ゲット！", new Vector2(width / 2, 420), 56, new Color(0.5f, 1f, 1f));
                sound.Big();
            }
            else sound.Match(combo);

            if (Game.Phase == GamePhase.GameOver)
            {
                sound.Win();
                fireworkTimer = 0f;
            }
        }

        void Update()
        {
            fx.Update(Time.deltaTime);
            if (Game == null) return;

            if (Game.Phase == GamePhase.GameOver)
            {
                fireworkTimer -= Time.deltaTime;
                if (fireworkTimer > 0f) return;
                fireworkTimer = 0.45f;
                fx.Firework(new Vector2(Random.Range(150f, width - 150f), Random.Range(140f, 360f)));
                return;
            }

            if (Game.Phase == GamePhase.ShowingMismatch)
            {
                timer += Time.deltaTime;
                if (timer < mismatchSeconds) return;
                var faceUp = Enumerable.Range(0, Game.CardCount).Where(i => Game.StateOf(i) == CardState.FaceUp).ToArray();
                Game.ResolveMismatch();
                foreach (var i in faceUp) flipStart[i] = Time.time;
                timer = 0f;
                return;
            }

            var cpu = cpus[Game.CurrentPlayer];
            if (cpu == null) { timer = 0f; return; }
            timer += Time.deltaTime;
            if (timer < cpuDelaySeconds) return;
            timer = 0f;
            cursor = cpu.ChooseCard(Game);
            Flip(cursor);
        }

        bool HumanTurn => Game != null && Game.Phase != GamePhase.GameOver && cpus[Game.CurrentPlayer] == null;

        void Flip(int index)
        {
            lastFlipped = index;
            if (!Game.Flip(index)) return;
            flipStart[index] = Time.time;
            sound.Flip();
            if (Game.Phase != GamePhase.ShowingMismatch) return;
            mismatchStart = Time.time;
            sound.Miss();
        }

        string SeatName(int seat) =>
            seatKinds[seat] == SeatKind.Cpu ? $"CPU{seat + 1}（{LevelNames[(int)seatLevels[seat]]}）" : $"P{seat + 1}";

        Rect CardRect(int index)
        {
            var left = width / 2 - (Columns * CardW + (Columns - 1) * Gap) / 2;
            return new Rect(left + index % Columns * (CardW + Gap), GridTop + index / Columns * (CardH + Gap), CardW, CardH);
        }

        void OnGUI()
        {
            var scale = Screen.height / 720f;
            width = Screen.width / scale;
            var shake = fx.ShakeOffset;
            GUI.matrix = Matrix4x4.TRS(new Vector3(shake.x * scale, shake.y * scale, 0f), Quaternion.identity, new Vector3(scale, scale, 1f));
            FillRect(new Rect(-40, -40, width + 80, 800), Game == null ? new Color(0.12f, 0.08f, 0.05f) : Color.black);

            HandleKeys();
            if (Game == null) DrawSetup();
            else DrawGame();
            fx.Draw(width);
            if (Game != null && Game.Phase == GamePhase.GameOver) DrawResult();
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
                case KeyCode.Escape: Game = null; fx.Clear(); break;
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
            cursor = (cursor / Columns + dy + rows) % rows * Columns + (cursor % Columns + dx + Columns) % Columns;
        }

        void DrawSetup()
        {
            GUI.Label(new Rect(0, 40, width, 80), "ペアめくりパーティ", Style(54));
            GUI.Label(new Rect(0, 110, width, 40), "24枚・同じカードを2枚そろえてゲット！ カードがなくなるまで続くよ", Style(22, font: FontStyle.Normal));

            var left = width / 2 - 330;
            for (var s = 0; s < SeatCount; s++)
            {
                var y = 180 + s * 80;
                FillRect(new Rect(left, y, 660, 64), new Color(0.25f, 0.17f, 0.10f));
                FillRect(new Rect(left, y, 10, 64), SeatColors[s]);
                GUI.Label(new Rect(left + 20, y, 90, 64), $"席{s + 1}", Style(28, TextAnchor.MiddleLeft));
                var current = seatKinds[s] == SeatKind.Human ? 0 : seatKinds[s] == SeatKind.Cpu ? 1 : 2;
                var chosen = GUI.Toolbar(new Rect(left + 110, y + 10, 270, 44), current, new[] { "人", "CPU", "なし" }, ButtonStyle(22));
                seatKinds[s] = chosen == 0 ? SeatKind.Human : chosen == 1 ? SeatKind.Cpu : SeatKind.None;
                if (seatKinds[s] == SeatKind.Cpu)
                    seatLevels[s] = (CpuLevel)GUI.Toolbar(new Rect(left + 400, y + 10, 240, 44), (int)seatLevels[s], LevelNames, ButtonStyle(20));
            }

            var count = seatKinds.Count(k => k != SeatKind.None);
            GUI.enabled = count >= 2;
            if (GUI.Button(new Rect(width / 2 - 150, 520, 300, 70), count >= 2 ? "スタート（Enter）" : "2席以上を選択", ButtonStyle(28)))
                StartMatch();
            GUI.enabled = true;
            if (GUI.Button(new Rect(20, 20, 260, 50), "◀ 4×4モードへ", ButtonStyle(22)))
                SceneManager.LoadScene("MemoryMatch");
            GUI.Label(new Rect(0, 630, width, 40), "操作: クリック / 矢印キー＋Space ・ R: もう一回 ・ Esc: 席設定へ", Style(20, font: FontStyle.Normal));
        }

        void DrawGame()
        {
            DrawHud();
            for (var i = 0; i < Game.CardCount; i++) DrawCard(i, CardRect(i));
            DrawMessage();
        }

        void DrawHud()
        {
            var slot = width / players.Length;
            for (var p = 0; p < players.Length; p++)
            {
                var rect = new Rect(p * slot + 6, 6, slot - 12, 92);
                var color = SeatColors[playerSeat[p]];
                var active = Game.Phase != GamePhase.GameOver && Game.CurrentPlayer == p;
                FillRect(rect, Color.black);
                Outline(rect, active ? color : new Color(0.35f, 0.35f, 0.35f), active ? 4 : 2);
                GUI.Label(new Rect(rect.x + 10, rect.y + 4, rect.width - 20, 30), (active ? "▶" : "") + players[p].Name, Style(20, TextAnchor.MiddleLeft));
                var combo = active && Game.Combo >= 2 ? $" {Game.Combo}れんさ中" : "";
                GUI.Label(new Rect(rect.x + 10, rect.y + 4, rect.width - 20, 30), $"{Game.PointsOf(p)}pt{combo}", Style(20, TextAnchor.MiddleRight));
                var prizes = Game.PrizesOf(p);
                var step = prizes.Count == 0 ? 30f : Mathf.Min(30f, (rect.width - 20) / prizes.Count);
                for (var k = 0; k < prizes.Count; k++)
                    GUI.DrawTexture(new Rect(rect.x + 10 + k * step, rect.y + 44, step - 2, step - 2), PixelArt.Of(prizes[k]));
            }
        }

        void DrawMessage()
        {
            var rect = new Rect(width / 2 - 380, 604, 760, 108);
            FillRect(rect, Color.black);
            Outline(rect, Color.white, 4);
            Outline(new Rect(rect.x + 8, rect.y + 8, rect.width - 16, rect.height - 16), Color.white, 2);
            var name = players[Game.CurrentPlayer].Name;
            var next = players[(Game.CurrentPlayer + 1) % players.Length].Name;
            var text = Game.Phase == GamePhase.ShowingMismatch ? $"ざんねん！ つぎは {next} のばん"
                : Game.Phase == GamePhase.GameOver ? "ぜんぶ なくなった！"
                : Game.Combo >= 1 ? $"{name} そろった！ もう1回 めくれるよ！"
                : $"{name} のばん。カードを 2まい めくって\nおなじものを そろえよう！";
            var style = Style(26, TextAnchor.MiddleLeft);
            style.wordWrap = true;
            GUI.Label(new Rect(rect.x + 30, rect.y + 10, rect.width - 60, rect.height - 20), text, style);
        }

        void DrawCard(int index, Rect rect)
        {
            var state = Game.StateOf(index);
            if (flipStart.TryGetValue(index, out var start))
            {
                var t = (Time.time - start) / flipSeconds;
                if (t < 1f) rect = new Rect(rect.center.x - rect.width * Mathf.Abs(1f - 2f * t) / 2, rect.y, rect.width * Mathf.Abs(1f - 2f * t), rect.height);
            }

            var mismatch = Game.Phase == GamePhase.ShowingMismatch && state == CardState.FaceUp;
            if (mismatch)
            {
                var shakeT = Time.time - mismatchStart;
                if (shakeT < 0.45f) rect.x += Mathf.Sin(shakeT * 70f) * 7f * (1f - shakeT / 0.45f);
            }
            FillRect(rect, mismatch ? new Color(0.9f, 0.2f, 0.2f) : Color.white);
            var inner = new Rect(rect.x + 5, rect.y + 5, rect.width - 10, rect.height - 10);

            if (state == CardState.FaceDown)
            {
                GUI.DrawTexture(inner, PixelArt.CardBack, ScaleMode.StretchToFill);
            }
            else
            {
                var prize = (Prize)Game.VisiblePairId(index);
                FillRect(inner, state == CardState.Matched ? new Color(0.70f, 0.70f, 0.70f) : new Color(0.98f, 0.96f, 0.88f));
                var icon = Mathf.Min(inner.width - 10, inner.height - 30);
                GUI.DrawTexture(new Rect(inner.center.x - icon / 2, inner.y + 5, icon, icon), PixelArt.Of(prize));
                var label = new GUIStyle(Style(13)) { normal = { textColor = Color.black } };
                GUI.Label(new Rect(inner.x, inner.yMax - 24, inner.width, 22), PrizeNames[(int)prize], label);
                var owner = Game.OwnerOf(index);
                if (owner >= 0) FillRect(new Rect(inner.x, inner.yMax - 4, inner.width, 4), SeatColors[playerSeat[owner]]);
            }

            if (HumanTurn && index == cursor)
            {
                var bob = Mathf.Sin(Time.time * 8f) * 4f;
                var arrow = new GUIStyle(Style(26)) { normal = { textColor = new Color(1f, 0.85f, 0.1f) } };
                GUI.Label(new Rect(rect.center.x - 20, rect.y - 26 + bob, 40, 26), "▼", arrow);
                Outline(new Rect(rect.x - 4, rect.y - 4, rect.width + 8, rect.height + 8), new Color(1f, 0.85f, 0.1f), 3);
            }

            if (HumanTurn && Game.IsSelectable(index) && GUI.Button(rect, GUIContent.none, GUIStyle.none))
            {
                cursor = index;
                Flip(index);
            }
        }

        void DrawResult()
        {
            var winners = Game.Winners();
            var pulse = 1f + Mathf.Sin(Time.time * 6f) * 0.06f;
            var text = winners.Length > 1
                ? "ひきわけ！ " + string.Join("・", winners.Select(w => players[w].Name))
                : $"{players[winners[0]].Name} のかち！";
            var rect = new Rect(width / 2 - 400, 250, 800, 220);
            FillRect(rect, new Color(0, 0, 0, 0.85f));
            Outline(rect, SeatColors[playerSeat[winners[0]]], 6);
            var title = Style(Mathf.RoundToInt(60 * pulse));
            title.normal.textColor = Color.Lerp(Color.white, new Color(1f, 0.85f, 0.2f), Mathf.PingPong(Time.time * 2f, 1f));
            GUI.Label(new Rect(rect.x, rect.y + 20, rect.width, 90), text, title);
            var scores = string.Join("   ", Enumerable.Range(0, players.Length).Select(p => $"{players[p].Name} {Game.PointsOf(p)}pt"));
            GUI.Label(new Rect(rect.x, rect.y + 115, rect.width, 40), scores, Style(22, font: FontStyle.Normal));
            GUI.Label(new Rect(rect.x, rect.y + 165, rect.width, 36), "R: もう一回 ・ Esc: 席設定へ", Style(22, font: FontStyle.Normal));
        }

        static GUIStyle Style(int size, TextAnchor anchor = TextAnchor.MiddleCenter, FontStyle font = FontStyle.Bold) =>
            new GUIStyle(GUI.skin.label) { fontSize = size, alignment = anchor, fontStyle = font, wordWrap = false };

        static GUIStyle ButtonStyle(int size) => new GUIStyle(GUI.skin.button) { fontSize = size, fontStyle = FontStyle.Bold };

        static void FillRect(Rect rect, Color color) =>
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, color, 0, 0);

        static void Outline(Rect rect, Color color, float thickness) =>
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, color, thickness, 0);
    }
}
