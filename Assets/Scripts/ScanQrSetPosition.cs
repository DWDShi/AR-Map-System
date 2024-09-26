using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Text;
using TMPro;
using Unity.Collections;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ZXing;

/*
Code below is adapted from Unity documentation
Title: Accessing the device camera image on the CPU
Author: Unity
Date: 12 February 2021
Availability: https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.1/manual/cpu-camera-image.html
*/

public class ScanQrSetPosition : MonoBehaviour
{
    [SerializeField]
    private ARSession session;
    [SerializeField]
    private XROrigin origin;
    [SerializeField]
    private ARCameraManager cameraManager;
    [SerializeField]
    private List<Target> markerList = new List<Target>();

    private Texture2D newTexture;
    private IBarcodeReader reader = new BarcodeReader();

    void OnEnable()
    {
        cameraManager.frameReceived += OnCameraFrameReceived;
    }

    void OnDisable()
    {
        cameraManager.frameReceived -= OnCameraFrameReceived;
    }

    private void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
            return;

        var conversionParams = new XRCpuImage.ConversionParams
        {
            // Get the entire image.
            inputRect = new RectInt(0, 0, image.width, image.height),

            // Downsample by 2.
            outputDimensions = new Vector2Int(image.width / 2, image.height / 2),

            // Choose RGBA format.
            outputFormat = TextureFormat.RGBA32, 

            // Flip across the vertical axis (mirror image).
            transformation = XRCpuImage.Transformation.MirrorY
        };

        // See how many bytes you need to store the final image.
        int size = image.GetConvertedDataSize(conversionParams);

        // Allocate a buffer to store the image.
        var buffer = new NativeArray<byte>(size, Allocator.Temp);

        // Extract the image data
        image.Convert(conversionParams, buffer);

        // The image was converted to RGBA32 format and written into the provided buffer
        // so you can dispose of the XRCpuImage. You must do this or it will leak resources.
        image.Dispose();

        // At this point, you can process the image, pass it to a computer vision algorithm, etc.
        // In this example, you apply it to a texture to visualize it.

        // You've got the data; let's put it into a texture so you can visualize it.
        newTexture = new Texture2D(
            conversionParams.outputDimensions.x,
            conversionParams.outputDimensions.y,
            conversionParams.outputFormat,
            false);

        newTexture.LoadRawTextureData(buffer);
        newTexture.Apply();

        // Done with your temporary data, so you can dispose it.
        buffer.Dispose();

        var result = reader.Decode(newTexture.GetPixels32(), newTexture.width, newTexture.height);  //tries to decode image from the main camera

        if (result != null)
        {
            //Finds the correct target in the list by comparing the text of the QR code and the name of the value within the list
            Target currentMarker = markerList.Find(newMarker => newMarker.targetName.Equals(result.Text)) ;

            session.Reset();    //Resets the transform of ARSession and destroys all trackables

            origin.transform.position = currentMarker.targetObject.transform.position;
            origin.transform.rotation = currentMarker.targetObject.transform.rotation;
        }
    }
}

