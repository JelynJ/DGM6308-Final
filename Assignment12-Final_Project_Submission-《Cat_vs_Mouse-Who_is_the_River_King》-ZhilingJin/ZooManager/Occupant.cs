using System;

namespace ZooManager
{
    public class Occupant
    {
        public string emoji; // A string representing the emoji or icon for this occupant
        public string species; // A string representing the species of this occupant
        public Point location; // A Point structure representing the location of this occupant on the board

        public Player Owner { get; set; } // The player who owns this occupant

        public virtual void ReportLocation()
        {
            Console.WriteLine($"I am at {location.x},{location.y}"); // Prints the current location of this occupant
        }
    }
}