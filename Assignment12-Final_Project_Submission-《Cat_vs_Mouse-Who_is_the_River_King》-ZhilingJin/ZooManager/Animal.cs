using System;
using System.Collections.Generic;
using System.Linq;

namespace ZooManager
{
    public abstract class Animal : Occupant
    {
        public string name; // The name of the animal
        public int attackPower; // The attack power of the animal
        public int healthPoints; // The health points of the animal
        public int AttackRange { get; protected set; } // The attack range of the animal
        public int MaxMoveDistance { get; protected set; } // The maximum move distance of the animal

        public bool HasUsedSpecialAbility { get; protected set; } = false; // Indicates whether the animal has used its special ability
        public bool CanMoveAndAttackThisTurn { get; set; } = true; // Indicates whether the animal can move and attack in the same turn

        public enum Faction { Cat, Mouse } // Enum for the animal's faction
        public Faction CurrentFaction { get; set; } // The current faction of the animal

        public InteractionEffect CurrentEffect { get; set; } = InteractionEffect.None; // The current interaction effect on the animal
        public int EffectDuration { get; protected set; } = 0; // The duration of the current interaction effect
        public int MoveDistanceModifier { get; set; } = 0; // A modifier to the animal's move distance
        public bool CanAttack { get; protected set; } = true; // Indicates whether the animal can attack
        public bool CanMoveAgain { get; protected set; } = false; // Indicates whether the animal can move again
        // New: Effect source
        public Animal EffectSource { get; set; } // The source animal of the current interaction effect
        // New: Can attack again (used for pursuit effect)
        public bool CanAttackAgain { get; set; } // Indicates whether the animal can attack again (used for pursuit effect)
        public bool IsParalyzed { get; set; } = false; // Indicates whether the animal is paralyzed
        public Animal IsFleeingFrom { get; set; } = null; // The animal that this animal is fleeing from
        public List<Animal> NearbyAnimals { get; private set; } = new List<Animal>(); // A list of nearby animals
        public bool HasFledTurn { get; set; } = false; // Indicates whether the animal has fled this turn

        // New: Method to detect nearby animals
        public void DetectNearbyAnimals()
        {
            NearbyAnimals.Clear();
            foreach (var row in Game.animalZones)
            {
                foreach (var zone in row)
                {
                    if (zone.occupant is Animal otherAnimal && otherAnimal != this)
                    {
                        int distance = Math.Abs(this.location.x - otherAnimal.location.x) +
                                       Math.Abs(this.location.y - otherAnimal.location.y);
                        if (distance <= Game.GetEffectRange(this, otherAnimal) && !Game.IsSeparatedByRiver(this, otherAnimal))
                        {
                            NearbyAnimals.Add(otherAnimal);
                        }
                    }
                }
            }
        }

        // New: Method to handle interaction effects
        public void HandleInteractions()
        {
            var interactionsToApply = new List<(Animal source, InteractionEffect effect)>();
            foreach (var nearbyAnimal in NearbyAnimals.ToList())
            {
                InteractionEffect effect = Game.GetInteractionEffect(this, nearbyAnimal);
                if (effect != InteractionEffect.None)
                {
                    interactionsToApply.Add((nearbyAnimal, effect));
                }
            }
            foreach (var (source, effect) in interactionsToApply)
            {
                ApplyInteractionEffect(effect, source);
                HandleFlee();
            }
        }

