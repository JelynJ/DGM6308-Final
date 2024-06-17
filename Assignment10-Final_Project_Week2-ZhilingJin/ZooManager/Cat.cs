using System;

namespace ZooManager
{
    public class Cat : Animal
    {
        public Cat(string name) : base(name)
        {
            emoji = "🐱";
            species = "cat";
            animalType = "cat";
        }
    }
}