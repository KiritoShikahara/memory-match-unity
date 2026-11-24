using System.Collections.Generic;
using System.IO;
using System.Linq;
using MemoryMatch.Core;
using UnityEngine;

namespace MemoryMatch.Game
{
    /// ユーザーが用意した画像フォルダの全柄で遊ぶ神経衰弱。ルールは MemoryGame / CpuPlayer、演出は Celebration / SoundFx。
    [RequireComponent(typeof(AudioSource))]
    public class ImageCardApp : MonoBehaviour
    {
        const int SeatCount = 4;
        const float Gap = 6f, GridTop = 92f, GridBottom = 640f, Margin = 20f, PanelW = 340f;

        [SerializeField] string imageFolder = "";
        [SerializeField] float mismatchSeconds = 1.2f;
        [SerializeField] float cpuDelaySeconds = 0.5f;
        [SerializeField] float flipSeconds = 0.15f;
        [SerializeField] bool confirmAfterPair = true;
        [SerializeField] bool startOnPlay;

        static readonly Color[] SeatColors =
        {
            new Color(0.35f, 0.65f, 1f), new Color(1f, 0.45f, 0.45f), new Color(0.45f, 0.85f, 0.45f), new Color(1f, 0.8f, 0.3f),
        };
        static readonly string[] LevelNames = { "弱い", "普通", "強い" };

        readonly SeatKind[] seatKinds = { SeatKind.Human, SeatKind.Cpu, SeatKind.None, SeatKind.None };
        readonly CpuLevel[] seatLevels = { CpuLevel.Normal, CpuLevel.Normal, CpuLevel.Normal, CpuLevel.Normal };
        readonly Dictionary<int, float> flipStart = new Dictionary<int, float>();
        readonly Celebration fx = new Celebration();
        readonly List<Texture2D> faces = new List<Texture2D>();

        SoundFx sound;
        PlayerSlot[] players;
        int[] playerSeat;
        CpuPlayer[] cpus;
        int[] pairsBefore;
        float timer, mismatchStart, fireworkTimer, width = 1280f, aspect = 0.716f;
        int cursor, combo, previewIndex = -1;
        bool zoomed;
        GridFit fit;

        bool awaitingConfirm;

        public int PreviewIndex => previewIndex;
        public bool Zoomed => zoomed;
        public bool AwaitingConfirm => awaitingConfirm;

        public bool ConfirmAfterPair
        {
            get => confirmAfterPair;
            set => confirmAfterPair = value;
        }

        /// 2枚目の確認を終えて次へ進む。外れなら裏に戻して交代する。
        public void Confirm()
        {
            if (!awaitingConfirm) return;
            awaitingConfirm = false;
            timer = 0f;
            if (Game.Phase == GamePhase.ShowingMismatch) ResolveMismatch();
        }

        void ResolveMismatch()
        {
            var faceUp = Enumerable.Range(0, Game.CardCount).Where(i => Game.StateOf(i) == CardState.FaceUp).ToArray();
            Game.ResolveMismatch();
            foreach (var i in faceUp) flipStart[i] = Time.time;
        }

        /// 拡大表示の切替。表向きのカードを選んでいないときは開けない。
        public bool SetZoom(bool on)
        {
            if (on && (Game == null || previewIndex < 0 || Game.StateOf(previewIndex) == CardState.FaceDown)) return false;
            zoomed = on;
            return true;
        }

        float GridWidth => width - PanelW - Margin * 3;

        public MemoryGame Game { get; private set; }
        public int ImageCount => faces.Count;
        public Celebration Fx => fx;

        public ImageDeck Deck => TitleApp.SelectedDeck;

        public string Folder => string.IsNullOrEmpty(imageFolder) ? ImageDeckPaths.Resolve(Deck.FolderName) : imageFolder;

        void Awake()
        {
            sound = new SoundFx(GetComponent<AudioSource>());
        }

        void Start()
        {
            if (faces.Count == 0) LoadImages(Folder);
            if (startOnPlay) StartMatch();
        }

        public void LoadImages(string folder)
        {
            imageFolder = folder;
            foreach (var t in faces) Destroy(t);
            faces.Clear();
            if (!Directory.Exists(folder)) return;
            foreach (var path in ImageCatalog.Select(Directory.GetFiles(folder)))
            {
                var texture = new Texture2D(2, 2) { name = Path.GetFileName(path), filterMode = FilterMode.Trilinear, anisoLevel = 4 };
                if (texture.LoadImage(File.ReadAllBytes(path))) faces.Add(texture);
                else Destroy(texture);
            }
            if (faces.Count > 0) aspect = (float)faces[0].width / faces[0].height;
        }

