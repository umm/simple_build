using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace SimpleBuild
{
    public interface IPostBuildPlayer : IOrderedCallback
    {
        void OnPostBuildPlayer(BuildPlayerOptions buildPlayerOptions, BuildReport buildReport);
    }
}