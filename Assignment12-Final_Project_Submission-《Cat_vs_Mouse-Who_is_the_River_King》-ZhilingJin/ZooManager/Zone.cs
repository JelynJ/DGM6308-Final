namespace ZooManager
{
    public class Zone
    {
        private Occupant _occupant = null; // Private field to store the occupant
        public Occupant occupant
        {
            get { return _occupant; } // Get the occupant
            set
            {
                _occupant = value; // Set the occupant
                if (_occupant != null)
                {
                    _occupant.location = location; // Update the occupant's location if it's not null
                }
            }
        }

        public Point location; // The location of the zone
        public bool isRiver { get; set; } // Indicates whether the zone is a river

        public string emoji
        {
            get
            {
                if (occupant == null) return ""; // Return an empty string if there's no occupant
                return occupant.emoji; // Return the occupant's emoji
            }
        }

        public Zone(int x, int y, Occupant occupant, bool isRiver = false)
        {
            location.x = x; // Set the x-coordinate of the location
            location.y = y; // Set the y-coordinate of the location
            this.occupant = occupant; // Set the occupant
            this.isRiver = isRiver; // Set whether the zone is a river
        }
    }
}