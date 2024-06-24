using System;

namespace ZooManager
{
    public class InteractionRule
    {
        public InteractionEffect Effect { get; set; }
        public Func<Animal, Animal, bool> Condition { get; set; }

        public bool ShouldApply(Animal source, Animal target)
        {
            return Condition(source, target);
        }
    }
}