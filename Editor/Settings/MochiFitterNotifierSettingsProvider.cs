using UnityEditor;
using UnityEngine;

namespace MochiFitterNotifier
{
    /// <summary>
    /// Edit → Preferences → MochiFitter Notifier に設定 UI を追加する。
    /// </summary>
    internal class MochiFitterNotifierSettingsProvider : SettingsProvider
    {
        private MochiFitterNotifierSettingsProvider()
            : base("Preferences/MochiFitter Notifier", SettingsScope.User) { }

        [SettingsProvider]
        public static SettingsProvider Create() => new MochiFitterNotifierSettingsProvider();

        public override void OnGUI(string searchContext)
        {
            using var indent = new EditorGUI.IndentLevelScope(1);

            EditorGUILayout.Space(8);

            // --- 有効/無効 ---
            MochiFitterNotifierSettings.IsEnabled = EditorGUILayout.Toggle(
                new GUIContent("通知を有効にする", "処理完了時に音声を再生します"),
                MochiFitterNotifierSettings.IsEnabled);

            EditorGUILayout.Space(12);

            // --- カスタム音声 ---
            EditorGUILayout.LabelField("通知音", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                string current  = MochiFitterNotifierSettings.AudioFilePath;
                bool   hasCustom = !string.IsNullOrEmpty(current);

                using (new EditorGUI.DisabledScope(true))
                    EditorGUILayout.TextField(hasCustom ? current : "(デフォルト: 7音メロディ)");

                if (GUILayout.Button("変更", GUILayout.Width(50)))
                {
                    string selected = EditorUtility.OpenFilePanel("通知音を選択", "", "wav,mp3,m4a");
                    if (!string.IsNullOrEmpty(selected))
                        MochiFitterNotifierSettings.AudioFilePath = selected;
                }

                using (new EditorGUI.DisabledScope(!hasCustom))
                {
                    if (GUILayout.Button("リセット", GUILayout.Width(62)))
                        MochiFitterNotifierSettings.AudioFilePath = string.Empty;
                }
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(
                "対応フォーマット: WAV / MP3 / M4A\n" +
                "※ Linux では WAV のみ再生可能です。",
                MessageType.None);

            EditorGUILayout.Space(12);

            // --- テスト ---
            using (new EditorGUI.DisabledScope(!MochiFitterNotifierSettings.IsEnabled))
            {
                if (GUILayout.Button("テスト再生", GUILayout.Width(100)))
                    NotificationAudioPlayer.Play();
            }
        }
    }
}
