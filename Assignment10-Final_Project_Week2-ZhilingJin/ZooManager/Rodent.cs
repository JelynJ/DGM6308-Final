using ZooManager;

public abstract class Rodent : Animal
{
    protected Rodent(string name) : base(name)
    {
        animalType = "rodent";
    }

    protected bool IsNear(Game game, Animal other, int distance)
    {
        return game.GetDistance(this, other) <= distance;
    }

    protected void Flee(Game game, Animal predator, int distance)
    {
        var fleeDistance = game.GetDistance(this, predator);
        if (fleeDistance <= distance)
        {
            var direction = game.GetFleeDirection(this, predator);
            var newPosition = game.GetNewPosition(this, direction, distance);
            if (game.IsValidPosition(newPosition))
            {
                game.Move(this, newPosition);
            }
        }
    }

    protected void MoveTowards(Game game, Animal target, int distance)
    {
        var direction = game.GetDirectionTowards(this, target);
        var newPosition = game.GetNewPosition(this, direction, distance);
        if (game.IsValidPosition(newPosition))
        {
            game.Move(this, newPosition);
        }
    }
}