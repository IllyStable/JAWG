using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public Rigidbody bulletPrefab;


    private void Update()
    {
        if (Input.GetAxis("Fire") > 0)
        {
            Rigidbody bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
            bullet.velocity = transform.parent.transform.forward * 50 + transform.parent.GetComponent<Rigidbody>().velocity;
        }
    }
}
