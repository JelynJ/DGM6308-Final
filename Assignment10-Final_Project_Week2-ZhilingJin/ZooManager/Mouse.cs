using System;
using ZooManager;

public class Mouse : Rodent
{
    public Mouse(string name) : base(name)
    {
        emoji = "🐁";
        species = "mouse";
        attack = 3;
        health = 1;
        ApplyEffect(typeof(Leopard), Effect.Flee);
        ApplyEffect(typeof(Tiger), Effect.Flee);
        ApplyEffect(typeof(Squirrel), Effect.Attract);
    }

    public override void TakeDamage(int damage)
    {
        health = Math.Max(health - damage, 1);
    }

    public override void Move(Game game)
    {
        base.Move(game);
        foreach (var item in effects)
        {
            if (item.Value == Effect.Flee && IsNear(game, item.Key, 2))
            {
                Flee(game, item.Key, 2);
            }
            else if (item.Value == Effect.Attract && IsNear(game, item.Key, 1))
            {
                MoveTowards(game, item.Key, 1);
            }
        }
    }
}