using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TraitType {
    Attack, Health, Range, FireRate // types of traits
}

[CreateAssetMenu(fileName = "NewTrait", menuName = "Traits/Trait")]
public class Trait : ScriptableObject {
    public string traitName;
    public TraitType traitType;

    public virtual void ApplyTraitEffect(AnimalCombat combat, float modifier) { }

    public virtual void ApplyTraitEffect(AnimalHealth health, float modifier) { }

}