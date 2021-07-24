using System;
using UnityEngine;

#pragma warning disable 649
namespace Game.Tank
{
    internal enum CarDriveType
    {
        FrontWheelDrive,
        RearWheelDrive,
        FourWheelDrive
    }

    internal enum SpeedType
    {
        MPH,
        KPH
    }

    public class CarController : MonoBehaviour
    {
        [SerializeField] private CarDriveType m_CarDriveType = CarDriveType.FourWheelDrive;
        [SerializeField] private WheelCollider[] m_WheelColliders = new WheelCollider[4];
        [SerializeField] private GameObject[] m_WheelMeshes = new GameObject[4];
        [SerializeField] private Vector3 m_CentreOfMassOffset;
        [Range(0, 1)] [SerializeField] private float m_SteerHelper;
        [Range(0, 1)] [SerializeField] private float m_TractionControl;
        [SerializeField] private float m_FullTorqueOverAllWheels;
        [SerializeField] private float m_ReverseTorque;
        [SerializeField] private float m_Downforce = 100f;
        [SerializeField] private SpeedType m_SpeedType;
        [SerializeField] private float m_Topspeed = 200;
        [SerializeField] private static int NoOfGears = 5;
        [SerializeField] private float m_RevRangeBoundary = 1f;
        [SerializeField] private float m_SlipLimit;
        [SerializeField] private float m_BrakeTorque;

        private Quaternion[] _mWheelMeshLocalRotations;
        private Vector3 _mPrevpos, _mPos;
        private float _mSteerAngle;
        private int _mGearNum;
        private float _mGearFactor;
        private float _mOldRotation;
        private float _mCurrentTorque;
        private Rigidbody _mRigidbody;
        private const float KReversingThreshold = 0.01f;

        public bool Skidding { get; private set; }
        public float CurrentSteerAngle{ get { return _mSteerAngle; }}
        public float CurrentSpeed{ get { return _mRigidbody.velocity.magnitude*2.23693629f; }}
        public float MaxSpeed{get { return m_Topspeed; }}
        public float Revs { get; private set; }
        public float AccelInput { get; private set; }

        // Use this for initialization
        private void Start()
        {
            _mWheelMeshLocalRotations = new Quaternion[4];
            for (int i = 0; i < 4; i++)
            {
                _mWheelMeshLocalRotations[i] = m_WheelMeshes[i].transform.localRotation;
            }
            m_WheelColliders[0].attachedRigidbody.centerOfMass = m_CentreOfMassOffset;

            _mRigidbody = GetComponent<Rigidbody>();
            _mCurrentTorque = m_FullTorqueOverAllWheels - (m_TractionControl*m_FullTorqueOverAllWheels);
        }


        private void GearChanging()
        {
            float f = Mathf.Abs(CurrentSpeed/MaxSpeed);
            float upgearlimit = (1/(float) NoOfGears)*(_mGearNum + 1);
            float downgearlimit = (1/(float) NoOfGears)*_mGearNum;

            if (_mGearNum > 0 && f < downgearlimit)
            {
                _mGearNum--;
            }

            if (f > upgearlimit && (_mGearNum < (NoOfGears - 1)))
            {
                _mGearNum++;
            }
        }


        // simple function to add a curved bias towards 1 for a value in the 0-1 range
        private static float CurveFactor(float factor)
        {
            return 1 - (1 - factor)*(1 - factor);
        }


        // unclamped version of Lerp, to allow value to exceed the from-to range
        private static float ULerp(float from, float to, float value)
        {
            return (1.0f - value)*from + value*to;
        }


        private void CalculateGearFactor()
        {
            float f = (1/(float) NoOfGears);
            // gear factor is a normalised representation of the current speed within the current gear's range of speeds.
            // We smooth towards the 'target' gear factor, so that revs don't instantly snap up or down when changing gear.
            var targetGearFactor = Mathf.InverseLerp(f*_mGearNum, f*(_mGearNum + 1), Mathf.Abs(CurrentSpeed/MaxSpeed));
            _mGearFactor = Mathf.Lerp(_mGearFactor, targetGearFactor, Time.deltaTime*5f);
        }


        private void CalculateRevs()
        {
            // calculate engine revs (for display / sound)
            // (this is done in retrospect - revs are not used in force/power calculations)
            CalculateGearFactor();
            var gearNumFactor = _mGearNum/(float) NoOfGears;
            var revsRangeMin = ULerp(0f, m_RevRangeBoundary, CurveFactor(gearNumFactor));
            var revsRangeMax = ULerp(m_RevRangeBoundary, 1f, gearNumFactor);
            Revs = ULerp(revsRangeMin, revsRangeMax, _mGearFactor);
        }


