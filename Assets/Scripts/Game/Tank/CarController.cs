using System;
using Photon.Pun;
using UnityEngine;

#pragma warning disable 649
namespace Game.Tank
{
    public class CarController : MonoBehaviour
    {
        private const int NoOfGears = 5;
        private float _mCurrentTorque;
        private float _mGearFactor;
        private int _mGearNum;
        private float _mOldRotation;
        private Rigidbody _mRigidbody;

        private Quaternion[] _mWheelMeshLocalRotations;
        [SerializeField] private float brakeTorque;
        [SerializeField] private Vector3 centreOfMassOffset;
        [Range(0, 1000)][SerializeField] private float downForce;
        [Range(2500, 7500)][SerializeField] private float fullTorqueOverAllWheels;
        [Range(2500, 7500)][SerializeField] private float reverseTorque;
        [SerializeField] private float revRangeBoundary = 1f;
        [SerializeField] private float slipLimit;
        [Range(0, 1)] [SerializeField] private float steerHelper;
        [Range(10, 150)][SerializeField] private float topSpeed = 200;
        [Range(0, 1)] [SerializeField] private float tractionControl;
        [SerializeField] private WheelCollider[] wheelColliders = new WheelCollider[4];
        [SerializeField] private GameObject[] wheelMeshes = new GameObject[4];

        private float CurrentSpeed => _mRigidbody.velocity.magnitude * 2.23693629f;
        private float MaxSpeed => topSpeed;

        private void Awake()
        {
            if(!gameObject.GetPhotonView().IsMine) enabled = false;
        }

        private void Start()
        {
            _mWheelMeshLocalRotations = new Quaternion[4];
            for (var i = 0; i < 4; i++) _mWheelMeshLocalRotations[i] = wheelMeshes[i].transform.localRotation;
            wheelColliders[0].attachedRigidbody.centerOfMass = centreOfMassOffset;

            _mRigidbody = GetComponent<Rigidbody>();
            _mCurrentTorque = fullTorqueOverAllWheels - tractionControl * fullTorqueOverAllWheels;
        }


        private void GearChanging()
        {
            var f = Mathf.Abs(CurrentSpeed / MaxSpeed);
            var upGearLimit = 1 / (float) NoOfGears * (_mGearNum + 1);
            var downGearLimit = 1 / (float) NoOfGears * _mGearNum;

            if (_mGearNum > 0 && f < downGearLimit) _mGearNum--;

            if (f > upGearLimit && _mGearNum < NoOfGears - 1) _mGearNum++;
        }
        
        private static float CurveFactor(float factor)
        {
            return 1 - (1 - factor) * (1 - factor);
        }
        
        private static float ULerp(float from, float to, float value)
        {
            return (1.0f - value) * from + value * to;
        }


        private void CalculateGearFactor()
        {
            var f = 1 / (float) NoOfGears;
            var targetGearFactor =
                Mathf.InverseLerp(f * _mGearNum, f * (_mGearNum + 1), Mathf.Abs(CurrentSpeed / MaxSpeed));
            _mGearFactor = Mathf.Lerp(_mGearFactor, targetGearFactor, Time.deltaTime * 5f);
        }


        private void CalculateRevs()
        {
            CalculateGearFactor();
            var gearNumFactor = _mGearNum / (float) NoOfGears;
            var revsRangeMin = ULerp(0f, revRangeBoundary, CurveFactor(gearNumFactor));
            var revsRangeMax = ULerp(revRangeBoundary, 1f, gearNumFactor);
            ULerp(revsRangeMin, revsRangeMax, _mGearFactor);
        }


        public void Move(float steering, float accel, float footbrake)
        {
            for (var i = 0; i < 4; i++)
            {
                wheelColliders[i].GetWorldPose(out var position, out var quat);
                wheelMeshes[i].transform.position = position;
                wheelMeshes[i].transform.rotation = quat;
            }

            steering = Mathf.Clamp(steering, -1, 1);
            accel = Mathf.Clamp(accel, 0, 1);
            footbrake = -1 * Mathf.Clamp(footbrake, -1, 0);

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
            var speed = _mRigidbody.velocity.magnitude;
            
            speed *= 3.6f;
            if (speed > topSpeed)
                _mRigidbody.velocity = topSpeed / 3.6f * _mRigidbody.velocity.normalized;
        }


