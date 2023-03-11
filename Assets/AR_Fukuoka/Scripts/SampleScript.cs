using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//ARFoundationとARCoreExtensions関連を使用する
using Google.XR.ARCoreExtensions;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
namespace AR_Fukuoka
{
    public class SampleScript : MonoBehaviour
    {
        [SerializeField] private AREarthManager _arEarthManager;
        [SerializeField] private VpsInitializer _vpsInitializer;
        [SerializeField] private Text OutputText;

        public double HeadingThreshold = 25;
        public double HorizontalThreshold = 20;
        public double Latitude;
        public double Longitude;
        public double Heading;
        public double Altitude;
        public GameObject ContentPrefab;
        private GameObject displayObject;
        public ARAnchorManager ARAnchorManager;
        

        private void Update()
        {
            var status = "";
            if (!_vpsInitializer.IsReady || _arEarthManager.EarthTrackingState != TrackingState.Tracking)
            {
                Debug.LogWarning("Don not ready vps initialize");
                return;
            }

            GeospatialPose geospatialPose = _arEarthManager.CameraGeospatialPose;

            if (geospatialPose.OrientationYawAccuracy > HeadingThreshold ||
                geospatialPose.HorizontalAccuracy > HorizontalThreshold)
            {
                status = "低精度: 周辺を見回してください";
            }
            else
            {
                status = "高精度: High Tracking Accuracy";
                if (displayObject == null)
                {
                    // 高精度ができたタイミングでオブジェクトを生成
                    Altitude = geospatialPose.Altitude - 1.5f;
                    Quaternion quaternion = Quaternion.AngleAxis(180f - (float) Heading, Vector3.up);

                    ARGeospatialAnchor anchor = ARAnchorManager.AddAnchor(
                        latitude: Latitude,
                        longitude: Longitude,
                        altitude: Altitude,
                        eunRotation: quaternion);

                    if (anchor != null)
                    {
                        displayObject = Instantiate(ContentPrefab, anchor.transform);
                    }
                }
            }
            ShowTrackingInfo(status: status, geospatialPose:geospatialPose);
        }

        private void ShowTrackingInfo(string status, GeospatialPose geospatialPose)
        {
            OutputText.text = string.Format("Latitude/Longitude: {0}°, {1}¥n" +
                                            "Horizontal Accuracy: {2}m¥n" +
                                            "Altitude: {3}m¥N" + 
                                            "Vertical Accuracy: {4}m¥n" +
                                            "heading: {5}°¥n" + 
                                            "Heading Accuracy: {6}°¥n", "{7}¥n", 
                geospatialPose.Latitude.ToString("F6"),
                geospatialPose.Longitude.ToString("F6"),
                geospatialPose.HorizontalAccuracy.ToString("F6"),
                geospatialPose.Altitude.ToString("F2"),
                geospatialPose.VerticalAccuracy.ToString("F2"),
                geospatialPose.EunRotation.ToString("F1"),
                geospatialPose.OrientationYawAccuracy.ToString("F1"), 
                status);
        }
    }
}


