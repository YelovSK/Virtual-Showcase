using System;
using UnityEngine;

public sealed class WebcamInput : MonoBehaviour
{
    [SerializeField] Vector2Int _aspectRatio = new Vector2Int(1, 1);
    WebCamTexture _webcam;
    RenderTexture _buffer;
    public RenderTexture Texture => _buffer;
    public WebCamTexture WebCamTexture => _webcam;
    
    public bool IsCameraRunning() => _webcam != null && _webcam.isPlaying;

    public bool CameraUpdated() => _webcam.didUpdateThisFrame;

    void Awake()
    {
        _webcam = new WebCamTexture(PlayerPrefs.GetString("cam"));
        _webcam.Play();
    }

    void Update()
    {
        // takes a bit for the webcam to initialize, width is very small at first
        if (_buffer == null && _webcam.width > 100)
            _buffer = new RenderTexture(_webcam.width, _webcam.height, 0);
        if (_webcam == null || !_webcam.didUpdateThisFrame)
            return;
        var aspect1 = (float)_webcam.width / _webcam.height;
        var aspect2 = (float)_aspectRatio.x / _aspectRatio.y;
        var gap = aspect2 / aspect1;
        
        var vflip = _webcam.videoVerticallyMirrored;
        var scale = new Vector2(gap, vflip ? -1 : 1);
        var offset = new Vector2((1 - gap) / 2, vflip ? 1 : 0);
        
        // put buffer (default 1:1 aspect ratio) into webcam
        Graphics.Blit(_webcam, _buffer, scale, offset);
    }
    
    void OnDestroy()
    {
        if (_webcam == null)
            return;
        _webcam.Stop();
        Destroy(_webcam);
        Destroy(_buffer);
    }
}
