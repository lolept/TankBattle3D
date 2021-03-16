using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    public Transform Bullet;
    public GameObject spawn;
    public GameObject body;
    public Rigidbody TankRb;
    public float Reloading = 0.0f;
    public int BulletForce = 5000;
    public ParticleSystem PS;
    public AudioSource AS;

    private void Start()
    {
        TankRb = body.GetComponent<Rigidbody>();
        AS = GameObject.Find("tank_player1_head").GetComponent<AudioSource>();
    }

    void Update()
    {
        Reloading -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Space) && Reloading <= 0)
        {
            TankRb.AddForce(transform.forward * 4000000 * 0.015f, ForceMode.Impulse);
            var angles = body.transform.rotation.eulerAngles;
            Reloading = 5.0f;
            Transform BulletInstance = (Transform) Instantiate(Bullet, spawn.transform.position, Quaternion.identity);
            BulletInstance.localRotation = Quaternion.Euler(90, angles.y - 180, 0);
            BulletInstance.GetComponent<Rigidbody>().AddForce(transform.forward * BulletForce * -0.02f, ForceMode.Impulse);
            PS.Play();
            AS.Play();
        }
    }
}
