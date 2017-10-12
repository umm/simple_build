using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace SimpleBuild {

    public class BuildAssetBundleTest {

        [Test]
        public void PreprocessBuildTest() {
            MockProcessor.HasCalledOnPreprocess = false;
            BuildAssetBundle.Run();
            Assert.True(MockProcessor.HasCalledOnPreprocess);
        }

        [Test]
        public void PostprocessBuildTest() {
            MockProcessor.HasCalledOnPostprocess = false;
            BuildAssetBundle.Run();
            Assert.True(MockProcessor.HasCalledOnPostprocess);
        }

    }

    internal class MockProcessor : IPreprocessBuildAssetBundle, IPostprocessBuildAssetBundle {

        public static bool HasCalledOnPreprocess { get; set; }

        public static bool HasCalledOnPostprocess { get; set; }

        public void OnPreprocessBuildAssetBundle(string outputPath) {
            HasCalledOnPreprocess = true;
        }

        public void OnPostprocessBuildAssetBundle(string outputPath) {
            HasCalledOnPostprocess = true;
        }

        public int callbackOrder { get; private set; }

    }

}