using System.Collections.Generic;
using System;
using ZooManager;

public abstract class Animal : Occupant
{
    public string name;
    public int attack;
    public int health;
    public bool isKing;
    public string animalType;
    public Dictionary<Animal, Effect> effects = new Dictionary<Animal, Effect>();
    public int MovementDistance { get; set; }

    public Animal(string name)
    {
        this.name = name;
    }

    public void ApplyEffect(Type animalType, Effect effect)
    {
        foreach (var animal in Game.GetAnimalsOfType(animalType))
        {
            if (animal != this && !effects.ContainsKey(animal))
            {
                effects[animal] = effect;
            }
        }
    }

    public void RemoveEffect(Animal target)
    {
        if (effects.ContainsKey(target))
        {
            effects.Remove(target);
        }
    }

    public virtual void Move(Game game)
    {
        // Default move logic
    }

    public virtual void AttackSpecies(Game game, Animal target)
    {
        // Default attack logic
        target.TakeDamage(attack);
    }

    public virtual void TakeDamage(int damage)
    {
        health -= damage;
    }

    public bool IsDead()
    {
        return health <= 0;
    }
}