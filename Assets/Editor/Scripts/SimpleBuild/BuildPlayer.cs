using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SimpleBuild {

    /// <summary>
    /// 実機ビルドの構築
    /// </summary>
    public class BuildPlayer
    {

        /// <summary>
        /// 出力先ディレクトリ名
        /// </summary>
        private const string OutputDirectoryName = "Build";

        /// <summary>
        /// 環境変数: 開発ビルドかどうか
        /// </summary>
        private const string EnvironmentVariableBuildDevelopment = "BUILD_DEVELOPMENT";

        /// <summary>
        /// 環境変数: クリーンビルドするかどうか
        /// </summary>
        private const string EnvironmentVariableBuildClean = "BUILD_CLEAN";

        /// <summary>
        /// 環境変数: プロファイラに接続するかどうか
        /// </summary>
        private const string EnvironmentVariableBuildConnectWithProfiler = "BUILD_CONNECT_WITH_PROFILER";

        /// <summary>
        /// 環境変数: デバッグを許可するかどうか
        /// </summary>
        private const string EnvironmentVariableBuildAllowDebugging = "BUILD_ALLOW_DEBUGGING";

        /// <summary>
        /// BuildTarget と BuildTargetGroup のディクショナリ
        /// </summary>
        private static readonly Dictionary<BuildTarget, BuildTargetGroup> BuildTargetGroupMap = new Dictionary<BuildTarget, BuildTargetGroup>() {
            { BuildTarget.iOS, BuildTargetGroup.iOS },
            { BuildTarget.Android, BuildTargetGroup.Android },
        };

        /// <summary>
        /// BuildTarget と出力拡張子のディクショナリ
        /// </summary>
        private static readonly Dictionary<BuildTarget, string> OutputExtensionMap = new Dictionary<BuildTarget, string>() {
            { BuildTarget.iOS, string.Empty },
            { BuildTarget.Android, ".apk" },
        };

        /// <summary>
        /// BuildTarget とディレクトリを作るべきかどうか？のディクショナリ
        /// </summary>
        private static readonly Dictionary<BuildTarget, bool> ShouldCreateDirectoryMap = new Dictionary<BuildTarget, bool>() {
            { BuildTarget.iOS, true },
            { BuildTarget.Android, false },
        };

        /// <summary>
        /// ビルドターゲットの実体
        /// </summary>
        private BuildTarget buildTarget;

        /// <summary>
        /// ビルドターゲット
        /// </summary>
        private BuildTarget BuildTarget {
            get {
                if (buildTarget == default(BuildTarget)) {
                    buildTarget = EditorUserBuildSettings.activeBuildTarget;
                }
                return buildTarget;
            }
            set {
                buildTarget = value;
            }
        }

        /// <summary>
        /// ビルドを実行する
        /// </summary>
        /// <remarks>アクティブビルドターゲットでのビルドを行う</remarks>
        [MenuItem("Project/Build/Player")]
        public static void Run() {
            new BuildPlayer().Execute();
        }

        /// <summary>
        /// iOS 向けビルドを実行する
        /// </summary>
        /// <remarks>Jenkins などの CI ツールからのキックを想定してサフィックスをつけています。</remarks>
        // ReSharper disable once UnusedMember.Global
        public static void Run_iOS() {
            new BuildPlayer() {
                BuildTarget = BuildTarget.iOS,
            }.Execute();
        }

        /// <summary>
        /// Android 向けビルドを実行する
        /// </summary>
        /// <remarks>Jenkins などの CI ツールからのキックを想定してサフィックスをつけています。</remarks>
        // ReSharper disable once UnusedMember.Global
        public static void Run_Android() {
            new BuildPlayer() {
                BuildTarget = BuildTarget.Android,
            }.Execute();
        }

        /// <summary>
        /// ビルドを実行する
        /// </summary>
        private void Execute() {
            EditorUserBuildSettings.development = Environment.GetEnvironmentVariable(EnvironmentVariableBuildDevelopment) != "false";
            var options = new BuildPlayerOptions {
                target = BuildTarget,
                targetGroup = BuildTargetGroupMap[BuildTarget],
                locationPathName = DeterminateOutputPath(),
                scenes = EditorBuildSettings.scenes.Select(x => x.path).ToArray()
            };
            if (ShouldCreateDirectoryMap[BuildTarget] && !Directory.Exists(options.locationPathName)) {
                Directory.CreateDirectory(options.locationPathName);
            }

            if (EditorUserBuildSettings.development) {
                options.options |= UnityEditor.BuildOptions.Development;
                // default: ConnectWithProfiler 設定 / BUILD_CONNECT_WITH_PROFILER=false で解除
                if (Environment.GetEnvironmentVariable(EnvironmentVariableBuildConnectWithProfiler) != "false")
                {
                    options.options |= UnityEditor.BuildOptions.ConnectWithProfiler;
                }
                // default: AllowDebugging 設定 / BUILD_ALLOW_DEBUGGING=false で解除
                if (Environment.GetEnvironmentVariable(EnvironmentVariableBuildAllowDebugging) != "false")
                {
                    options.options |= UnityEditor.BuildOptions.AllowDebugging;
                }
                // default: Append 設定 / BUILD_CLEAN=true で Replace 設定
                if (BuildTarget == BuildTarget.iOS && Environment.GetEnvironmentVariable(EnvironmentVariableBuildClean) != "true")
                {
                    options.options |= UnityEditor.BuildOptions.AcceptExternalModificationsToPlayer;
                }
            }
            options.options |= UnityEditor.BuildOptions.CompressWithLz4;
            BuildPipeline.BuildPlayer(options);
        }

        /// <summary>
        /// 出力先パスを決定する
        /// </summary>
        /// <returns></returns>
        private string DeterminateOutputPath() {
            return new[] {
                Path.GetDirectoryName(Application.dataPath),
                OutputDirectoryName,
                BuildTarget.ToString(),
                EditorUserBuildSettings.development ? "development" : "production",
                $"{Application.productName}{OutputExtensionMap[BuildTarget]}",
            }.Aggregate(Path.Combine);
        }

    }

}
