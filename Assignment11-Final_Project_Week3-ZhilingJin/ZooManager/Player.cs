using System.Collections.Generic;
using System.Linq;

namespace ZooManager
{
    public class Player
    {
        public int Id { get; private set; }
        public List<string> AssignedAnimalTypes { get; private set; }
        public List<Animal> Pieces { get; private set; }
        public Animal King { get; set; }
        public int ExchangeCount { get; set; } = 0;
        public List<Animal> DeadPieces { get; } = new List<Animal>();

        public Player(int id)
        {
            Id = id;
            AssignedAnimalTypes = new List<string>();
            Pieces = new List<Animal>();
        }

        public void AssignAnimalType(string animalType)
        {
            if (AssignedAnimalTypes.Count < 2 || animalType == "CatKing" || animalType == "MouseKing")
            {
                AssignedAnimalTypes.Add(animalType);
            }
        }

        public void AddPiece(Animal animal)
        {
            Pieces.Add(animal);
            animal.Owner = this;
            if (animal is CatKing || animal is MouseKing)
            {
                King = animal;
            }
        }

        public void RemovePiece(Animal animal)
        {
            Pieces.Remove(animal);
            DeadPieces.Add(animal);
            if (animal == King)
            {
                King = null;
            }
        }

        public bool AllPiecesMovedOrAttacked()
        {
            return Pieces.All(piece => !piece.CanMoveAndAttackThisTurn);
        }
    }
}