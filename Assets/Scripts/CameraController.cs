using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Tooltip("An array representing camera positions")]
    [SerializeField] Transform[] povs;
    [Tooltip("The speed at which the camera moves")]
    [SerializeField] float speed;

    private int index = 0;
    private Vector3 target;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) index = 0;

        target = povs[index].position;
    }

    private void FixedUpdate()
    {
        // Move camera
        transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * speed);
        transform.forward = povs[index].forward;
    }
}
