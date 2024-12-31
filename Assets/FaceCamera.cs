using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Camera _camera;
    private Transform _cameraTransform;
    private void Start() {
        _camera = Camera.main; // always set this up on start, rather than on update to avoid overhead.
        if (_camera != null)
            _cameraTransform = _camera.transform;

    }

    private void LateUpdate() {
        if (_cameraTransform == null) { // in case a camera does not exists.
            if (_camera != null) {
                _cameraTransform = _camera.transform; // use it to assign again the value
            } else
                return;

        }
        transform.LookAt(transform.position + _cameraTransform.rotation * Vector3.forward, _cameraTransform.rotation * Vector3.up);  // update the transform direction every frame relative to the camera view.
    }
}