        public void Move(float steering, float accel, float footbrake)
        {
            for (int i = 0; i < 4; i++)
            {
                Quaternion quat;
                Vector3 position;
                m_WheelColliders[i].GetWorldPose(out position, out quat);
                m_WheelMeshes[i].transform.position = position;
                m_WheelMeshes[i].transform.rotation = quat;
            }

            steering = Mathf.Clamp(steering, -1, 1);
            AccelInput = accel = Mathf.Clamp(accel, 0, 1);
            footbrake = -1*Mathf.Clamp(footbrake, -1, 0);

            SteerHelper();
            ApplyDrive(accel, footbrake, steering);
            CapSpeed();
            
            CalculateRevs();
            GearChanging();

            AddDownForce();
            CheckForWheelSpin();
            TractionControl();
        }


        private void CapSpeed()
        {
            float speed = _mRigidbody.velocity.magnitude;
            switch (m_SpeedType)
            {
                case SpeedType.MPH:

                    speed *= 2.23693629f;
                    if (speed > m_Topspeed)
                        _mRigidbody.velocity = (m_Topspeed/2.23693629f) * _mRigidbody.velocity.normalized;
                    break;

                case SpeedType.KPH:
                    speed *= 3.6f;
                    if (speed > m_Topspeed)
                        _mRigidbody.velocity = (m_Topspeed/3.6f) * _mRigidbody.velocity.normalized;
                    break;
            }
        }


        private void ApplyDrive(float accel, float footbrake, float steering)
    {
        float thrustTorque;
        float thrustBackTorque;
        float deviation = 0.2f;
        switch (m_CarDriveType)
        {
            case CarDriveType.FourWheelDrive:
                thrustTorque = accel * (_mCurrentTorque / 4f);
                thrustBackTorque = -footbrake * (m_ReverseTorque / 4f);
                if (Math.Abs(steering) < deviation && footbrake >= deviation)
                {
                    m_WheelColliders[0].motorTorque = m_WheelColliders[1].motorTorque = thrustBackTorque;
                    m_WheelColliders[2].motorTorque = m_WheelColliders[3].motorTorque = thrustBackTorque;
                }
                else if (Math.Abs(steering) < deviation)
                {
                    m_WheelColliders[0].motorTorque = m_WheelColliders[1].motorTorque = thrustTorque;
                    m_WheelColliders[2].motorTorque = m_WheelColliders[3].motorTorque = thrustTorque;
                }
                else if (steering > 0 && accel >= deviation)
                {
                    m_WheelColliders[0].motorTorque = thrustTorque * (1 - steering);
                    m_WheelColliders[1].motorTorque = thrustTorque;
                    m_WheelColliders[2].motorTorque = thrustTorque * (1 - steering);
                    m_WheelColliders[3].motorTorque = thrustTorque;
                }
                else if (steering < 0 && accel >= deviation)
                {
                    m_WheelColliders[0].motorTorque = thrustTorque * (1 - steering);
                    m_WheelColliders[1].motorTorque = thrustTorque * steering;
                    m_WheelColliders[2].motorTorque = thrustTorque * (1 - steering);
                    m_WheelColliders[3].motorTorque = thrustTorque * steering;
                }
                else if (steering > 0 && footbrake >= deviation)
                {
                    m_WheelColliders[0].motorTorque = thrustBackTorque * (1 - steering);
                    m_WheelColliders[1].motorTorque = thrustBackTorque * steering;
                    m_WheelColliders[2].motorTorque = thrustBackTorque * (1 - steering);
                    m_WheelColliders[3].motorTorque = thrustBackTorque * steering;
                }
                else if (steering < 0 && footbrake >= deviation)
                {
                    m_WheelColliders[0].motorTorque = thrustBackTorque * (1 - steering);
                    m_WheelColliders[1].motorTorque = thrustBackTorque * steering;
                    m_WheelColliders[2].motorTorque = thrustBackTorque * (1 - steering);
                    m_WheelColliders[3].motorTorque = thrustBackTorque * steering;
                }
                else if (steering > 0 && accel < deviation)
                {
                    m_WheelColliders[0].motorTorque = m_ReverseTorque / 4f * -steering;
                    m_WheelColliders[1].motorTorque = m_ReverseTorque / 4f * steering;
                    m_WheelColliders[2].motorTorque = m_ReverseTorque / 4f * -steering;
                    m_WheelColliders[3].motorTorque = m_ReverseTorque / 4f * steering;
                }
                else if (steering < 0 && accel < deviation)
                {
                    m_WheelColliders[0].motorTorque = m_ReverseTorque / 4f * -steering;
                    m_WheelColliders[1].motorTorque = m_ReverseTorque / 4f * steering;
                    m_WheelColliders[2].motorTorque = m_ReverseTorque / 4f * -steering;
                    m_WheelColliders[3].motorTorque = m_ReverseTorque / 4f * steering;
                }
                break;

            case CarDriveType.RearWheelDrive:
                thrustTorque = accel * (_mCurrentTorque / 2f);
                m_WheelColliders[2].motorTorque = m_WheelColliders[3].motorTorque = thrustTorque;
                break;

        }
        for (int i = 0; i < 4; i++)
        {
            if (Math.Abs(CurrentSpeed) > 0.1f && accel < deviation && footbrake < deviation && Math.Abs(steering) < deviation)
            {
                m_WheelColliders[i].brakeTorque = m_BrakeTorque / 20;
            }
            else if (Math.Abs(CurrentSpeed) > 0.3f && accel < deviation && footbrake < deviation && steering > deviation)
            {
                m_WheelColliders[i].brakeTorque = m_BrakeTorque / 50;
            }
            else if (CurrentSpeed > 2.5f && Vector3.Angle(transform.forward, _mRigidbody.velocity) < 50f)
            {
                m_WheelColliders[i].brakeTorque = m_BrakeTorque * footbrake;
            }
            else if (CurrentSpeed > 0.5f && Vector3.Angle(transform.forward, _mRigidbody.velocity) > 50f)
            {
                m_WheelColliders[i].brakeTorque = m_BrakeTorque * accel;
            }
            else
            {
                m_WheelColliders[i].brakeTorque = 0;
            }
        }
    }


