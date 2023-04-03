using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Net.Http;
using UnityEngine.XR;

public class BaseMovement : MonoBehaviour
{
    string website = "http://34.23.107.56/";
    static public string baseIP;
    static string robot_motion, robot_head;

    static bool useProxy = false;

    static HttpClientHandler hch = new HttpClientHandler 
    {
        UseProxy = useProxy,
    };

    static public readonly HttpClient client = new HttpClient(hch);

    private InputDevice rightController, leftController;

    bool crouched = false;
    int xAxis = 127;
    int yAxis = 127;

    // Start is called before the first frame update
    async void Start()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDeviceCharacteristics rcChara = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
        InputDeviceCharacteristics lcChara = InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
        InputDevices.GetDevicesWithCharacteristics(rcChara, devices);

        if (devices.Count > 0) { rightController = devices[0]; }

        InputDevices.GetDevicesWithCharacteristics(lcChara, devices);

        if (devices.Count > 0) { leftController = devices[0]; }

        baseIP = await client.GetStringAsync(website); // Calls the cloud website to get the updated IP address of the Raspberry Pi
        baseIP = "http://" + baseIP.Replace("\n", "").Replace("\r", "") + ":50000/";
        robot_head = baseIP + "motor?id=";
        robot_motion = baseIP + "motion/"; // Adds the trail to the IP address to access the motion API commands
        Debug.Log(robot_motion);  // Outputs the Pi IP to console if needed

    }

    /* Sit command leaves robot limp
     * Can use motor requests while sitting
     * Need to use pc_control request if motion requests are input in-between motor requests
    */
    // Update is called once per frame
    void FixedUpdate()
    {
        rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 rightVec);
        leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 leftVec);
        rightController.TryGetFeatureValue(CommonUsages.trigger, out float rightTrigger);
        rightController.TryGetFeatureValue(CommonUsages.grip, out float rightGrip);
        leftController.TryGetFeatureValue(CommonUsages.trigger, out float leftTrigger);
        leftController.TryGetFeatureValue(CommonUsages.grip, out float leftGrip);

        // Walk left/right movement
        if (leftVec.x < -0.5)
        {
            APICall(robot_motion + "walk_left");
        }
        else if (leftVec.x > 0.5)
        {
            APICall(robot_motion + "walk_right");
        }

        // Walk forward movement
        else if (leftVec.y > 0.5)
        {
            APICall(robot_motion + "walk_forward_short");
        }

        // Turn left/right movement
        else if (rightVec.x > 0.5)
        {
            APICall(robot_motion + "turn_right");
        }
        else if (rightVec.x < -.5)
        {
            APICall(robot_motion + "turn_left");
        }

        // Turn head right/left
        else if (rightTrigger > 0.3)
        {
            if (xAxis < 210) { xAxis += 1; }
            APICall(robot_head + $"23&position={xAxis}");
        }
        else if (leftTrigger > 0.3)
        {
            if (xAxis > 45) { xAxis -= 1; }
            APICall(robot_head + $"23&position={xAxis}");
        }

        // Turn head up/down
        else if (rightGrip > 0.3)
        {
            if (yAxis < 160) { yAxis += 1; }
            APICall(robot_head + $"24&position={yAxis}");
        }
        else if (leftGrip > 0.3)
        {
            if (yAxis > 100) { yAxis -= 1; }
            APICall(robot_head + $"24&position={yAxis}");
        }

        /* 
         * Crouch is inconsistent
         * Needs work as it resets all motor values
         * This includes the head and arm motors
         * Need these motors to still be responsive as the user will need to crouch and pick stuff up
         * 
         * Motion commands reset all individual motor values
         * Have to use individual motor requests to move everything
         * Some movement commands are incorrect in terms of default values: left upper shoulder 80 default
         * 
         */
         
        else if (rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out bool Bbutton) && Bbutton)
        {
            // APICall(robot_motion + "")
            if (!crouched) 
            {
                APICall(robot_motion + "sit_down");
                crouched = true;
            }
            else 
            { 
                APICall(robot_motion + "reset");
                crouched = false;
            }
        }
        
        // For fun :) ("dance_gangnamstyle")
        // For individual motor requests ("pc_control")
        else if (leftController.TryGetFeatureValue(CommonUsages.secondaryButton, out bool Ybutton) && Ybutton)
        {
            APICall(robot_motion + "pc_control");
            BodyTracking.motorIPNeck = BaseMovement.baseIP + "motor?id=23&position="; // Neck is for left/right
            BodyTracking.motorIPHead = BaseMovement.baseIP + "motor?id=24&position="; // Head is for up/down
        }

        // Put the robot back into intial standing position then walking position (left then right joystick click)
        else if (leftController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool LJClick) && LJClick)
        {
            APICall(robot_motion + "reset");
        }
        else if (rightController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool RJClick) && RJClick)
        {
            APICall(robot_motion + "basic_motion");
        }
    }

    static public async Task APICall(string ipAddress)
    {
        string responseBody = await client.GetStringAsync(ipAddress);
    }
}

