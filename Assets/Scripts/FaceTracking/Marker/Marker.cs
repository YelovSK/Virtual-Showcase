using MediaPipe.BlazeFace;
using UnityEngine;

namespace VirtualVitrine.FaceTracking.Marker
{
    public sealed class Marker : MonoBehaviour
    {
        public FaceDetector.Detection Detection { get; set; }
    }
}