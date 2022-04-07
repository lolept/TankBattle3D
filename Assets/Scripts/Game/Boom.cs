using System;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game
{
    public class Boom : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        private int _type;
        public GameObject destroy;
        public GameObject f;
        public ParticleSystem[] pSf;
        public ParticleSystem[] pSs;
        public GameObject s;


        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case 8:
                    var data = photonEvent.CustomData.Unwrap<string[]>();
                    var id = int.Parse(data[0]);
                    if(transform.parent == null) return;
                    if (id == transform.parent.gameObject.GetPhotonView().ViewID){
                        if (gameObject.name.Equals(data[1]))
                        {
                            Destroy(gameObject);
                        }
                    }
                    break;
            }
        }

        private void Start()
        {
            pSf = f.GetComponentsInChildren<ParticleSystem>();
            pSs = s.GetComponentsInChildren<ParticleSystem>();
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Bullet") && gameObject.CompareTag("Wall"))
            {
                destroy = other.gameObject;
                _type = Random.Range(0, 1);
                switch (_type)
                {
                    case 0:
                        PhotonNetwork.Instantiate(f.name, gameObject.transform.position, Quaternion.identity);
                        for (var i = 0; i < 2; i++) pSf[i].Play();
                        break;
                    case 1:
                        PhotonNetwork.Instantiate(s.name, gameObject.transform.position, Quaternion.identity);
                        for (var i = 0; i < 2; i++) pSs[i].Play();
                        break;
                }
                
                var data = new[] {destroy.GetPhotonView().ViewID.ToString(), destroy.name};
                PhotonNetwork.RaiseEvent(8, data, new RaiseEventOptions {Receivers = ReceiverGroup.All},
                    new SendOptions {Reliability = true});
                
                data = new[] {transform.parent.gameObject.GetPhotonView().ViewID.ToString(), gameObject.name};
                PhotonNetwork.RaiseEvent(8, data, new RaiseEventOptions {Receivers = ReceiverGroup.All},
                    new SendOptions {Reliability = true});
            }

            else if (other.CompareTag("Bullet") && gameObject.CompareTag("floor"))
            {
                if(!other.gameObject.GetPhotonView().AmController) return;
                destroy = other.gameObject;
                PhotonNetwork.Instantiate(f.name, destroy.transform.position, Quaternion.identity);
                for (var i = 0; i < 2; i++) pSs[i].Play();
                
                var data = new[] {destroy.GetPhotonView().ViewID.ToString(), destroy.name};
                PhotonNetwork.RaiseEvent(8, data, new RaiseEventOptions {Receivers = ReceiverGroup.All},
                    new SendOptions {Reliability = true});
            }
        }
    }
}