        public void HandleFlee()
        {
            if (this.IsFleeingFrom != null && !HasFledTurn)
            {
                try
                {
                    Point fleeDestination = Game.GetFleeDestination(this, this.IsFleeingFrom);
                    if (!fleeDestination.Equals(this.location))
                    {
                        bool moveSuccess = Game.MoveAnimal(this, fleeDestination);
                        if (moveSuccess)
                        {
                            Console.WriteLine($"{this.name} fled from {this.IsFleeingFrom.name} to ({fleeDestination.x}, {fleeDestination.y})");
                            HasFledTurn = true;
                        }
                        else
                        {
                            Console.WriteLine($"{this.name} attempted to flee but couldn't move to ({fleeDestination.x}, {fleeDestination.y})");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"{this.name} couldn't find a valid flee destination from {this.IsFleeingFrom.name}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during flee attempt for {this.name}: {ex.Message}");
                }
                this.IsFleeingFrom = null;
            }
        }

        // New: Method to apply interaction effects
        private void ApplyInteractionEffect(InteractionEffect effect, Animal otherAnimal)
        {
            switch (effect)
            {
                case InteractionEffect.Pursue:
                    this.CanAttackAgain = true;
                    otherAnimal.IsFleeingFrom = this;
                    Console.WriteLine($"{this.name} is pursuing {otherAnimal.name}");
                    break;
                case InteractionEffect.Flee:
                    this.IsFleeingFrom = otherAnimal;
                    Console.WriteLine($"{this.name} is fleeing from {otherAnimal.name}");
                    break;
                case InteractionEffect.Attract:
                    if (this.name != "MouseKing")
                    {
                        this.MoveDistanceModifier = 1;
                    }

                    Console.WriteLine($"{otherAnimal.name} attracted {this.name}");
                    break;
                case InteractionEffect.Repel:
                    this.MoveDistanceModifier = -1;
                    Console.WriteLine($"{otherAnimal.name} repelled {this.name}");
                    break;
                case InteractionEffect.Intimidate:
                    if (new Random().Next(2) == 0) // 50% chance
                    {
                        otherAnimal.IsParalyzed = true;
                        otherAnimal.CanAttack = false;
                        otherAnimal.MoveDistanceModifier = -1;
                        Console.WriteLine($"{this.name} intimidated and paralyzed {otherAnimal.name}");
                    }
                    else
                    {
                        Console.WriteLine($"{this.name} attempted to intimidate {otherAnimal.name}, but failed");
                    }
                    break;
            }
        }

        protected Animal(string name, string emoji, string species, int attackPower, int healthPoints, Faction initialFaction)
        {
            this.name = name;
            this.emoji = emoji;
            this.species = species;
            this.attackPower = attackPower;
            this.healthPoints = healthPoints;
            this.CurrentFaction = initialFaction;
            this.AttackRange = 1;
            this.MaxMoveDistance = 3;
        }

        public virtual bool Move(Point destination)
        {
            Zone currentZone = Game.animalZones[this.location.y][this.location.x];
            Zone destinationZone = Game.animalZones[destination.y][destination.x];

            if (destinationZone.occupant != null)
            {
                // Moving to a cell with a piece should be handled in Game.MoveOrAttack
                return false;
            }

            currentZone.occupant = null;
            destinationZone.occupant = this;
            this.location = destination;
            this.CanMoveAndAttackThisTurn = false;

            // Detect and handle interactions after moving
            DetectNearbyAnimals();
            HandleInteractions();

            return true;
        }

        public virtual bool ShouldEndTurnAfterSpecialAbility()
        {
            return true; // By default, end the turn after using a special ability
        }

        public virtual bool UseSpecialAbility()
        {
            if (HasUsedSpecialAbility) return false; // Cannot use special ability if it has already been used
            HasUsedSpecialAbility = true;
            return true;
        }

        public virtual void ApplyEffect(InteractionEffect effect, int duration, Animal source)
        {
            CurrentEffect = effect;
            EffectDuration = duration;
            EffectSource = source;

            switch (effect)
            {
                case InteractionEffect.Attract:
                case InteractionEffect.Repel:
                case InteractionEffect.Paralyze:
                    //These effects apply to the target animal itself
                    CurrentEffect = effect;
                    EffectDuration = duration;
                    EffectSource = source; // Always set EffectSource to the source animal
                    MoveDistanceModifier = effect == InteractionEffect.Attract ? 1 : -1; // Modify move distance based on effect
                    if (effect == InteractionEffect.Paralyze)
                    {
                        IsParalyzed = true;
                        CanAttack = false;
                    }
                    break;

                case InteractionEffect.Pursue:
                    // Pursue effect only applies to the source animal
                    source.CanAttackAgain = true;
                    break;
                case InteractionEffect.Flee:
                    // Flee effect only applies to the target animal
                    this.IsFleeingFrom = source;
                    break;
            }
        }

        public virtual void UpdateEffects()
        {
            if (EffectDuration > 0)
            {
                EffectDuration--; // Decrement effect duration
                if (EffectDuration == 0)
                {
                    ResetEffects(); // Reset effects when duration is over
                }
            }
        }

        protected virtual void ResetEffects()
        {
            CurrentEffect = InteractionEffect.None;
            MoveDistanceModifier = 0;
            CanAttack = true;
            IsParalyzed = false;
            IsFleeingFrom = null;
            EffectSource = null;
        }

        // Modify the GetEffectDescription method in Animal.cs
        public string GetEffectDescription()
        {
            List<string> effects = new List<string>();

            if (IsFleeingFrom != null)
            {
                effects.Add($"Fleeing from {IsFleeingFrom.name}");
            }
            if (CanAttackAgain)
            {
                effects.Add("Pursuing");
            }
            if (MoveDistanceModifier > 0)
            {
                effects.Add("Attracted");
            }
            if (MoveDistanceModifier < 0)
            {
                effects.Add("Repelled");
            }
            if (IsParalyzed)
            {
                effects.Add("Paralyzed");
            }

            return string.Join(", ", effects); // Return a comma-separated string of active effects
        }

        public int GetModifiedMoveDistance()
        {
            if (this.name == "MouseKing")
            {
                return MaxMoveDistance; // MouseKing is not affected by move distance modifiers
            }
            return MaxMoveDistance + MoveDistanceModifier; // Apply move distance modifier for other animals
        }

        public abstract InteractionEffect GetInteractionEffect(Animal target); // Abstract method to get the interaction effect with another animal

        public override void ReportLocation()
        {
            base.ReportLocation();
            Console.WriteLine($"I am a {species} named {name}"); // Print the animal's species and name
        }

        // New: Method to switch factions
        public virtual void SwitchFaction()
        {
            CurrentFaction = (CurrentFaction == Faction.Cat) ? Faction.Mouse : Faction.Cat; // Switch between Cat and Mouse factions
        }

        public virtual void Attack(Animal target)
        {
            if (!CanAttack)
            {
                return; // Cannot attack if CanAttack is false
            }

            int damage = CalculateAttackDamage(target);
            if (CanAttackAgain) { damage *= 2; } // Double damage if CanAttackAgain is true
            target.TakeDamage(damage);

            // Trigger any special abilities or effects after attacking
            OnAfterAttack(target);
        }

        public virtual void TakeDamage(int damage)
        {
            healthPoints -= damage;
            // You can add special effects after taking damage here
        }

        protected virtual void OnAfterAttack(Animal target)
        {
            // Subclasses can override this method to implement special effects after attacking
        }

        public virtual int CalculateAttackDamage(Animal target)
        {
            return Math.Max(2, this.attackPower - target.attackPower); // Calculate attack damage based on the difference in attack power
        }
    }
}