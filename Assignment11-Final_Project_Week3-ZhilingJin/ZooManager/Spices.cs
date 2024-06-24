using System;

namespace ZooManager
{
    public class Leopard : Animal
    {
        public bool CanCrossRiverFreely { get; private set; } = false;

        public Leopard(string name, Faction faction) : base(name, "🐆", "leopard", 4, 3, faction)
        {
            MaxMoveDistance = 2;
        }

        public override bool ShouldEndTurnAfterSpecialAbility()
        {
            return false; // Leopard can continue moving after using its special ability
        }

        public override bool UseSpecialAbility()
        {
            if (base.UseSpecialAbility())
            {
                CanCrossRiverFreely = true;
                Console.WriteLine($"{this.name} activated the ability to cross the river freely!");
                return true;
            }
            return false;
        }

        public override bool Move(Point destination)
        {
            if (Game.IsCrossingRiver(this.location, destination))
            {
                if (!HasUsedSpecialAbility)
                {
                    UseSpecialAbility();
                    this.CanMoveAndAttackThisTurn = true;
                    Console.WriteLine($"{this.name} used its special ability to cross the river and can continue moving!");
                }
                else if (CanCrossRiverFreely)
                {
                    this.CanMoveAndAttackThisTurn = false;
                    Console.WriteLine($"{this.name} crossed the river normally and cannot move again this turn.");
                }
                else
                {
                    Console.WriteLine($"{this.name} cannot cross the river.");
                    return false;
                }
            }
            return base.Move(destination);
        }

        public override InteractionEffect GetInteractionEffect(Animal target)
        {
            if (target is Tiger)
                return InteractionEffect.Repel;
            if (target is Mouse || target is Squirrel)
                return InteractionEffect.Pursue;
            return InteractionEffect.None;
        }
    }

    public class Tiger : Animal
    {
        public Tiger(string name, Faction faction) : base(name, "🐅", "tiger", 4, 5, faction)
        {
            MaxMoveDistance = 2;
        }

        public override bool UseSpecialAbility()
        {
            if (base.UseSpecialAbility())
            {
                Console.WriteLine($"{this.name} is preparing to use the roar ability!");
                return Game.StartTigerAbility(this);
            }
            return false;
        }

        public override InteractionEffect GetInteractionEffect(Animal target)
        {
            if (target is Leopard)
                return InteractionEffect.Repel;
            if (target is Mouse || target is Squirrel)
                return InteractionEffect.Pursue;
            return InteractionEffect.None;
        }
    }

    public class Mouse : Animal
    {
        private int survivalAbilityCount = 1;
        public Mouse(string name, Faction faction) : base(name, "🐁", "mouse", 3, 1, faction)
        {
            MaxMoveDistance = 2;
        }

        // Modification: Override the TakeDamage method to implement the passive survival ability
        public override void TakeDamage(int damage)
        {
            int potentialNewHealth = this.healthPoints - damage;
            if (potentialNewHealth <= 0 && survivalAbilityCount > 0)
            {
                this.healthPoints = 1;
                survivalAbilityCount--;
                Console.WriteLine($"{this.name} triggered the survival ability, remaining uses: {survivalAbilityCount}");
            }
            else
            {
                base.TakeDamage(damage);
            }
        }

        public override InteractionEffect GetInteractionEffect(Animal target)
        {
            if (target is Leopard || target is Tiger)
                return InteractionEffect.Flee;
            if (target is Squirrel)
                return InteractionEffect.Attract;
            return InteractionEffect.None;
        }
    }

    public class Squirrel : Animal
    {
        public Squirrel(string name, Faction faction) : base(name, "🐿️", "squirrel", 3, 4, faction)
        {
            MaxMoveDistance = 2;
            AttackRange = 2;
        }

        public override bool UseSpecialAbility()
        {
            if (base.UseSpecialAbility())
            {
                Console.WriteLine($"{this.name} is preparing to use the throwing ability!");
                Game.StartSquirrelAbility(this);
                return true;
            }
            return false;
        }

        public override InteractionEffect GetInteractionEffect(Animal target)
        {
            if (target is Leopard || target is Tiger)
                return InteractionEffect.Flee;
            if (target is Mouse)
                return InteractionEffect.Attract;
            return InteractionEffect.None;
        }
    }

    public class CatKing : Animal
    {
        public CatKing(string name) : base(name, "🐱", "cat king", 0, 30, Faction.Cat)
        {
            MaxMoveDistance = 0;
        }

        public override bool UseSpecialAbility()
        {
            if (base.UseSpecialAbility())
            {
                Console.WriteLine($"{this.name} used the rally ability!");
                // Implement the rally ability logic
                return Game.CatKingRally(this);
            }
            return false;
        }

        public override InteractionEffect GetInteractionEffect(Animal target)
        {
            if (target is Leopard || target is Tiger)
                return InteractionEffect.Attract;
            if (target is Mouse || target is Squirrel)
                return InteractionEffect.Intimidate;
            return InteractionEffect.None;
        }
    }

    public class MouseKing : Animal
    {
        public MouseKing(string name) : base(name, "🐭", "mouse king", 0, 30, Faction.Mouse)
        {
            MaxMoveDistance = 0;
        }

        public override bool UseSpecialAbility()
        {
            if (base.UseSpecialAbility())
            {
                Console.WriteLine($"{this.name} used the revive ability!");
                // Implement the revive ability logic
                return Game.MouseKingRevive(this);
            }
            return false;
        }

        public override InteractionEffect GetInteractionEffect(Animal target)
        {
            if (target is Leopard || target is Tiger)
                return InteractionEffect.Repel;
            if (target is Mouse || target is Squirrel)
                return InteractionEffect.Attract;
            return InteractionEffect.None;
        }
    }
}