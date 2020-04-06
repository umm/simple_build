using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build;

namespace SimpleBuild
{
    internal static class InternalUtility
    {
        /// <summary>
        /// Reflection を用いて IPreprocessBuildAssetBundle や IPostprocessBuildAssetBundle のインスタンスを読み込む
        /// </summary>
        /// <typeparam name="T">IPreprocessBuildAssetBundle, IPostprocessBuildAssetBundle</typeparam>
        /// <returns>インスタンスのコレクション</returns>
        internal static IEnumerable<T> LoadProcessors<T>() where T : IOrderedCallback
        {
            return AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => typeof(T).IsAssignableFrom(x) && x.IsClass && !x.IsAbstract)
                .Select(x => (T) Activator.CreateInstance(x))
                .OrderBy(x => x.callbackOrder);
        }
    }
}