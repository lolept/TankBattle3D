using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Game
{
    public class Boom : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        public GameObject destroy;
        public GameObject F;
        public GameObject S;
        public GameObject deathCum;
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
            if (other.CompareTag("Wall"))
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
                Destroy(destroy);
                Destroy(gameObject);
                //destroy.SetActive(false);
                //gameObject.SetActive(false);
            }
            
            else if (other.CompareTag("floor"))
            {
                PhotonNetwork.Instantiate(F.name, gameObject.transform.position, Quaternion.identity);
            for (int i = 0; i < 2; i++)
            { 
                PSS[i].Play();
            }
            gameObject.SetActive(false);
                PhotonNetwork.RaiseEvent(199, -1, new RaiseEventOptions {Receivers = ReceiverGroup.Others},
                    new SendOptions {Reliability = true});
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
                if (destroy.GetPhotonView().AmOwner)
                {
                    Instantiate(deathCum, destroy.transform.position, Quaternion.identity);
                }
                destroy.SetActive(false);
                //Instantiate(DeathCam, this.gameObject.transform.position, Quaternion.identity);
                gameObject.SetActive(false);
                PhotonNetwork.RaiseEvent(199, destroy.GetPhotonView().ViewID, new RaiseEventOptions {Receivers = ReceiverGroup.Others},
                    new SendOptions {Reliability = true});
            }
        }


        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case 199:
                    //gameObject.SetActive(false);
                    if ((int) photonEvent.CustomData != -1)
                    {
                        var dest = PhotonView.Find((int) photonEvent.CustomData).gameObject;
                        Destroy(dest);
                        //dest.SetActive(false);
                    }
                    Destroy(gameObject);
                    break;
            }
        }
    }
}
