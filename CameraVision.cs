using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Net.Http;
using System;

public class CameraVision : MonoBehaviour
{
    string website = "http://34.23.107.56/camera";
    static string cameraIP;

    static bool useProxy = false;

    static HttpClientHandler hch = new HttpClientHandler
    {
        UseProxy = useProxy,
    };

    static public readonly HttpClient client = new HttpClient(hch);


    /// <param name="streamAddress">
    /// Set this to be the network address of the mjpg stream. 
    /// Example: "http://extcam-16.se.axis.com/mjpg/video.mjpg"
    /// </param>
    [Tooltip("Set this to be the network address of the mjpg stream. ")]
    public string streamAddress;

    /// <summary>
    /// Chunk size for stream processor in kilobytes.
    /// </summary>
    [Tooltip("Chunk size for stream processor in kilobytes.")]
    public int chunkSize = 4;

    Texture2D tex;

    const int initWidth = 2;
    const int initHeight = 2;

    bool updateFrame = false;

    MjpegProcessor mjpeg;

    float deltaTime = 0.0f;
    float mjpegDeltaTime = 0.0f;





    public async void Start()
    {
        cameraIP = await client.GetStringAsync(website);
        cameraIP = "http://" + cameraIP.Replace("\n", "").Replace("\r", "") + ":50000/stream.mjpg";

        mjpeg = new MjpegProcessor(chunkSize * 1024);
        mjpeg.FrameReady += OnMjpegFrameReady;
        mjpeg.Error += OnMjpegError;
        Uri mjpegAddress = new Uri(cameraIP);
        mjpeg.ParseStream(mjpegAddress);
        // Create a 16x16 texture with PVRTC RGBA4 format
        // and will it with raw PVRTC bytes.
        tex = new Texture2D(initWidth, initHeight, TextureFormat.PVRTC_RGBA4, false);
    }
    private void OnMjpegFrameReady(object sender, FrameReadyEventArgs e)
    {
        updateFrame = true;
    }
    void OnMjpegError(object sender, ErrorEventArgs e)
    {
        Debug.Log("Error received while reading the MJPEG.");
    }

    // Update is called once per frame
    void Update()
    {
        deltaTime += Time.deltaTime;

        if (updateFrame)
        {
            tex.LoadImage(mjpeg.CurrentFrame);
            // tex.Apply();
            // Assign texture to renderer's material.
            GetComponent<Renderer>().material.mainTexture = tex;
            updateFrame = false;

            mjpegDeltaTime += (deltaTime - mjpegDeltaTime) * 0.2f;

            deltaTime = 0.0f;
        }
    }

    // Below is the code that creates a plane and puts the stream texture on an object. If using the code below, attach this script to the main camera.
    // Credits to the two repositories that made this possible:
    // Attaching a texture to a summoned plane: https://github.com/EvgenyMuryshkin/qvr
    // MJPEG Stream texture: https://github.com/DanielArnett/SampleUnityMjpegViewer


    /*
    GameObject plane;
    Texture2D tex;
    byte[] frame = null;
    Vector3 baselineForward;

    [Tooltip("Chunk size for stream processor in kilobytes.")]
    public int chunkSize = 4;

    const int initWidth = 2;
    const int initHeight = 2;

    bool updateFrame = true;

    MjpegProcessor mjpeg;

    float deltaTime = 0.0f;
    float mjpegDeltaTime = 0.0f;
    
    // Start is called before the first frame update
    async void Start()
    {
        cameraIP = await client.GetStringAsync(website);
        cameraIP = "http://" + cameraIP.Replace("\n", "").Replace("\r", "") + ":50000/stream.mjpg";


        plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        mjpeg = new MjpegProcessor(chunkSize * 1024);
        mjpeg.FrameReady += OnMjpegFrameReady;
        mjpeg.Error += OnMjpegError;
        Uri mjpegAddress = new Uri(cameraIP);
        mjpeg.ParseStream(mjpegAddress);
        // Create a 16x16 texture with PVRTC RGBA4 format
        // and will it with raw PVRTC bytes.
        tex = new Texture2D(initWidth, initHeight, TextureFormat.PVRTC_RGBA4, false);
        AttachPlane();

        Invoke(nameof(LoadFrame), 0);
    }

    private void OnMjpegFrameReady(object sender, FrameReadyEventArgs e)
    {
        updateFrame = true;
    }
    void OnMjpegError(object sender, ErrorEventArgs e)
    {
        Debug.Log("Error received while reading the MJPEG.");
    }

    /// <summary>
    /// load latest image from pi
    /// </summary>
    /// <returns></returns>
    async Task LoadFrame()
    {
        try
        {
            var response = await client.GetAsync(cameraIP);
            frame = await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }

        Invoke(nameof(LoadFrame), 0.2f);
    }

    /// <summary>
    /// setup plane, it need to be in front of the camer all the times
    /// create plane, attach to camera frame, offset forward, store inital forward direction as baseline for angles
    /// </summary>
    void AttachPlane()
    {
        var lookAt = gameObject.transform.forward;
        var offset = lookAt * 15;
        plane.transform.SetParent(gameObject.transform);

        plane.transform.SetPositionAndRotation(gameObject.transform.position + offset, Quaternion.LookRotation(-lookAt));
        plane.transform.Rotate(90, 0, 0);

        baselineForward = gameObject.transform.rotation.eulerAngles;
    }

    /// <summary>
    /// Reload latest frame into a texture and apply to image plane, 
    /// rescale plane to match image aspect ratio
    /// </summary>
    void ReloadTexture()
    {
        deltaTime += Time.deltaTime;

        if (updateFrame)
        {
            tex.LoadImage(mjpeg.CurrentFrame);
            tex.Apply();

            //var heightScale = 2.0f * Camera.main.orthographicSize / 10.0f; //(float)tex.width / tex.height;
            //var widthScale = heightScale * Camera.main.aspect;
            plane.transform.localScale = new Vector3(2, 1, 1);
            plane.GetComponent<Renderer>().material.mainTexture = tex;

            updateFrame = false;

            mjpegDeltaTime += (deltaTime - mjpegDeltaTime) * 0.2f;

            deltaTime = 0.0f;
        }
    }


    // Update is called once per frame
    void Update()
    {
        ReloadTexture();
    }
    */
}
