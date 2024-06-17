using ZooManager;

public class Tiger : Cat
{
    public Tiger(string name) : base(name)
    {
        emoji = "🐅";
        species = "tiger";
        attack = 4;
        health = 5;
        ApplyEffect(typeof(Leopard), Effect.Repel);
        ApplyEffect(typeof(Mouse), Effect.Pursue);
        ApplyEffect(typeof(Squirrel), Effect.Pursue);
    }

    public void Roar(Game game, Animal target)
    {
        if (target.isKing)
        {
            target.TakeDamage(attack);
        }
    }

    public override void Move(Game game)
    {
        base.Move(game);
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

    public override void AttackSpecies(Game game, Animal target)
    {
        base.AttackSpecies(game, target);
        if (target.animalType == "mouse" && IsNear(game, target, 1))
        {
            // Chase the mouse
        }
    }

    private bool IsNear(Game game, Animal other, int distance)
    {
        // Check if two pieces are within the specified distance
        return game.GetDistance(this, other) <= distance;
    }
}