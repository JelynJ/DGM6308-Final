using ZooManager;

public class Squirrel : Rodent
{
    public Squirrel(string name) : base(name)
    {
        emoji = "🐿️";
        species = "squirrel";
        attack = 3;
        health = 4;
        ApplyEffect(typeof(Leopard), Effect.Flee);
        ApplyEffect(typeof(Tiger), Effect.Flee);
        ApplyEffect(typeof(Mouse), Effect.Attract);
    }

    public void Throw(Game game, Animal target1, Animal target2)
    {
        target1.TakeDamage(attack);
        target2.TakeDamage(attack);
    }

    public override void Move(Game game)
    {
        base.Move(game);
        foreach (var item in effects)
        {
            if (item.Value == Effect.Flee && IsNear(game, item.Key, 1))
            {
                Flee(game, item.Key, 1);
            }
            else if (item.Value == Effect.Attract && IsNear(game, item.Key, 1))
            {
                MoveTowards(game, item.Key, 1);
            }
        }
    }
}