using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace SimpleBuild {

    /// <summary>
    /// AssetBundle ビルド前処理用インタフェース
    /// </summary>
    /// <remarks>Unity 2017.1.1p3 時点では AssetBundle のビルド前後に処理を挟めないので自作する</remarks>
    public interface IPreprocessBuildAssetBundle : IOrderedCallback {

        void OnPreprocessBuildAssetBundle(BuildTarget buildTarget, string path);

    }

    /// <summary>
    /// AssetBundle ビルド後処理用インタフェース
    /// </summary>
    /// <remarks>Unity 2017.1.1p3 時点では AssetBundle のビルド前後に処理を挟めないので自作する</remarks>
    public interface IPostprocessBuildAssetBundle : IOrderedCallback {

        void OnPostprocessBuildAssetBundle(BuildTarget buildTarget, string path);

    }

    /// <summary>
    /// AssetBundle をビルドします
    /// </summary>
    public class BuildAssetBundle {

        /// <summary>
        /// 出力先パスフォーマット
        /// </summary>
        private const string OUTPUT_PATH_FORMAT = "Assets/AssetBundles/{0}";

        /// <summary>
        /// ビルドターゲットと出力先ディレクトリの辞書
        /// </summary>
        private static readonly Dictionary<BuildTarget, string> OUTPUT_DIRECTORY_MAP = new Dictionary<BuildTarget, string>() {
            { BuildTarget.iOS                     , "iOS" },
            { BuildTarget.Android                 , "Android" },
            { BuildTarget.StandaloneWindows       , "Standalone" },
            { BuildTarget.StandaloneWindows64     , "Standalone" },
            { BuildTarget.StandaloneOSXIntel      , "Standalone" },
            { BuildTarget.StandaloneOSXIntel64    , "Standalone" },
            { BuildTarget.StandaloneOSX  , "Standalone" },
            { BuildTarget.StandaloneLinux         , "Standalone" },
            { BuildTarget.StandaloneLinux64       , "Standalone" },
            { BuildTarget.StandaloneLinuxUniversal, "Standalone" },
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
#if UNITY_EDITOR_OSX && UNITY_EDITOR_64
                BuildTarget = BuildTarget.StandaloneOSXIntel64,
#elif UNITY_EDITOR_OSX && !UNITY_EDITOR_64
                BuildTarget = BuildTarget.StandaloneOSXIntel,
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
            if (!OUTPUT_DIRECTORY_MAP.ContainsKey(this.BuildTarget)) {
                Debug.LogErrorFormat("BuildTarget: {0} 向けの AssetBundle 構築はサポートしていません。", this.BuildTarget);
                return;
            }
            string outputPath = this.DeterminateOutputPath();
            string fullPath = Path.GetFullPath(Path.Combine(Path.Combine(Application.dataPath, ".."), outputPath));
            if (!Directory.Exists(fullPath)) {
                Directory.CreateDirectory(fullPath);
            }
            CallPreprocessBuild(this.BuildTarget, outputPath);
            BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.ChunkBasedCompression, this.BuildTarget);
            CallPostprocessBuild(this.BuildTarget, outputPath);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 出力先パスを確定させる
        /// </summary>
        /// <returns>出力先パス</returns>
        private string DeterminateOutputPath() {
            return string.Format(OUTPUT_PATH_FORMAT, OUTPUT_DIRECTORY_MAP[this.BuildTarget]);
        }

        /// <summary>
        /// AssetBundle 構築前処理をコール
        /// </summary>
        /// <param name="buildTarget">ビルドターゲットプラットフォーム</param>
        /// <param name="path">出力先パス</param>
        private static void CallPreprocessBuild(BuildTarget buildTarget, string path) {
            IEnumerable<IPreprocessBuildAssetBundle> preprocessors = LoadProcessors<IPreprocessBuildAssetBundle>();
            if (preprocessors == null) {
                return;
            }
            foreach (IPreprocessBuildAssetBundle preprocessor in preprocessors) {
                preprocessor.OnPreprocessBuildAssetBundle(buildTarget, path);
            }
        }

        /// <summary>
        /// AssetBundle 構築後処理をコール
        /// </summary>
        /// <param name="buildTarget">ビルドターゲットプラットフォーム</param>
        /// <param name="path">出力先パス</param>
        private static void CallPostprocessBuild(BuildTarget buildTarget, string path) {
            IEnumerable<IPostprocessBuildAssetBundle> postprocessors = LoadProcessors<IPostprocessBuildAssetBundle>();
            if (postprocessors == null) {
                return;
            }
            foreach (IPostprocessBuildAssetBundle postprocessor in postprocessors) {
                postprocessor.OnPostprocessBuildAssetBundle(buildTarget, path);
            }
        }

        /// <summary>
        /// Reflection を用いて IPreprocessBuildAssetBundle や IPostprocessBuildAssetBundle のインスタンスを読み込む
        /// </summary>
        /// <typeparam name="T">IPreprocessBuildAssetBundle, IPostprocessBuildAssetBundle</typeparam>
        /// <returns>インスタンスのコレクション</returns>
        private static IEnumerable<T> LoadProcessors<T>() where T : IOrderedCallback {
            // ReSharper disable once ConvertClosureToMethodGroup
            string dllPath = new [] {
                Path.GetDirectoryName(Application.dataPath),
                "Library",
                "ScriptAssemblies",
                "Assembly-CSharp-Editor.dll",
            }.Aggregate((a, b) => Path.Combine(a, b));
            if (!File.Exists(dllPath)) {
                return null;
            }
            Assembly assembly = Assembly.LoadFile(dllPath);
            return assembly.GetTypes()
                .Where(x => typeof(T).IsAssignableFrom(x))
                .Where(x => x != typeof(T))
                .Select(x => (T)Activator.CreateInstance(x))
                .OrderBy(x => x.callbackOrder);
        }


    }

}