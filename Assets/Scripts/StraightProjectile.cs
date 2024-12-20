using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightProjectile : BaseProjectile {
    [SerializeField] public float speed = 10f;
    [SerializeField] public float collisionRadius = 1f;
    [SerializeField] public LayerMask whatIsEnemy;

    public override void LaunchProjectile(Vector3 startingPosition, Vector3 targetPosition, float damage) {
        transform.position = startingPosition;
            Debug.Log("Projectile created, at " + transform.position);
        StartCoroutine(MoveProjectile(targetPosition, damage));
    }

    private IEnumerator MoveProjectile( Vector3 targetPosition, float damage) {

        Vector3 direction = (targetPosition - transform.position).normalized;

        while (Vector3.Distance(targetPosition, transform.position) > collisionRadius) {


            transform.position += direction * speed * Time.deltaTime;
            Collider[] colliders = Physics.OverlapSphere(transform.position, collisionRadius, whatIsEnemy);

            foreach (var hitCollider in colliders) {
                var damageableTarget = hitCollider.GetComponent<IDamageable>();
                if (damageableTarget != null && damageableTarget.IsAlive && !damageableTarget.Equals(this.GetComponent<IDamageable>())) {
                    damageableTarget.TakeDamage(damage); // deal damage to target

                    Debug.Log("Projectile Destroyed: " + gameObject.name);
                    DestroyProjectile(); // destroy projectile when collided with target.
                    yield break; // stop coroutine
                }
            }
            yield return null;
        }
        DestroyProjectile();
    }
}