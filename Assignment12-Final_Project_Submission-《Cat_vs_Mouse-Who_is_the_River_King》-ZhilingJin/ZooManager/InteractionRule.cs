using System;

namespace ZooManager
{
    public class InteractionRule
    {
        public InteractionEffect Effect { get; set; } // The interaction effect to be applied

        public Func<Animal, Animal, bool> Condition { get; set; } // A delegate that represents the condition for applying the interaction effect

        public bool ShouldApply(Animal source, Animal target)
        {
            return Condition(source, target); // Evaluates the condition delegate with the source and target animals to determine if the interaction effect should be applied
        }
    }
}