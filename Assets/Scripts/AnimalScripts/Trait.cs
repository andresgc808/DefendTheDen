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

    // Specific data for the trait:
    public float duration;    // How long the effect lasts (e.g., poison duration, invincibility duration)
    public float damagePerSecond;  // Damage-over-time amount (if applicable)
    public float armorModifier; // Flat armor bonus or reduction
    public float speedModifier;  // Speed bonus or reduction
    public float attackPowerModifier; // Flat attack power bonus or reduction
    public float rangeModifier; // Flat attack range bonus or reduction

    // Specific data for the visual effect
    public GameObject particlePrefab;
    public float effectDuration;


    // Apply methods will be used later for each case
    public virtual void ApplyTraitEffect(AnimalCombat combat) { }
    public virtual void ApplyTraitEffect(AnimalHealth health) { }
}