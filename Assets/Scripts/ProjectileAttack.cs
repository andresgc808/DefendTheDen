#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileAttack : BaseAttack {
    public BaseProjectile? projectilePrefab;

    public void LoadProjectilePrefab(string projectileName) {
        projectilePrefab = Resources.Load<BaseProjectile>($"Prefabs/{projectileName}"); // load projectile from resources by string.
        if (projectilePrefab == null) {
            Debug.LogError("Projectile prefab failed to load!");
        }
    }
    public override void PerformAttack(Transform attackerTransform, IDamageable target, float attackPower, Trait? attackTrait) {
        if (projectilePrefab == null) return;
        if (target == null) return;


        IProjectile projectile = Instantiate(projectilePrefab, attackerTransform.position, Quaternion.identity);
        Debug.Log($"Projectile Instantiated at: {projectile.GetTransform().position}");

        if (attackTrait != null)
            projectile.ApplyTrait(attackTrait);

        projectile.LaunchProjectile(attackerTransform.position, ((MonoBehaviour)target).transform.position, attackPower); // launch projectile using interface method
    }
}