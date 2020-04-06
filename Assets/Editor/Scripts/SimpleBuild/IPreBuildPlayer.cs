using UnityEditor;
using UnityEditor.Build;

namespace SimpleBuild
{
    public interface IPreBuildPlayer : IOrderedCallback
    {
        void OnPreBuildPlayer(BuildPlayerOptions buildPlayerOptions);
    }
}