using System;
using System.Collections.Generic;

namespace ZooManager
{
    public abstract class Animal : Occupant
    {
        public string name;
        public int attackPower;
        public int healthPoints;
        public int AttackRange { get; protected set; }
        public int MaxMoveDistance { get; protected set; }

        public bool HasUsedSpecialAbility { get; protected set; } = false;
        public bool CanMoveAndAttackThisTurn { get; set; } = true;

        public enum Faction { Cat, Mouse }
        public Faction CurrentFaction { get; set; }

        public InteractionEffect CurrentEffect { get; set; } = InteractionEffect.None;
        public int EffectDuration { get; protected set; } = 0;
        public int MoveDistanceModifier { get; set; } = 0;
        public bool CanAttack { get; protected set; } = true;
        public bool CanMoveAgain { get; protected set; } = false;
        // New: Effect source
        public Animal EffectSource { get; set; }
        // New: Can attack again (used for pursuit effect)
        public bool CanAttackAgain { get; set; }
        public bool IsParalyzed { get; set; } = false;
        public Animal IsFleeingFrom { get; set; } = null;
        public List<Animal> NearbyAnimals { get; private set; } = new List<Animal>();

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
            foreach (var nearbyAnimal in NearbyAnimals)
            {
                InteractionEffect effect = Game.GetInteractionEffect(this, nearbyAnimal);
                ApplyInteractionEffect(effect, nearbyAnimal);
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
                    this.MoveDistanceModifier = 1;
                    Console.WriteLine($"{otherAnimal.name} attracted {this.name}");
                    break;
                case InteractionEffect.Repel:
                    this.MoveDistanceModifier = -1;
                    Console.WriteLine($"{otherAnimal.name} repelled {this.name}");
                    break;
                case InteractionEffect.Intimidate:
                    if (new Random().Next(2) == 0) // 50% chance
                    {
                        this.IsParalyzed = true;
                        this.CanAttack = false;
                        this.MoveDistanceModifier = -1;
                        Console.WriteLine($"{otherAnimal.name} intimidated and paralyzed {this.name}");
                    }
                    else
                    {
                        Console.WriteLine($"{otherAnimal.name} attempted to intimidate {this.name}, but failed");
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

            if (Game.IsCrossingRiver(this.location, destination))
            {
                this.CanMoveAndAttackThisTurn = false;
            }

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
            if (HasUsedSpecialAbility) return false;
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
                    MoveDistanceModifier = effect == InteractionEffect.Attract ? 1 : -1;
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
                EffectDuration--;
                if (EffectDuration == 0)
                {
                    ResetEffects();
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

            return string.Join(", ", effects);
        }

        public int GetModifiedMoveDistance()
        {
            return Math.Max(0, MaxMoveDistance + MoveDistanceModifier);
        }

        public abstract InteractionEffect GetInteractionEffect(Animal target);

        public override void ReportLocation()
        {
            base.ReportLocation();
            Console.WriteLine($"I am a {species} named {name}");
        }

        // New: Method to switch factions
        public virtual void SwitchFaction()
        {
            CurrentFaction = (CurrentFaction == Faction.Cat) ? Faction.Mouse : Faction.Cat;
        }

        public virtual void Attack(Animal target)
        {
            if (!CanAttack)
            {
                return;
            }

            int damage = CalculateAttackDamage(target);
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
            return Math.Max(1, this.attackPower - target.attackPower + 1);
        }
    }
}