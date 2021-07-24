using Photon.Pun;
using System;
using UnityEngine;
using Joystick_Pack.Scripts.Base;
using UnityStandardAssets.CrossPlatformInput;
using Joystick = Joystick_Pack.Scripts.Base.Joystick;

namespace Game.Tank
{
    [RequireComponent(typeof (CarController))]
    [RequireComponent(typeof (Shooting))]
    public class CarUserControl : MonoBehaviour
    {
        private CarController _mCar;
        private Shooting _mShooting;
        public Joystick joystick;


        private void Awake()
        {
            if(!gameObject.GetPhotonView().IsMine) return;
            _mCar = GetComponent<CarController>();
            _mShooting = GetComponent<Shooting>();
            if(Application.platform == RuntimePlatform.Android)
                joystick = GameObject.Find("Fixed Joystick").GetComponent<Joystick>();
        }


        private void FixedUpdate()
        {
            if(!gameObject.GetPhotonView().IsMine) return;
            float h;
            float v;
            int shoot;
            if (Application.platform == RuntimePlatform.Android)
            {
                h = joystick.Horizontal;
                v = joystick.Vertical;
                shoot = 0;
            }
            else
            {
                h = CrossPlatformInputManager.GetAxis("Horizontal");
                v = CrossPlatformInputManager.GetAxis("Vertical");
                shoot = (int) Math.Round(CrossPlatformInputManager.GetAxis("Shoot"));
            }
            _mCar.Move(h, v, v);
            if(shoot == 1)
                _mShooting.Shoot();
        }
    }
}
