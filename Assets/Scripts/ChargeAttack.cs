#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeAttack : BaseAttack
{
    public float chargeSpeed = 10f;
    [SerializeField] public float range = 0.5f;
    [SerializeField] private float _rotationSpeed = 5f;
    private Matrix4x4 _matrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
    [SerializeField] private float _windUpTime = 1.5f;
    private Animator? _animator;

    private void Start() {
        _animator = GetComponentInChildren<Animator>(); // loading cached animator object
    }

    public override void PerformAttack(Transform attackerTransform, IDamageable target, float attackPower, Trait? attackTrait) {
        StartCoroutine(PerformCharge(attackerTransform, target, attackPower));
    }

    private IEnumerator PerformCharge(Transform attackerTransform, IDamageable target, float attackPower) {
        if (target == null) {
            yield break;
        }
        Transform targetTransform = ((MonoBehaviour)target).transform;


        // calculate direction
        var dir = targetTransform.position - attackerTransform.position;
        dir = dir.normalized; // normalize for only directional




        //var skewedDir = matrix.MultiplyPoint3x4(dir);
        //var skewedInput = matrix.MultiplyPoint3x4(_forwardDirection.up);
        //Rotation code start here:
        Quaternion targetRotation = Quaternion.LookRotation(dir, attackerTransform.up); // calculates new rotation to look at that direction


        float startTime = Time.time;
        while (Time.time - startTime <= _windUpTime) {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            yield return null; 
        }

        Vector3 originalPosition = attackerTransform.position;
        startTime = Time.time;

        Vector3 targetPosition = ((MonoBehaviour)target).transform.position;

        while (Time.time - startTime <= 0.5f) { // 1 second of charge time
            float timeRatio = (Time.time - startTime) / 0.5f;

            if (_animator != null) {
                _animator.SetBool("isMoving", true); // set movement animation on
                _animator.SetFloat("animSpeed", chargeSpeed);
            }

            attackerTransform.position = Vector3.Lerp(originalPosition, targetPosition, timeRatio);  // position is changed every frame based on the timer
            yield return null;  // wait until next frame
        }

        if (Vector3.Distance(targetPosition, attackerTransform.position) <= range)
            target.TakeDamage(attackPower);
        if (_animator != null) {
            _animator.SetFloat("animSpeed", 1f);
            _animator.SetBool("isMoving", false);
        }
        attackerTransform.position = originalPosition;
        yield break;
    }
}
