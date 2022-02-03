using System;
using UnityEngine;

public sealed class WebcamInput : MonoBehaviour
{
    [SerializeField] Vector2Int _resolution = new Vector2Int(1920, 1080);
    WebCamTexture _webcam;
    RenderTexture _buffer;
    public Texture Texture => _buffer;

    void OnDestroy()
    {
        if (_webcam == null)
            return;
        _webcam.Stop();
        Destroy(_webcam);
        Destroy(_buffer);
    }

    void Update()
    {
        if (_webcam == null)
            return;
        if (!_webcam.didUpdateThisFrame) return;

        var aspect1 = (float)_webcam.width / _webcam.height;
        var aspect2 = (float)_resolution.x / _resolution.y;
        var gap = aspect2 / aspect1;

        var vflip = _webcam.videoVerticallyMirrored;
        var scale = new Vector2(gap, vflip ? -1 : 1);
        var offset = new Vector2((1 - gap) / 2, vflip ? 1 : 0);

        Graphics.Blit(_webcam, _buffer, scale, offset);
    }

    public bool IsCameraRunning()
    {
        return _webcam != null && _webcam.isPlaying;
    }

    public WebCamTexture GetWebcam()
    {
        return _webcam;
    }

    public void StartWebcam()
    {
        _webcam = new WebCamTexture(PlayerPrefs.GetString("cam"), _resolution.x, _resolution.y);
        _buffer = new RenderTexture(_resolution.x, _resolution.y, 0);
        _webcam.Play();
    }
}
