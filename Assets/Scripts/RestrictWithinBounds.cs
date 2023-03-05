using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestrictWithinBounds : MonoBehaviour
{
    public Vector3 MapSize;

    private void FixedUpdate()
    {
        if (Mathf.Abs(transform.position.x) > MapSize.x) transform.position = new Vector3(0, transform.position.y, transform.position.z);
        if (Mathf.Abs(transform.position.y) > MapSize.y) transform.position = new Vector3(transform.position.x, 100, transform.position.z);
        if (Mathf.Abs(transform.position.z) > MapSize.z) transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    }
}
