using System;
using System.IO;
using MutluPrens.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MutluPrens.Core
{
    /// <summary>
    /// Teslim PDF'i için yardımcı: F12 ekran görüntüsü alır (proje klasöründe "Screenshots/"),
    /// H arayüzü gizler/gösterir.
    /// </summary>
    public class ScreenshotTool : MonoBehaviour
    {
        public HUDController hud;
        public string folderName = "Screenshots";
        [Range(1, 4)] public int superSize = 1;

        public static string Folder(string name) =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", name));

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.f12Key.wasPressedThisFrame) Capture(null);
            if (kb.hKey.wasPressedThisFrame && hud != null) hud.ToggleHudHidden();
        }

        public string Capture(string fileName)
        {
            string dir = Folder(folderName);
            Directory.CreateDirectory(dir);
            string file = Path.Combine(dir, (fileName ?? $"MutluPrens_{DateTime.Now:yyyyMMdd_HHmmss}") + ".png");
            ScreenCapture.CaptureScreenshot(file, superSize);
            Debug.Log($"[MutluPrens] Ekran görüntüsü: {file}");
            if (hud != null && fileName == null) StartCoroutine(ToastNextFrame());
            return file;
        }

        // Bildirim, görüntünün içine girmesin diye bir kare sonra gösterilir.
        private System.Collections.IEnumerator ToastNextFrame()
        {
            yield return new WaitForEndOfFrame();
            yield return null;
            hud.ShowToast("Ekran görüntüsü kaydedildi", 1.5f);
        }
    }
}
