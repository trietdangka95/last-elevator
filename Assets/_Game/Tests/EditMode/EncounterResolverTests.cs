using System;
using System.Collections.Generic;
using LastElevator.Core.Random;
using LastElevator.Core.State;
using LastElevator.Gameplay.Encounters;
using LastElevator.Gameplay.Run;
using LastElevator.Gameplay.Survivors;
using NUnit.Framework;
using UnityEngine;

namespace LastElevator.Tests.EditMode
{
    public sealed class EncounterResolverTests
    {
        private readonly List<EncounterDefinition> _definitions = new List<EncounterDefinition>();
        private readonly List<SurvivorDefinition> _survivors = new List<SurvivorDefinition>();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < _definitions.Count; i++)
            {
                UnityEngine.Object.DestroyImmediate(_definitions[i]);
            }

            _definitions.Clear();

            for (int i = 0; i < _survivors.Count; i++)
            {
                UnityEngine.Object.DestroyImmediate(_survivors[i]);
            }

            _survivors.Clear();
        }

        [Test]
        public void ChoiceWithUnmetEnergyConditionDoesNotMutateRun()
        {
            RunState state = RunRules.CreateInitialState(101);
            state.energy = 4;
            EncounterDefinition encounter = CreateEncounter(
                "event_locked_power",
                CreateChoice(
                    1f,
                    new[] { new ConditionData { type = ConditionType.MinEnergy, intValue = 5 } },
                    new[] { new EffectData { type = EffectType.AddScrap, intValue = 10 } }));
            var resolver = new EncounterResolver(new FixedRandomService(0f));

            EncounterResolution resolution = resolver.Resolve(state, encounter, 0);

            Assert.That(resolution.Status, Is.EqualTo(EncounterResolutionStatus.ConditionsNotMet));
            Assert.That(state.energy, Is.EqualTo(4));
            Assert.That(state.scrap, Is.Zero);
            Assert.That(state.resolvedEncounterIds, Is.Empty);
        }

        [Test]
        public void SuccessfulResolutionAppliesSupportedEffectsAndRecordsEncounter()
        {
            RunState state = RunRules.CreateInitialState(202);
            state.energy = 70;
            state.integrity = 80;
            state.scrap = 1;
            EncounterDefinition encounter = CreateEncounter(
                "event_maintenance_cache",
                CreateChoice(
                    1f,
                    Array.Empty<ConditionData>(),
                    new[]
                    {
                        new EffectData { type = EffectType.AddEnergy, intValue = 5 },
                        new EffectData { type = EffectType.AddIntegrity, intValue = 10 },
                        new EffectData { type = EffectType.AddScrap, intValue = 4 },
                        new EffectData { type = EffectType.DamageIntegrity, intValue = 3 },
                        new EffectData { type = EffectType.SetRunFlag, stringValue = "cache_opened" }
                    }));
            var resolver = new EncounterResolver(new FixedRandomService(0f));

            EncounterResolution resolution = resolver.Resolve(state, encounter, 0);

            Assert.That(resolution.Status, Is.EqualTo(EncounterResolutionStatus.Resolved));
            Assert.That(resolution.Succeeded, Is.True);
            Assert.That(state.energy, Is.EqualTo(75));
            Assert.That(state.integrity, Is.EqualTo(87));
            Assert.That(state.scrap, Is.EqualTo(5));
            Assert.That(state.flags, Is.EqualTo(new[] { "cache_opened" }));
            Assert.That(state.resolvedEncounterIds, Is.EqualTo(new[] { "event_maintenance_cache" }));
        }

        [Test]
        public void FailedRollAppliesOnlyFailureEffects()
        {
            RunState state = RunRules.CreateInitialState(303);
            EncounterDefinition encounter = CreateEncounter(
                "event_unstable_signal",
                new EncounterChoiceData
                {
                    label = "Answer",
                    successChance = 0.5f,
                    successEffects = new List<EffectData>
                    {
                        new EffectData { type = EffectType.AddScrap, intValue = 9 }
                    },
                    failureEffects = new List<EffectData>
                    {
                        new EffectData { type = EffectType.DamageIntegrity, intValue = 12 }
                    }
                });
            var resolver = new EncounterResolver(new FixedRandomService(0.75f));

            EncounterResolution resolution = resolver.Resolve(state, encounter, 0);

            Assert.That(resolution.Status, Is.EqualTo(EncounterResolutionStatus.Resolved));
            Assert.That(resolution.Succeeded, Is.False);
            Assert.That(state.scrap, Is.Zero);
            Assert.That(state.integrity, Is.EqualTo(88));
        }

        [Test]
        public void UnsupportedCombatEffectFailsWithoutPartialMutation()
        {
            RunState state = RunRules.CreateInitialState(404);
            EncounterDefinition encounter = CreateEncounter(
                "event_future_combat",
                CreateChoice(
                    1f,
                    Array.Empty<ConditionData>(),
                    new[]
                    {
                        new EffectData { type = EffectType.AddScrap, intValue = 5 },
                        new EffectData { type = EffectType.StartCombat }
                    }));
            var resolver = new EncounterResolver(new FixedRandomService(0f));

            EncounterResolution resolution = resolver.Resolve(state, encounter, 0);

            Assert.That(resolution.Status, Is.EqualTo(EncounterResolutionStatus.UnsupportedEffect));
            Assert.That(state.scrap, Is.Zero);
            Assert.That(state.survivorIds, Is.Empty);
            Assert.That(state.resolvedEncounterIds, Is.Empty);
        }

        [Test]
        public void InvalidRoleConditionCannotBeChosen()
        {
            RunState state = RunRules.CreateInitialState(505);
            EncounterDefinition encounter = CreateEncounter(
                "event_invalid_role",
                CreateChoice(
                    1f,
                    new[] { new ConditionData { type = ConditionType.HasRole, stringValue = "NotARole" } },
                    new[] { new EffectData { type = EffectType.AddEnergy, intValue = 10 } }));
            SurvivorDefinition maya = CreateSurvivor(
                "survivor_maya",
                SurvivorRole.Medic,
                2,
                SimplePassive.AddCombatPowerOne);
            var roster = new SurvivorRoster(new[] { maya });
            var resolver = new EncounterResolver(new FixedRandomService(0f), roster);

            EncounterResolution resolution = resolver.Resolve(state, encounter, 0);

            Assert.That(resolution.Status, Is.EqualTo(EncounterResolutionStatus.UnsupportedCondition));
            Assert.That(state.energy, Is.EqualTo(75));
        }

        [Test]
        public void SurvivorConditionsUseRosterRoleAndTotalPower()
        {
            SurvivorDefinition maya = CreateSurvivor(
                "survivor_maya",
                SurvivorRole.Medic,
                2,
                SimplePassive.AddCombatPowerOne);
            var roster = new SurvivorRoster(new[] { maya });
            RunState state = RunRules.CreateInitialState(506);
            state.survivorIds.Add(maya.id);
            EncounterDefinition encounter = CreateEncounter(
                "event_medic_access",
                CreateChoice(
                    1f,
                    new[]
                    {
                        new ConditionData { type = ConditionType.HasRole, stringValue = "Medic" },
                        new ConditionData { type = ConditionType.MinTeamPower, intValue = 3 }
                    },
                    new[] { new EffectData { type = EffectType.AddScrap, intValue = 2 } }));
            var resolver = new EncounterResolver(new FixedRandomService(0f), roster);

            EncounterResolution resolution = resolver.Resolve(state, encounter, 0);

            Assert.That(resolution.Status, Is.EqualTo(EncounterResolutionStatus.Resolved));
            Assert.That(state.scrap, Is.EqualTo(2));
        }

        [Test]
        public void AddSurvivorEffectRecruitsWhenCapacityIsAvailable()
        {
            SurvivorDefinition maya = CreateSurvivor("survivor_maya", SurvivorRole.Medic, 2);
            var roster = new SurvivorRoster(new[] { maya });
            RunState state = RunRules.CreateInitialState(507);
            EncounterDefinition encounter = CreateEncounter(
                "event_rescue_maya",
                CreateChoice(
                    1f,
                    Array.Empty<ConditionData>(),
                    new[]
                    {
                        new EffectData { type = EffectType.AddSurvivor, survivor = maya },
                        new EffectData { type = EffectType.AddScrap, intValue = 2 }
                    }));
            var resolver = new EncounterResolver(new FixedRandomService(0f), roster);

            EncounterResolution resolution = resolver.Resolve(state, encounter, 0);

            Assert.That(resolution.Status, Is.EqualTo(EncounterResolutionStatus.Resolved));
            Assert.That(state.survivorIds, Is.EqualTo(new[] { maya.id }));
            Assert.That(state.scrap, Is.EqualTo(2));
            Assert.That(state.resolvedEncounterIds, Is.EqualTo(new[] { encounter.id }));
        }

        [Test]
        public void AddSurvivorAtCapacityDefersEveryEffectUntilReplacement()
        {
            SurvivorDefinition maya = CreateSurvivor("survivor_maya", SurvivorRole.Medic, 2);
            SurvivorDefinition rin = CreateSurvivor("survivor_rin", SurvivorRole.Guard, 3);
            var roster = new SurvivorRoster(new[] { maya, rin });
            RunState state = RunRules.CreateInitialState(508);
            state.capacity = 1;
            state.survivorIds.Add(maya.id);
            EncounterDefinition encounter = CreateEncounter(
                "event_rescue_rin",
                CreateChoice(
                    1f,
                    Array.Empty<ConditionData>(),
                    new[]
                    {
                        new EffectData { type = EffectType.AddScrap, intValue = 2 },
                        new EffectData { type = EffectType.AddSurvivor, survivor = rin }
                    }));
            var resolver = new EncounterResolver(new FixedRandomService(0f), roster);

            EncounterResolution resolution = resolver.Resolve(state, encounter, 0);

            Assert.That(
                resolution.Status,
                Is.EqualTo(EncounterResolutionStatus.SurvivorReplacementRequired));
            Assert.That(resolution.PendingSurvivor, Is.SameAs(rin));
            Assert.That(state.survivorIds, Is.EqualTo(new[] { maya.id }));
            Assert.That(state.scrap, Is.Zero);
            Assert.That(state.resolvedEncounterIds, Is.Empty);
        }

        [Test]
        public void CompletingReplacementRemovesSelectedSurvivorAndFinalizesEffects()
        {
            SurvivorDefinition maya = CreateSurvivor("survivor_maya", SurvivorRole.Medic, 2);
            SurvivorDefinition rin = CreateSurvivor("survivor_rin", SurvivorRole.Guard, 3);
            var roster = new SurvivorRoster(new[] { maya, rin });
            RunState state = RunRules.CreateInitialState(509);
            state.capacity = 1;
            state.survivorIds.Add(maya.id);
            EncounterDefinition encounter = CreateEncounter(
                "event_rescue_rin",
                CreateChoice(
                    1f,
                    Array.Empty<ConditionData>(),
                    new[]
                    {
                        new EffectData { type = EffectType.AddSurvivor, survivor = rin },
                        new EffectData { type = EffectType.AddScrap, intValue = 2 }
                    }));
            var resolver = new EncounterResolver(new FixedRandomService(0f), roster);
            EncounterResolution pending = resolver.Resolve(state, encounter, 0);

            EncounterResolution completed = resolver.CompleteSurvivorReplacement(
                state,
                pending,
                maya.id);

            Assert.That(completed.Status, Is.EqualTo(EncounterResolutionStatus.Resolved));
            Assert.That(state.survivorIds, Is.EqualTo(new[] { rin.id }));
            Assert.That(state.survivorIds.Count, Is.EqualTo(state.capacity));
            Assert.That(state.scrap, Is.EqualTo(2));
            Assert.That(state.resolvedEncounterIds, Is.EqualTo(new[] { encounter.id }));
        }

        [Test]
        public void RefusingPendingSurvivorKeepsRosterAndFinalizesOtherEffects()
        {
            SurvivorDefinition maya = CreateSurvivor("survivor_maya", SurvivorRole.Medic, 2);
            SurvivorDefinition rin = CreateSurvivor("survivor_rin", SurvivorRole.Guard, 3);
            var roster = new SurvivorRoster(new[] { maya, rin });
            RunState state = RunRules.CreateInitialState(510);
            state.capacity = 1;
            state.survivorIds.Add(maya.id);
            EncounterDefinition encounter = CreateEncounter(
                "event_rescue_rin",
                CreateChoice(
                    1f,
                    Array.Empty<ConditionData>(),
                    new[]
                    {
                        new EffectData { type = EffectType.AddSurvivor, survivor = rin },
                        new EffectData { type = EffectType.AddScrap, intValue = 2 }
                    }));
            var resolver = new EncounterResolver(new FixedRandomService(0f), roster);
            EncounterResolution pending = resolver.Resolve(state, encounter, 0);

            EncounterResolution completed = resolver.CompleteSurvivorReplacement(state, pending, null);

            Assert.That(completed.Status, Is.EqualTo(EncounterResolutionStatus.Resolved));
            Assert.That(state.survivorIds, Is.EqualTo(new[] { maya.id }));
            Assert.That(state.scrap, Is.EqualTo(2));
            Assert.That(state.resolvedEncounterIds, Is.EqualTo(new[] { encounter.id }));
        }

        [Test]
        public void InvalidChoiceIndexDoesNotMutateRun()
        {
            RunState state = RunRules.CreateInitialState(606);
            EncounterDefinition encounter = CreateEncounter(
                "event_single_choice",
                CreateChoice(
                    1f,
                    Array.Empty<ConditionData>(),
                    new[] { new EffectData { type = EffectType.AddScrap, intValue = 10 } }));
            var resolver = new EncounterResolver(new FixedRandomService(0f));

            EncounterResolution resolution = resolver.Resolve(state, encounter, 2);

            Assert.That(resolution.Status, Is.EqualTo(EncounterResolutionStatus.InvalidChoice));
            Assert.That(state.scrap, Is.Zero);
        }

        [Test]
        public void SeededResolversProduceTheSameOutcomeSequence()
        {
            EncounterDefinition encounter = CreateEncounter(
                "event_seeded",
                CreateChoice(
                    0.5f,
                    Array.Empty<ConditionData>(),
                    new[] { new EffectData { type = EffectType.AddScrap, intValue = 1 } },
                    new[] { new EffectData { type = EffectType.DamageIntegrity, intValue = 1 } }));
            var first = new EncounterResolver(new SeededRandomService(18492301));
            var second = new EncounterResolver(new SeededRandomService(18492301));
            RunState firstState = RunRules.CreateInitialState(18492301);
            RunState secondState = RunRules.CreateInitialState(18492301);

            for (int i = 0; i < 8; i++)
            {
                bool firstSucceeded = first.Resolve(firstState, encounter, 0).Succeeded;
                bool secondSucceeded = second.Resolve(secondState, encounter, 0).Succeeded;

                Assert.That(firstSucceeded, Is.EqualTo(secondSucceeded));
            }
        }

        private EncounterDefinition CreateEncounter(string id, EncounterChoiceData choice)
        {
            EncounterDefinition encounter = ScriptableObject.CreateInstance<EncounterDefinition>();
            encounter.id = id;
            encounter.choices = new List<EncounterChoiceData> { choice };
            _definitions.Add(encounter);
            return encounter;
        }

        private SurvivorDefinition CreateSurvivor(
            string id,
            SurvivorRole role,
            int combatPower,
            SimplePassive passive = SimplePassive.None)
        {
            SurvivorDefinition survivor = ScriptableObject.CreateInstance<SurvivorDefinition>();
            survivor.id = id;
            survivor.displayName = id;
            survivor.role = role;
            survivor.combatPower = combatPower;
            survivor.passive = passive;
            _survivors.Add(survivor);
            return survivor;
        }

        private static EncounterChoiceData CreateChoice(
            float successChance,
            IReadOnlyList<ConditionData> conditions,
            IReadOnlyList<EffectData> successEffects,
            IReadOnlyList<EffectData> failureEffects = null)
        {
            return new EncounterChoiceData
            {
                label = "Choose",
                successChance = successChance,
                conditions = new List<ConditionData>(conditions),
                successEffects = new List<EffectData>(successEffects),
                failureEffects = failureEffects == null
                    ? new List<EffectData>()
                    : new List<EffectData>(failureEffects)
            };
        }

        private sealed class FixedRandomService : IRandomService
        {
            private readonly float _value;

            public FixedRandomService(float value)
            {
                _value = value;
            }

            public int Range(int minInclusive, int maxExclusive)
            {
                return minInclusive;
            }

            public float Value()
            {
                return _value;
            }

            public T PickWeighted<T>(IReadOnlyList<T> items, Func<T, int> getWeight)
            {
                return items[0];
            }
        }
    }
}
