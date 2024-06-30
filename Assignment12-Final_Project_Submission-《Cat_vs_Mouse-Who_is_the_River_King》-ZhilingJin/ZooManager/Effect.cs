namespace ZooManager
{
    // Enum to represent different effects that can be applied to animals in the game
    public enum Effect
    {
        // No effect
        None,
        // Increases the target's movement by 1 for the next turn
        Attract,
        // Decreases the target's movement by 1 for the next turn
        Repel,
        // Allows double damage to be dealt this turn
        Pursue,
        // Moves the target 0-1 spaces away
        Flee,
        // 50% chance to paralyze the target
        Intimidate,
        // Paralyzes the target, preventing it from attacking and reducing its movement by 1 next turn
        Paralyze
    }
}

