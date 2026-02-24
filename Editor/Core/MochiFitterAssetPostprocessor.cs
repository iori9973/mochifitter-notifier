using UnityEditor;

namespace MochiFitterNotifier
{
    /// <summary>
    /// エディタ起動・ドメインリロード直後の自動再インポートで音が鳴らないよう、
    /// 準備完了フラグを管理する。
    /// </summary>
    [InitializeOnLoad]
    internal static class MochiFitterStartupGuard
    {
        private static bool _ready = false;
        internal static bool IsReady => _ready;

        static MochiFitterStartupGuard()
        {
            // 起動時の自動再インポートが終わった後の最初のフレームで ready にする
            EditorApplication.delayCall += () => _ready = true;
        }
    }

    /// <summary>
    /// Assets/OutfitRetargetingSystem/Outputs/ への Prefab 追加を検知して通知音を鳴らす。
    /// </summary>
    internal class MochiFitterAssetPostprocessor : AssetPostprocessor
    {
        private const string WatchPath = "Assets/OutfitRetargetingSystem/Outputs/";

        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (!MochiFitterStartupGuard.IsReady)
                return;

            if (!MochiFitterNotifierSettings.IsEnabled)
                return;

            foreach (string path in importedAssets)
            {
                if (path.StartsWith(WatchPath) && path.EndsWith(".prefab"))
                {
                    NotificationAudioPlayer.Play();
                    return; // 複数 Prefab が同時にインポートされても1回だけ再生
                }
            }
        }
    }
}
