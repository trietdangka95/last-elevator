using System.Collections.Generic;
using LastElevator.Core.State;
using LastElevator.Gameplay.Run;
using LastElevator.Gameplay.Survivors;
using NUnit.Framework;
using UnityEngine;

namespace LastElevator.Tests.EditMode
{
    public sealed class SurvivorRosterTests
    {
        private readonly List<SurvivorDefinition> _definitions = new List<SurvivorDefinition>();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < _definitions.Count; i++)
            {
                Object.DestroyImmediate(_definitions[i]);
            }

            _definitions.Clear();
        }

        [Test]
        public void RecruitStoresDefinitionIdWhenCapacityIsAvailable()
        {
            SurvivorDefinition maya = CreateSurvivor("survivor_maya", SurvivorRole.Medic, 2);
            var roster = new SurvivorRoster(new[] { maya });
            RunState state = RunRules.CreateInitialState(101);

            SurvivorRecruitmentStatus status = roster.TryRecruit(state, maya);

            Assert.That(status, Is.EqualTo(SurvivorRecruitmentStatus.Added));
            Assert.That(state.survivorIds, Is.EqualTo(new[] { "survivor_maya" }));
        }

        [Test]
        public void RecruitAtCapacityRequiresReplacementWithoutOverflow()
        {
            SurvivorDefinition maya = CreateSurvivor("survivor_maya", SurvivorRole.Medic, 2);
            SurvivorDefinition rin = CreateSurvivor("survivor_rin", SurvivorRole.Guard, 3);
            var roster = new SurvivorRoster(new[] { maya, rin });
            RunState state = RunRules.CreateInitialState(202);
            state.capacity = 1;
            state.survivorIds.Add(maya.id);

            SurvivorRecruitmentStatus status = roster.TryRecruit(state, rin);

            Assert.That(status, Is.EqualTo(SurvivorRecruitmentStatus.ReplacementRequired));
            Assert.That(state.survivorIds, Is.EqualTo(new[] { maya.id }));
            Assert.That(state.survivorIds.Count, Is.LessThanOrEqualTo(state.capacity));
        }

        [Test]
        public void ReplaceRemovesSelectedSurvivorAndKeepsRosterAtCapacity()
        {
            SurvivorDefinition maya = CreateSurvivor("survivor_maya", SurvivorRole.Medic, 2);
            SurvivorDefinition rin = CreateSurvivor("survivor_rin", SurvivorRole.Guard, 3);
            var roster = new SurvivorRoster(new[] { maya, rin });
            RunState state = RunRules.CreateInitialState(303);
            state.capacity = 1;
            state.survivorIds.Add(maya.id);

            bool replaced = roster.TryReplace(state, maya.id, rin);

            Assert.That(replaced, Is.True);
            Assert.That(state.survivorIds, Is.EqualTo(new[] { rin.id }));
            Assert.That(state.survivorIds.Count, Is.EqualTo(state.capacity));
        }

        [Test]
        public void InvalidReplacementDoesNotMutateRoster()
        {
            SurvivorDefinition maya = CreateSurvivor("survivor_maya", SurvivorRole.Medic, 2);
            SurvivorDefinition rin = CreateSurvivor("survivor_rin", SurvivorRole.Guard, 3);
            var roster = new SurvivorRoster(new[] { maya, rin });
            RunState state = RunRules.CreateInitialState(404);
            state.capacity = 1;
            state.survivorIds.Add(maya.id);

            bool replaced = roster.TryReplace(state, "survivor_missing", rin);

            Assert.That(replaced, Is.False);
            Assert.That(state.survivorIds, Is.EqualTo(new[] { maya.id }));
        }

        [Test]
        public void DirectPassiveCalculationsModifyRosterRules()
        {
            SurvivorDefinition maya = CreateSurvivor(
                "survivor_maya",
                SurvivorRole.Medic,
                2,
                SimplePassive.AddCombatPowerOne);
            SurvivorDefinition ken = CreateSurvivor(
                "survivor_ken",
                SurvivorRole.Engineer,
                1,
                SimplePassive.ReduceTravelEnergyOne);
            var roster = new SurvivorRoster(new[] { maya, ken });
            RunState state = RunRules.CreateInitialState(505);
            state.survivorIds.Add(maya.id);
            state.survivorIds.Add(ken.id);

            Assert.That(roster.GetTotalPower(state), Is.EqualTo(4));
            Assert.That(roster.GetTravelEnergyReduction(state), Is.EqualTo(1));
            Assert.That(
                RunRules.GetTravelEnergyCost(state, 2, roster.GetTravelEnergyReduction(state)),
                Is.EqualTo(3));
            Assert.That(roster.HasRole(state, SurvivorRole.Medic), Is.True);
            Assert.That(roster.HasRole(state, SurvivorRole.Guard), Is.False);
        }

        private SurvivorDefinition CreateSurvivor(
            string id,
            SurvivorRole role,
            int combatPower,
            SimplePassive passive = SimplePassive.None)
        {
            SurvivorDefinition definition = ScriptableObject.CreateInstance<SurvivorDefinition>();
            definition.id = id;
            definition.displayName = id;
            definition.role = role;
            definition.combatPower = combatPower;
            definition.passive = passive;
            _definitions.Add(definition);
            return definition;
        }
    }
}
