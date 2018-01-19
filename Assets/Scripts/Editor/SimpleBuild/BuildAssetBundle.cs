using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SimpleBuild {

    /// <summary>
    /// AssetBundle をビルドします
    /// </summary>
    public class BuildAssetBundle {

        /// <summary>
        /// 環境変数: 開発ビルドかどうか
        /// </summary>
        private const string ENVIRONMENT_VARIABLE_BUILD_DEVELOPMENT = "BUILD_DEVELOPMENT";

        /// <summary>
        /// 出力先パスフォーマット
        /// </summary>
        private const string OUTPUT_PATH_FORMAT = "Assets/AssetBundles/{0}";

        /// <summary>
        /// ビルドターゲットと出力先ディレクトリの辞書
        /// </summary>
        private static readonly Dictionary<BuildTarget, string> OUTPUT_DIRECTORY_MAP = new Dictionary<BuildTarget, string>() {
            { BuildTarget.iOS                , "iOS" },
            { BuildTarget.Android            , "Android" },
            { BuildTarget.StandaloneWindows  , "Standalone" },
            { BuildTarget.StandaloneWindows64, "Standalone" },
            { BuildTarget.StandaloneOSX      , "Standalone" },
            { BuildTarget.StandaloneLinux    , "Standalone" },
        };

        /// <summary>
        /// ビルドターゲットの実体
        /// </summary>
        private BuildTarget buildTarget;

        /// <summary>
        /// ビルドターゲット
        /// </summary>
        public BuildTarget BuildTarget {
            get {
                if (this.buildTarget == default(BuildTarget)) {
                    this.buildTarget = EditorUserBuildSettings.activeBuildTarget;
                }
                return this.buildTarget;
            }
            set {
                this.buildTarget = value;
            }
        }

        /// <summary>
        /// ビルドを実行する
        /// </summary>
        /// <remarks>アクティブビルドターゲットでのビルドを行う</remarks>
        [MenuItem("Project/Build/AssetBundle")]
        public static void Run() {
            new BuildAssetBundle().Execute();
        }

        /// <summary>
        /// Standalone 向けビルドを実行する
        /// </summary>
        /// <remarks>Jenkins などの CI ツールからのキックを想定してサフィックスをつけています。</remarks>
        public static void Run_Standalone() {
            new BuildAssetBundle() {
#if UNITY_EDITOR_OSX
                BuildTarget = BuildTarget.StandaloneOSX,
#elif UNITY_EDITOR_WIN && UNITY_EDITOR_64
                BuildTarget = BuildTarget.StandaloneWindows64,
#elif UNITY_EDITOR_WIN && !UNITY_EDITOR_64
                BuildTarget = BuildTarget.StandaloneWindows,
#else
                BuildTarget = BuildTarget.StandaloneLinux,
#endif
            }.Execute();
        }

        /// <summary>
        /// iOS 向けビルドを実行する
        /// </summary>
        /// <remarks>Jenkins などの CI ツールからのキックを想定してサフィックスをつけています。</remarks>
        public static void Run_iOS() {
            new BuildAssetBundle() {
                BuildTarget = BuildTarget.iOS,
            }.Execute();
        }

        /// <summary>
        /// Android 向けビルドを実行する
        /// </summary>
        /// <remarks>Jenkins などの CI ツールからのキックを想定してサフィックスをつけています。</remarks>
        public static void Run_Android() {
            new BuildAssetBundle() {
                BuildTarget = BuildTarget.Android,
            }.Execute();
        }

        /// <summary>
        /// BuildPipeline.BuildAssetBundles を実行する
        /// </summary>
        private void Execute() {
            EditorUserBuildSettings.development = Environment.GetEnvironmentVariable(ENVIRONMENT_VARIABLE_BUILD_DEVELOPMENT) == "true";
            if (!OUTPUT_DIRECTORY_MAP.ContainsKey(this.BuildTarget)) {
                Debug.LogErrorFormat("BuildTarget: {0} 向けの AssetBundle 構築はサポートしていません。", this.BuildTarget);
                return;
            }
            string outputPath = this.DeterminateOutputPath();
            string fullPath = Path.GetFullPath(Path.Combine(Path.Combine(Application.dataPath, ".."), outputPath));
            if (!Directory.Exists(fullPath)) {
                Directory.CreateDirectory(fullPath);
            }
            BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.ChunkBasedCompression, this.BuildTarget);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 出力先パスを確定させる
        /// </summary>
        /// <returns>出力先パス</returns>
        private string DeterminateOutputPath() {
            return string.Format(OUTPUT_PATH_FORMAT, OUTPUT_DIRECTORY_MAP[this.BuildTarget]);
        }


    }

}