        private void SteerHelper()
        {
            for (int i = 0; i < 4; i++)
            {
                WheelHit wheelhit;
                m_WheelColliders[i].GetGroundHit(out wheelhit);
                if (wheelhit.normal == Vector3.zero)
                    return; // wheels arent on the ground so dont realign the rigidbody velocity
            }

            // this if is needed to avoid gimbal lock problems that will make the car suddenly shift direction
            if (Mathf.Abs(_mOldRotation - transform.eulerAngles.y) < 10f)
            {
                var turnadjust = (transform.eulerAngles.y - _mOldRotation) * m_SteerHelper;
                Quaternion velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
                _mRigidbody.velocity = velRotation * _mRigidbody.velocity;
            }
            _mOldRotation = transform.eulerAngles.y;
        }


        // this is used to add more grip in relation to speed
        private void AddDownForce()
        {
            m_WheelColliders[0].attachedRigidbody.AddForce(-transform.up*m_Downforce*
                                                         m_WheelColliders[0].attachedRigidbody.velocity.magnitude);
        }
        private void CheckForWheelSpin()
        {
            for (int i = 0; i < 4; i++)
            {
                WheelHit wheelHit;
                m_WheelColliders[i].GetGroundHit(out wheelHit);
                
            }
        }

        private void TractionControl()
        {
            WheelHit wheelHit;
            switch (m_CarDriveType)
            {
                case CarDriveType.FourWheelDrive:
                    // loop through all wheels
                    for (int i = 0; i < 4; i++)
                    {
                        m_WheelColliders[i].GetGroundHit(out wheelHit);

                        AdjustTorque(wheelHit.forwardSlip);
                    }
                    break;

                case CarDriveType.RearWheelDrive:
                    m_WheelColliders[2].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);

                    m_WheelColliders[3].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);
                    break;

                case CarDriveType.FrontWheelDrive:
                    m_WheelColliders[0].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);

                    m_WheelColliders[1].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);
                    break;
            }
        }


        private void AdjustTorque(float forwardSlip)
        {
            if (forwardSlip >= m_SlipLimit && _mCurrentTorque >= 0)
            {
                _mCurrentTorque -= 10 * m_TractionControl;
            }
            else
            {
                _mCurrentTorque += 10 * m_TractionControl;
                if (_mCurrentTorque > m_FullTorqueOverAllWheels)
                {
                    _mCurrentTorque = m_FullTorqueOverAllWheels;
                }
            }
        }
    }
}
