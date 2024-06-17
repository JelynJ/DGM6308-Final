using ZooManager;

public class MouseKing : Rodent
{
    public MouseKing(string name) : base(name)
    {
        emoji = "🐭";
        species = "mouseKing";
        attack = 0;
        health = 30;
        isKing = true;
        ApplyEffect(typeof(Leopard), Effect.Repel);
        ApplyEffect(typeof(Tiger), Effect.Repel);
        ApplyEffect(typeof(Mouse), Effect.Attract);
        ApplyEffect(typeof(Squirrel), Effect.Attract);
    }

    public void Revive(Game game, Animal ally)
    {
        if (ally.IsDead())
        {
            ally.health = 1;
            var position = game.GetRandomAdjacentPosition(this);
            game.Move(ally, position);
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