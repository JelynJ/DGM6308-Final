using System;
using System.Collections.Generic;

namespace ZooManager
{
    public class BoardManager
    {
        // Number of cells in the X direction (horizontal)
        public int NumCellsX { get; private set; }
        // Number of cells in the Y direction (vertical)
        public int NumCellsY { get; private set; }
        // Column index of the river
        public int RiverColumn { get; private set; }
        // 2D list of zones representing the game board
        public List<List<Zone>> AnimalZones { get; private set; }

        // Constructor to initialize the AnimalZones list
        public BoardManager()
        {
            AnimalZones = new List<List<Zone>>();
        }

        // Method to initialize the board with a given size
        public void InitializeBoard(int size)
        {
            NumCellsX = size;
            NumCellsY = size;
            RiverColumn = NumCellsX / 2; // River is in the middle column

            AnimalZones.Clear();
            for (var y = 0; y < NumCellsY; y++)
            {
                List<Zone> rowList = new List<Zone>();
                for (var x = 0; x < NumCellsX; x++)
                {
                    bool isRiver = (x == RiverColumn); // Check if the current cell is in the river column
                    rowList.Add(new Zone(x, y, null, isRiver)); // Add new Zone to the row list
                }
                AnimalZones.Add(rowList); // Add row list to AnimalZones
            }
        }

        // Method to check if a point is within the boundaries of the board
        private bool IsWithinBoard(Point point)
        {
            return point.x >= 0 && point.x < NumCellsX && point.y >= 0 && point.y < NumCellsY;
        }

        // Method to check if a movement crosses the river
        public bool IsCrossingRiver(Point start, Point end)
        {
            return (start.x <= RiverColumn && end.x > RiverColumn) || (start.x > RiverColumn && end.x <= RiverColumn);
        }

        // Method to get a Zone at a specific position
        public Zone GetZone(Point position)
        {
            if (IsWithinBoard(position))
            {
                return AnimalZones[position.y][position.x];
            }
            throw new ArgumentOutOfRangeException(nameof(position), "Position is outside the board.");
        }

        // Method to update the occupant of a Zone
        public void UpdateZoneOccupant(Point position, Animal animal)
        {
            if (!IsWithinBoard(position))
            {
                throw new ArgumentOutOfRangeException(nameof(position), "Position is outside the board.");
            }

            Zone zone = AnimalZones[position.y][position.x];

            if (zone.occupant != null)
            {
                // If an animal is being replaced, log the replacement
                if (animal != null)
                {
                    Console.WriteLine($"Replacing {zone.occupant.species} with {animal.species} at position ({position.x}, {position.y})");
                }
                else
                {
                    // If an animal is being removed, log the removal
                    Console.WriteLine($"Removing {zone.occupant.species} from position ({position.x}, {position.y})");
                }

                // Clear the old animal's location
                zone.occupant.location = new Point { x = -1, y = -1 };
            }
            else if (animal != null)
            {
                // Log the placement of a new animal
                Console.WriteLine($"Placing {animal.species} at position ({position.x}, {position.y})");
            }

            // Update the zone's occupant and the animal's location
            zone.occupant = animal;
            if (animal != null)
            {
                animal.location = position;
            }
        }

        // Method to get the column index of the river
        public int GetRiverColumn()
        {
            return RiverColumn;
        }

        // Method to get the size of the board
        public (int, int) GetBoardSize()
        {
            return (NumCellsX, NumCellsY);
        }
    }
}