namespace ZooManager
{
    public class Zone
    {
        private Occupant _occupant = null;
        public Occupant occupant
        {
            get { return _occupant; }
            set
            {
                _occupant = value;
                if (_occupant != null)
                {
                    _occupant.location = location;
                }
            }
        }

        public Point location;
        public bool isRiver { get; set; }

        public string emoji
        {
            get
            {
                if (occupant == null) return "";
                return occupant.emoji;
            }
        }

        public Zone(int x, int y, Occupant occupant, bool isRiver = false)
        {
            location.x = x;
            location.y = y;
            this.occupant = occupant;
            this.isRiver = isRiver;
        }
    }
}