using UnityEngine;

namespace MediaPipe.BlazeFace {

public sealed class Marker : MonoBehaviour
{
    public FaceDetector.Detection detection { get; set; }
    public EyeTracker EyeTracker => GetComponent<EyeTracker>();
}

} // namespace MediaPipe.BlazeFace
