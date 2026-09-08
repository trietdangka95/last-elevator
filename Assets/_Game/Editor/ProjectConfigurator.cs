using UnityEditor;
using UnityEngine;

namespace LastElevator.Editor
{
    [InitializeOnLoad]
    internal static class ProjectConfigurator
    {
        static ProjectConfigurator()
        {
            EditorApplication.delayCall += Configure;
        }

        private static void Configure()
        {
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.defaultScreenWidth = 1080;
            PlayerSettings.defaultScreenHeight = 1920;

            if (Application.isBatchMode || EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                return;
            }

            if (BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            {
                EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Android, BuildTarget.Android);
            }
        }
    }
}
