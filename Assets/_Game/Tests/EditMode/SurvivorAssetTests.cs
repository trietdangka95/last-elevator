using System.Linq;
using LastElevator.Gameplay.Encounters;
using LastElevator.Gameplay.Survivors;
using NUnit.Framework;
using UnityEditor;

namespace LastElevator.Tests.EditMode
{
    public sealed class SurvivorAssetTests
    {
        private static readonly string[] ExpectedSurvivorIds =
        {
            "survivor_maya_medic",
            "survivor_ken_engineer",
            "survivor_rin_guard",
            "survivor_tom_civilian"
        };

        [Test]
        public void M1DaySixProvidesFourValidPlaceholderSurvivors()
        {
            SurvivorDefinition[] survivors = AssetDatabase
                .FindAssets("t:SurvivorDefinition", new[] { "Assets/_Game/Data/Survivors" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<SurvivorDefinition>)
                .ToArray();

            Assert.That(survivors.Select(survivor => survivor.id), Is.EquivalentTo(ExpectedSurvivorIds));

            for (int i = 0; i < survivors.Length; i++)
            {
                SurvivorDefinition survivor = survivors[i];
                Assert.That(survivor.displayName, Is.Not.Empty, survivor.id);
                Assert.That(survivor.combatPower, Is.InRange(0, 5), survivor.id);
            }
        }

        [Test]
        public void KnockingDoorEncounterRecruitsARegisteredSurvivor()
        {
            string encounterPath = AssetDatabase.GUIDToAssetPath(
                AssetDatabase.FindAssets("event_knocking_door t:EncounterDefinition").Single());
            EncounterDefinition encounter = AssetDatabase.LoadAssetAtPath<EncounterDefinition>(encounterPath);
            SurvivorDefinition[] survivors = AssetDatabase
                .FindAssets("t:SurvivorDefinition", new[] { "Assets/_Game/Data/Survivors" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<SurvivorDefinition>)
                .ToArray();

            Assert.That(encounter.category, Is.EqualTo(EncounterCategory.Survivor));
            Assert.That(encounter.choices, Has.Count.EqualTo(2));
            EffectData recruitEffect = encounter.choices[0].successEffects.Single(
                effect => effect.type == EffectType.AddSurvivor);
            Assert.That(recruitEffect.survivor, Is.Not.Null);
            Assert.That(survivors, Does.Contain(recruitEffect.survivor));
        }
    }
}