        public void SetSeat(int seat, SeatKind kind, CpuLevel level = CpuLevel.Normal)
        {
            seatKinds[seat] = kind;
            seatLevels[seat] = level;
        }

        public void StartMatch()
        {
            var seats = Enumerable.Range(0, SeatCount).Where(s => seatKinds[s] != SeatKind.None).ToArray();
            if (seats.Length < 2 || faces.Count == 0) return;

            playerSeat = seats;
            players = seats.Select(s => new PlayerSlot(SeatName(s), seatKinds[s], seatLevels[s])).ToArray();
            Game = MemoryGame.Create(players, faces.Count, System.Environment.TickCount);
            cpus = players.Select((p, i) => p.Kind == SeatKind.Cpu ? new CpuPlayer(p.Level, System.Environment.TickCount + i) : null).ToArray();
            foreach (var cpu in cpus.Where(c => c != null)) Game.CardRevealed += cpu.Observe;
            pairsBefore = new int[players.Length];
            fit = MemoryMatch.Core.GridLayout.Fit(Game.CardCount, GridWidth, GridBottom - GridTop, aspect, Gap);
            previewIndex = -1;
            zoomed = false;
            awaitingConfirm = false;
            timer = 0f;
            cursor = 0;
            combo = 0;
            flipStart.Clear();
            fx.Clear();
        }

        void Update()
        {
            if (zoomed) return;
            fx.Update(Time.deltaTime);
            if (Game == null || awaitingConfirm) return;

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
                ResolveMismatch();
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
            var player = Game.CurrentPlayer;
            var second = Game.Phase == GamePhase.WaitingSecond;
            if (!Game.Flip(index)) return;
            flipStart[index] = Time.time;
            previewIndex = index;
            if (second && confirmAfterPair && Game.Phase != GamePhase.GameOver) awaitingConfirm = true;
            sound.Flip();

            if (Game.Phase == GamePhase.ShowingMismatch)
            {
                mismatchStart = Time.time;
                combo = 0;
                sound.Miss();
                return;
            }
            if (Game.PairsOf(player) == pairsBefore[player]) return;

            pairsBefore[player] = Game.PairsOf(player);
            combo++;
            var at = CardRect(index).center;
            fx.Burst(at, 25 + combo * 15);
            fx.Text("ゲット！", at, 34, new Color(1f, 0.95f, 0.4f));
            fx.Shake(0.25f + combo * 0.12f);
            if (combo >= 2)
                fx.Text($"{combo}れんさ" + new string('！', Mathf.Min(combo, 5)), new Vector2(width / 2, 330), 44 + Mathf.Min(combo, 6) * 10, new Color(1f, 0.6f, 0.2f));
            sound.Match(combo);
            if (Game.Phase != GamePhase.GameOver) return;
            sound.Win();
            fireworkTimer = 0f;
        }

        string SeatName(int seat) =>
            seatKinds[seat] == SeatKind.Cpu ? $"CPU{seat + 1}（{LevelNames[(int)seatLevels[seat]]}）" : $"P{seat + 1}";

        Rect CardRect(int index)
        {
            var cols = fit.Columns;
            var rows = (Game.CardCount + cols - 1) / cols;
            var left = Margin + (GridWidth - (cols * fit.CardWidth + (cols - 1) * Gap)) / 2;
            var top = GridTop + (GridBottom - GridTop - (rows * fit.CardHeight + (rows - 1) * Gap)) / 2;
            return new Rect(left + index % cols * (fit.CardWidth + Gap), top + index / cols * (fit.CardHeight + Gap), fit.CardWidth, fit.CardHeight);
        }

        void OnGUI()
        {
            var scale = Screen.height / 720f;
            var newWidth = Screen.width / scale;
            var resized = !Mathf.Approximately(newWidth, width);
            width = newWidth;
            if (Game != null && resized)
                fit = MemoryMatch.Core.GridLayout.Fit(Game.CardCount, GridWidth, GridBottom - GridTop, aspect, Gap);
            var shake = fx.ShakeOffset;
            GUI.matrix = Matrix4x4.TRS(new Vector3(shake.x * scale, shake.y * scale, 0f), Quaternion.identity, new Vector3(scale, scale, 1f));
            FillRect(new Rect(-40, -40, width + 80, 800), Game == null ? new Color(0.05f, 0.10f, 0.20f) : new Color(0.02f, 0.04f, 0.10f));

            HandleKeys();
            if (Game == null) DrawSetup();
            else DrawGame();
            fx.Draw(width);
            if (Game != null && Game.Phase == GamePhase.GameOver) DrawResult();
            if (zoomed) DrawZoom();
        }

