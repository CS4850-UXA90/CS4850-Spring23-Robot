using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using System;

public class BodyTracking : MonoBehaviour
{
    public static string motorIPHead, motorIPNeck;
    private InputDevice headset;

    int xRot, yRot;
    float xAngle, yAngle;

    int counter = 0;

    Renderer ren;
    
    // Start is called before the first frame update
    async void Start()
    {
        List<InputDevice> headsets = new List<InputDevice>();
        //InputDevices.GetDevicesAtXRNode(XRNode.Head, headsets);
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeadMounted, headsets);

        ren = GetComponent<Renderer>();
        

        if (headsets.Count == 1)
        {
            headset = headsets[0];
        }
    }

    // Update is called once per frame
    void FixedUpdate() 
    {
        counter++;
        headset.TryGetFeatureValue(CommonUsages.centerEyeRotation, out Quaternion headsetRotation);

        yAngle = headsetRotation[1] * 2;
        xAngle = headsetRotation[0] * 2;

        // Robot only accepts y values in range 100-160 and x values in range 1-254
        yRot = Convert.ToInt32(82*yAngle + 127); // 45 is left and 210 is right

        xRot = Convert.ToInt32(30*xAngle + 130); // Invert Y because 100 is up and 160 is down
        if (counter % 5 == 0)
        {
            BaseMovement.APICall(motorIPNeck + $"{yRot}"); // It's rotation about their respective axis, so rotating about the y-axis
            BaseMovement.APICall(motorIPHead + $"{xRot}"); // changes the left/right position, and about the x-axis changes up and down
            counter = 0;
        }

    }
}
