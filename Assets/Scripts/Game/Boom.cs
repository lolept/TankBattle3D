using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Game
{
    public class Boom : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        public GameObject destroy;
        public Transform tankDestroy;
        public GameObject F;
        public GameObject S;
        // public GameObject G;
        //public Camera DeathCam;
        public ParticleSystem[] PSF;
        public ParticleSystem[] PSS;
        //public AudioSource ASF;
        //public AudioSource ASS;
        private int _type;


        void Start()
        {
            PSF = F.GetComponentsInChildren<ParticleSystem>();
            PSS = S.GetComponentsInChildren<ParticleSystem>(); 
            //F = GameObject.Find("boom1");
            //S = GameObject.Find("boom2");
            //ASF = F.GetComponent<AudioSource>();
            //ASS = S.GetComponent<AudioSource>();
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Wall")
            {
                destroy = other.gameObject;
                _type = Random.Range(0, 1);
                switch (_type)
                {
                    case 0:
                        PhotonNetwork.Instantiate(F.name, destroy.transform.position, Quaternion.identity);
                        for (var i = 0; i < 2; i++)
                        {
                            PSF[i].Play();
                        }
                        break;
                    case 1:
                        PhotonNetwork.Instantiate(S.name, destroy.transform.position, Quaternion.identity);
                        for (var i = 0; i < 2; i++)
                        {
                            PSS[i].Play();
                        }
                        break;
                }
                PhotonNetwork.RaiseEvent(199, destroy.GetPhotonView().ViewID, new RaiseEventOptions {Receivers = ReceiverGroup.Others},
                    new SendOptions {Reliability = true});
                destroy.SetActive(false);
                this.gameObject.SetActive(false);
            
            }
            else if (other.CompareTag("floor"))
            {
                /*
            Instantiate(G, this.gameObject.transform.position, Quaternion.identity);
            for (int i = 0; i < 2; i++)
            { 
                PSS[i].Play();
            }
            ASS.Play();
            */
                this.gameObject.SetActive(false);
                PhotonNetwork.RaiseEvent(199, destroy.GetPhotonView().ViewID, new RaiseEventOptions {Receivers = ReceiverGroup.Others},
                    new SendOptions {Reliability = true});
            }
            else if (other.CompareTag("Enemy"))
            {
                destroy = other.gameObject;
                if (destroy.name == "Enemyhead");
                {
                    tankDestroy = destroy.transform.parent;
                }
                /*
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
*/
                if (destroy.name == "Enemyhead")
                    tankDestroy.gameObject.SetActive(false);
                else
                    destroy.SetActive(false);
                this.gameObject.SetActive(false);
            }
            else if (other.CompareTag("Player"))
            {
                destroy = other.gameObject;
                /*
            type = UnityEngine.Random.Range(0, 1);
            if (type == 0)
            {
                if(destroy.name == "Head")
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
                if(destroy.name == "Head")
                    Instantiate(S, TankDestroy.transform.position, Quaternion.identity);
                else
                    Instantiate(S, destroy.transform.position, Quaternion.identity);
                for (int i = 0; i < 2; i++)
                {
                    PSS[i].Play();
                }
                ASS.Play();
            }
*/
            
                //GameObject.Find("defaultHead").GetComponentInChildren<Camera>().gameObject.SetActive(false);
                destroy.SetActive(false);
                //Instantiate(DeathCam, this.gameObject.transform.position, Quaternion.identity);
                this.gameObject.SetActive(false);
                PhotonNetwork.RaiseEvent(199, destroy.GetPhotonView().ViewID, new RaiseEventOptions {Receivers = ReceiverGroup.Others},
                    new SendOptions {Reliability = true});
            }
        }


        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case 199:
                    gameObject.SetActive(false);
                    if ((int) photonEvent.CustomData != -1)
                    {
                        var dest = PhotonView.Find((int) photonEvent.CustomData).gameObject;
                        dest.SetActive(false);
                    }
                    break;
            }
        }
    }
}
