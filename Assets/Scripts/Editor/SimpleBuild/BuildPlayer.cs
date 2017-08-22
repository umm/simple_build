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
    public class BuildPlayer {

        /// <summary>
        /// BuildTarget と BuildTargetGroup のディクショナリ
        /// </summary>
        private static readonly Dictionary<BuildTarget, BuildTargetGroup> BUILD_TARGET_GROUP_MAP = new Dictionary<BuildTarget, BuildTargetGroup>() {
            { BuildTarget.iOS, BuildTargetGroup.iOS },
            { BuildTarget.Android, BuildTargetGroup.Android },
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
            BuildPlayerOptions options = new BuildPlayerOptions {
                target = this.BuildTarget,
                targetGroup = BUILD_TARGET_GROUP_MAP[this.BuildTarget],
                locationPathName = this.DeterminateOuputPath(),
                scenes = EditorBuildSettings.scenes.Select(x => x.path).ToArray()
            };
            if (!Directory.Exists(options.locationPathName)) {
                Directory.CreateDirectory(options.locationPathName);
            }
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
                this.BuildTarget.ToString(),
                Application.productName,
            }.Aggregate(Path.Combine);
        }

    }

}