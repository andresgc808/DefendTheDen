using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalObject : MonoBehaviour
{
    public AnimalTower AnimalData;
    private Renderer[] _renderers;
    private Collider _collider;
    private Rigidbody _rigidbody;
    private MoveableObject _moveableObject;
    private AnimalCombat combat;
    private AnimalHealth health;

    private UnityEngine.AI.NavMeshAgent _navMeshAgent;
    private UnitMovement _unitMovement;

    private void Awake() {
        // Cache components that are critical for deactivation
        _renderers = GetComponentsInChildren<Renderer>(true);
        _collider = GetComponent<Collider>();
        _rigidbody = GetComponent<Rigidbody>();
        _moveableObject = GetComponent<MoveableObject>();

        // for navmesh agent
        _navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        _unitMovement = GetComponent<UnitMovement>();
    }

    public void Start() {
        health = GetComponent<AnimalHealth>();
        combat = GetComponent<AnimalCombat>();

        if (health != null) {
            health.Health = AnimalData.health;

            //if (AnimalData.healthTrait != null)
            //    AnimalData.healthTrait.ApplyTraitEffect(health);
        }
        if (combat != null) {
            combat.AttackRange = AnimalData.range;
            combat.AttackPower = AnimalData.attackPower;
            combat.FireRate = AnimalData.fireRate;
            combat.attackType = AnimalData.attackType;
            //if (AnimalData.attackTrait != null)
            //    AnimalData.attackTrait.ApplyTraitEffect(combat);

            Debug.Log($"AnimalObject: {gameObject.name} has been initialized with {AnimalData.speciesName} data.");
            Debug.Log($"AnimalObject: {gameObject.name} has {health.Health} health, {combat.AttackPower} attack power, {combat.FireRate} fire rate, and {combat.AttackRange} attack range.");
            var attackComponent = this.gameObject.AddComponent(System.Type.GetType(AnimalData.attackType.ToString() + "Attack")) as BaseAttack;

            if (attackComponent is ProjectileAttack projectileAttack && AnimalData.attackType == AttackType.Projectile) {
                projectileAttack.LoadProjectilePrefab($"{AnimalData.speciesName}Projectile");
            }
            //if (attackComponent != null)
            //    attackComponent.Range = AnimalData.range;

            if(_moveableObject != null){
                if (_navMeshAgent != null) _navMeshAgent.enabled = false;
                if (_unitMovement != null) _unitMovement.enabled = false;
            }else {
                if (_navMeshAgent != null) _navMeshAgent.enabled = true;
                //if (_unitMovement != null) _unitMovement.enabled = true;
            }
        }
    }

    public void Deactivate() {
        // Disable rendering
        foreach (var renderer in _renderers) {
            renderer.enabled = false;
        }

        // Disable collider
        if (_collider) _collider.enabled = false;

        // Disable rigidbody physics
        if (_rigidbody) {
            _rigidbody.isKinematic = true;
        }

        // Disable movement script
        if (_moveableObject) _moveableObject.enabled = false;
        if (_unitMovement != null) _unitMovement.enabled = false;
        if (_navMeshAgent != null) _navMeshAgent.enabled = false;

        // deselect animal
        UnitSelectionManager.Instance.DeselectObject(gameObject);

        // Disable combat and health scripts
        if (combat) combat.enabled = false;
        if (health) health.enabled = false;

        Debug.Log($"{gameObject.name} deactivated.");
    }

    public void Activate() {
        // Enable rendering
        foreach (var renderer in _renderers) {
            renderer.enabled = true;
        }

        // Enable collider
        if (_collider) _collider.enabled = true;

        // Re-enable rigidbody physics
        if (_rigidbody) _rigidbody.isKinematic = false;

        // Re-enable movement script
        if (_moveableObject) _moveableObject.enabled = true;
        //if (_unitMovement != null) _unitMovement.enabled = true;
        if (_navMeshAgent != null) _navMeshAgent.enabled = true;

        // Re-enable combat and health scripts
        if (combat) combat.enabled = true;
        if (health) health.enabled = true;

        Debug.Log($"{gameObject.name} reactivated.");
    }

}
