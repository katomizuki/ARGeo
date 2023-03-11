
namespace AR_Fukuoka
{
    using System.Collections;
    using Google.XR.ARCoreExtensions;
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;

    public class VpsInitializer : MonoBehaviour
    {
        [Header("AR Components")]

        [SerializeField] AREarthManager EarthManager;
  
        [SerializeField] ARCoreExtensions ARCoreExtensions;

        private bool _isReturning = false;
        private bool _enablingGeospatial = false;
        private float _configurePrepareTime = 3f;

        bool _isReady = false;
        public bool IsReady { get { return _isReady; } }

        public bool _lockScreenToPortrait = true;

        private IEnumerator _startLocationService = null;
       
        public void Awake()
        {
            if (_lockScreenToPortrait)
            {
                // Lock screen to portrait.
                Screen.autorotateToLandscapeLeft = false;
                Screen.autorotateToLandscapeRight = false;
                Screen.autorotateToPortraitUpsideDown = false;
                Screen.orientation = ScreenOrientation.Portrait;
            }
            
            Application.targetFrameRate = 60;

            if (ARCoreExtensions == null)
            {
                Debug.LogError("Cannot find ARCoreExtensions.");
            }
        }

        public void OnEnable()
        {       
            _isReturning = false;
            _enablingGeospatial = false;
            _isReady = false;
            _startLocationService = StartLocationService();
            StartCoroutine(_startLocationService);
        }
        private bool _waitingForLocationService = false;
        private IEnumerator StartLocationService()
        {
            _waitingForLocationService = true;


            if (!Input.location.isEnabledByUser)
            {
                Debug.Log("Location service is disabled by User.");
                _waitingForLocationService = false;
                yield break;
            }

            Debug.Log("Start location service.");
            Input.location.Start();

            while (Input.location.status == LocationServiceStatus.Initializing)
            {
                yield return null;
            }

            _waitingForLocationService = false;
            if (Input.location.status != LocationServiceStatus.Running)
            {
                Debug.LogWarningFormat(
                    "Location service ends with {0} status.", Input.location.status);
                Input.location.Stop();
            }
        }
        public void OnDisable()
        {
            StopCoroutine(_startLocationService);
            _startLocationService = null;
            Debug.Log("Stop location services.");
            Input.location.Stop();
        }

        public void Update()
        {
            // Check session error status.
            LifecycleUpdate();
            if (_isReturning)
            {
                return;
            }

            // トラッキング状態ではない場合はreturn
            if (ARSession.state != ARSessionState.SessionInitializing &&
                ARSession.state != ARSessionState.SessionTracking)
            {
                return;
            }

            // Check feature support and enable Geospatial API when it's supported.
            // ジオスパチュアルAPIをサポートしているかチェックする。
            var featureSupport = EarthManager.IsGeospatialModeSupported(GeospatialMode.Enabled);
            switch (featureSupport)
            {
                case FeatureSupported.Unknown:
                    return;
                case FeatureSupported.Unsupported:
                    ReturnWithReason("Geospatial API is not supported by this devices.");
                    return;
                case FeatureSupported.Supported:
                    if (ARCoreExtensions.ARCoreExtensionsConfig.GeospatialMode ==
                        GeospatialMode.Disabled)
                    {
                        Debug.Log("Geospatial sample switched to GeospatialMode.Enabled.");
                        ARCoreExtensions.ARCoreExtensionsConfig.GeospatialMode =
                            GeospatialMode.Enabled;
                        _configurePrepareTime = 3.0f;
                        _enablingGeospatial = true;
                        return;
                    }

                    break;
            }

            // Waiting for new configuration taking effect.
            // geospatialapiに対応していたら
            if (_enablingGeospatial)
            {
                _configurePrepareTime -= Time.deltaTime;
                if (_configurePrepareTime < 0)
                {
                    _enablingGeospatial = false;
                }
                else
                {
                    return;
                }
            }

            // Check earth state.
            // EarthManagerのStateをチェック。
            var earthState = EarthManager.EarthState;
            if (earthState == EarthState.ErrorEarthNotReady)
            {
                ReturnWithReason( "Initializing Geospatial functionalities.");
                return;
            }
            else if (earthState != EarthState.Enabled)
            {
                ReturnWithReason(
                    "Geospatial sample encountered an EarthState error: " + earthState);
                return;
            }

            // Check earth localization.
            // ARSessionがトラッキング中かつ　ロケーションが動いている。
            bool isSessionReady = ARSession.state == ARSessionState.SessionTracking &&
                Input.location.status == LocationServiceStatus.Running;
            //If the process can reach this line and isSessionReady is true, the GeospatialAPI is available
            _isReady = isSessionReady;
        }


        private void LifecycleUpdate()
        {
            // Pressing 'back' button quits the app.
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                Application.Quit();
            }

            if (_isReturning)
            {
                return;
            }

            // Only allow the screen to sleep when not tracking.
            var sleepTimeout = SleepTimeout.NeverSleep;
            if (ARSession.state != ARSessionState.SessionTracking)
            {
                sleepTimeout = SleepTimeout.SystemSetting;
            }

            Screen.sleepTimeout = sleepTimeout;

            // Quit the app if ARSession is in an error status.
            string returningReason = string.Empty;
            if (ARSession.state != ARSessionState.CheckingAvailability &&
                ARSession.state != ARSessionState.Ready &&
                ARSession.state != ARSessionState.SessionInitializing &&
                ARSession.state != ARSessionState.SessionTracking)
            {
                returningReason = string.Format(
                    "Geospatial sample encountered an ARSession error state {0}.\n" +
                    "Please start the app again.",
                    ARSession.state);
            }
            else if (Input.location.status == LocationServiceStatus.Failed)
            {
                returningReason =
                    "Geospatial sample failed to start location service.\n" +
                    "Please start the app again and grant precise location permission.";
            }
            else if (ARCoreExtensions == null)
            {
                returningReason = string.Format(
                    "Geospatial sample failed with missing AR Components.");
            }

            ReturnWithReason(returningReason);
        }

        private void ReturnWithReason(string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                return;
            }

            Debug.LogError(reason);
            _isReturning = true;
            _isReady = false;
        }

        private void QuitApplication()
        {
            Application.Quit();
        }
    }
}