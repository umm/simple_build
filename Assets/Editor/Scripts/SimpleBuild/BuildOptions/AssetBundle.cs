using System;
using UnityEditor;

namespace SimpleBuild.BuildOptions
{
    public static class AssetBundle
    {
        private const string EnvironmentVariableBuildAssetBundleForceRebuild = "BUILD_ASSETBUNDLE_FORCE_REBUILD";

        private const string MenuItemNameForceRebuild = "Project/AssetBundle/Options/Force Rebuild";

        public static bool ShouldForceRebuildAssetBundles()
        {
            return
                Environment.GetEnvironmentVariable(EnvironmentVariableBuildAssetBundleForceRebuild) != "false"
                && Menu.GetChecked(MenuItemNameForceRebuild);
        }

        [MenuItem(MenuItemNameForceRebuild)]
        public static void ToggleForceRebuildAssetBundles()
        {
            Menu.SetChecked(MenuItemNameForceRebuild, !ShouldForceRebuildAssetBundles());
        }
    }
}