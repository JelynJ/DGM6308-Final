using System;

namespace ZooManager
{
    public class Leopard : Animal
    {
        public bool CanCrossRiverFreely { get; private set; } = false; // Indicates if the leopard can cross the river freely

        public Leopard(string name, Faction faction) : base(name, "🐆", "leopard", 4, 3, faction)
        {
            MaxMoveDistance = 2; // Set the maximum move distance for the leopard
        }

        public override bool ShouldEndTurnAfterSpecialAbility()
        {
            return false; // Leopard can continue moving after using its special ability
        }

        public override bool UseSpecialAbility()
        {
            if (base.UseSpecialAbility())
            {
                CanCrossRiverFreely = true; // Enable the ability to cross the river freely
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
                    UseSpecialAbility(); // Use the special ability to cross the river
                    this.CanMoveAndAttackThisTurn = true; // Allow the leopard to continue moving after crossing the river
                    Console.WriteLine($"{this.name} used its special ability to cross the river and can continue moving!");
                }
                else if (CanCrossRiverFreely)
                {
                    this.CanMoveAndAttackThisTurn = false; // Cannot move again after crossing the river normally
                    Console.WriteLine($"{this.name} crossed the river normally and cannot move again this turn.");
                }
                else
                {
                    Console.WriteLine($"{this.name} cannot cross the river.");
                    return false;
                }
            }
            return base.Move(destination); // Call the base Move method
        }

        public override InteractionEffect GetInteractionEffect(Animal target)
        {
            if (target is Tiger)
                return InteractionEffect.Repel; // Repel tigers
            if (target is Mouse || target is Squirrel)
                return InteractionEffect.Pursue; // Pursue mice and squirrels
            return InteractionEffect.None; // No interaction effect for other animals
        }
    }

    public class Tiger : Animal
    {
        public Tiger(string name, Faction faction) : base(name, "🐅", "tiger", 4, 4, faction)
        {
            MaxMoveDistance = 2; // Set the maximum move distance for the tiger
        }

        public override bool UseSpecialAbility()
        {
            if (base.UseSpecialAbility())
            {
                Console.WriteLine($"{this.name} is preparing to use the roar ability!");
                return Game.StartTigerAbility(this); // Use the roar ability
            }
            return false;
        }

        public override InteractionEffect GetInteractionEffect(Animal target)
        {
            if (target is Leopard)
                return InteractionEffect.Repel; // Repel leopards
            if (target is Mouse || target is Squirrel)
                return InteractionEffect.Pursue; // Pursue mice and squirrels
            return InteractionEffect.None; // No interaction effect for other animals
        }
    }

    public class Mouse : Animal
    {
        private int survivalAbilityCount = 1; // Number of times the survival ability can be used

        public Mouse(string name, Faction faction) : base(name, "🐁", "mouse", 5, 1, faction)
        {
            MaxMoveDistance = 2; // Set the maximum move distance for the mouse
            HasUsedSpecialAbility = true; // The mouse doesn't have a special ability
        }

        // Modification: Override the TakeDamage method to implement the passive survival ability
        public override void TakeDamage(int damage)
        {
            int potentialNewHealth = this.healthPoints - damage;
            if (potentialNewHealth <= 0 && survivalAbilityCount > 0)
            {
                this.healthPoints = 1; // Set health to 1 when the survival ability is triggered
                survivalAbilityCount--; // Decrement the survival ability count
                Console.WriteLine($"{this.name} triggered the survival ability, remaining uses: {survivalAbilityCount}");
            }
            else
            {
                base.TakeDamage(damage); // Call the base TakeDamage method
            }
        }

        public override InteractionEffect GetInteractionEffect(Animal target)
        {
            if (target is Leopard || target is Tiger)
                return InteractionEffect.Flee; // Flee from leopards and tigers
            if (target is Squirrel)
                return InteractionEffect.Attract; // Attract squirrels
            return InteractionEffect.None; // No interaction effect for other animals
        }
    }

    public class Squirrel : Animal
    {
        public Squirrel(string name, Faction faction) : base(name, "🐿️", "squirrel", 2, 5, faction)
        {
            MaxMoveDistance = 2; // Set the maximum move distance for the squirrel
            AttackRange = 2; // Set the attack range for the squirrel
        }

        public override bool UseSpecialAbility()
        {
            if (base.UseSpecialAbility())
            {
                Console.WriteLine($"{this.name} is preparing to use the throwing ability!");
                Game.StartSquirrelAbility(this); // Use the throwing ability
                return true;
            }
            return false;
        }

        public override InteractionEffect GetInteractionEffect(Animal target)
        {
            if (target is Leopard || target is Tiger)
                return InteractionEffect.Flee; // Flee from leopards and tigers
            if (target is Mouse)
                return InteractionEffect.Attract; // Attract mice
            return InteractionEffect.None; // No interaction effect for other animals
        }
    }

    public class CatKing : Animal
    {
        public CatKing(string name) : base(name, "🐱", "cat king", 0, 50, Faction.Cat)
        {
            MaxMoveDistance = 0; // Set the maximum move distance for the cat king
        }

        public override bool UseSpecialAbility()
        {
            if (base.UseSpecialAbility())
            {
                Console.WriteLine($"{this.name} used the rally ability!");
                // Implement the rally ability logic
                return Game.CatKingRally(this); // Use the rally ability
            }
            return false;
        }

        public override InteractionEffect GetInteractionEffect(Animal target)
        {
            if (target is Leopard || target is Tiger)
                return InteractionEffect.Attract; // Attract leopards and tigers
            if (target is Mouse || target is Squirrel)
                return InteractionEffect.Intimidate; // Intimidate mice and squirrels
            return InteractionEffect.None; // No interaction effect for other animals
        }
    }

    public class MouseKing : Animal
    {
        public MouseKing(string name) : base(name, "🐭", "mouse king", 0, 50, Faction.Mouse)
        {
            MaxMoveDistance = 0; // Set the maximum move distance for the mouse king
        }

        public override bool UseSpecialAbility()
        {
            if (base.UseSpecialAbility())
            {
                Console.WriteLine($"{this.name} used the revive ability!");
                // Implement the revive ability logic
                return Game.MouseKingRevive(this); // Use the revive ability
            }
            return false;
        }

        public override InteractionEffect GetInteractionEffect(Animal target)
        {
            if (target is Leopard || target is Tiger)
                return InteractionEffect.Repel; // Repel leopards and tigers
            if (target is Mouse || target is Squirrel)
                return InteractionEffect.Attract; // Attract mice and squirrels
            return InteractionEffect.None; // No interaction effect for other animals
        }
    }
}