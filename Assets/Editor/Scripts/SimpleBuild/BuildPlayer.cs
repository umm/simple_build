﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SimpleBuild {

    /// <summary>
    /// 実機ビルドの構築
    /// </summary>
    public class BuildPlayer {

        /// <summary>
        /// 環境変数: 開発ビルドかどうか
        /// </summary>
        private const string EnvironmentVariableBuildDevelopment = "BUILD_DEVELOPMENT";

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
        public BuildTarget BuildTarget {
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
        public static void Run_iOS() {
            new BuildPlayer() {
                BuildTarget = BuildTarget.iOS,
            }.Execute();
        }

        /// <summary>
        /// Android 向けビルドを実行する
        /// </summary>
        /// <remarks>Jenkins などの CI ツールからのキックを想定してサフィックスをつけています。</remarks>
        public static void Run_Android() {
            new BuildPlayer() {
                BuildTarget = BuildTarget.Android,
            }.Execute();
        }

        /// <summary>
        /// ビルドを実行する
        /// </summary>
        private void Execute() {
            EditorUserBuildSettings.development = Environment.GetEnvironmentVariable(EnvironmentVariableBuildDevelopment) == "true";
            var options = new BuildPlayerOptions {
                target = BuildTarget,
                targetGroup = BuildTargetGroupMap[BuildTarget],
                locationPathName = DeterminateOuputPath(),
                scenes = EditorBuildSettings.scenes.Select(x => x.path).ToArray()
            };
            if (ShouldCreateDirectoryMap[BuildTarget] && !Directory.Exists(options.locationPathName)) {
                Directory.CreateDirectory(options.locationPathName);
            }
            if (EditorUserBuildSettings.development) {
                options.options |= BuildOptions.Development;
                options.options |= BuildOptions.ConnectWithProfiler;
                options.options |= BuildOptions.AllowDebugging;
            }
            options.options |= BuildOptions.CompressWithLz4;
            BuildPipeline.BuildPlayer(options);
        }

        /// <summary>
        /// 出力先パスを決定する
        /// </summary>
        /// <returns></returns>
        private string DeterminateOuputPath() {
            return new[] {
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                "Developer", // XXX: ココの決め方をどうにか考えたい。
                "out",
                BuildTarget.ToString(),
                string.Format("{0}{1}", Application.productName, OutputExtensionMap[BuildTarget]),
            }.Aggregate(Path.Combine);
        }

    }

}