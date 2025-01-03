using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseProjectile : MonoBehaviour, IProjectile {
    public abstract void LaunchProjectile(Vector3 startingPosition, Vector3 targetPosition, float damage);

    public virtual void ApplyTrait(Trait trait) { }

    public GameObject GetGameObject() {
        return this.gameObject;
    }

    public Transform GetTransform() {
        return this.transform;
    }

    public void DestroyProjectile() {
        Destroy(gameObject);
    }
}