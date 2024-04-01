using System.Linq;
using MediaPipe.BlazeFace;
using UnityEngine;
using VirtualShowcase.FaceTracking.Marker;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.FaceTracking
{
    public class Detector : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField]
        private ResourceSet resources;

        [SerializeField]
        private KeyPointsUpdater keyPointsUpdater;

        #endregion

        // Barracuda face detector
        private FaceDetector _detector;

        #region Event Functions

        private void Start()
        {
            _detector = new FaceDetector(resources);
        }

        private void OnDestroy()
        {
            _detector?.Dispose();
        }

        #endregion

        /// <summary>
        ///     Runs detection and returns true if face was found
        /// </summary>
        /// <param name="input">Texture to detect from</param>
        /// <returns>True if face was found, False if wasn't found</returns>
        public bool RunDetector(Texture input)
        {
            // Face detection.
            _detector.ProcessImage(input, (float)MyPrefs.DetectionThreshold / 100);

            // Check if any detections were found.
            Detection[] detections = _detector.Detections.ToArray();
            bool faceFound = detections.Any();

            // Activate/Deactivate marker if face was/wasn't found.
            keyPointsUpdater.gameObject.SetActive(faceFound);

            if (faceFound)
            {
                // Get detection with largest bounding box.
                KeyPointsUpdater.Detection = detections
                    .OrderByDescending(x => x.extent.magnitude)
                    .First();
            }

            return faceFound;
        }
    }
}