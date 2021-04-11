using System;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace Game
{
    public class HeadRotation : MonoBehaviour
    {
        public GameObject go;
        public GameObject  goHead;
        public GameObject  goDulo;
        private float   _myAngle;
        public MouseLook mouseLook = new MouseLook();

        private void Start()
        {
            mouseLook.Init (transform, goDulo.transform);
        }

        private void Update () {
            mouseLook.LookRotation (goHead.transform, goDulo.transform);
        }

        private void FixedUpdate () {
            /*if (!Input.GetMouseButton(0)) return;
            _myAngle = 0;
            _myAngle = sensitivity*((_mousePos.x-(Screen.width/2))/Screen.width);
            goHead.transform.RotateAround(position, goHead.transform.up, _myAngle);*/
            
        }
    }
}