        private void ApplyDrive(float accel, float footbrake, float steering)
        {
            const float deviation = 0.2f;
            var thrustTorque = accel * (_mCurrentTorque / 4f);
            var thrustBackTorque = -footbrake * (reverseTorque / 4f);
            var steerAbs = Math.Abs(steering);
            if (steerAbs < deviation) steering = 0;
            if (steerAbs < deviation)
            {
                wheelColliders[0].motorTorque = wheelColliders[1].motorTorque = thrustBackTorque + thrustTorque;
                wheelColliders[2].motorTorque = wheelColliders[3].motorTorque = thrustBackTorque + thrustTorque;
            }
            else if (footbrake >= deviation || accel >= deviation)
            {
                wheelColliders[0].motorTorque = thrustTorque * (steerAbs - steering) / 2 + thrustBackTorque * (steerAbs - steering) / 2;
                wheelColliders[1].motorTorque = thrustTorque * (steerAbs + steering) / 2 + thrustBackTorque * (steerAbs + steering) / 2;
                wheelColliders[2].motorTorque = thrustTorque * (steerAbs - steering) / 2 + thrustBackTorque * (steerAbs - steering) / 2;
                wheelColliders[3].motorTorque = thrustTorque * (steerAbs + steering) / 2 + thrustBackTorque * (steerAbs + steering) / 2;
            }
            else
            {
                wheelColliders[0].motorTorque = reverseTorque / 4f * -steering;
                wheelColliders[1].motorTorque = reverseTorque / 4f * steering;
                wheelColliders[2].motorTorque = reverseTorque / 4f * -steering;
                wheelColliders[3].motorTorque = reverseTorque / 4f * steering;
            }

            for (var i = 0; i < 4; i++)
                if (Math.Abs(CurrentSpeed) > 0.1f && accel < deviation && footbrake < deviation &&
                    Math.Abs(steering) < deviation)
                    wheelColliders[i].brakeTorque = brakeTorque / 20;
                else if (Math.Abs(CurrentSpeed) > 0.3f && accel < deviation && footbrake < deviation &&
                         steering > deviation)
                    wheelColliders[i].brakeTorque = brakeTorque / 50;
                else if (CurrentSpeed > 2.5f && Vector3.Angle(transform.forward, _mRigidbody.velocity) < 50f)
                    wheelColliders[i].brakeTorque = brakeTorque * footbrake;
                else if (CurrentSpeed > 0.5f && Vector3.Angle(transform.forward, _mRigidbody.velocity) > 50f)
                    wheelColliders[i].brakeTorque = brakeTorque * accel;
                else
                    wheelColliders[i].brakeTorque = 0;
        }


        private void SteerHelper()
        {
            for (var i = 0; i < 4; i++)
            {
                wheelColliders[i].GetGroundHit(out var wheelHit);
                if (wheelHit.normal == Vector3.zero)
                    return;
            }

            if (Mathf.Abs(_mOldRotation - transform.eulerAngles.y) < 10f)
            {
                var turnAdjust = (transform.eulerAngles.y - _mOldRotation) * steerHelper;
                var velRotation = Quaternion.AngleAxis(turnAdjust, Vector3.up);
                _mRigidbody.velocity = velRotation * _mRigidbody.velocity;
            }

            _mOldRotation = transform.eulerAngles.y;
        }


        private void AddDownForce()
        {
            wheelColliders[0].attachedRigidbody
                .AddForce(-transform.up * (downForce * wheelColliders[0].attachedRigidbody.velocity.magnitude));
        }

        private void CheckForWheelSpin()
        {
            for (var i = 0; i < 4; i++) wheelColliders[i].GetGroundHit(out _);
        }

        private void TractionControl()
        {
            for (var i = 0; i < 4; i++)
            {
                wheelColliders[i].GetGroundHit(out var wheelHit);

                AdjustTorque(wheelHit.forwardSlip);
            }
        }


        private void AdjustTorque(float forwardSlip)
        {
            if (forwardSlip >= slipLimit && _mCurrentTorque >= 0)
            {
                _mCurrentTorque -= 10 * tractionControl;
            }
            else
            {
                _mCurrentTorque += 10 * tractionControl;
                if (_mCurrentTorque > fullTorqueOverAllWheels) _mCurrentTorque = fullTorqueOverAllWheels;
            }
        }
    }
}