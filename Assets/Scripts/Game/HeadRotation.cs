using System;
using Photon.Pun;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace Game
{
    public class HeadRotation : MonoBehaviour
    {
        public GameObject go;
        public GameObject  goHead;
        public GameObject  goDulo;
        private float _myAngle;
        public float sensitivity;
        
        public MouseLook mouseLook = new MouseLook();

        public Joystick joystick;

        private void Start()
        {
            if(!go.GetPhotonView().IsMine) return;
            if (Application.platform == RuntimePlatform.Android)
            {
                joystick = GameObject.Find("Floating Joystick").GetComponent<Joystick>();
            }
            else
            {
                mouseLook.Init (transform, goDulo.transform);
            }
        }

        private void Update () {
            if(!go.GetPhotonView().IsMine) return;
            if (Application.platform == RuntimePlatform.Android)
            {
                _myAngle = 0;
                _myAngle = sensitivity * joystick.Horizontal;
                goHead.transform.RotateAround(goHead.transform.position, goHead.transform.up, _myAngle);
                if (!(Mathf.Abs(goDulo.transform.rotation.eulerAngles.x) < 9)) return;
                _myAngle = 0;
                _myAngle = sensitivity * joystick.Vertical;
                goDulo.transform.RotateAround(goDulo.transform.position, goDulo.transform.right, _myAngle);
            }
            else
            {
                mouseLook.LookRotation(goHead.transform, goDulo.transform, 0, 0);
            }
        }
    }
}