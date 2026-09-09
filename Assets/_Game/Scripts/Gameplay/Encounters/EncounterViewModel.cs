using System;
using System.Collections.Generic;
using LastElevator.Core.State;

namespace LastElevator.Gameplay.Encounters
{
    public sealed class EncounterViewModel
    {
        internal EncounterViewModel(
            EncounterDefinition definition,
            RunState state,
            EncounterResolver resolver)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (resolver == null)
            {
                throw new ArgumentNullException(nameof(resolver));
            }

            Id = definition.id;
            Title = definition.title;
            Body = definition.body;
            Category = definition.category;

            var choices = new List<EncounterChoiceViewModel>();
            if (definition.choices != null)
            {
                for (int i = 0; i < definition.choices.Count; i++)
                {
                    EncounterChoiceData choice = definition.choices[i];
                    choices.Add(new EncounterChoiceViewModel(
                        i,
                        choice == null ? "Unavailable choice" : choice.label,
                        resolver.CanChoose(state, choice)));
                }
            }

            Choices = choices.AsReadOnly();
        }

        public string Id { get; }

        public string Title { get; }

        public string Body { get; }

        public EncounterCategory Category { get; }

        public IReadOnlyList<EncounterChoiceViewModel> Choices { get; }
    }
}
