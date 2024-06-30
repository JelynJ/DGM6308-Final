using System.Collections.Generic;
using System.Linq;

namespace ZooManager
{
    public class Player
    {
        public int Id { get; private set; } // The unique identifier for this player
        public List<string> AssignedAnimalTypes { get; private set; } // A list of animal types assigned to this player
        public List<Animal> Pieces { get; private set; } // A list of animal pieces owned by this player
        public Animal King { get; set; } // The king animal piece owned by this player
        public int ExchangeCount { get; set; } = 0; // A counter for the number of piece exchanges performed by this player
        public List<Animal> DeadPieces { get; } = new List<Animal>(); // A list of defeated animal pieces owned by this player

        public Player(int id)
        {
            Id = id; // Set the player's unique identifier
            AssignedAnimalTypes = new List<string>(); // Initialize the list of assigned animal types
            Pieces = new List<Animal>(); // Initialize the list of owned animal pieces
        }

        public void AssignAnimalType(string animalType)
        {
            // Assign an animal type to this player if they have fewer than 2 assigned types or if the animal type is CatKing or MouseKing
            if (AssignedAnimalTypes.Count < 2 || animalType == "CatKing" || animalType == "MouseKing")
            {
                AssignedAnimalTypes.Add(animalType);
            }
        }

        public void AddPiece(Animal animal)
        {
            Pieces.Add(animal); // Add the animal piece to the list of owned pieces
            animal.Owner = this; // Set this player as the owner of the animal piece

            // If the added piece is a CatKing or MouseKing, set it as the player's king
            if (animal is CatKing || animal is MouseKing)
            {
                King = animal;
            }
        }

        public void RemovePiece(Animal animal)
        {
            Pieces.Remove(animal); // Remove the animal piece from the list of owned pieces
            DeadPieces.Add(animal); // Add the animal piece to the list of defeated pieces

            // If the removed piece is the king, set the king to null
            if (animal == King)
            {
                King = null;
            }
        }

        public bool AllPiecesMovedOrAttacked()
        {
            // Check if all owned pieces have moved or attacked this turn
            return Pieces.All(piece => !piece.CanMoveAndAttackThisTurn);
        }
    }
}