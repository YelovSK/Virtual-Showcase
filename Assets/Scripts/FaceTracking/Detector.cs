using System.Linq;
using MediaPipe.BlazeFace;
using UnityEngine;
using VirtualVitrine.FaceTracking.Marker;

namespace VirtualVitrine
{
    public class Detector : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private ResourceSet resources;
        [SerializeField] private KeyPointsUpdater keyPointsUpdater;

        #endregion

        // Barracuda face detector
        private FaceDetector detector;

        #region Event Functions

        private void Start()
        {
            detector = new FaceDetector(resources);
        }

        private void OnDestroy()
        {
            detector?.Dispose();
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
            detector.ProcessImage(input, MyPrefs.DetectionThreshold);

            // Check if any detections were found.
            Detection[] detections = detector.Detections.ToArray();
            bool faceFound = detections.Any();

            // Activate/Deactivate marker if face was/wasn't found.
            keyPointsUpdater.gameObject.SetActive(faceFound);

            if (faceFound)
            {
                // Get detection with largest bounding box.
                Detection largestFace = detections
                    .OrderByDescending(x => x.extent.magnitude)
                    .First();
                KeyPointsUpdater.Detection = largestFace;
            }

            return faceFound;
        }
    }
}