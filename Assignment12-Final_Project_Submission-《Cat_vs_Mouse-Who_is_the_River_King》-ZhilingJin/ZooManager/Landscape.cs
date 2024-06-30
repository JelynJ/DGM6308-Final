using System;

namespace ZooManager
{
    public class Landscape : Occupant
    {
        // This class represents a landscape element on the game board.
        // It inherits from the Occupant class, which provides common properties and methods for objects occupying cells on the board.

        // Properties inherited from Occupant:
        // public string emoji; // A string representing the emoji or icon for this landscape element
        // public string species; // A string representing the species or type of this landscape element (e.g., "Tree", "Rock", etc.)
        // public Point location; // A Point structure representing the location of this landscape element on the board
        // public Player Owner { get; set; } // The player who owns this landscape element (if applicable)

        // Method inherited from Occupant:
        // public virtual void ReportLocation()
        // {
        //     Console.WriteLine($"I am at {location.x},{location.y}"); // Prints the current location of this landscape element
        // }

        // No additional members or functionality are defined in this class.
    }
}