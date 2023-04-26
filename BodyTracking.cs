using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using System;

public class BodyTracking : MonoBehaviour
{
    public static string motorIPHead, motorIPNeck; //, motorIPArms;
    private InputDevice headset; //, rightController, leftController;
    //private int[] armRotations = {0, 0, 0, 0}; // Left upper, left lower, right upper, right lower

    int xRot, yRot;
    float xAngle, yAngle;

    int counter = 0;
    public static bool move = false;
    
    // Start is called before the first frame update
    async void Start()
    {
        List<InputDevice> headsets = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeadMounted, headsets);

        if (headsets.Count == 1) { headset = headsets[0]; }

        //List<InputDevice> devices = new List<InputDevice>();
        //InputDeviceCharacteristics rcChara = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
        //InputDeviceCharacteristics lcChara = InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
        //InputDevices.GetDevicesWithCharacteristics(rcChara, devices);

        //if (devices.Count > 0) { rightController = devices[0]; }

        //InputDevices.GetDevicesWithCharacteristics(lcChara, devices);

        //if (devices.Count > 0) { leftController = devices[0]; }
    }

    // Update is called once per frame
    async void FixedUpdate()
    {
        counter++;
        headset.TryGetFeatureValue(CommonUsages.centerEyeRotation, out Quaternion headsetRotation);

        yAngle = headsetRotation[1] * 2;
        xAngle = headsetRotation[0] * 2;

        // Robot only accepts y values in range 100-160 and x values in range 1-254
        // Slope sign is dependant on where user is facing when using the headset, ie. if the user is facing one direction the
        // signs may be positive, but if the user turns around and initializes the headset then the signs would be the opposite
        // Slope correlates to relative user direction
        yRot = Convert.ToInt32(-82*yAngle + 127); // 45 is left and 210 is right

        xRot = Convert.ToInt32(-30*xAngle + 130); // Invert Y because 100 is up and 160 is down
        if (counter % 5 == 0 && move)
        {
            string neckIP = motorIPNeck + yRot.ToString();
            string headIP = motorIPHead + xRot.ToString();
            await BaseMovement.APICall(neckIP); // It's rotation about their respective axis, so rotating about the y-axis
            await BaseMovement.APICall(headIP); // changes the left/right position, and about the x-axis changes up and down
            counter = 0;
        }

        // Note for later: application needs to be started from the same position the headset was put on in
        // The application sets the initial zero vector when the headset is put on. If the headset
        // is put on then the user rotates, the application won't work properly. Make sure to put the headset
        // on and not rotate from the initial position.

        // for arms
        // get controller quaternion for position, not rotation

        // x position: lower-shoulder, arm, elbow
        // y position: upper-shoulder, arm, elbow
        // z position: upper-shoulder, arm, elbow

        // Left upper shoulder - id:12 ; min: 10 ; max:254 ; default:180 ; inverted:true ; min is slightly behind the user, max is straight up
        // Right upper shoulder - id:13 ; min: 10 ; max:254 ; default:180 ; inverted:false ; min is straight up, max is
        // Left lower shoulder - id:14 ; min:135 ; max:254 ; default:135 ; inverted:false
        // Right lower shoulder - id:15 ; min:1 ; max:120 ; default:120 ; inverted:true

        // lus - max 254; min 50
        // rus - max 200; min 10
        // lls - same
        // rls - same

        
        /*
        leftController.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 leftPosition);
        rightController.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 rightPosition);

        Quaternion leftQuat = Quaternion.Euler(leftPosition);
        Quaternion rightQuat = Quaternion.Euler(rightPosition);

        armRotations[0] = Convert.ToInt32(102 * leftQuat.y + 152); // leftQuat[1] + 152); // May need to change these values later to function with both y and z
        armRotations[2] = Convert.ToInt32(-95 * rightQuat.y + 152); // rightQuat[1] + 105);

        
        //armRotations[1] = Convert.ToInt32(59.5 * leftPosition.x + 194.5); // leftQuat[0] + 194.5);
        //armRotations[3] = Convert.ToInt32(-59.5 * rightPosition.x + 59.5); // rightQuat[0] + 59.5);

        if (counter % 10 == 0)
        {
            await BaseMovement.APICall(motorIPArms + $"12&position={armRotations[0]}");
            //BaseMovement.APICall(motorIPArms + $"14&position={armRotations[1]}");
            await BaseMovement.APICall(motorIPArms + $"13&position={armRotations[2]}");
            //BaseMovement.APICall(motorIPArms + $"15&position={armRotations[3]}");

            counter = 0;
        }
        */
        

    }
}
