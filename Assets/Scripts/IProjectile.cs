using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IProjectile {
    void LaunchProjectile(Vector3 staringPosition, Vector3 targetPosition, float damage);

    GameObject GetGameObject();
    Transform GetTransform();
    void DestroyProjectile();
}