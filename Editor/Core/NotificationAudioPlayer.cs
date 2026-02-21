using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MochiFitterNotifier
{
    /// <summary>
    /// OS に応じた方法で音声ファイルを再生する。
    /// Windows: WAV → winmm.dll PlaySound / その他 → PowerShell + WPF MediaPlayer
    /// macOS  : afplay コマンド（WAV・MP3・AAC・FLAC・AIFF 等対応）
    /// Linux  : aplay コマンド（WAV のみ）
    /// </summary>
    internal static class NotificationAudioPlayer
    {
        public static void Play()
        {
            string path = ResolveAudioPath();
            if (string.IsNullOrEmpty(path))
            {
                UnityEngine.Debug.LogWarning("[MochiFitterNotifier] 再生できる音声ファイルが見つかりませんでした。");
                return;
            }
            Play(path);
        }

        public static void Play(string absolutePath)
        {
            try
            {
                PlayPlatformSpecific(absolutePath);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"[MochiFitterNotifier] 音声の再生に失敗しました: {e.Message}");
            }
        }

        private static string ResolveAudioPath()
        {
            string custom = MochiFitterNotifierSettings.AudioFilePath;
            if (!string.IsNullOrEmpty(custom) && File.Exists(custom))
                return custom;

            return DefaultSoundGenerator.EnsureDefaultSound();
        }

        private static void PlayPlatformSpecific(string absolutePath)
        {
#if UNITY_EDITOR_WIN
            bool isWav = string.Equals(Path.GetExtension(absolutePath), ".wav",
                                       StringComparison.OrdinalIgnoreCase);
            if (isWav)
                PlayWindows(absolutePath);
            else
                PlayWindowsNonWav(absolutePath);
#elif UNITY_EDITOR_OSX
            PlayMac(absolutePath);
#else
            PlayLinux(absolutePath);
#endif
        }

#if UNITY_EDITOR_WIN
        // ---- WAV 再生: PlaySound（軽量・ノンブロッキング） -----------------------

        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        private static extern bool PlaySound(string pszSound, IntPtr hmod, uint fdwSound);

        private const uint SndFilename  = 0x00020000;
        private const uint SndAsync     = 0x00000001;
        private const uint SndNoDefault = 0x00000002;

        private static void PlayWindows(string path)
        {
            PlaySound(path, IntPtr.Zero, SndFilename | SndAsync | SndNoDefault);
        }

        // ---- WAV 以外: PowerShell + WPF MediaPlayer ------------------------------
        // Windows 11 では MCI (mciSendString) が MP3 を再生できない場合があるため、
        // Windows 標準の WPF MediaPlayer を PowerShell 経由で呼び出して再生する。

        private static void PlayWindowsNonWav(string path)
        {
            // file:/// URI に変換（スペースや日本語パスを安全に扱うため）
            string uri = new Uri(path).AbsoluteUri.Replace("'", "''");

            // Base64 エンコードでクォート問題を回避
            string script =
                "Add-Type -AssemblyName presentationCore\n" +
                $"$p = New-Object System.Windows.Media.MediaPlayer\n" +
                $"$p.Open([uri]'{uri}')\n" +
                "$p.Play()\n" +
                "Start-Sleep -Milliseconds 500\n" +
                "$nd = $p.NaturalDuration\n" +
                "if ($nd.HasTimeSpan) { $d = [int]$nd.TimeSpan.TotalSeconds + 1 } else { $d = 10 }\n" +
                "Start-Sleep -Seconds $d\n" +
                "$p.Stop()";

            string encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName               = "powershell.exe",
                Arguments              = $"-NonInteractive -WindowStyle Hidden -EncodedCommand {encoded}",
                UseShellExecute        = false,
                CreateNoWindow         = true
            };
            Task.Run(() => System.Diagnostics.Process.Start(psi));
        }

#elif UNITY_EDITOR_OSX
        // afplay は WAV・MP3・AAC・AIFF・FLAC・M4A 等に対応
        private static void PlayMac(string path)
        {
            System.Diagnostics.Process.Start("afplay", $"\"{path}\"");
        }

#else
        // aplay は WAV のみ対応
        private static void PlayLinux(string path)
        {
            System.Diagnostics.Process.Start("aplay", $"\"{path}\"");
        }
#endif
    }
}
