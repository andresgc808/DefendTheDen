using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAnimal", menuName = "Animals/Animal")]
public class AnimalTower : ScriptableObject {
    public string speciesName;
    public float attackPower;
    public float health;
    public float fireRate;
    public float range;
    //public Trait dominantAttackTrait;
    //public Trait recessiveAttackTrait;
    //public Trait dominantHealthTrait;
    //public Trait recessiveHealthTrait;
    //public Trait dominantFireRateTrait;
    //public Trait recessiveFireRateTrait;
    //public Trait dominantRangeTrait;
    //public Trait recessiveRangeTrait;

    
    public Dictionary<string, List<string>> compatibleBreeds = new Dictionary<string, List<string>>();


    public List<string> GetOffspringOptions(AnimalTower partner) {
        if (compatibleBreeds.ContainsKey(partner.speciesName)) {
            return compatibleBreeds[partner.speciesName];
        }
        return null;
    }
}