using System.Collections;
using System.Collections.Generic;
using MutluPrens.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MutluPrens.UI
{
    /// <summary>
    /// Oyun içi arayüz. Ekranın kenarlarına yaslanan, kutusuz ve küçük öğelerden oluşur:
    /// sol üstte görev ve adım noktaları, üst ortada pusula, sol altta ısı çubuğu,
    /// sağ üstte yardımın bedeli, altta altyazı ve etkileşim ipucu.
    /// Hiyerarşi editörde "Mutlu Prens > Bölümü Yeniden Oluştur" ile kurulur.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Kök")]
        public CanvasGroup hudGroup;

        [Header("Görev")]
        public Text objectiveHeader;
        public Text objectiveText;
        public Image[] stepDots;

        [Header("Pusula")]
        public GameObject compassRoot;
        public RectTransform arrow;
        public Text targetText;
        public Text distanceText;

        [Header("Isı ve göç")]
        public RectTransform warmthFill;
        public Image warmthFillImage;
        public RectTransform warmthCap;
        public Text warmthValue;
        public Text statusText;
        public Text migrationText;

        [Header("Yardımın bedeli")]
        public CanvasGroup costGroup;
        public Text costRuby;
        public Text costEyes;
        public RectTransform costEyesFill;
        public Text costMigration;

        [Header("Etkileşim")]
        public CanvasGroup promptGroup;
        public Text promptText;
        public GameObject promptProgressRoot;
        public RectTransform promptProgressFill;

        [Header("Altyazı ve bildirim")]
        public CanvasGroup subtitleGroup;
        public Text speakerText;
        public Text subtitleText;
        public CanvasGroup toastGroup;
        public Text toastText;
        public Text alertText;

        [Header("Kontroller")]
        public GameObject controlsPanel;
        public GameObject controlsHint;

        [Header("Ekran efektleri")]
        public Image frostImage;
        public Image fadeImage;

        [Header("Yazı tipleri")]
        public Font regularFont;
        public Font italicFont;

        [Header("Renkler")]
        public Color gold = new(1f, 0.83f, 0.52f);
        public Color ice = new(0.66f, 0.85f, 1f);
        public Color ink = new(0.94f, 0.95f, 0.98f);
        public Color muted = new(0.62f, 0.67f, 0.78f);
        public Color danger = new(1f, 0.55f, 0.5f);

        public bool IsDialogueBusy => _subtitleRoutine != null || _queue.Count > 0;

        private readonly Queue<DialogueLine> _queue = new();
        private Coroutine _subtitleRoutine;
        private bool _skipRequested;
        private float _toastTimer;
        private float _alertTimer;
        private float _hudTarget = 1f;
        private float _promptTarget;
        private float _costTarget;
        private float _subtitleTarget;
        private bool _hudHidden;

        private void Awake()
        {
            SetPrompt(null);
            if (subtitleGroup) subtitleGroup.alpha = 0f;
            if (toastGroup) toastGroup.alpha = 0f;
            if (alertText) alertText.text = "";
            if (fadeImage) SetAlpha(fadeImage, 0f);
            if (costGroup) costGroup.alpha = 0f;
            if (controlsPanel) controlsPanel.SetActive(false);
            SetFrost(0f);
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            Approach(hudGroup, _hudHidden ? 0f : _hudTarget, dt * 3f);
            Approach(promptGroup, _promptTarget, dt * 8f);
            Approach(costGroup, _costTarget, dt * 3f);
            Approach(subtitleGroup, _subtitleTarget, dt * 6f);

            if (_toastTimer > 0f)
            {
                _toastTimer -= dt;
                if (toastGroup) toastGroup.alpha = Mathf.Clamp01(Mathf.Min(_toastTimer * 1.5f, 1f));
            }
            if (_alertTimer > 0f)
            {
                _alertTimer -= dt;
                if (alertText) alertText.color = WithAlpha(alertText.color, Mathf.Clamp01(_alertTimer * 2f));
            }
        }

        private static void Approach(CanvasGroup g, float target, float step)
        {
            if (g != null) g.alpha = Mathf.MoveTowards(g.alpha, target, step);
        }

        // ---------------- Görünürlük ----------------

        public void SetHudVisible(bool visible, bool instant = false)
        {
            _hudTarget = visible ? 1f : 0f;
            if (instant && hudGroup) hudGroup.alpha = _hudTarget;
        }

        /// <summary>Temiz ekran görüntüsü için arayüzü gizler/gösterir (H).</summary>
        public void ToggleHudHidden() => _hudHidden = !_hudHidden;

        public void SetControlsVisible(bool visible)
        {
            if (controlsPanel) controlsPanel.SetActive(visible);
            if (controlsHint) controlsHint.SetActive(!visible);
        }

        public void ToggleControls() => SetControlsVisible(controlsPanel != null && !controlsPanel.activeSelf);

        // ---------------- Görev ----------------

        public void SetObjective(int completedSteps, string text)
        {
            int total = StoryText.Steps.Length;
            if (objectiveHeader)
                objectiveHeader.text = completedSteps >= total
                    ? "GÖREV TAMAMLANDI"
                    : $"GÖREV  {completedSteps + 1}/{total}  ·  {StoryText.Steps[completedSteps].ToUpperInvariant()}";
            if (objectiveText) objectiveText.text = text;

            if (stepDots == null) return;
            for (int i = 0; i < stepDots.Length; i++)
            {
                if (stepDots[i] == null) continue;
                bool done = i < completedSteps, current = i == completedSteps;
                stepDots[i].color = done ? gold : current ? ink : WithAlpha(muted, 0.35f);
                stepDots[i].rectTransform.localScale = Vector3.one * (current ? 1.25f : 1f);
            }
        }

        // ---------------- Pusula ----------------

        public void SetCompass(bool visible, string label, float distance, float relativeAngle)
        {
            if (compassRoot) compassRoot.SetActive(visible);
            if (!visible) return;
            if (arrow) arrow.localRotation = Quaternion.Euler(0f, 0f, -relativeAngle);
            if (targetText) targetText.text = label;
            if (distanceText) distanceText.text = $"{Mathf.CeilToInt(distance)} m";
        }

        // ---------------- Isı ----------------

        public void SetWarmth(float normalized, float capNormalized, bool inCold, bool warming, bool tooHigh, bool strongWind)
        {
            SetFill(warmthFill, normalized);
            if (warmthFillImage)
            {
                var cold = new Color(0.5f, 0.78f, 1f);
                var warm = new Color(1f, 0.66f, 0.3f);
                warmthFillImage.color = Color.Lerp(cold, warm, Mathf.InverseLerp(0.15f, 0.8f, normalized));
            }
            if (warmthCap)
            {
                warmthCap.gameObject.SetActive(capNormalized < 0.999f);
                warmthCap.anchorMin = new Vector2(capNormalized, 0f);
            }
            if (warmthValue) warmthValue.text = $"{Mathf.RoundToInt(normalized * 100f)}";

            if (statusText)
            {
                string s = "";
                if (tooHigh) s = "Çok yüksek, ayaz seni aşağı itiyor";
                else if (normalized < 0.25f) s = "Donmak üzeresin! Bir fenere ya da sıcak pencereye git";
                else if (warming) s = "Isınıyorsun";
                else if (strongWind) s = "Sert rüzgâr";
                else if (inCold) s = "Soğuk rüzgâr ısını düşürüyor";
                statusText.text = s;
                statusText.color = normalized < 0.25f ? danger : warming ? gold : ice;
            }
        }

        public void SetMigration(bool postponed, bool flockGone)
        {
            if (migrationText == null) return;
            migrationText.text = postponed ? "Göç ertelendi  ·  soğuk şehirde kaldın"
                : flockGone ? "Sürü güneye gitti"
                : "Sürü güneyde bekliyor";
            migrationText.color = postponed ? ice : muted;
        }

        // ---------------- Bedel ----------------

        public void SetCost(bool visible, float princeSight, bool rubyGiven, bool postponed)
        {
            _costTarget = visible ? 1f : 0f;
            if (!visible) return;
            if (costRuby) costRuby.text = rubyGiven ? "Yakut  ·  terzi kadına verildi" : "Yakut  ·  kılıçtan söküldü";
            if (costEyes) costEyes.text = princeSight <= 0.001f ? "Prens'in gözleri  ·  karardı" : "Prens'in gözlerinin ışığı";
            SetFill(costEyesFill, princeSight);
            if (costMigration)
            {
                costMigration.text = postponed ? "Göç  ·  ertelendi" : "Göç  ·  sürü uzaklaşıyor";
                costMigration.color = postponed ? ice : muted;
            }
        }

        // ---------------- Etkileşim / altyazı / bildirim ----------------

        public void SetPrompt(string text, float progress = -1f)
        {
            bool show = !string.IsNullOrEmpty(text);
            _promptTarget = show ? 1f : 0f;
            if (promptGroup && !show && promptGroup.alpha <= 0.01f) promptGroup.alpha = 0f;
            if (show && promptText) promptText.text = text;
            if (promptProgressRoot) promptProgressRoot.SetActive(show && progress >= 0f);
            if (progress >= 0f) SetFill(promptProgressFill, progress);
        }

        public void QueueLines(DialogueLine[] lines, bool interrupt)
        {
            if (lines == null || lines.Length == 0) return;
            if (interrupt)
            {
                _queue.Clear();
                if (_subtitleRoutine != null) StopCoroutine(_subtitleRoutine);
                _subtitleRoutine = null;
            }
            foreach (var l in lines) _queue.Enqueue(l);
            if (_subtitleRoutine == null) _subtitleRoutine = StartCoroutine(PlayQueue());
        }

        public void SkipLine() => _skipRequested = true;

        private IEnumerator PlayQueue()
        {
            while (_queue.Count > 0)
            {
                var line = _queue.Dequeue();
                Color c = line.Speaker == StoryText.Prince ? gold
                    : line.Speaker == StoryText.Swallow ? ice
                    : line.Speaker == StoryText.Hint ? new Color(0.78f, 0.9f, 0.65f)
                    : new Color(0.95f, 0.75f, 0.85f);
                if (speakerText)
                {
                    speakerText.text = line.Speaker.ToUpperInvariant();
                    speakerText.color = c;
                }
                if (subtitleText)
                {
                    subtitleText.text = line.Text;
                    var f = line.Speaker == StoryText.Hint ? regularFont : italicFont;
                    if (f != null) subtitleText.font = f;
                }
                _subtitleTarget = 1f;
                _skipRequested = false;
                for (float t = 0f; t < line.Duration && !_skipRequested; t += Time.deltaTime) yield return null;
                _skipRequested = false;
            }
            _subtitleTarget = 0f;
            _subtitleRoutine = null;
        }

        public void ShowToast(string text, float duration = 2.5f)
        {
            if (toastText == null) return;
            toastText.text = text;
            _toastTimer = duration;
            if (toastGroup) toastGroup.alpha = 1f;
        }

        public void ShowAlert(string text, float duration = 1.5f)
        {
            if (alertText == null) return;
            alertText.text = text;
            alertText.color = WithAlpha(danger, 1f);
            _alertTimer = duration;
        }

        // ---------------- Efektler ----------------

        public void SetFrost(float amount)
        {
            if (frostImage) SetAlpha(frostImage, Mathf.Clamp01(amount) * 0.6f);
        }

        public IEnumerator Fade(float target, float duration)
        {
            if (fadeImage == null) yield break;
            float start = fadeImage.color.a;
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / Mathf.Max(0.01f, duration))
            {
                SetAlpha(fadeImage, Mathf.Lerp(start, target, t));
                yield return null;
            }
            SetAlpha(fadeImage, target);
        }

        // ---------------- Yardımcılar ----------------

        private static void SetFill(RectTransform rt, float t)
        {
            if (rt == null) return;
            rt.anchorMin = new Vector2(0f, rt.anchorMin.y);
            rt.anchorMax = new Vector2(Mathf.Clamp01(t), rt.anchorMax.y);
        }

        private static void SetAlpha(Graphic g, float a) => g.color = WithAlpha(g.color, a);
        private static Color WithAlpha(Color c, float a) { c.a = a; return c; }
    }
}