        void DrawPreview()
        {
            var panel = new Rect(width - PanelW - Margin, GridTop, PanelW, GridBottom - GridTop);
            FillRect(panel, new Color(0.06f, 0.10f, 0.20f));
            Outline(panel, new Color(0.4f, 0.5f, 0.7f), 2);
            var shown = previewIndex >= 0 && Game.StateOf(previewIndex) != CardState.FaceDown;
            if (!shown)
            {
                GUI.Label(panel, "めくったカードが\nここに大きく出るよ", Style(20, font: FontStyle.Normal));
                return;
            }
            var image = new Rect(panel.x + 10, panel.y + 10, panel.width - 20, panel.height - 50);
            GUI.DrawTexture(image, faces[Game.VisiblePairId(previewIndex)], ScaleMode.ScaleToFit);
            GUI.Label(new Rect(panel.x, panel.yMax - 40, panel.width, 36), "クリック / Z で全画面", Style(18, font: FontStyle.Normal));
            if (GUI.Button(panel, GUIContent.none, GUIStyle.none)) SetZoom(true);
        }

        void DrawZoom()
        {
            var screen = new Rect(0, 0, width, 720);
            FillRect(screen, new Color(0, 0, 0, 0.92f));
            GUI.DrawTexture(new Rect(10, 10, width - 20, 700), faces[Game.VisiblePairId(previewIndex)], ScaleMode.ScaleToFit);
            if (GUI.Button(screen, GUIContent.none, GUIStyle.none)) zoomed = false;
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

            if (zoomed)
            {
                if (e.keyCode == KeyCode.Z || e.keyCode == KeyCode.Escape || e.keyCode == KeyCode.Space || e.keyCode == KeyCode.Return) zoomed = false;
                e.Use();
                return;
            }

            var cols = fit.Columns;
            switch (e.keyCode)
            {
                case KeyCode.Z: SetZoom(true); break;
                case KeyCode.R: StartMatch(); break;
                case KeyCode.Escape: Game = null; fx.Clear(); break;
                case KeyCode.LeftArrow: cursor = (cursor - 1 + Game.CardCount) % Game.CardCount; break;
                case KeyCode.RightArrow: cursor = (cursor + 1) % Game.CardCount; break;
                case KeyCode.UpArrow: cursor = (cursor - cols + Game.CardCount) % Game.CardCount; break;
                case KeyCode.DownArrow: cursor = (cursor + cols) % Game.CardCount; break;
                case KeyCode.Space:
                case KeyCode.Return:
                    if (awaitingConfirm) Confirm();
                    else if (HumanTurn) Flip(cursor);
                    break;
                default: return;
            }
            e.Use();
        }

        void DrawSetup()
        {
            GUI.Label(new Rect(0, 30, width, 80), Deck.Title, Style(54));
            var info = faces.Count > 0
                ? $"{faces.Count} 種類 × 2枚 = {faces.Count * 2} 枚で勝負！"
                : $"画像がありません: {Folder}";
            GUI.Label(new Rect(0, 105, width, 40), info, Style(22, font: FontStyle.Normal));

            var left = width / 2 - 330;
            for (var s = 0; s < SeatCount; s++)
            {
                var y = 170 + s * 80;
                FillRect(new Rect(left, y, 660, 64), new Color(0.10f, 0.18f, 0.32f));
                FillRect(new Rect(left, y, 10, 64), SeatColors[s]);
                GUI.Label(new Rect(left + 20, y, 90, 64), $"席{s + 1}", Style(28, TextAnchor.MiddleLeft));
                var current = seatKinds[s] == SeatKind.Human ? 0 : seatKinds[s] == SeatKind.Cpu ? 1 : 2;
                var chosen = GUI.Toolbar(new Rect(left + 110, y + 10, 270, 44), current, new[] { "人", "CPU", "なし" }, ButtonStyle(22));
                seatKinds[s] = chosen == 0 ? SeatKind.Human : chosen == 1 ? SeatKind.Cpu : SeatKind.None;
                if (seatKinds[s] == SeatKind.Cpu)
                    seatLevels[s] = (CpuLevel)GUI.Toolbar(new Rect(left + 400, y + 10, 240, 44), (int)seatLevels[s], LevelNames, ButtonStyle(20));
            }

            var ready = seatKinds.Count(k => k != SeatKind.None) >= 2 && faces.Count > 0;
            GUI.enabled = ready;
            if (GUI.Button(new Rect(width / 2 - 150, 510, 300, 70), ready ? "スタート（Enter）" : "準備ができていません", ButtonStyle(26)))
                StartMatch();
            GUI.enabled = true;
            if (GUI.Button(new Rect(20, 20, 260, 50), "◀ ゲーム選択へ", ButtonStyle(22)))
                TitleApp.BackToMenu();
            GUI.Label(new Rect(0, 630, width, 40), "操作: クリック / 矢印キー＋Space ・ R: もう一回 ・ Esc: 席設定へ", Style(20, font: FontStyle.Normal));
        }

