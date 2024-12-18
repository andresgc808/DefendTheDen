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

    private void Awake() {
        // Cache components that are critical for deactivation
        _renderers = GetComponentsInChildren<Renderer>(true);
        _collider = GetComponent<Collider>();
        _rigidbody = GetComponent<Rigidbody>();
        _moveableObject = GetComponent<MoveableObject>();
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

        Debug.Log($"{gameObject.name} reactivated.");
    }

}
