using System;
using System.Linq;
using ZooManager;

public class CatKing : Cat
{
    public CatKing(string name) : base(name)
    {
        emoji = "🐱";
        species = "catKing";
        attack = 0;
        health = 30;
        isKing = true;
        ApplyEffect(typeof(Leopard), Effect.Attract);
        ApplyEffect(typeof(Tiger), Effect.Attract);
        ApplyEffect(typeof(Mouse), Effect.Intimidate);
        ApplyEffect(typeof(Squirrel), Effect.Intimidate);
    }

    public void Gather(Game game, Animal ally)
    {
        var nearbyPositions = game.GetAdjacentPositions(this);
        var validPositions = nearbyPositions.Where(game.IsValidPosition);
        if (validPositions.Any())
        {
            var position = validPositions.First();
            game.Move(ally, position);
            ally.health = Math.Min(ally.health + 2, game.GetMaxHealth(ally));
        }
    }

    public override void Move(Game game)
    {
        // King cannot move
    }

    public override void AttackSpecies(Game game, Animal target)
    {
        // King cannot attack
    }
}