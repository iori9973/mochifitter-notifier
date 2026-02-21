using UnityEditor;

namespace MochiFitterNotifier
{
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
