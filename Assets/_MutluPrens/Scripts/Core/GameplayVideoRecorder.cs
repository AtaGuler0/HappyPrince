#if UNITY_EDITOR
using System.Collections;
using System.IO;
using UnityEditor.Media;
using UnityEngine;

namespace MutluPrens.Core
{
    /// <summary>
    /// YALNIZCA EDİTÖR: Oyun ekranını (arayüz dahil) sabit 30 fps ile 1920x1080 MP4 olarak kaydeder.
    /// "Mutlu Prens > Oynanış Videosu Kaydet" menüsü, otomatik test pilotunu bu kayıtla birlikte çalıştırır.
    /// Time.captureFramerate kullanıldığı için video, kaydın ne kadar sürdüğünden bağımsız olarak gerçek hızda oynar.
    /// </summary>
    public class GameplayVideoRecorder : MonoBehaviour
    {
        public const string FlagPath = "Temp/MP_video_run";
        public const string OutputPath = "Docs/Teslim/MutluPrens_Oynanis_Videosu.mp4";
        private const int Width = 1920, Height = 1080, Fps = 30;

        public int FrameCount { get; private set; }
        public bool IsRecording { get; private set; }

        private MediaEncoder _encoder;
        private RenderTexture _screenRT, _outRT;
        private Texture2D _frame;

        public static bool Requested => File.Exists(FlagPath);

        private void Start()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));
            if (File.Exists(OutputPath)) File.Delete(OutputPath);
            var attr = new VideoTrackAttributes
            {
                frameRate = new MediaRational(Fps),
                width = Width,
                height = Height,
                includeAlpha = false,
                bitRateMode = UnityEditor.VideoBitrateMode.High,
            };
            _encoder = new MediaEncoder(Path.GetFullPath(OutputPath), attr);
            _outRT = new RenderTexture(Width, Height, 0, RenderTextureFormat.ARGB32);
            _frame = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            Time.captureFramerate = Fps;
            IsRecording = true;
            StartCoroutine(CaptureLoop());
            Debug.Log("[MP-VIDEO] Kayıt başladı: " + Path.GetFullPath(OutputPath));
        }

        private IEnumerator CaptureLoop()
        {
            var eof = new WaitForEndOfFrame();
            while (IsRecording)
            {
                yield return eof;
                if (!IsRecording) break;
                int sw = Screen.width, sh = Screen.height;
                if (_screenRT == null || _screenRT.width != sw || _screenRT.height != sh)
                {
                    if (_screenRT != null) _screenRT.Release();
                    _screenRT = new RenderTexture(sw, sh, 0, RenderTextureFormat.ARGB32);
                }
                ScreenCapture.CaptureScreenshotIntoRenderTexture(_screenRT);
                if (SystemInfo.graphicsUVStartsAtTop) Graphics.Blit(_screenRT, _outRT, new Vector2(1f, -1f), new Vector2(0f, 1f));
                else Graphics.Blit(_screenRT, _outRT);

                var prev = RenderTexture.active;
                RenderTexture.active = _outRT;
                _frame.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                _frame.Apply();
                RenderTexture.active = prev;

                if (FrameCount == 30) File.WriteAllBytes("Temp/MP_video_frame_check.png", _frame.EncodeToPNG());
                _encoder.AddFrame(_frame);
                FrameCount++;
            }
        }

        public void StopRecording()
        {
            if (!IsRecording) return;
            IsRecording = false;
            _encoder?.Dispose();
            _encoder = null;
            Time.captureFramerate = 0;
            Debug.Log($"[MP-VIDEO] Kayıt bitti: {FrameCount} kare ({FrameCount / (float)Fps:0.0} sn) -> {Path.GetFullPath(OutputPath)}");
        }

        private void OnDestroy()
        {
            StopRecording();
            if (_screenRT != null) _screenRT.Release();
            if (_outRT != null) _outRT.Release();
        }
    }
}
#endif
