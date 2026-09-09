using System.Linq;
using LastElevator.Gameplay.Encounters;
using NUnit.Framework;
using UnityEditor;

namespace LastElevator.Tests.EditMode
{
    public sealed class EncounterAssetTests
    {
        private static readonly string[] ExpectedEncounterIds =
        {
            "event_battery_room",
            "event_maintenance_cache",
            "event_power_overload",
            "event_strange_radio"
        };

        [Test]
        public void M1DayFiveProvidesFourPlayablePlaceholderEncounters()
        {
            EncounterDefinition[] encounters = AssetDatabase
                .FindAssets("t:EncounterDefinition", new[] { "Assets/_Game/Data/Encounters" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<EncounterDefinition>)
                .ToArray();

            string[] actualIds = encounters.Select(encounter => encounter.id).ToArray();

            for (int i = 0; i < ExpectedEncounterIds.Length; i++)
            {
                Assert.That(actualIds, Does.Contain(ExpectedEncounterIds[i]));
                EncounterDefinition encounter = encounters.Single(
                    definition => definition.id == ExpectedEncounterIds[i]);
                Assert.That(encounter.title, Is.Not.Empty, encounter.id);
                Assert.That(encounter.body, Is.Not.Empty, encounter.id);
                Assert.That(encounter.clueText, Is.Not.Empty, encounter.id);
                Assert.That(encounter.minFloor, Is.EqualTo(1), encounter.id);
                Assert.That(encounter.maxFloor, Is.EqualTo(30), encounter.id);
                Assert.That(encounter.choices, Has.Count.EqualTo(2), encounter.id);
            }
        }
    }
}
