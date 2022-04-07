using Joystick_Pack.Scripts.Base;
using Photon.Pun;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace Game
{
    public class HeadRotation : MonoBehaviour
    {
        private bool _isAndroid;
        private float _myAngle;
        public GameObject go;
        public GameObject goDulo;
        public GameObject goHead;

        public Joystick joystick;

        public MouseLook mouseLook = new MouseLook();
        public float sensitivity;

        
        private void Awake()
        {
            if (!go.GetPhotonView().IsMine) Destroy(this);
            if (Application.platform == RuntimePlatform.Android)
            {
                _isAndroid = true;
                joystick = FindObjectOfType<FloatingJoystick>(true);
            }
            else
            {
                mouseLook.Init(transform, goDulo.transform);
            }
        }

        private void Update()
        {
            if (_isAndroid)
            {
                _myAngle = 0;
                _myAngle = sensitivity * joystick.Horizontal;
                goHead.transform.RotateAround(goHead.transform.position, goHead.transform.up, _myAngle);
                if ((goDulo.transform.rotation.eulerAngles.x > 9 && joystick.Vertical > 0) || (goDulo.transform.rotation.eulerAngles.x < -9 && joystick.Vertical < 0)) return;
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