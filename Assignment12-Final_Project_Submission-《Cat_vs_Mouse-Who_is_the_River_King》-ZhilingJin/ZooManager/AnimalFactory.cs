using System;

namespace ZooManager
{
    public class AnimalFactory
    {
        public static Animal CreateAnimal(string animalType, string name, Animal.Faction faction)
        {
            // This method creates an instance of an Animal subclass based on the provided animalType
            switch (animalType)
            {
                case "Leopard": return new Leopard(name, faction); // Create a new Leopard instance
                case "Tiger": return new Tiger(name, faction); // Create a new Tiger instance
                case "Mouse": return new Mouse(name, faction); // Create a new Mouse instance
                case "Squirrel": return new Squirrel(name, faction); // Create a new Squirrel instance
                case "CatKing": return new CatKing(name); // Create a new CatKing instance
                case "MouseKing": return new MouseKing(name); // Create a new MouseKing instance
                default: throw new ArgumentException($"Unknown animal type: {animalType}"); // Throw an exception if the animalType is unknown
            }
        }
    }
}