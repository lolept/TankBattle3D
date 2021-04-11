using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class Shooting : MonoBehaviour
    {
        public Transform bullet;
        public GameObject spawn;
        public GameObject body;
        public GameObject head;
        public Rigidbody tankRb;
        public float reloading;
        public int bulletForce = 5000;
        public ParticleSystem ps;
        public AudioSource aS;
        public Button shoot;

        private void Start()
        {
            if(!body.GetPhotonView().IsMine) return;
            shoot = GameObject.Find("ShootBtn").GetComponent<Button>();
            tankRb = body.GetComponent<Rigidbody>();
            shoot.onClick.AddListener(Shoot);
            //aS = GameObject.Find("tank_player1_head").GetComponent<AudioSource>();
        }

        private void Update()
        {
            if(!body.GetPhotonView().IsMine) return;
            reloading -= Time.deltaTime;
            if (!Input.GetMouseButtonDown(1)) return;
            Shoot();
            //aS.Play();
        }

        public void Shoot()
        {
            if (reloading > 0) return;
            var forward = spawn.transform.forward;
            tankRb.AddForce(forward * (450 * 0.015f), ForceMode.VelocityChange);
            var angles = spawn.transform.rotation.eulerAngles;
            reloading = 5.0f;
            var bulletInstance = PhotonNetwork.Instantiate("Bullet", spawn.transform.position, Quaternion.identity);
            bulletInstance.transform.localRotation = Quaternion.Euler(90, angles.y - 180, 0);
            bulletInstance.GetComponent<Rigidbody>().AddForce(-forward * (bulletForce * 0.02f), ForceMode.Impulse);
            ps.Play();
        }
    }
}
