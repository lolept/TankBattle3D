using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class Shooting : MonoBehaviour
    {
        private float _reloading;
        private Button _shoot;
        private Rigidbody _tankRb;
        private bool _isTankMine;
        [SerializeField] private GameObject body;
        [SerializeField] private int bulletForce;
        [SerializeField] private ParticleSystem ps;
        [SerializeField] private Image rel;
        [SerializeField] private GameObject spawn;

        private void Awake()
        {
            _isTankMine = body.GetPhotonView().IsMine;
            if (!_isTankMine) enabled = false;
            _shoot = FindObjectOfType<ShootBtn>(true).GetComponent<Button>();
            _shoot.onClick.AddListener(Shoot);
            rel = FindObjectOfType<realoadingInd>(true).GetComponent<Image>();
            _tankRb = body.GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (!_isTankMine) return;
            _reloading -= Time.deltaTime;
            
        }

        public void Shoot()
        {
            if (!_isTankMine)
            {
                enabled = false;
                return;
            }
            if (_reloading > 0) return;
            var forward = spawn.transform.forward;
            _tankRb.AddForce(forward * bulletForce, ForceMode.Impulse);
            var angles = spawn.transform.rotation.eulerAngles;
            _reloading = 5.0f;
            StartCoroutine(SmoothAnim(true));
            var bulletInstance = PhotonNetwork.Instantiate("Bullet", spawn.transform.position, Quaternion.identity);
            bulletInstance.transform.localRotation = Quaternion.Euler(90, angles.y - 180, 0);
            bulletInstance.GetComponent<Rigidbody>().AddForce(-forward * bulletForce, ForceMode.Impulse);
            ps.Play();
        }

        private IEnumerator SmoothAnim(bool reduce)
        {
            if (reduce)
            {
                while (rel.fillAmount > 0)
                {
                    
                        rel.fillAmount -= 0.05f;
                        yield return new WaitForSeconds(0.01f);
                }

                StartCoroutine(SmoothAnim(false));
            }
            else
            {
                while (rel.fillAmount < 1)
                {
                    rel.fillAmount = (5.0f - _reloading) / 5.0f;
                    yield return new WaitForSeconds(0.01f);
                }
            }
        }
    }
}