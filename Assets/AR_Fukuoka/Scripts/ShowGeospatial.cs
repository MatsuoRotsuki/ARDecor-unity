using System.Collections;
using System.Collections.Generic;
using AR_Fukuoka;
using Google.XR.ARCoreExtensions;
using Unity.VisualScripting.InputSystem;
using UnityEngine;
using UnityEngine.UI;

public class ShowGeospatial : MonoBehaviour
{
    public AREarthManager EarthManager;
    public VpsInitializer Initializer;
    public Text outputText;

    // Update is called once per frame
    void Update()
    {
        string status = "";
        if (!Initializer.IsReady ||
            EarthManager.EarthTrackingState != UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
        {
            return;
        }

        GeospatialPose pose = EarthManager.CameraGeospatialPose;
        ShowTrackingInfo(status, pose);
    }

    void ShowTrackingInfo(string status, GeospatialPose pose)
    {
        outputText.text = string.Format(
            "Latitude/Longtitude: {0}°, {1}°\n" +
            "Horizontal Accuracy: {2}m\n" +
            "Altitude: {3}m\n" +
            "Vertical Accuracy: {4}m\n" +
            "Heading: {5}°\n" +
            "Heading Accuracy: {6}°\n" +
            "{7}\n",
            pose.Latitude.ToString("F6"),
            pose.Longitude.ToString("F6"),
            pose.HorizontalAccuracy.ToString("F6"),
            pose.Altitude.ToString("F2"),
            pose.VerticalAccuracy.ToString("F2"),
            pose.EunRotation.eulerAngles.ToString("F2"),
            pose.OrientationYawAccuracy.ToString("F1"),
            status
        );
    }
}
