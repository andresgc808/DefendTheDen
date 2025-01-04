#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightProjectile : BaseProjectile {
    [SerializeField] public float speed = 10f;
    [SerializeField] public float collisionRadius = 1f;
    [SerializeField] public LayerMask whatIsEnemy;
    private GameObject? _effectObject;
    private Trait? _appliedTrait;

    public override void LaunchProjectile(Vector3 startingPosition, Vector3 targetPosition, float damage) {
        transform.position = startingPosition;
        Debug.Log("Projectile created, at " + transform.position);
        StartCoroutine(MoveProjectile(targetPosition, damage));
    }

    public override void ApplyTrait(Trait trait) {
        _appliedTrait = trait;
        if (_appliedTrait != null && _appliedTrait.particlePrefab != null) {

            _effectObject = Instantiate(_appliedTrait.particlePrefab, transform); // attach the effect to the projectile
            StartCoroutine(DestroyParticle(_appliedTrait.effectDuration)); // destroys particle after some time
        }
    }

    private IEnumerator DestroyParticle(float duration) {
        yield return new WaitForSeconds(duration);

        if (_effectObject != null)
            Destroy(_effectObject); // destroy the visual particle effect
    }

    private IEnumerator MoveProjectile(Vector3 targetPosition, float damage) {

        Vector3 direction = (targetPosition - transform.position).normalized;

        while (Vector3.Distance(targetPosition, transform.position) > collisionRadius) {


            transform.position += direction * speed * Time.deltaTime;
            Collider[] colliders = Physics.OverlapSphere(transform.position, collisionRadius, whatIsEnemy);

            foreach (var hitCollider in colliders) {
                var damageableTarget = hitCollider.GetComponent<IDamageable>();
                if (damageableTarget != null && damageableTarget.IsAlive && !damageableTarget.Equals(this.GetComponent<IDamageable>())) {
                    damageableTarget.TakeDamage(damage); // deal damage to target
                    if (_appliedTrait != null) {
                        StartCoroutine(ApplyPoison(damageableTarget, _appliedTrait.duration, _appliedTrait.damagePerSecond));
                    }
                    Debug.Log("Projectile Destroyed: " + gameObject.name);
                    DestroyProjectile();
                    yield break;
                }
            }
            yield return null;
        }
        DestroyProjectile();
    }

    private IEnumerator ApplyPoison(IDamageable target, float duration, float damagePerSecond) {
        float timeElapsed = 0;
        while (timeElapsed <= duration) {
            target.TakeDamage(damagePerSecond * Time.deltaTime);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }
}