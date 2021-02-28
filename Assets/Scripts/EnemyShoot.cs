using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShoot : MonoBehaviour
{
    
    public Transform Bullet;
    public GameObject spawn;
    public GameObject body;
    public int BulletForce = 5000;
    public float Reloading;

    private void Update()
    {
        Reloading -= Time.deltaTime;
        if (Reloading <= 0)
        {
            var angles = body.transform.rotation.eulerAngles;
            Reloading = 5.0f;
            Transform BulletInstance = (Transform) Instantiate(Bullet, spawn.transform.position, Quaternion.identity);
            BulletInstance.localRotation = Quaternion.Euler(90, angles.y - 180, 0);
            BulletInstance.GetComponent<Rigidbody>().AddForce(transform.forward * BulletForce * -0.02f, ForceMode.Impulse);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Wall")
        {
            var angles = body.transform.rotation.eulerAngles;
            Transform BulletInstance = (Transform) Instantiate(Bullet, spawn.transform.position, Quaternion.identity);
            BulletInstance.localRotation = Quaternion.Euler(90, angles.y - 180, 0);
            BulletInstance.GetComponent<Rigidbody>().AddForce(transform.forward * BulletForce * -0.02f, ForceMode.Impulse);
        }
    }
}
