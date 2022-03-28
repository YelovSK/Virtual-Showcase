using MediaPipe.BlazeFace;
using UnityEngine;

namespace VirtualVitrine.FaceTracking.Visualize
{
    public sealed class Marker : MonoBehaviour
    {
        public FaceDetector.Detection Detection { get; set; }
        public EyeTracker EyeTracker => GetComponent<EyeTracker>();
    }
}