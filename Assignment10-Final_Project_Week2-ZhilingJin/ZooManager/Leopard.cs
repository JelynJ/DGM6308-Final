using ZooManager;

public class Leopard : Cat
{
    public Leopard(string name) : base(name)
    {
        emoji = "🐆";
        species = "leopard";
        attack = 4;
        health = 3;
        ApplyEffect(typeof(Tiger), Effect.Repel);
        ApplyEffect(typeof(Mouse), Effect.Pursue);
        ApplyEffect(typeof(Squirrel), Effect.Pursue);
    }

    public override void Move(Game game)
    {
        // Implement river crossing ability
        if (IsRiver(game, location.x, location.y))
        {
            // Movement logic on the river
        }
        else
        {
            base.Move(game);
        }

        foreach (var item in effects)
        {
            if (item.Value == Effect.Attract && IsNear(game, item.Key, 2))
            {
                // Increase movement distance by 1
                MovementDistance++;
            }
            else if (item.Value == Effect.Repel && IsNear(game, item.Key, 2))
            {
                // Decrease movement distance by 1
                MovementDistance--;
            }
        }
    }

    private bool IsRiver(Game game, int x, int y)
    {
        // Check if the given position is a river
        return game.IsRiver(x, y);
    }

    private bool IsNear(Game game, Animal other, int distance)
    {
        // Check if two pieces are within the specified distance
        return game.GetDistance(this, other) <= distance;
    }
}