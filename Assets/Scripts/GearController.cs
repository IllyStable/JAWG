using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearController : MonoBehaviour
{
    bool gear = false;
    bool hold = false;
    private void Update()
    {
        if (Input.GetKey(KeyCode.G))
        {
            if (!hold)
            {
                gear = !gear;
                hold = true;
            }
        } else
        {
            hold = false;
        }
        if (gear)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        if (!gear)
        {
            transform.rotation = Quaternion.Euler(90, 0, 0);
        }
    }
}
