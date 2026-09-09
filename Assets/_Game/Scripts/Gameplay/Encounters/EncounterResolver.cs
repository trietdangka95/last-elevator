using System;
using System.Collections.Generic;
using LastElevator.Core.Random;
using LastElevator.Core.State;
using LastElevator.Gameplay.Run;

namespace LastElevator.Gameplay.Encounters
{
    public sealed class EncounterResolver
    {
        private readonly IRandomService _random;

        public EncounterResolver(IRandomService random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public bool CanChoose(RunState state, EncounterChoiceData choice)
        {
            return EvaluateConditions(state, choice) == EncounterResolutionStatus.Resolved;
        }

        public EncounterResolution Resolve(
            RunState state,
            EncounterDefinition encounter,
            int choiceIndex)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (encounter == null || encounter.choices == null)
            {
                return CreateFailure(EncounterResolutionStatus.InvalidEncounter, encounter, choiceIndex);
            }

            if (choiceIndex < 0 || choiceIndex >= encounter.choices.Count ||
                encounter.choices[choiceIndex] == null)
            {
                return CreateFailure(EncounterResolutionStatus.InvalidChoice, encounter, choiceIndex);
            }

            EncounterChoiceData choice = encounter.choices[choiceIndex];
            EncounterResolutionStatus conditionStatus = EvaluateConditions(state, choice);

            if (conditionStatus != EncounterResolutionStatus.Resolved)
            {
                return CreateFailure(conditionStatus, encounter, choiceIndex);
            }

            EncounterResolutionStatus effectStatus = ValidateEffects(choice.successEffects);

            if (effectStatus == EncounterResolutionStatus.Resolved)
            {
                effectStatus = ValidateEffects(choice.failureEffects);
            }

            if (effectStatus != EncounterResolutionStatus.Resolved)
            {
                return CreateFailure(effectStatus, encounter, choiceIndex);
            }

            bool succeeded = RollSuccess(choice.successChance);
            ApplyEffects(state, succeeded ? choice.successEffects : choice.failureEffects);
            RecordResolvedEncounter(state, encounter.id);

            return new EncounterResolution(
                EncounterResolutionStatus.Resolved,
                encounter.id,
                choiceIndex,
                succeeded);
        }

        private static EncounterResolutionStatus EvaluateConditions(
            RunState state,
            EncounterChoiceData choice)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (choice == null)
            {
                return EncounterResolutionStatus.InvalidChoice;
            }

            IReadOnlyList<ConditionData> conditions = choice.conditions;

            if (conditions == null)
            {
                return EncounterResolutionStatus.Resolved;
            }

            for (int i = 0; i < conditions.Count; i++)
            {
                ConditionData condition = conditions[i];

                if (condition == null)
                {
                    return EncounterResolutionStatus.UnsupportedCondition;
                }

                switch (condition.type)
                {
                    case ConditionType.None:
                        break;
                    case ConditionType.MinEnergy:
                        if (state.energy < condition.intValue)
                        {
                            return EncounterResolutionStatus.ConditionsNotMet;
                        }

                        break;
                    case ConditionType.MinIntegrity:
                        if (state.integrity < condition.intValue)
                        {
                            return EncounterResolutionStatus.ConditionsNotMet;
                        }

                        break;
                    case ConditionType.MinScrap:
                        if (state.scrap < condition.intValue)
                        {
                            return EncounterResolutionStatus.ConditionsNotMet;
                        }

                        break;
                    case ConditionType.HasFreeCapacity:
                        if (RunRules.GetAvailableCapacity(state) <= 0)
                        {
                            return EncounterResolutionStatus.ConditionsNotMet;
                        }

                        break;
                    case ConditionType.MinTeamPower:
                    case ConditionType.HasRole:
                    default:
                        return EncounterResolutionStatus.UnsupportedCondition;
                }
            }

            return EncounterResolutionStatus.Resolved;
        }

        private static EncounterResolutionStatus ValidateEffects(IReadOnlyList<EffectData> effects)
        {
            if (effects == null)
            {
                return EncounterResolutionStatus.Resolved;
            }

            for (int i = 0; i < effects.Count; i++)
            {
                EffectData effect = effects[i];

                if (effect == null)
                {
                    return EncounterResolutionStatus.InvalidEffect;
                }

                switch (effect.type)
                {
                    case EffectType.None:
                    case EffectType.AddEnergy:
                    case EffectType.AddScrap:
                        break;
                    case EffectType.AddIntegrity:
                    case EffectType.DamageIntegrity:
                        if (effect.intValue < 0)
                        {
                            return EncounterResolutionStatus.InvalidEffect;
                        }

                        break;
                    case EffectType.SetRunFlag:
                        if (string.IsNullOrWhiteSpace(effect.stringValue))
                        {
                            return EncounterResolutionStatus.InvalidEffect;
                        }

                        break;
                    case EffectType.AddSurvivor:
                    case EffectType.StartCombat:
                    default:
                        return EncounterResolutionStatus.UnsupportedEffect;
                }
            }

            return EncounterResolutionStatus.Resolved;
        }

        private bool RollSuccess(float successChance)
        {
            if (successChance <= 0f)
            {
                return false;
            }

            if (successChance >= 1f)
            {
                return true;
            }

            return _random.Value() < successChance;
        }

        private static void ApplyEffects(RunState state, IReadOnlyList<EffectData> effects)
        {
            if (effects == null)
            {
                return;
            }

            for (int i = 0; i < effects.Count; i++)
            {
                EffectData effect = effects[i];

                switch (effect.type)
                {
                    case EffectType.None:
                        break;
                    case EffectType.AddEnergy:
                        RunRules.ChangeEnergy(state, effect.intValue);
                        break;
                    case EffectType.AddIntegrity:
                        RunRules.RepairIntegrity(state, effect.intValue);
                        break;
                    case EffectType.AddScrap:
                        RunRules.ChangeScrap(state, effect.intValue);
                        break;
                    case EffectType.DamageIntegrity:
                        RunRules.DamageIntegrity(state, effect.intValue);
                        break;
                    case EffectType.SetRunFlag:
                        if (!state.flags.Contains(effect.stringValue))
                        {
                            state.flags.Add(effect.stringValue);
                        }

                        break;
                }
            }
        }

        private static void RecordResolvedEncounter(RunState state, string encounterId)
        {
            if (!string.IsNullOrWhiteSpace(encounterId) &&
                !state.resolvedEncounterIds.Contains(encounterId))
            {
                state.resolvedEncounterIds.Add(encounterId);
            }
        }

        private static EncounterResolution CreateFailure(
            EncounterResolutionStatus status,
            EncounterDefinition encounter,
            int choiceIndex)
        {
            return new EncounterResolution(status, encounter == null ? null : encounter.id, choiceIndex, false);
        }
    }
}
