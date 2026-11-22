using System.Collections;
using MutluPrens.Core;
using MutluPrens.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MutluPrens.UI
{
    /// <summary>
    /// Ana menü (Oyuna Başla / Nasıl Oynanır / Masal Hakkında / Ayarlar / Çıkış), duraklatma menüsü ve
    /// bölüm sonu ekranı. Klavye (yön tuşları + Enter, Esc geri) ve fareyle kullanılabilir.
    /// </summary>
    public class MenuController : MonoBehaviour
    {
        public GameManager game;

        [Header("Ana menü")]
        public CanvasGroup mainMenuGroup;
        public GameObject mainButtons;
        public Selectable mainFirst;
        public GameObject detailCard;
        public GameObject howToPanel;
        public GameObject aboutPanel;
        public GameObject settingsPanel;
        public Selectable howToBack;
        public Selectable aboutBack;
        public Selectable settingsFirst;

        [Header("Ayarlar etiketleri")]
        public Text volumeLabel;
        public Text qualityLabel;
        public Text fullscreenLabel;
        public Text shakeLabel;

        [Header("Duraklatma")]
        public GameObject pausePanel;
        public Selectable pauseFirst;

        [Header("Bölüm sonu")]
        public CanvasGroup endGroup;
        public Text endTitle;
        public Text endStats;
        public Text endCost;
        public Selectable endFirst;

        private Selectable _defaultSelection;
        private GameObject _openDetail;
        private const string VolumeKey = "MP_Volume";
        private const string ShakeKey = "MP_Shake";

        private void Awake()
        {
            AudioListener.volume = PlayerPrefs.GetFloat(VolumeKey, 0.8f);
            ChaseCamera.ShakeEnabled = PlayerPrefs.GetInt(ShakeKey, 1) == 1;
            if (pausePanel) pausePanel.SetActive(false);
            if (endGroup) endGroup.gameObject.SetActive(false);
            CloseDetail();
            RefreshSettingsLabels();
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            // Fareyle boşluğa tıklanınca seçim kaybolursa klavyeyle yeniden seçilebilsin.
            if (_defaultSelection != null && EventSystem.current != null &&
                EventSystem.current.currentSelectedGameObject == null &&
                (kb.upArrowKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame ||
                 kb.wKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame || kb.tabKey.wasPressedThisFrame))
                Select(_defaultSelection);

            bool back = kb.escapeKey.wasPressedThisFrame || kb.backspaceKey.wasPressedThisFrame;
            if (back && _openDetail != null && mainMenuGroup != null && mainMenuGroup.gameObject.activeSelf) OnBack();
        }

        // ================================================================ Ana menü

        public void ShowMainMenu(bool instant)
        {
            if (mainMenuGroup == null) return;
            mainMenuGroup.gameObject.SetActive(true);
            mainMenuGroup.interactable = true;
            mainMenuGroup.blocksRaycasts = true;
            CloseDetail();
            if (instant) mainMenuGroup.alpha = 1f;
            else StartCoroutine(FadeGroup(mainMenuGroup, 0f, 1f, 1.2f));
            Select(mainFirst);
        }

        public void HideMainMenu()
        {
            if (mainMenuGroup == null || !mainMenuGroup.gameObject.activeSelf) return;
            mainMenuGroup.interactable = false;
            mainMenuGroup.blocksRaycasts = false;
            StartCoroutine(FadeGroup(mainMenuGroup, mainMenuGroup.alpha, 0f, 0.6f, true));
            Select(null);
        }

        public void OnPlay()
        {
            if (game != null) game.StartLevel();
        }

        public void OnHowTo() => OpenDetail(howToPanel, howToBack);
        public void OnAbout() => OpenDetail(aboutPanel, aboutBack);
        public void OnSettings() => OpenDetail(settingsPanel, settingsFirst);

        public void OnBack()
        {
            CloseDetail();
            Select(mainFirst);
        }

        public void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OpenDetail(GameObject panel, Selectable first)
        {
            CloseDetail();
            if (detailCard) detailCard.SetActive(true);
            if (panel) panel.SetActive(true);
            _openDetail = panel;
            Select(first);
        }

        private void CloseDetail()
        {
            if (howToPanel) howToPanel.SetActive(false);
            if (aboutPanel) aboutPanel.SetActive(false);
            if (settingsPanel) settingsPanel.SetActive(false);
            if (detailCard) detailCard.SetActive(false);
            _openDetail = null;
        }

        // ================================================================ Ayarlar

        public void OnVolume()
        {
            float v = Mathf.Round(AudioListener.volume * 5f) / 5f + 0.2f;
            if (v > 1.01f) v = 0f;
            AudioListener.volume = v;
            PlayerPrefs.SetFloat(VolumeKey, v);
            RefreshSettingsLabels();
        }

        public void OnQuality()
        {
            int n = QualitySettings.names.Length;
            if (n > 0) QualitySettings.SetQualityLevel((QualitySettings.GetQualityLevel() + 1) % n, true);
            RefreshSettingsLabels();
        }

        public void OnFullscreen()
        {
            Screen.fullScreen = !Screen.fullScreen;
            RefreshSettingsLabels(!Screen.fullScreen);
        }

        public void OnShake()
        {
            ChaseCamera.ShakeEnabled = !ChaseCamera.ShakeEnabled;
            PlayerPrefs.SetInt(ShakeKey, ChaseCamera.ShakeEnabled ? 1 : 0);
            RefreshSettingsLabels();
        }

        private void RefreshSettingsLabels(bool? fullscreenOverride = null)
        {
            if (volumeLabel) volumeLabel.text = $"Ses düzeyi   ‹ %{Mathf.RoundToInt(AudioListener.volume * 100f)} ›";
            if (qualityLabel)
            {
                var names = QualitySettings.names;
                int q = QualitySettings.GetQualityLevel();
                qualityLabel.text = $"Görüntü kalitesi   ‹ {(q < names.Length ? names[q] : "-")} ›";
            }
            bool fs = fullscreenOverride ?? Screen.fullScreen;
            if (fullscreenLabel) fullscreenLabel.text = $"Tam ekran   ‹ {(fs ? "Açık" : "Kapalı")} ›";
            if (shakeLabel) shakeLabel.text = $"Kamera sarsıntısı   ‹ {(ChaseCamera.ShakeEnabled ? "Açık" : "Kapalı")} ›";
        }

        // ================================================================ Duraklatma

        public void ShowPause(bool show)
        {
            if (pausePanel) pausePanel.SetActive(show);
            Select(show ? pauseFirst : null);
        }

        public void OnResume() { if (game != null) game.Resume(); }
        public void OnCheckpoint() { if (game != null) game.RespawnFromMenu(); }
        public void OnRestart() { if (game != null) game.RestartLevel(true); }
        public void OnMainMenu() { if (game != null) game.RestartLevel(false); }

        // ================================================================ Bölüm sonu

        public void ShowEnd(string stats, string cost)
        {
            if (endGroup == null) return;
            if (endStats) endStats.text = stats;
            if (endCost) endCost.text = cost;
            endGroup.gameObject.SetActive(true);
            StartCoroutine(FadeGroup(endGroup, 0f, 1f, 1.2f));
            Select(endFirst);
        }

        public void OnReplay() { if (game != null) game.RestartLevel(true); }

        // ================================================================ Yardımcılar

        private void Select(Selectable s)
        {
            _defaultSelection = s;
            if (EventSystem.current == null) return;
            EventSystem.current.SetSelectedGameObject(null);
            if (s != null) EventSystem.current.SetSelectedGameObject(s.gameObject);
        }

        private static IEnumerator FadeGroup(CanvasGroup g, float from, float to, float duration, bool deactivate = false)
        {
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / duration)
            {
                g.alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            g.alpha = to;
            if (deactivate) g.gameObject.SetActive(false);
        }
    }
}
