using System;
using UnityEditor;

namespace SimpleBuild.BuildOptions
{
    public static class AssetBundle
    {
        private const string EnvironmentVariableBuildAssetBundleForceRebuild = "BUILD_ASSETBUNDLE_FORCE_REBUILD";
        private const string EnvironmentVariableBuildAssetBundleUncompressed = "BUILD_ASSETBUNDLE_UNCOMPRESSED";

        private const string MenuItemNameForceRebuild = "Project/AssetBundle/Options/Force Rebuild";
        private const string MenuItemNameUncompressed = "Project/AssetBundle/Options/Uncompressed";

        public static bool ShouldForceRebuildAssetBundles()
        {
            return
                Environment.GetEnvironmentVariable(EnvironmentVariableBuildAssetBundleForceRebuild) == "true"
                || Menu.GetChecked(MenuItemNameForceRebuild);
        }

        public static bool ShouldUncompressedAssetBundles()
        {
            return
                Environment.GetEnvironmentVariable(EnvironmentVariableBuildAssetBundleUncompressed) == "true"
                || Menu.GetChecked(MenuItemNameForceRebuild);
        }

        [MenuItem(MenuItemNameForceRebuild)]
        public static void ToggleForceRebuildAssetBundles()
        {
            Menu.SetChecked(MenuItemNameForceRebuild, !ShouldForceRebuildAssetBundles());
        }

        [MenuItem(MenuItemNameUncompressed)]
        public static void ToggleUncompressedAssetBundles()
        {
            Menu.SetChecked(MenuItemNameUncompressed, !ShouldUncompressedAssetBundles());
        }
    }
}