        void DrawGame()
        {
            var slot = width / players.Length;
            for (var p = 0; p < players.Length; p++)
            {
                var rect = new Rect(p * slot + 6, 6, slot - 12, 76);
                var active = Game.Phase != GamePhase.GameOver && Game.CurrentPlayer == p;
                FillRect(rect, Color.black);
                Outline(rect, active ? SeatColors[playerSeat[p]] : new Color(0.35f, 0.35f, 0.35f), active ? 4 : 2);
                GUI.Label(new Rect(rect.x + 12, rect.y, rect.width - 24, rect.height), (active ? "▶" : "") + players[p].Name, Style(22, TextAnchor.MiddleLeft));
                var streak = active && combo >= 2 ? $"{combo}れんさ中  " : "";
                GUI.Label(new Rect(rect.x + 12, rect.y, rect.width - 24, rect.height), $"{streak}{Game.PairsOf(p)} ペア", Style(26, TextAnchor.MiddleRight));
            }

            for (var i = 0; i < Game.CardCount; i++) DrawCard(i, CardRect(i));
            DrawPreview();

            var rest = Enumerable.Range(0, Game.CardCount).Count(i => Game.StateOf(i) != CardState.Matched) / 2;
            if (awaitingConfirm)
            {
                var result = Game.Phase == GamePhase.ShowingMismatch ? "ざんねん！" : "そろった！";
                GUI.Label(new Rect(Margin, 650, GridWidth / 2, 60), $"{result} カードをクリックで拡大", Style(24, TextAnchor.MiddleLeft));
                if (GUI.Button(new Rect(Margin + GridWidth / 2, 652, GridWidth / 2, 56), "つぎへ ▶（Space）", ButtonStyle(26))) Confirm();
                return;
            }
            var message = Game.Phase == GamePhase.ShowingMismatch ? "ざんねん！ 交代！" : $"{players[Game.CurrentPlayer].Name} のばん ・ のこり {rest} ペア";
            GUI.Label(new Rect(0, 650, width, 60), message, Style(28));
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
                if (shakeT < 0.45f) rect.x += Mathf.Sin(shakeT * 70f) * 6f * (1f - shakeT / 0.45f);
                FillRect(new Rect(rect.x - 3, rect.y - 3, rect.width + 6, rect.height + 6), new Color(0.9f, 0.2f, 0.2f));
            }

            if (state == CardState.FaceDown)
            {
                FillRect(rect, Color.white);
                GUI.DrawTexture(new Rect(rect.x + 3, rect.y + 3, rect.width - 6, rect.height - 6), PixelArt.CardBack, ScaleMode.StretchToFill);
            }
            else
            {
                var tint = state == CardState.Matched ? new Color(0.55f, 0.55f, 0.55f) : Color.white;
                GUI.DrawTexture(rect, faces[Game.VisiblePairId(index)], ScaleMode.ScaleToFit, true, 0, tint, 0, 0);
                var owner = Game.OwnerOf(index);
                if (owner >= 0) FillRect(new Rect(rect.x, rect.yMax - 4, rect.width, 4), SeatColors[playerSeat[owner]]);
            }

            if (HumanTurn && index == cursor)
                Outline(new Rect(rect.x - 3, rect.y - 3, rect.width + 6, rect.height + 6), new Color(1f, 0.85f, 0.1f), 3);

            if (zoomed) return;
            if (state != CardState.FaceDown)
            {
                if (GUI.Button(rect, GUIContent.none, GUIStyle.none)) previewIndex = index;
                return;
            }
            if (!awaitingConfirm && HumanTurn && Game.IsSelectable(index) && GUI.Button(rect, GUIContent.none, GUIStyle.none))
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
            var scores = string.Join("   ", Enumerable.Range(0, players.Length).Select(p => $"{players[p].Name} {Game.PairsOf(p)}ペア"));
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
