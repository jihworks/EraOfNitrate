// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

#if INFRASTRUCTURE_USE_CINEMACHINE

using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Jih.Unity.Infrastructure.Cinemachine
{
    /// <remarks>
    /// * Setup Process:<br/>
    /// 1. Add a <b>Cinemachine Camera</b> to the scene.<br/>
    /// 2. Add a <b>Empty</b> Game Object to the scene.<br/>
    /// 3. Allocate the Game Object previously created to Cinemachine Camera's <b>Tracking Target</b>.<br/>
    /// 4. Select <b>Follow</b> from Cinemachine Camera component's <b>Position Control</b>.<br/>
    /// 5. Select <b>Hard Look At</b> from Cinemachine Camera component's <b>Rotation Control</b>.<br/>
    /// 6. Add <b>this script</b> to the Cinemachine Camera. But, have to implement abstract method first.<br/>
    /// <br/>
    /// Initial Y of Tracking Target should be configured properly before use. Generally, <c>0</c>.<br/>
    /// <br/>
    /// If camera is too much shaking when moving, change Position Damping to lower value from Cinemachine Follow.
    /// </remarks>
    [RequireComponent(typeof(CinemachineCamera))]
    [RequireComponent(typeof(CinemachineFollow))]
    public abstract class BaseStrategyCamera : MonoBehaviour
    {
        [Header("Properties")]
        [Tooltip("Effects to Orbit, Zoom and Target Y.")]
        [SerializeField] float _cameraOffsetSmoothTime = 0.2f;
        [Tooltip("Effects to Movement.")]
        [SerializeField] float _movementLocationSmoothTime = 0.2f;

        [Space(12f)]
        [Tooltip("Movement Speed by Camera Distance(Normalized Position)")]
        [SerializeField] AnimationCurve _movementSpeedCurve = new(new Keyframe(0f, 20f), new Keyframe(1f, 80f));
        [SerializeField] float _sprintScale = 2f;

        [Space(6f)]
        [Tooltip("Zoom Speed by Camera Distance(Normalized Position)")]
        [SerializeField] AnimationCurve _zoomSpeedCurve = new(new Keyframe(0f, 20f), new Keyframe(1f, 80f));

        [Space(12f)]
        [Tooltip("Degrees per second")]
        [SerializeField] float _cameraOrbitYawSpeed = 60f;

        [Space(6f)]
        [Tooltip("Camera Orbit Pitch by Camera Distance(Normalized Position)")]
        [SerializeField] AnimationCurve _cameraPitchCurve = new(new Keyframe(0f, 0f), new Keyframe(1f, 45f));

        [Space(12f)]
        [Tooltip("Maximum Zoom in distance.")]
        [SerializeField] float _minCameraDistance = 30f;
        [Tooltip("Maximum Zoom out distance.")]
        [SerializeField] float _maxCameraDistance = 200f;

        [Space(6f)]
        [Tooltip("Camera Tracking Target's Y by Camera Distance(Normalized Position)")]
        [SerializeField] AnimationCurve _cameraTargetYCurve = new(new Keyframe(0f, 4f), new Keyframe(1f, 0f));
        
        [Header("Monitoring")]
        [Tooltip("Editing this value has no effect. Edit 'Movement Speed Curve'.")]
        [SerializeField] float _currentMovementSpeed = 80f;
        [Tooltip("Editing this value has no effect. Edit 'Zoom Speed Curve'.")]
        [SerializeField] float _currentZoomSpeed = 20f;

        [Space(12f)]
        [SerializeField] float _currentCameraDistance = 100f;
        [SerializeField] float _currentCameraOrbitYaw = 45f;
        [Tooltip("Editing this value has no effect. Edit 'Camera Pitch Curve'")]
        [SerializeField] float _currentCameraOrbitPitch = 45f;

        [Space(6f)]
        [Tooltip("Editing this value has no effect. Edit 'Camera Target Y Curve'.")]
        [SerializeField] float _currentTargetY = 0f;

        CinemachineCamera? _cinemachineCamera;
        public CinemachineCamera CinemachineCamera
        {
            get
            {
                if (_cinemachineCamera == null)
                {
                    _cinemachineCamera = GetComponent<CinemachineCamera>();
                }
                return _cinemachineCamera;
            }
        }

        CinemachineFollow? _cinemachineFollow;
        public CinemachineFollow CinemachineFollow
        {
            get
            {
                if (_cinemachineFollow == null)
                {
                    _cinemachineFollow = GetComponent<CinemachineFollow>();
                }
                return _cinemachineFollow;
            }
        }

        Transform? _tackingTarget;
        public Transform TrackingTarget
        {
            get
            {
                if (_tackingTarget == null)
                {
                    _tackingTarget = CinemachineCamera.Follow;
                }
                return _tackingTarget.ThrowIfNull(nameof(TrackingTarget));
            }
        }

        float _initialCameraDistance, _initialCameraOrbitYaw;
        Vector3 _currentCameraOffset, _dampCameraOffset;

        // Y value not used.
        Vector3 _initialMoveLocation;
        Vector3 _currentMoveLocation, _dampMoveLocation;

        float _initialTargetYValue;
        float _currentTargetYValue, _dampTargetYValue;

        protected virtual void Awake()
        {
            MathEx.Clamp(ref _currentCameraDistance, _minCameraDistance, _maxCameraDistance);

            float initialAlpha = GetCurrentCameraDistanceAlpha();
            _currentZoomSpeed = _zoomSpeedCurve.Evaluate(initialAlpha);
        }

        protected virtual void Start()
        {
            _initialCameraDistance = _currentCameraDistance;
            _initialCameraOrbitYaw = _currentCameraOrbitYaw;

            UpdateCameraByDistance();

            CinemachineFollow.FollowOffset = _currentCameraOffset = GetCurrentCameraOffset();

            _currentMoveLocation = _initialMoveLocation = TrackingTarget.position;
            _currentTargetYValue = _initialTargetYValue = _initialMoveLocation.y;
        }

        protected virtual void Update()
        {
            UpdateMovementInput();
            UpdateCameraDistanceInput();
            UpdateCameraYawInput();

            {
                Vector3 target = GetCurrentCameraOffset();
                _currentCameraOffset = Vector3.SmoothDamp(_currentCameraOffset, target, ref _dampCameraOffset, _cameraOffsetSmoothTime, float.PositiveInfinity, Time.deltaTime);

                CinemachineFollow.FollowOffset = _currentCameraOffset;
            }
            {
                _currentTargetYValue = Mathf.SmoothDamp(_currentTargetYValue, _initialTargetYValue + _currentTargetY, ref _dampTargetYValue, _cameraOffsetSmoothTime, float.PositiveInfinity, Time.deltaTime);

                Vector3 location = TrackingTarget.position;
                location.y = _currentTargetYValue;
                TrackingTarget.position = location;
            }
        }

        public void ResetCameraAndMovement()
        {
            _dampCameraOffset = Vector3.zero;
            _dampMoveLocation = Vector3.zero;
            _dampTargetYValue = 0f;

            _currentCameraOrbitYaw = _initialCameraOrbitYaw;

            _currentCameraDistance = _initialCameraDistance;
            UpdateCameraByDistance();

            CinemachineFollow.FollowOffset = _currentCameraOffset = GetCurrentCameraOffset();

            TrackingTarget.position = _currentMoveLocation = _initialMoveLocation;
            _currentTargetYValue = _initialTargetYValue;
        }

        void UpdateCameraByDistance()
        {
            float alpha = GetCurrentCameraDistanceAlpha();

            _currentCameraOrbitPitch = _cameraPitchCurve.Evaluate(alpha);

            _currentTargetY = _cameraTargetYCurve.Evaluate(alpha);

            _currentMovementSpeed = _movementSpeedCurve.Evaluate(alpha);
            _currentZoomSpeed = _zoomSpeedCurve.Evaluate(alpha);
        }

        void UpdateMovementInput()
        {
            Vector2 moveInput = ReadMoveInput();
            float sprintInput = ReadSprintInput();

            if (moveInput.x != 0f || moveInput.y != 0f)
            {
                moveInput = moveInput.normalized;
            }

            float moveSpeed = _currentMovementSpeed;
            if (sprintInput > 0f)
            {
                moveSpeed *= _sprintScale;
            }

            Quaternion yaw = Quaternion.AngleAxis(_currentCameraOrbitYaw, Vector3.up);

            Vector3 moveDir = new(moveInput.x, 0f, moveInput.y);
            moveDir = yaw * moveDir;

            Vector3 delta = moveDir * moveSpeed;

            _currentMoveLocation = Vector3.SmoothDamp(_currentMoveLocation, _currentMoveLocation + delta, ref _dampMoveLocation, _movementLocationSmoothTime, float.PositiveInfinity, Time.deltaTime);

            ModifyCameraMoveLocation(ref _currentMoveLocation);

            {
                Vector3 location = TrackingTarget.position;
                location.x = _currentMoveLocation.x;
                location.z = _currentMoveLocation.z;
                TrackingTarget.position = location;
            }
        }

        void UpdateCameraYawInput()
        {
            float yawInput = ReadOrbitYawInput();
            if (yawInput == 0f)
            {
                return;
            }

            float delta = _cameraOrbitYawSpeed * -yawInput * Time.deltaTime;
            _currentCameraOrbitYaw = MathEx.CollapseDegrees(_currentCameraOrbitYaw + delta);
        }

        void UpdateCameraDistanceInput()
        {
            float inputValue = ReadZoomInput();
            if (inputValue == 0f)
            {
                return;
            }

            float delta = _currentZoomSpeed * -inputValue;
            _currentCameraDistance = Math.Clamp(_currentCameraDistance + delta, _minCameraDistance, _maxCameraDistance);

            UpdateCameraByDistance();
        }

        Vector3 GetCurrentCameraOffset()
        {
            Vector3 dir = Vector3.back;
            Quaternion pitch = Quaternion.AngleAxis(_currentCameraOrbitPitch, Vector3.right);
            Quaternion yaw = Quaternion.AngleAxis(_currentCameraOrbitYaw, Vector3.up);
            dir = pitch * dir;
            dir = yaw * dir;
            return dir * _currentCameraDistance;
        }

        float GetCurrentCameraDistanceAlpha()
        {
            float distDelta = _maxCameraDistance - _minCameraDistance;
            return (_currentCameraDistance - _minCameraDistance).SafeDivide(distDelta);
        }

        protected virtual void ModifyCameraMoveLocation(ref Vector3 location)
        {
        }

        /// <returns>X value effects left-right movement and Y value effects forward-back movement.</returns>
        protected abstract Vector2 ReadMoveInput();
        /// <returns>
        /// Positive value will apply sprint scale to movement speed.<br/>
        /// Expecting [0, 1].
        /// </returns>
        protected abstract float ReadSprintInput();
        /// <returns>
        /// Positive value will rotate Yaw of the camera to CCW direction. Negative value will rotate CW direction.<br/>
        /// Expecting [-1, 1]
        /// </returns>
        protected abstract float ReadOrbitYawInput();
        /// <returns>
        /// Positive value will zoom out the camera. Negative value will zoom in.<br/>
        /// Represents the zoom increment or decrement count in the current frame.
        /// </returns>
        protected abstract float ReadZoomInput();
    }
}

#endif
