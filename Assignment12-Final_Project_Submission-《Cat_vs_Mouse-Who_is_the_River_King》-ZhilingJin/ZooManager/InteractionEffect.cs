namespace ZooManager
{
    public enum InteractionEffect
    {
        None, // Represents no interaction effect
        Attract, // Represents an attraction effect between animals
        Repel, // Represents a repelling effect between animals
        Pursue, // Represents a pursuing effect where one animal pursues another
        Flee, // Represents a fleeing effect where an animal flees from another
        Paralyze, // Represents a paralysis effect that immobilizes an animal
        Intimidate // Represents an intimidation effect that can potentially paralyze an animal
    }
}