using System.Collections.Generic;
using UnityEngine;

namespace MemoryMatch.Game
{
    /// IMGUI 座標で描く演出（紙吹雪・文字ポップ・画面揺れ・フラッシュ・花火）。
    public sealed class Celebration
    {
        struct Particle { public Vector2 Pos, Vel; public Color Color; public float Life, Size; }
        struct Popup { public string Text; public Vector2 Pos; public Color Color; public float Age, Size; }

        const float PopupSeconds = 1.1f;
        static readonly Color[] Confetti =
        {
            new Color(1f, 0.35f, 0.5f), new Color(1f, 0.85f, 0.2f), new Color(0.35f, 0.85f, 1f),
            new Color(0.5f, 1f, 0.45f), new Color(0.8f, 0.5f, 1f), Color.white,
        };

        readonly List<Particle> particles = new List<Particle>();
        readonly List<Popup> popups = new List<Popup>();
        float shake;
        float flash;
        Color flashColor;

        public Vector2 ShakeOffset => shake <= 0f ? Vector2.zero : Random.insideUnitCircle * shake * 14f;
        public int ParticleCount => particles.Count;

        public void Clear()
        {
            particles.Clear();
            popups.Clear();
            shake = flash = 0f;
        }

        public void Burst(Vector2 center, int count, float speed = 420f)
        {
            for (var i = 0; i < count; i++)
                particles.Add(new Particle
                {
                    Pos = center,
                    Vel = Random.insideUnitCircle.normalized * Random.Range(0.3f, 1f) * speed + Vector2.down * 200f,
                    Color = Confetti[Random.Range(0, Confetti.Length)],
                    Life = Random.Range(0.8f, 1.6f),
                    Size = Random.Range(6f, 12f),
                });
        }

        public void Firework(Vector2 center)
        {
            var color = Confetti[Random.Range(0, Confetti.Length)];
            for (var i = 0; i < 40; i++)
            {
                var angle = i * Mathf.PI * 2f / 40f;
                particles.Add(new Particle
                {
                    Pos = center,
                    Vel = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(220f, 300f),
                    Color = color,
                    Life = 1.3f,
                    Size = 7f,
                });
            }
        }

        public void Text(string text, Vector2 at, float size, Color color) =>
            popups.Add(new Popup { Text = text, Pos = at, Size = size, Color = color });

        public void Shake(float amount) => shake = Mathf.Max(shake, amount);

        public void Flash(Color color)
        {
            flashColor = color;
            flash = 1f;
        }

        public void Update(float dt)
        {
            for (var i = particles.Count - 1; i >= 0; i--)
            {
                var p = particles[i];
                p.Life -= dt;
                if (p.Life <= 0f) { particles.RemoveAt(i); continue; }
                p.Vel += Vector2.up * 520f * dt;
                p.Pos += p.Vel * dt;
                particles[i] = p;
            }
            for (var i = popups.Count - 1; i >= 0; i--)
            {
                var p = popups[i];
                p.Age += dt;
                if (p.Age >= PopupSeconds) { popups.RemoveAt(i); continue; }
                p.Pos += Vector2.down * 70f * dt;
                popups[i] = p;
            }
            shake = Mathf.Max(0f, shake - dt * 2.5f);
            flash = Mathf.Max(0f, flash - dt * 2.5f);
        }

        public void Draw(float width)
        {
            foreach (var p in particles)
            {
                var c = p.Color;
                c.a = Mathf.Clamp01(p.Life * 1.5f);
                Fill(new Rect(p.Pos.x - p.Size / 2, p.Pos.y - p.Size / 2, p.Size, p.Size), c);
            }
            foreach (var p in popups)
            {
                var t = p.Age / PopupSeconds;
                var scale = t < 0.15f ? Mathf.Lerp(0.4f, 1.25f, t / 0.15f) : Mathf.Lerp(1.25f, 1f, Mathf.Min(1f, (t - 0.15f) * 4f));
                var size = Mathf.RoundToInt(p.Size * scale);
                var alpha = 1f - Mathf.Max(0f, t - 0.7f) / 0.3f;
                var rect = new Rect(p.Pos.x - 400, p.Pos.y - size, 800, size * 2);
                Label(new Rect(rect.x + 3, rect.y + 3, rect.width, rect.height), p.Text, size, new Color(0, 0, 0, alpha));
                Label(rect, p.Text, size, new Color(p.Color.r, p.Color.g, p.Color.b, alpha));
            }
            if (flash > 0f) Fill(new Rect(0, 0, width, 720), new Color(flashColor.r, flashColor.g, flashColor.b, flash * 0.55f));
        }

        static void Label(Rect rect, string text, int size, Color color)
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = size, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = color },
            };
            GUI.Label(rect, text, style);
        }

        static void Fill(Rect rect, Color color) =>
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, color, 0, 0);
    }
}
