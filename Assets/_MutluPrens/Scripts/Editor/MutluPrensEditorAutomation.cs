using System.IO;
using MutluPrens.Core;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MutluPrens.EditorTools
{
    /// <summary>
    /// Editör kolaylıkları ve teslim araçları:
    ///  • Bölüm sahnesi hiç yoksa ilk derlemeden sonra bir kez otomatik kurulur.
    ///  • Mutlu Prens > Otomatik Oynanış Testi: test pilotu bölümü baştan sona uçar ("[MP-AUTO]").
    ///  • Mutlu Prens > Oynanış Videosu Kaydet: aynı uçuşu 1920x1080 MP4 olarak Docs/Teslim klasörüne kaydeder.
    ///  • Mutlu Prens > Windows Build Al: Builds/Windows/MutluPrens.exe üretir.
    ///  • Proje dışından tetikleme: Temp/MP_request_* dosyaları (build, autotest, video, winbuild, stop, view).
    /// </summary>
    [InitializeOnLoad]
    public static class MutluPrensEditorAutomation
    {
        private const string BuildRequest = "Temp/MP_request_build";
        private const string TestRequest = "Temp/MP_request_autotest";
        private const string VideoRequest = "Temp/MP_request_video";
        private const string WinBuildRequest = "Temp/MP_request_winbuild";
        private const string StopRequest = "Temp/MP_request_stop";
        private const string ViewRequest = "Temp/MP_request_view";
        public const string WindowsBuildPath = "Builds/Windows/MutluPrens.exe";
        private static double _nextPoll;

        static MutluPrensEditorAutomation()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += state =>
            {
                if (state != PlayModeStateChange.EnteredEditMode) return;
                if (File.Exists(AutopilotDriver.FlagPath)) File.Delete(AutopilotDriver.FlagPath);
                if (File.Exists(GameplayVideoRecorder.FlagPath)) File.Delete(GameplayVideoRecorder.FlagPath);
                Time.timeScale = 1f;
            };
        }

        private static void Poll()
        {
            if (EditorApplication.timeSinceStartup < _nextPoll) return;
            _nextPoll = EditorApplication.timeSinceStartup + 1.0;
            if (EditorApplication.isPlaying && File.Exists(StopRequest))
            {
                File.Delete(StopRequest);
                Debug.Log("[MutluPrens] Dış istek: Play modu durduruluyor.");
                EditorApplication.isPlaying = false;
                return;
            }
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;
            if (File.Exists(StopRequest)) File.Delete(StopRequest);

            if (!File.Exists(MutluPrensLevelBuilder.ScenePath) && !SessionState.GetBool("MP_AutoBuilt", false))
            {
                SessionState.SetBool("MP_AutoBuilt", true);
                Debug.Log("[MutluPrens] Bölüm sahnesi bulunamadı; otomatik oluşturuluyor...");
                if (MutluPrensLevelBuilder.Build(true)) EditorSceneManager.OpenScene(MutluPrensLevelBuilder.ScenePath);
                return;
            }

            if (File.Exists(BuildRequest) || File.Exists(TestRequest) || File.Exists(VideoRequest) || File.Exists(WinBuildRequest) || File.Exists(ViewRequest))
            {
                // Bekleyen betik değişikliği varsa önce derlensin.
                AssetDatabase.Refresh();
                if (EditorApplication.isCompiling) return;
            }

            if (File.Exists(BuildRequest))
            {
                File.Delete(BuildRequest);
                Debug.Log("[MutluPrens] Dış istek: sahne yeniden oluşturuluyor.");
                MutluPrensLevelBuilder.Build(false);
                return;
            }
            if (File.Exists(TestRequest))
            {
                File.Delete(TestRequest);
                StartAutotest(false, false);
                return;
            }
            if (File.Exists(VideoRequest))
            {
                File.Delete(VideoRequest);
                StartAutotest(false, true);
                return;
            }
            if (File.Exists(WinBuildRequest))
            {
                File.Delete(WinBuildRequest);
                BuildWindows();
                return;
            }
            if (File.Exists(ViewRequest))
            {
                string which = File.ReadAllText(ViewRequest).Trim();
                File.Delete(ViewRequest);
                PrepareEditorView(which);
            }
        }

        [MenuItem("Mutlu Prens/Otomatik Oynanış Testi (Play)", priority = 20)]
        private static void MenuAutotest() => StartAutotest(true, false);

        [MenuItem("Mutlu Prens/Oynanış Videosu Kaydet (Play)", priority = 21)]
        private static void MenuVideo() => StartAutotest(true, true);

        [MenuItem("Mutlu Prens/Windows Build Al", priority = 40)]
        private static void MenuWinBuild() => BuildWindows();

        // Teslim PDF'i için editör ekran görüntüsü hazırlığı: görünümü kur, Win+Shift+S ile al, sonra düzeni geri yükle.
        [MenuItem("Mutlu Prens/Teslim/1) Editör görünümü: Sahne + Hierarchy + Inspector", priority = 60)]
        private static void MenuView1() => PrepareEditorView("1");

        [MenuItem("Mutlu Prens/Teslim/2) Editör görünümü: Proje klasör yapısı", priority = 61)]
        private static void MenuView2() => PrepareEditorView("2");

        [MenuItem("Mutlu Prens/Teslim/3) Önceki pencere düzenimi geri yükle", priority = 62)]
        private static void MenuViewRestore() => PrepareEditorView("restore");

        private static void StartAutotest(bool askToSave, bool video)
        {
            if (askToSave && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!File.Exists(MutluPrensLevelBuilder.ScenePath)) MutluPrensLevelBuilder.Build(false);
            EditorSceneManager.OpenScene(MutluPrensLevelBuilder.ScenePath);
            Directory.CreateDirectory("Temp");
            File.WriteAllText(AutopilotDriver.FlagPath, "1");
            if (video) File.WriteAllText(GameplayVideoRecorder.FlagPath, "1");
            Debug.Log(video ? "[MP-AUTO] Oynanış videosu kaydı başlatılıyor..." : "[MP-AUTO] Otomatik oynanış testi başlatılıyor...");
            EditorApplication.EnterPlaymode();
        }

        public static void BuildWindows()
        {
            if (!File.Exists(MutluPrensLevelBuilder.ScenePath)) MutluPrensLevelBuilder.Build(false);
            PlayerSettings.productName = "Mutlu Prens";
            var options = new BuildPlayerOptions
            {
                scenes = new[] { MutluPrensLevelBuilder.ScenePath },
                locationPathName = WindowsBuildPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None,
            };
            Debug.Log("[MP-BUILD] Windows build başlıyor...");
            var report = BuildPipeline.BuildPlayer(options);
            var s = report.summary;
            Debug.Log($"[MP-BUILD] Sonuç: {s.result}  hata={s.totalErrors}  uyarı={s.totalWarnings}  boyut={s.totalSize / (1024f * 1024f):0.0} MB  süre={s.totalTime}  -> {Path.GetFullPath(WindowsBuildPath)}");
        }

        /// <summary>Teslim PDF'i için editör ekran görüntüsüne uygun görünüm hazırlar.</summary>
        private const string SavedLayout = "Temp/MP_kullanici_duzeni.wlt";
        private const string TemplateLayout = "Assets/TutorialInfo/Layout.wlt";

        /// <summary>Kullanıcının mevcut pencere düzenini kaydeder (Window > Layouts > Save Layout ile aynı iç işlev).</summary>
        private static bool SaveUserLayout()
        {
            var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.WindowLayout");
            if (type == null) return false;
            foreach (var m in type.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public))
            {
                if (m.Name != "SaveWindowLayout") continue;
                var ps = m.GetParameters();
                if (ps.Length == 0 || ps[0].ParameterType != typeof(string)) continue;
                var args = new object[ps.Length];
                args[0] = Path.GetFullPath(SavedLayout);
                for (int i = 1; i < ps.Length; i++) args[i] = ps[i].HasDefaultValue ? ps[i].DefaultValue : (ps[i].ParameterType == typeof(bool) ? (object)false : null);
                m.Invoke(null, args);
                return File.Exists(SavedLayout);
            }
            return false;
        }

        private static void PrepareEditorView(string which)
        {
            if (which == "restore")
            {
                if (SessionState.GetBool("MP_LayoutSwitched", false) && File.Exists(SavedLayout))
                    EditorUtility.LoadWindowLayout(Path.GetFullPath(SavedLayout));
                SessionState.SetBool("MP_LayoutSwitched", false);
                Tools.visibleLayers = SessionState.GetInt("MP_VisibleLayers", -1);
                SessionState.SetBool("MP_LayersSaved", false);
                Debug.Log("[MP-VIEW] Kullanıcı düzeni geri yüklendi.");
                return;
            }
            if (!SessionState.GetBool("MP_LayoutSwitched", false))
            {
                if (File.Exists(SavedLayout)) File.Delete(SavedLayout);
                if (!SaveUserLayout()) { Debug.LogWarning("[MP-VIEW] Mevcut düzen kaydedilemedi; düzen değiştirilmedi."); return; }
                if (File.Exists(TemplateLayout)) EditorUtility.LoadWindowLayout(Path.GetFullPath(TemplateLayout));
                SessionState.SetBool("MP_LayoutSwitched", true);
            }
            if (EditorSceneManager.GetActiveScene().path != MutluPrensLevelBuilder.ScenePath)
                EditorSceneManager.OpenScene(MutluPrensLevelBuilder.ScenePath);

            if (!SessionState.GetBool("MP_LayersSaved", false))
            {
                SessionState.SetBool("MP_LayersSaved", true);
                SessionState.SetInt("MP_VisibleLayers", Tools.visibleLayers);
            }
            Tools.visibleLayers &= ~(1 << 5); // Scene görünümünde dev UI kanvasını gizle

            var sv = SceneView.lastActiveSceneView ?? EditorWindow.GetWindow<SceneView>();
            sv.Focus();
            sv.orthographic = false;
            sv.drawGizmos = true;

            if (which == "2")
            {
                // Project penceresinde betik klasörleri; sahnede Prens'in sütunu yakından
                var folder = AssetDatabase.LoadAssetAtPath<Object>("Assets/_MutluPrens/Scripts");
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = folder;
                EditorGUIUtility.PingObject(folder);
                sv.LookAt(new Vector3(0f, 13f, 0f), Quaternion.Euler(12f, 210f, 0f), 18f);
            }
            else
            {
                // Şehrin kuşbakışı görünümü; hiyerarşide ve Inspector'da sallanan bir gemi (Pendulum + yalpalama)
                var ship = GameObject.Find("Gemi_z232");
                if (ship != null)
                {
                    Selection.activeGameObject = ship;
                    EditorGUIUtility.PingObject(ship);
                }
                sv.LookAt(new Vector3(52f, 8f, 160f), Quaternion.Euler(34f, -38f, 0f), 62f);
            }
            sv.Repaint();
            Debug.Log($"[MP-VIEW] Görünüm {which} hazır.");
        }
    }
}
