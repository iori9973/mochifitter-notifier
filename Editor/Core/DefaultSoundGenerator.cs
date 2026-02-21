using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace MochiFitterNotifier
{
    /// <summary>
    /// デフォルト通知音（7音メロディ）を PCM WAV として生成・キャッシュする。
    /// ファイルは Unity の temporaryCachePath に保存され、外部ファイル依存がない。
    /// </summary>
    internal static class DefaultSoundGenerator
    {
        // ファイル名を変えると音を差し替えた際に自動再生成される
        private static readonly string CachePath = Path.Combine(
            Application.temporaryCachePath,
            "MochiFitterNotifier",
            "complete_v3.wav");

        /// <summary>
        /// デフォルト音の絶対パスを返す。未生成なら先に生成する。
        /// </summary>
        public static string EnsureDefaultSound()
        {
            if (!File.Exists(CachePath))
                GenerateChime(CachePath);

            return CachePath;
        }

        // ---- WAV 生成 -------------------------------------------------------

        private static void GenerateChime(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            const int sampleRate    = 44100;
            const int channels      = 1;
            const int bitsPerSample = 16;

            // ド×4 → シ♭ → レ → ドー（テンポよく7音）
            int gapSamples = (int)(sampleRate * 0.025); // 25ms の無音

            float[] n1 = GenerateBellTone(523.25, 0.10, sampleRate, decayRate: 12.0); // ド
            float[] n2 = GenerateBellTone(523.25, 0.10, sampleRate, decayRate: 12.0); // ド
            float[] n3 = GenerateBellTone(523.25, 0.10, sampleRate, decayRate: 12.0); // ド
            float[] n4 = GenerateBellTone(523.25, 0.10, sampleRate, decayRate: 12.0); // ド
            float[] n5 = GenerateBellTone(466.16, 0.10, sampleRate, decayRate: 12.0); // シ♭
            float[] n6 = GenerateBellTone(587.33, 0.10, sampleRate, decayRate: 12.0); // レ
            float[] n7 = GenerateBellTone(523.25, 0.40, sampleRate, decayRate:  3.5); // ドー

            int totalSamples = n1.Length + gapSamples
                             + n2.Length + gapSamples
                             + n3.Length + gapSamples
                             + n4.Length + gapSamples
                             + n5.Length + gapSamples
                             + n6.Length + gapSamples
                             + n7.Length;
            float[] samples = new float[totalSamples];
            int offset = 0;

            n1.CopyTo(samples, offset); offset += n1.Length + gapSamples;
            n2.CopyTo(samples, offset); offset += n2.Length + gapSamples;
            n3.CopyTo(samples, offset); offset += n3.Length + gapSamples;
            n4.CopyTo(samples, offset); offset += n4.Length + gapSamples;
            n5.CopyTo(samples, offset); offset += n5.Length + gapSamples;
            n6.CopyTo(samples, offset); offset += n6.Length + gapSamples;
            n7.CopyTo(samples, offset);

            int dataSize = samples.Length * (bitsPerSample / 8);

            using var stream = new FileStream(path, FileMode.Create);
            using var writer = new BinaryWriter(stream);

            // RIFF ヘッダー
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + dataSize);
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));

            // fmt チャンク
            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((short)1);                                     // PCM
            writer.Write((short)channels);
            writer.Write(sampleRate);
            writer.Write(sampleRate * channels * (bitsPerSample / 8)); // バイトレート
            writer.Write((short)(channels * (bitsPerSample / 8)));     // ブロックアライン
            writer.Write((short)bitsPerSample);

            // data チャンク
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(dataSize);

            foreach (float s in samples)
            {
                float clamped = Math.Max(-1f, Math.Min(1f, s));
                writer.Write((short)(clamped * short.MaxValue));
            }
        }

        /// <summary>
        /// 倍音付きのベル音を生成する。
        /// 基音＋第2倍音＋第3倍音の合成と指数関数的な減衰でベル/マリンバに近い音色を実現。
        /// </summary>
        private static float[] GenerateBellTone(double frequency, double durationSec, int sampleRate,
                                                double decayRate = 5.0)
        {
            int total         = (int)(sampleRate * durationSec);
            int attackSamples = (int)(sampleRate * 0.004); // 4ms の短いアタック
            float[] samples   = new float[total];

            for (int i = 0; i < total; i++)
            {
                double t = (double)i / sampleRate;

                // アタック: 短い線形立ち上がり
                double attack = i < attackSamples
                    ? (double)i / attackSamples
                    : 1.0;

                // 指数関数的な減衰（ベルらしい自然な余韻）
                double decay    = Math.Exp(-decayRate * t);
                double envelope = attack * decay;

                // 基音 + 倍音の合成（倍音が音に暖かみと深みを加える）
                double wave = Math.Sin(2 * Math.PI * frequency       * t) * 1.00  // 基音
                            + Math.Sin(2 * Math.PI * frequency * 2.0 * t) * 0.35  // 第2倍音
                            + Math.Sin(2 * Math.PI * frequency * 3.0 * t) * 0.12; // 第3倍音

                // 合計振幅を正規化（1.0 + 0.35 + 0.12 = 1.47）してピークを抑える
                samples[i] = (float)(wave / 1.47 * envelope * 0.75);
            }

            return samples;
        }
    }
}
