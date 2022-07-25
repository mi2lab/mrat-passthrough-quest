using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Avatar2;
using UnityEngine;
using UnityEngine.XR;
using Button = OVRInput.Button;
using Touch = OVRInput.Touch;

using Node = UnityEngine.XR.XRNode;

public class CustomedOVRInputManager : OvrAvatarInputManager
{
    /*
    public void UpdateTarget(Transform head_t, Transform left_hand_t, Transform right_hand_t, Transform tracking_space_t)
    {
        BodyTracking.InputTrackingDelegate.UpdateTarget(head_t, left_hand_t, right_hand_t, tracking_space_t);
    }
    */

    [SerializeField] private Transform _tackingSpace;
    [SerializeField] private Transform _headTarget;
    [SerializeField] private Transform _leftHandTarget;
    [SerializeField] private Transform _rightHandTarget;

    private class CustomedInputTrackingDelegate : OvrAvatarInputTrackingDelegate
    {
        private Transform _headTarget;
        private Transform _leftHandTarget;
        private Transform _rightHandTarget;
        private Transform _trackingSpace;

        public CustomedInputTrackingDelegate(Transform headTarget, Transform leftHandTarget, Transform rightHandTarget, Transform trackingSpace)
        {
            _headTarget = headTarget;
            _leftHandTarget = leftHandTarget;
            _rightHandTarget = rightHandTarget;
            _trackingSpace = trackingSpace;
        }
        
        public override bool GetRawInputTrackingState(out OvrAvatarInputTrackingState inputTrackingState)
        {
            inputTrackingState = new OvrAvatarInputTrackingState();
            inputTrackingState.headsetActive = true;
            inputTrackingState.leftControllerActive = true;
            inputTrackingState.rightControllerActive = true;
            inputTrackingState.leftControllerVisible = false;
            inputTrackingState.rightControllerVisible = false;

            Pose headPose = GetTrackingSpacePose(_headTarget.position, _headTarget.rotation);
            Pose leftHandPose = GetTrackingSpacePose(_leftHandTarget.position, _leftHandTarget.rotation);
            Pose rightHandPose = GetTrackingSpacePose(_rightHandTarget.position, _rightHandTarget.rotation);

            inputTrackingState.headset.position = headPose.position;
            inputTrackingState.headset.orientation = headPose.rotation;
            inputTrackingState.headset.scale = Vector3.one;
            inputTrackingState.leftController.position = leftHandPose.position;
            inputTrackingState.rightController.position = rightHandPose.position;
            inputTrackingState.leftController.orientation = leftHandPose.rotation;
            inputTrackingState.rightController.orientation = rightHandPose.rotation;
            inputTrackingState.leftController.scale = Vector3.one;
            inputTrackingState.rightController.scale = Vector3.one;

            return true;
        }

        private Pose GetTrackingSpacePose(Vector3 worldPosition, Quaternion worldRotation)
        {
            Vector3 position = _trackingSpace.InverseTransformPoint(worldPosition);
            Quaternion rotation = Quaternion.Inverse(_trackingSpace.rotation) * worldRotation;

            return new Pose(position, rotation);
        }
    }

    private class CustomInputControlDelegate : OvrAvatarInputControlDelegate
    {
        public override bool GetInputControlState(out OvrAvatarInputControlState inputControlState)
        {
            inputControlState = new OvrAvatarInputControlState();
            inputControlState.type = GetControllerType();

            UpdateControllerInput(ref inputControlState.leftControllerState, OVRInput.Controller.LTouch);
            UpdateControllerInput(ref inputControlState.rightControllerState, OVRInput.Controller.RTouch);

            return true;
        }

        private void UpdateControllerInput(ref OvrAvatarControllerState controllerState, OVRInput.Controller controller)
        {
            controllerState.buttonMask = 0;
            controllerState.touchMask = 0;

            // Button Press
            if (OVRInput.Get(Button.One, controller))
            {
                controllerState.buttonMask |= CAPI.ovrAvatar2Button.One;
            }
            if (OVRInput.Get(Button.Two, controller))
            {
                controllerState.buttonMask |= CAPI.ovrAvatar2Button.Two;
            }
            if (OVRInput.Get(Button.Three, controller))
            {
                controllerState.buttonMask |= CAPI.ovrAvatar2Button.Three;
            }
            if (OVRInput.Get(Button.PrimaryThumbstick, controller))
            {
                controllerState.buttonMask |= CAPI.ovrAvatar2Button.Joystick;
            }

            // Button Touch
            if (OVRInput.Get(Touch.One, controller))
            {
                controllerState.touchMask |= CAPI.ovrAvatar2Touch.One;
            }
            if (OVRInput.Get(Touch.Two, controller))
            {
                controllerState.touchMask |= CAPI.ovrAvatar2Touch.Two;
            }
            if (OVRInput.Get(Touch.PrimaryThumbstick, controller))
            {
                controllerState.touchMask |= CAPI.ovrAvatar2Touch.Joystick;
            }
            if (OVRInput.Get(Touch.PrimaryThumbRest, controller))
            {
                controllerState.touchMask |= CAPI.ovrAvatar2Touch.ThumbRest;
            }

            // Trigger
            controllerState.indexTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controller);
            if (OVRInput.Get(Touch.PrimaryIndexTrigger, controller))
            {
                controllerState.touchMask |= CAPI.ovrAvatar2Touch.Index;
            }
            else if (controllerState.indexTrigger <= 0f)
            {
                // TODO: Not sure if this is the correct way to do this
                controllerState.touchMask |= CAPI.ovrAvatar2Touch.Pointing;
            }

            // Grip
            controllerState.handTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, controller);

            // Set ThumbUp if no other thumb-touch is set.
            // TODO: Not sure if this is the correct way to do this
            if ((controllerState.touchMask & (CAPI.ovrAvatar2Touch.One | CAPI.ovrAvatar2Touch.Two |
                                              CAPI.ovrAvatar2Touch.Joystick | CAPI.ovrAvatar2Touch.ThumbRest)) == 0)
            {
                controllerState.touchMask |= CAPI.ovrAvatar2Touch.ThumbUp;
            }
        }
    }

    private void Start()
    {
        if (BodyTracking != null)
        {
            // BodyTracking.InputTrackingDelegate = new CustomInputTrackingDelegate(() => _currentInputTracking);
            BodyTracking.InputTrackingDelegate = new CustomedInputTrackingDelegate(_headTarget, _leftHandTarget, _rightHandTarget, _tackingSpace);
            BodyTracking.InputControlDelegate = new CustomInputControlDelegate();
        }
    }

    protected override void OnDestroyCalled()
    {
        base.OnDestroyCalled();
    }
}