using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LastElevator.Tests.EditMode
{
    public sealed class ProjectConfigurationTests
    {
        private static readonly string[] ExpectedScenePaths =
        {
            "Assets/_Game/Scenes/00_Bootstrap.unity",
            "Assets/_Game/Scenes/01_MainMenu.unity",
            "Assets/_Game/Scenes/02_Game.unity"
        };

        [Test]
        public void AndroidIsTheActiveBuildTarget()
        {
            Assert.That(EditorUserBuildSettings.activeBuildTarget, Is.EqualTo(BuildTarget.Android));
        }

        [Test]
        public void MobileOrientationIsLockedToPortrait()
        {
            Assert.That(PlayerSettings.defaultInterfaceOrientation, Is.EqualTo(UIOrientation.Portrait));
            Assert.That(PlayerSettings.allowedAutorotateToPortrait, Is.True);
            Assert.That(PlayerSettings.allowedAutorotateToPortraitUpsideDown, Is.False);
            Assert.That(PlayerSettings.allowedAutorotateToLandscapeLeft, Is.False);
            Assert.That(PlayerSettings.allowedAutorotateToLandscapeRight, Is.False);
            Assert.That(PlayerSettings.defaultScreenWidth, Is.EqualTo(1080));
            Assert.That(PlayerSettings.defaultScreenHeight, Is.EqualTo(1920));
        }

        [Test]
        public void BuildScenesAreEnabledInBootOrder()
        {
            string[] actualScenePaths = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            Assert.That(actualScenePaths, Is.EqualTo(ExpectedScenePaths));
        }
    }
}
