using UnityEngine;

namespace MemoryMatch.Game
{
    /// 矩形波でその場で作る効果音。外部素材は使わない。
    public sealed class SoundFx
    {
        const int Rate = 22050;

        readonly AudioSource source;
        readonly AudioClip flip, miss, big, win;
        readonly AudioClip[] match = new AudioClip[6];

        public SoundFx(AudioSource source)
        {
            this.source = source;
            flip = Tones("flip", 0.04f, 0.25f, 1046f);
            miss = Tones("miss", 0.16f, 0.3f, 247f, 185f);
            big = Tones("big", 0.07f, 0.3f, 523f, 659f, 784f, 1046f, 1318f, 1568f, 2093f);
            win = Tones("win", 0.12f, 0.3f, 392f, 523f, 659f, 784f, 659f, 784f, 1046f, 1046f);
            for (var c = 0; c < match.Length; c++)
            {
                var up = Mathf.Pow(2f, c * 2 / 12f);
                match[c] = Tones("match" + c, 0.06f, 0.3f, 659f * up, 784f * up, 1046f * up);
            }
        }

        public void Flip() => Play(flip);
        public void Miss() => Play(miss);
        public void Big() => Play(big);
        public void Win() => Play(win);
        public void Match(int combo) => Play(match[Mathf.Clamp(combo - 1, 0, match.Length - 1)]);

        void Play(AudioClip clip)
        {
            if (source != null && source.isActiveAndEnabled) source.PlayOneShot(clip);
        }

        static AudioClip Tones(string name, float noteSeconds, float volume, params float[] freqs)
        {
            var per = Mathf.RoundToInt(noteSeconds * Rate);
            var data = new float[per * freqs.Length];
            for (var n = 0; n < freqs.Length; n++)
            for (var i = 0; i < per; i++)
            {
                var t = (float)i / Rate;
                var square = Mathf.Repeat(t * freqs[n], 1f) < 0.5f ? 1f : -1f;
                var envelope = 1f - (float)i / per * 0.7f;
                data[n * per + i] = square * volume * envelope;
            }
            var clip = AudioClip.Create(name, data.Length, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
