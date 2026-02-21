using UnityEditor;

namespace MochiFitterNotifier
{
    /// <summary>
    /// EditorPrefs を介して設定値を読み書きする。
    /// 値はユーザーごとに保存され、プロジェクトには含まれない。
    /// </summary>
    internal static class MochiFitterNotifierSettings
    {
        private const string EnabledKey       = "MochiFitterNotifier.Enabled";
        private const string AudioFilePathKey = "MochiFitterNotifier.AudioFilePath";

        /// <summary>通知を有効にするか（デフォルト: true）</summary>
        public static bool IsEnabled
        {
            get => EditorPrefs.GetBool(EnabledKey, true);
            set => EditorPrefs.SetBool(EnabledKey, value);
        }

        /// <summary>
        /// カスタム音声ファイルの絶対パス。
        /// 空文字列のときはデフォルト音を使用する。
        /// </summary>
        public static string AudioFilePath
        {
            get => EditorPrefs.GetString(AudioFilePathKey, string.Empty);
            set => EditorPrefs.SetString(AudioFilePathKey, value);
        }
    }
}
