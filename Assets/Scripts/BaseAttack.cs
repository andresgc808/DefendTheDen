using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAttack : MonoBehaviour
{
    public abstract void PerformAttack(Transform attackerTransform, IDamageable target, float attackPower);

}
