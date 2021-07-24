using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game
{
    public class Shooting : MonoBehaviour
    {
        [SerializeField] private GameObject spawn;
        [SerializeField] private GameObject body;
        [SerializeField] private int bulletForce = 5000;
        [SerializeField] private ParticleSystem ps;
        private Rigidbody _tankRb;
        private float _reloading;
        private AudioSource _aS;
        private Button _shoot;

        private void Awake()
        {
            if(!body.GetPhotonView().IsMine) return;
            if(Application.platform == RuntimePlatform.Android) 
                _shoot = GameObject.Find("ShootBtn").GetComponent<Button>();
            _tankRb = body.GetComponent<Rigidbody>();
            //aS = GameObject.Find("tank_player1_head").GetComponent<AudioSource>();
        }

        private void Update()
        {
            if(!body.GetPhotonView().IsMine) return;
            _reloading -= Time.deltaTime;
            if(Application.platform == RuntimePlatform.Android && _shoot.onClick.GetPersistentTarget(0).name.Equals("ShootBtn"))
                _shoot.onClick.AddListener(Shoot);
        }

        public void Shoot()
        {
            if (_reloading > 0) return;
            var forward = spawn.transform.forward;
            _tankRb.AddForce(forward * (450 * 0.015f), ForceMode.VelocityChange);
            var angles = spawn.transform.rotation.eulerAngles;
            _reloading = 5.0f;
            var bulletInstance = PhotonNetwork.Instantiate("Bullet", spawn.transform.position, Quaternion.identity);
            bulletInstance.transform.localRotation = Quaternion.Euler(90, angles.y - 180, 0);
            bulletInstance.GetComponent<Rigidbody>().AddForce(-forward * (bulletForce * 0.02f), ForceMode.Impulse);
            ps.Play();
        }
    }
}
