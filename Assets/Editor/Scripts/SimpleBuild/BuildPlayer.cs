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
        /// 環境変数: 高速ビルドするかどうか
        /// </summary>
        private const string EnvironmentVariableBuildFaster = "BUILD_FASTER";

        /// <summary>
        /// 環境変数: プロファイラに接続するかどうか
        /// </summary>
        private const string EnvironmentVariableBuildConnectWithProfiler = "BUILD_CONNECT_WITH_PROFILER";

        /// <summary>
        /// 環境変数: デバッグを許可するかどうか
        /// </summary>
        private const string EnvironmentVariableBuildAllowDebugging = "BUILD_ALLOW_DEBUGGING";

        /// <summary>
        /// 環境変数: Proguard Minification を有効にするかどうか
        /// </summary>
        private const string EnvironmentVariableAndroidMinification = "BUILD_ANDROID_MINIFICATION";

        /// <summary>
        /// 環境変数: Android App Bundle を有効にするかどうか
        /// </summary>
        private const string EnvironmentVariableAndroidAppBundle = "BUILD_ANDROID_APP_BUNDLE";

        /// <summary>
        /// 環境変数: Apple Development Team ID
        /// </summary>
        private const string EnvironmentVariableAppleDeveloperTeamID = "APPLE_DEVELOPER_TEAM_ID";

        /// <summary>
        /// 環境変数: Android SDK のパス
        /// </summary>
        private const string EnvironmentVariableAndroidSdkPath = "BUILD_ANDROID_SDK_PATH";

        /// <summary>
        /// 環境変数: Android NDK のパス
        /// </summary>
        private const string EnvironmentVariableAndroidNdkPath = "BUILD_ANDROID_NDK_PATH";

        /// <summary>
        /// 環境変数: JDK のパス
        /// </summary>
        private const string EnvironmentVariableJdkPath = "BUILD_JDK_PATH";

        /// <summary>
        /// BuildTarget と BuildTargetGroup のディクショナリ
        /// </summary>
        private static readonly Dictionary<BuildTarget, BuildTargetGroup> BuildTargetGroupMap = new Dictionary<BuildTarget, BuildTargetGroup>() {
            { BuildTarget.iOS, BuildTargetGroup.iOS },
            { BuildTarget.Android, BuildTargetGroup.Android },
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
            if (BuildTarget == BuildTarget.Android)
            {
                var minification = Environment.GetEnvironmentVariable(EnvironmentVariableAndroidMinification) == "true";
                var appBundle = Environment.GetEnvironmentVariable(EnvironmentVariableAndroidAppBundle) == "true";
                EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
                EditorUserBuildSettings.androidDebugMinification = minification ? AndroidMinification.Proguard : AndroidMinification.None;
                EditorUserBuildSettings.androidReleaseMinification = minification ? AndroidMinification.Proguard : AndroidMinification.None;
                EditorUserBuildSettings.buildAppBundle = appBundle;

                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(EnvironmentVariableAndroidSdkPath)))
                {
                    EditorPrefs.SetString("AndroidSdkRoot", Environment.GetEnvironmentVariable(EnvironmentVariableAndroidSdkPath));
                }
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(EnvironmentVariableAndroidNdkPath)))
                {
                    EditorPrefs.SetString("AndroidNdkRoot", Environment.GetEnvironmentVariable(EnvironmentVariableAndroidNdkPath));
                    EditorPrefs.SetString("AndroidNdkRootR16b", Environment.GetEnvironmentVariable(EnvironmentVariableAndroidNdkPath));
                }
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(EnvironmentVariableJdkPath)))
                {
                    EditorPrefs.SetString("JdkPath", Environment.GetEnvironmentVariable(EnvironmentVariableJdkPath));
                }
            }

            var options = new BuildPlayerOptions {
                target = BuildTarget,
                targetGroup = BuildTargetGroupMap[BuildTarget],
                locationPathName = DeterminateOutputPath(),
                scenes = EditorBuildSettings.scenes.Select(x => x.path).ToArray()
            };
            if (ShouldCreateDirectoryMap[BuildTarget] && !Directory.Exists(options.locationPathName)) {
                Directory.CreateDirectory(options.locationPathName);
            }

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(EnvironmentVariableAppleDeveloperTeamID)))
            {
                PlayerSettings.iOS.appleEnableAutomaticSigning = true;
                PlayerSettings.iOS.appleDeveloperTeamID = Environment.GetEnvironmentVariable(EnvironmentVariableAppleDeveloperTeamID);
            }

            if (EditorUserBuildSettings.development) {
                options.options |= UnityEditor.BuildOptions.Development;
                // default: ConnectWithProfiler 設定 / BUILD_CONNECT_WITH_PROFILER=false で解除
                if (Environment.GetEnvironmentVariable(EnvironmentVariableBuildConnectWithProfiler) != "false")
                {
                    options.options |= UnityEditor.BuildOptions.ConnectWithProfiler;
                }
                // default: AllowDebugging 設定 / BUILD_ALLOW_DEBUGGING=true で設定
                if (Environment.GetEnvironmentVariable(EnvironmentVariableBuildAllowDebugging) == "true")
                {
                    // このoptionはいつくかのiOS buildで linker exceptionを起こすことがあるので注意. (#38)
                    options.options |= UnityEditor.BuildOptions.AllowDebugging;
                }
                // default: Append 設定 / BUILD_CLEAN=true で Replace 設定
                if (BuildTarget == BuildTarget.iOS && Environment.GetEnvironmentVariable(EnvironmentVariableBuildFaster) == "true")
                {
                    options.options |= UnityEditor.BuildOptions.AcceptExternalModificationsToPlayer;
                }
            }
            options.options |= UnityEditor.BuildOptions.CompressWithLz4;

            // IPostprocessBuildWithReport などとは別に、処理を呼び出したい場合に利用する interface を実装したクラス群を取得
            var preBuildPlayers = InternalUtility.LoadProcessors<IPreBuildPlayer>();
            var postBuildPlayers = InternalUtility.LoadProcessors<IPostBuildPlayer>();

            // IPreBuildPlayer を実行
            preBuildPlayers.ToList().ForEach(x => x.OnPreBuildPlayer(options));

            var buildReport = BuildPipeline.BuildPlayer(options);

            // IPostBuildPlayer を実行
            postBuildPlayers.ToList().ForEach(x => x.OnPostBuildPlayer(options, buildReport));
        }

        /// <summary>
        /// 出力先パスを決定する
        /// </summary>
        /// <returns></returns>
        private string DeterminateOutputPath()
        {
            var extension = "";
            if (BuildTarget == BuildTarget.Android)
            {
                extension = Environment.GetEnvironmentVariable(EnvironmentVariableAndroidAppBundle) == "true" ? ".aab" : ".apk";
            }
            return new[] {
                Path.GetDirectoryName(Application.dataPath),
                OutputDirectoryName,
                BuildTarget.ToString(),
                EditorUserBuildSettings.development ? "development" : "production",
                $"{Application.productName}{extension}",
            }.Aggregate(Path.Combine);
        }

    }

}
