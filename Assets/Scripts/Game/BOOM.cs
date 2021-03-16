using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Random = System.Random;

public class BOOM : MonoBehaviour
{
    public GameObject destroy;
    public Transform TankDestroy;
    public GameObject F;
    public GameObject S;
    public GameObject G;
    public Camera DeathCam;
    public ParticleSystem[] PSF;
    public ParticleSystem[] PSS;
    public AudioSource ASF;
    public AudioSource ASS;
    private int type;
    

    void Start()
    {
        PSF = F.GetComponentsInChildren<ParticleSystem>();
        PSS = S.GetComponentsInChildren<ParticleSystem>();
        F = GameObject.Find("boom1");
        S = GameObject.Find("boom2");
        ASF = F.GetComponent<AudioSource>();
        ASS = S.GetComponent<AudioSource>();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Wall")
        {
            destroy = other.gameObject;
            type = UnityEngine.Random.Range(0, 1);
            if (type == 0)
            {
                Instantiate(F, destroy.transform.position, Quaternion.identity);
                for (int i = 0; i < 2; i++)
                {
                    PSF[i].Play();
                }
                ASF.Play();
            }
            else if (type == 1)
            {
                Instantiate(S, destroy.transform.position, Quaternion.identity);
                for (int i = 0; i < 2; i++)
                {
                    PSS[i].Play();
                }
                ASS.Play();
            }
            destroy.SetActive(false);
            this.gameObject.SetActive(false);
        }
        else if (other.tag == "floor")
        {
            Instantiate(G, this.gameObject.transform.position, Quaternion.identity);
            for (int i = 0; i < 2; i++)
            { 
                PSS[i].Play();
            }
            ASS.Play();
            this.gameObject.SetActive(false);
        }
        else if (other.tag == "Enemy")
        {
            destroy = other.gameObject;
            if (destroy.name == "Enemyhead");
            {
                TankDestroy = destroy.transform.parent;
            }
            type = UnityEngine.Random.Range(0, 1);
            if (type == 0)
            {
                if(destroy.name == "Enemyhead")
                    Instantiate(F, TankDestroy.transform.position, Quaternion.identity);
                else
                    Instantiate(F, destroy.transform.position, Quaternion.identity);
                for (int i = 0; i < 2; i++)
                {
                    PSF[i].Play();
                }
                ASF.Play();
            }
            else if (type == 1)
            {
                if(destroy.name == "Enemyhead")
                    Instantiate(S, TankDestroy.transform.position, Quaternion.identity);
                else
                    Instantiate(S, destroy.transform.position, Quaternion.identity);
                for (int i = 0; i < 2; i++)
                {
                    PSS[i].Play();
                }
                ASS.Play();
            }

            if (destroy.name == "Enemyhead")
                TankDestroy.gameObject.SetActive(false);
            else
                destroy.SetActive(false);
            this.gameObject.SetActive(false);
        }
        else if (other.tag == "Player")
        {
            destroy = other.gameObject;
            if (destroy.name == "defaultHead");
            {
                TankDestroy = destroy.transform.parent.transform.parent;
            }
            type = UnityEngine.Random.Range(0, 1);
            if (type == 0)
            {
                if(destroy.name == "defaultHead")
                    Instantiate(F, TankDestroy.transform.position, Quaternion.identity);
                else
                    Instantiate(F, destroy.transform.position, Quaternion.identity);
                for (int i = 0; i < 2; i++)
                {
                    PSF[i].Play();
                }
                ASF.Play();
            }
            else if (type == 1)
            {
                if(destroy.name == "defaultHead")
                    Instantiate(S, TankDestroy.transform.position, Quaternion.identity);
                else
                    Instantiate(S, destroy.transform.position, Quaternion.identity);
                for (int i = 0; i < 2; i++)
                {
                    PSS[i].Play();
                }
                ASS.Play();
            }

            if (destroy.name == "defaultHead")
            {
                destroy.GetComponentInChildren<Camera>().gameObject.SetActive(false);
                TankDestroy.gameObject.SetActive(false);
            }
            else
            {
                GameObject.Find("defaultHead").GetComponentInChildren<Camera>().gameObject.SetActive(false);
                destroy.SetActive(false);
            }
            Instantiate(DeathCam, this.gameObject.transform.position, Quaternion.identity);
            this.gameObject.SetActive(false);
        }
    }
}
