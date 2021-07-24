using System;
using Joystick_Pack.Scripts.Base;
using Photon.Pun;
using UnityEngine;

namespace Game
{
    public class MoveTank : MonoBehaviourPunCallbacks
    {
        //public int mPlayerNumber = 1;              // Used to identify which tank belongs to which player.  This is set by this tank's manager.
        public float mSpeed = 12f;                 // How fast the tank moves forward and back.
        public float mTurnSpeed = 50f;            // How fast the tank turns in degrees per second.
        //public AudioSource m_MovementAudio;         // Reference to the audio source used to play engine sounds. NB: different to the shooting audio source.
        //public AudioClip m_EngineIdling;            // Audio to play when the tank isn't moving.
        //public AudioClip m_EngineDriving;           // Audio to play when the tank is moving.
        //public float mPitchRange = 0.2f;             // The amount by which the pitch of the engine noises can vary.
        public GameObject tank;
        //public GameObject tankHead;
    

        private string _mMovementAxisName;
        private string _mTurnAxisName;
        private Rigidbody _mRigidbody;
        private float _mMovementInputValue;
        private float _mTurnInputValue;
        public Joystick joystick;


        private void Awake ()
        {
            _mRigidbody = tank.GetComponent<Rigidbody> ();
        }


        public override void OnEnable ()
        {
            // When the tank is turned on, make sure it's not kinematic.
            _mRigidbody.isKinematic = false;

            // Also reset the input values.
            _mMovementInputValue = 0f;
            _mTurnInputValue = 0f;
        }


        public override void OnDisable ()
        {
            // When the tank is turned off, set it to kinematic so it stops moving.
            _mRigidbody.isKinematic = true;
        }


        private void Start ()
        {
            if (!gameObject.GetComponent<PhotonView>().IsMine)
            {
                gameObject.GetComponentInChildren<Camera>().gameObject.SetActive(false);
                return;
            }
            gameObject.GetComponentInChildren<Camera>().gameObject.SetActive(true);
            //m_MovementAudio = this.gameObject.GetComponent<AudioSource>();
            _mMovementAxisName = "Vertical";
            _mTurnAxisName = "Horizontal";
            if (Application.platform == RuntimePlatform.Android)
                joystick = GameObject.Find("Fixed Joystick").GetComponent<Joystick>();
            //m_OriginalPitch = m_MovementAudio.pitch;
        }


        private void Update ()
        {
            if (!gameObject.GetComponent<PhotonView>().IsMine) return;
            if (Application.platform == RuntimePlatform.Android)
            {
                _mMovementInputValue = joystick.Vertical;
                _mTurnInputValue = joystick.Horizontal;
            }
            else
            {
                _mMovementInputValue = Input.GetAxis(_mMovementAxisName);
                _mTurnInputValue = Input.GetAxis(_mTurnAxisName);
            }

            //EngineAudio ();
        }

        /*
        private void EngineAudio ()
        {
            //if(!gameObject.GetComponent<PhotonView>().IsMine) return;
            // If there is no input (the tank is stationary)...
            if (Mathf.Abs (m_MovementInputValue) < 0.1f && Mathf.Abs (m_TurnInputValue) < 0.1f)
        {
            // ... and if the audio source is currently playing the driving clip...
            if (m_MovementAudio.clip == m_EngineDriving)
            {
                // ... change the clip to idling and play it.
                m_MovementAudio.clip = m_EngineIdling;
                m_MovementAudio.pitch = Random.Range (m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                m_MovementAudio.Play ();
            }
        }
        else
        {
            // Otherwise if the tank is moving and if the idling clip is currently playing...
            if (m_MovementAudio.clip == m_EngineIdling)
            {
                // ... change the clip to driving and play.
                m_MovementAudio.clip = m_EngineDriving;
                m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                m_MovementAudio.Play();
            }
        }
        }

*/
        private void FixedUpdate ()
        {
            if(!gameObject.GetComponent<PhotonView>().IsMine) return;
            Move ();
            Turn ();
        }


        private void Move ()
        {
            if(!gameObject.GetComponent<PhotonView>().IsMine) return;
            if(_mRigidbody.velocity.x >= 10 || _mRigidbody.velocity.x <= -10 || _mRigidbody.velocity.z >= 10 || _mRigidbody.velocity.z <= -10) return;
            if (Math.Abs(_mMovementInputValue) < 0.01f) return;
            _mRigidbody.AddForce(transform.forward * (-1 * _mMovementInputValue * mSpeed * _mRigidbody.mass * 2));
        }


        private void Turn ()
        {
            if(!gameObject.GetComponent<PhotonView>().IsMine) return;
            if (Math.Abs(_mTurnInputValue) > 0 && _mRigidbody.velocity != new Vector3(0, 0, 0))
            {
                var velocity1 = _mRigidbody.velocity;
                var velocity = velocity1;
                _mRigidbody.velocity = -transform.forward * ((float)Math.Sqrt(velocity.x * velocity.x + velocity.z * velocity.z) * Math.Abs(_mMovementInputValue) / _mMovementInputValue);
            }
            
            var turn = _mTurnInputValue * mTurnSpeed * Time.deltaTime * 3;
            var turnRotation = Quaternion.Euler (0f, turn, 0f);
            _mRigidbody.MoveRotation (_mRigidbody.rotation * turnRotation);
        }
    }
}
