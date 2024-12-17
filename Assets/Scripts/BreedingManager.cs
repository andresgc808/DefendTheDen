using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreedingManager : MonoBehaviour {
    [SerializeField] private Dictionary<(string, string), List<string>> _breedingPairs;

    void Awake() { //start properties
        _PopulateBreedingPairDatabase();
    }
    private void _PopulateBreedingPairDatabase() { 
        _breedingPairs = new Dictionary<(string, string), List<string>> {
            // keep dictionary symmetrical *might not be needed
               { ("Turtle", "Squirrel"), new List<string> { "ArmoredSquirrel" } },
               { ("Squirrel", "Turtle"), new List<string> { "ArmoredSquirrel" } },
                };
    }

    public AnimalTower BreedAnimals(AnimalTower parent1, AnimalTower parent2) {
        if (parent1 == parent2) {
            Debug.Log("Cannot breed same Animal!");
            return null;
        }

        if (parent1.speciesName == parent2.speciesName) {

            if (parent1 == parent2) {
                Debug.Log("Breeding the same animal type!");
                // shortcutting since we know they are the same species, no need for lookup
                var offspringType = parent1.speciesName;
                return Resources.Load<AnimalTower>($"Animals/{offspringType}");
            }

            List<string> possibleOffspring = new List<string>();

            // check if the pair is in the dictionary
            if (_breedingPairs.ContainsKey((parent1.speciesName, parent2.speciesName))) {
                possibleOffspring = _breedingPairs[(parent1.speciesName, parent2.speciesName)];
            } // no need to check inverse since dictionary is symmetrical

            // TODO: add variability in offspring selection
            string _offspringType = possibleOffspring[Random.Range(0, possibleOffspring.Count)]; // random selection

            AnimalTower offspring = GenerateOffspring(_offspringType, parent1, parent2);

            return offspring;
        }

        List<string> possibleOffspring = parent1.GetOffspringOptions(parent2);

        if (possibleOffspring == null || possibleOffspring.Count == 0) {
            Debug.Log("These animals cannot breed.");
            return null;
        }

        string offspringType = possibleOffspring[Random.Range(0, possibleOffspring.Count)];
        AnimalTower offspring = GenerateOffspring(offspringType, parent1, parent2);
        return offspring;
    }


    private AnimalTower GenerateOffspring(string offspringType, AnimalTower parent1, AnimalTower parent2) {

        //Load our offspring from resources, we don't want to create a new asset here!
        AnimalTower offspring = Resources.Load<AnimalTower>($"Animals/{offspringType}");

        if (offspring == null) {
            Debug.LogError($"Animal {offspringType} not found");
            return null;
        }

        offspring = Instantiate(offspring);


        offspring.attackPower = (parent1.attackPower + parent2.attackPower) / 2 + Random.Range(-5, 5);
        offspring.health = (parent1.health + parent2.health) / 2 + Random.Range(-20, 20);

        //offspring.dominantAttackTrait = DetermineDominantTrait(parent1.dominantAttackTrait, parent2.dominantAttackTrait);
        //offspring.recessiveAttackTrait = DetermineRecessiveTrait(parent1.recessiveAttackTrait, parent2.recessiveAttackTrait);

        //offspring.dominantHealthTrait = DetermineDominantTrait(parent1.dominantHealthTrait, parent2.dominantHealthTrait);
        //offspring.recessiveHealthTrait = DetermineRecessiveTrait(parent1.recessiveHealthTrait, parent2.recessiveHealthTrait);

        //offspring.dominantFireRateTrait = DetermineDominantTrait(parent1.dominantFireRateTrait, parent2.dominantFireRateTrait);
        //offspring.recessiveFireRateTrait = DetermineRecessiveTrait(parent1.recessiveFireRateTrait, parent2.recessiveFireRateTrait);

        //offspring.dominantRangeTrait = DetermineDominantTrait(parent1.dominantRangeTrait, parent2.dominantRangeTrait);
        //offspring.recessiveRangeTrait = DetermineRecessiveTrait(parent1.recessiveRangeTrait, parent2.recessiveRangeTrait);

        return offspring;
    }

    //private Trait DetermineDominantTrait(Trait parent1Trait, Trait parent2Trait) {
    //    // Logic to decide which dominant trait the offspring gets
    //    return Random.value > 0.5f ? parent1Trait : parent2Trait;
    //}

    //private Trait DetermineRecessiveTrait(Trait parent1Trait, Trait parent2Trait) {
    //    // Similar logic for recessive traits
    //    return Random.value > 0.5f ? parent1Trait : parent2Trait;
    //}
}