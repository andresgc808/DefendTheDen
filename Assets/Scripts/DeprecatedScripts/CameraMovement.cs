using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private Transform player;
    //[SerializeField] private float smoothSpeed = 0.1f;
    private Vector3 _offset = new Vector3(0, 10.0f, 0);
    private Vector3 _currentVelocity = Vector3.zero;

    private void LateUpdate() {
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z);
    }
}
