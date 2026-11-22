using System.Collections;
using System.Collections.Generic;
using MutluPrens.Player;
using MutluPrens.UI;
using MutluPrens.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MutluPrens.Core
{
    public enum GamePhase
    {
        Title,    // ana menü
        Listen,   // Prens'i dinle (Kırlangıç tereddüt eder)
        TakeRuby, // yakutu kılıçtan al
        Deliver,  // yakutu terzi kadına götür
        Fan,      // kanatlarla çocuğu serinlet
        Return,   // Prens'e geri dön
        Ending,   // bölüm sonu
    }

    /// <summary>Bölümün akışı: menü, aşamalar, etkileşimler, kontrol noktaları, başarısızlık ve arayüz güncellemesi.</summary>
    [DefaultExecutionOrder(-50)]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        /// <summary>"Tekrar Oyna" ile sahne yeniden yüklenince ana menüyü atla.</summary>
        public static bool SkipMenuOnce;

        [Header("Sahne referansları")]
        public SwallowController swallow;
        public SwallowWarmth warmth;
        public SwallowVisual swallowVisual;
        public ChaseCamera chaseCamera;
        public MenuCamera menuCamera;
        public PrinceStatue prince;
        public SeamstressHouse house;
        public RouteGuide routeGuide;
        public HUDController hud;
        public MenuController menu;
        public SnowController snow;
        public FlockOfSwallows flock;
        public Transform startPoint;
        public Checkpoint[] checkpoints;

        [Header("Ayarlar")]
        public float listenRadius = 30f;
        public float fadeTime = 0.5f;
        [Tooltip("Çocuğu serinletmek için E'nin basılı tutulacağı süre (sn).")]
        public float fanDuration = 3f;
        [Tooltip("Yakut taşınırken Prens'in gözlerinin ulaşacağı en düşük ışık (teslimde 0 olur).")]
        public float sightWhileCarrying = 0.35f;

        public GamePhase Phase { get; private set; } = GamePhase.Title;
        public bool IsPaused { get; private set; }
        public bool IsBusy { get; private set; }
        public bool MigrationPostponed { get; private set; }
        public float LevelTime { get; private set; }
        public int FreezeCount { get; private set; }
        public int HitCount { get; private set; }
        public float FanProgress { get; private set; }
        public bool IsAutopilot { get; private set; }

        public int CompletedSteps => Phase switch
        {
            GamePhase.Title or GamePhase.Listen => 0,
            GamePhase.TakeRuby => 1,
            GamePhase.Deliver => 2,
            GamePhase.Fan => 3,
            GamePhase.Return => 4,
            _ => 5,
        };

        public event System.Action<GamePhase> PhaseChanged;
        public event System.Action Respawned;

        private readonly List<InteractionZone> _zones = new();
        private Checkpoint _lastCheckpoint;
        private Vector3 _respawnPosition;
        private Quaternion _respawnRotation;
        private bool _listenStarted;
        private Camera _camera;

        private void Awake()
        {
            Instance = this;
            Time.timeScale = 1f;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            Time.timeScale = 1f;
        }

        private void Start()
        {
            _camera = chaseCamera != null ? chaseCamera.GetComponent<Camera>() : Camera.main;
            _respawnPosition = startPoint.position;
            _respawnRotation = startPoint.rotation;
            swallow.Teleport(_respawnPosition, _respawnRotation);
            swallow.InputEnabled = false;
            warmth.Active = false;
            warmth.Frozen += OnFrozen;
            swallow.HitHazard += OnHitHazard;

            prince.SetSight(1f);
            prince.SetCrying(true);
            RefreshHud();

#if UNITY_EDITOR
            if (System.IO.File.Exists(AutopilotDriver.FlagPath))
            {
                IsAutopilot = true;
                SkipMenuOnce = false;
                gameObject.AddComponent<AutopilotDriver>();
            }
#endif
            if (SkipMenuOnce)
            {
                SkipMenuOnce = false;
                if (menu && menu.mainMenuGroup) menu.mainMenuGroup.gameObject.SetActive(false);
                if (menuCamera) menuCamera.enabled = false;
                chaseCamera.enabled = true;
                chaseCamera.SnapToTarget();
                StartLevel();
            }
            else
            {
                chaseCamera.enabled = false;
                if (menuCamera) menuCamera.enabled = true;
                hud.SetHudVisible(false, true);
                if (menu) menu.ShowMainMenu(false);
            }
        }

        // ================================================================== Döngü

        private void Update()
        {
            if (Phase == GamePhase.Title || Phase == GamePhase.Ending) return;

            var kb = Keyboard.current;
            if (kb != null && kb.escapeKey.wasPressedThisFrame && !IsBusy) SetPaused(!IsPaused);
            if (IsPaused) return;

            if (kb != null)
            {
                if (kb.rKey.wasPressedThisFrame && !IsBusy) StartCoroutine(RespawnRoutine(null));
                if (kb.f1Key.wasPressedThisFrame) hud.ToggleControls();
                if (kb.enterKey.wasPressedThisFrame || kb.tabKey.wasPressedThisFrame) hud.SkipLine();
            }

            LevelTime += Time.deltaTime;

            if (Phase == GamePhase.Listen && !_listenStarted &&
                Vector3.Distance(swallow.transform.position, prince.rubyPoint.position) < listenRadius)
            {
                _listenStarted = true;
                StartCoroutine(ListenRoutine());
            }

            if (Phase == GamePhase.Fan) UpdateFanning();
            else
            {
                string prompt = IsBusy ? null : CurrentPrompt(out _);
                hud.SetPrompt(prompt);
                if (prompt != null && swallow.InputSource != null && swallow.InputSource.InteractPressedThisFrame())
                    Interact();
            }

            UpdateGuidance();
            UpdateColdFeedback();
        }

        // ================================================================== Aşamalar

        public void StartLevel()
        {
            if (Phase != GamePhase.Title) return;
            if (menu) menu.HideMainMenu();
            if (menuCamera) menuCamera.enabled = false;
            if (!chaseCamera.enabled)
            {
                chaseCamera.enabled = true;
                chaseCamera.BeginBlend(1.6f);
            }
            hud.SetHudVisible(true);
            hud.SetControlsVisible(true); // ilk dakikalarda kontroller görünsün; yakut alınınca gizlenir (F1)
            swallow.InputEnabled = true;
            warmth.Active = true;
            SetPhase(GamePhase.Listen);
            hud.ShowToast("Mutlu Prens'in sütununa yaklaş", 3f);
        }

        private IEnumerator ListenRoutine()
        {
            hud.QueueLines(StoryText.PrinceRequest, true);
            yield return null;
            while (hud.IsDialogueBusy && Phase == GamePhase.Listen) yield return null;
            if (Phase == GamePhase.Listen)
            {
                SetPhase(GamePhase.TakeRuby);
                hud.ShowToast("Karar senin: sürü güneyde bekliyor", 3f);
            }
        }

        private void SetPhase(GamePhase phase)
        {
            Phase = phase;
            Debug.Log($"[MutluPrens] Aşama: {phase}  (süre {LevelTime:0.0} sn)");
            RefreshHud();
            PhaseChanged?.Invoke(phase);
        }

        private void RefreshHud()
        {
            string objective = Phase switch
            {
                GamePhase.Title or GamePhase.Listen => StoryText.ObjListen,
                GamePhase.TakeRuby => StoryText.ObjTakeRuby,
                GamePhase.Deliver => StoryText.ObjDeliver,
                GamePhase.Fan => StoryText.ObjFan,
                GamePhase.Return => StoryText.ObjReturn,
                _ => StoryText.ObjEnding,
            };
            hud.SetObjective(CompletedSteps, objective);
            hud.SetMigration(MigrationPostponed, flock != null && (flock.HasLeft || Phase >= GamePhase.Deliver));
            hud.SetCost(Phase >= GamePhase.Deliver && Phase != GamePhase.Ending, prince.Sight, house.RubyPlaced, MigrationPostponed);

            switch (Phase)
            {
                case GamePhase.Deliver:
                    routeGuide.SetMode(GuideMode.Outbound);
                    routeGuide.SetBeacon(house.approachPoint.position);
                    break;
                case GamePhase.Fan:
                    routeGuide.SetMode(GuideMode.None);
                    routeGuide.SetBeacon(null);
                    break;
                case GamePhase.Return:
                    routeGuide.SetMode(GuideMode.Return);
                    routeGuide.SetBeacon(prince.approachPoint.position);
                    break;
                default:
                    routeGuide.SetMode(GuideMode.None);
                    routeGuide.SetBeacon(null);
                    break;
            }
        }

        // ================================================================== Etkileşim

        public void EnterZone(InteractionZone zone)
        {
            if (!_zones.Contains(zone)) _zones.Add(zone);
        }

        public void ExitZone(InteractionZone zone) => _zones.Remove(zone);

        public bool InZone(InteractionKind kind)
        {
            foreach (var z in _zones) if (z != null && z.kind == kind) return true;
            return false;
        }

        /// <summary>Şu an E ile yapılabilecek bir etkileşim var mı? (Otomatik test pilotu da kullanır.)</summary>
        public bool CanInteractNow => !IsBusy && !IsPaused && CurrentPrompt(out _) != null;

        /// <summary>Çocuğu serinletme sırasında E basılı tutulmalı mı?</summary>
        public bool CanFanNow => Phase == GamePhase.Fan && !IsBusy && !IsPaused && InZone(InteractionKind.Delivery);

        private string CurrentPrompt(out InteractionZone zone)
        {
            foreach (var z in _zones)
            {
                zone = z;
                if (z == null) continue;
                if (z.kind == InteractionKind.Ruby && Phase == GamePhase.TakeRuby) return "Yakutu kılıcın kabzasından al";
                if (z.kind == InteractionKind.Delivery && Phase == GamePhase.Deliver) return "Yakutu yüksüğün yanına bırak";
                if (z.kind == InteractionKind.Prince && Phase == GamePhase.Return) return "Prens'in omzuna kon";
            }
            zone = null;
            return null;
        }

        private void Interact()
        {
            CurrentPrompt(out var zone);
            if (zone == null) return;

            switch (zone.kind)
            {
                case InteractionKind.Ruby:
                    prince.RemoveRuby();
                    prince.SetSight(0.8f);
                    prince.SetCrying(false);
                    swallowVisual.SetCarrying(true);
                    if (flock) flock.Leave();
                    hud.QueueLines(StoryText.Accept, true);
                    hud.ShowToast("Yakut alındı", 2.5f);
                    hud.SetControlsVisible(false);
                    _respawnPosition = startPoint.position;
                    SetPhase(GamePhase.Deliver);
                    break;

                case InteractionKind.Delivery:
                    house.PlaceRuby();
                    swallowVisual.SetCarrying(false);
                    prince.SetSight(0f);
                    prince.SetDulled(1f);
                    hud.QueueLines(StoryText.RubyPlaced, true);
                    hud.ShowToast("Yakut terzi kadına ulaştı", 2.5f);
                    FanProgress = 0f;
                    // Pencerenin dışından, Kırlangıç'ın arkasından odaya, yatağa bakan sabit kamera
                    chaseCamera.SetFixedView(true, house.approachPoint.position + new Vector3(-1f, 1f, -2.5f),
                        house.childRenderer != null ? house.childRenderer.transform.position : house.approachPoint.position + Vector3.right * 8f);
                    SetPhase(GamePhase.Fan);
                    break;

                case InteractionKind.Prince:
                    SetPhase(GamePhase.Ending);
                    StartCoroutine(EndingRoutine());
                    break;
            }
        }

        private void UpdateFanning()
        {
            bool inZone = InZone(InteractionKind.Delivery);
            bool holding = inZone && !IsBusy && swallow.InputSource != null && swallow.InputSource.InteractHeld();
            swallowVisual.Fanning = holding;
            if (holding)
            {
                FanProgress = Mathf.Min(1f, FanProgress + Time.deltaTime / Mathf.Max(0.1f, fanDuration));
                house.SetFanProgress(FanProgress);
            }
            hud.SetPrompt(inZone ? "Basılı tut: kanatlarınla çocuğu serinlet" : "Pencereye geri dön", inZone ? FanProgress : -1f);

            if (FanProgress >= 1f) CompleteFanning();
        }

        private void CompleteFanning()
        {
            swallowVisual.Fanning = false;
            chaseCamera.SetFixedView(false);
            MigrationPostponed = true;
            warmth.SetMigrationPostponed(true);
            if (snow) snow.SetIntensity(2.2f);
            hud.QueueLines(StoryText.Fanned, true);
            hud.ShowToast("Çocuk uykuya daldı", 3f);
            _respawnPosition = house.returnSpawnPoint.position;
            _respawnRotation = house.returnSpawnPoint.rotation;
            _lastCheckpoint = null;
            SetPhase(GamePhase.Return);
        }

        private IEnumerator EndingRoutine()
        {
            swallow.InputEnabled = false;
            // Kırlangıç Prens'in omzunun yanına konar; kamera kararmış gözlere bakar.
            Vector3 perch = prince.rubyPoint.position + new Vector3(1.2f, 0.9f, -4.2f);
            Vector3 look = prince.eyeRenderers.Length > 0 ? prince.eyeRenderers[0].transform.position : prince.rubyPoint.position;
            Vector3 flat = look - perch;
            flat.y = 0f;
            swallow.Teleport(perch, Quaternion.LookRotation(flat));
            chaseCamera.SnapToTarget();
            hud.SetControlsVisible(false);
            warmth.Active = false;
            hud.SetPrompt(null);
            hud.SetCompass(false, "", 0f, 0f);
            routeGuide.SetBeacon(null);
            hud.QueueLines(StoryText.Ending, true);
            yield return null;
            float t = 0f;
            while (hud.IsDialogueBusy && t < 15f) { t += Time.deltaTime; yield return null; }

            hud.SetHudVisible(false);
            int min = Mathf.FloorToInt(LevelTime / 60f), sec = Mathf.FloorToInt(LevelTime % 60f);
            string stats = $"Süre  {min}:{sec:00}        Donma  {FreezeCount}        Çarpma  {HitCount}";
            string cost =
                "Mutlu Prens kılıcındaki yakutu verdi; gözlerinin ışığı söndü.\n" +
                "Kırlangıç sürüsüyle güneye göç etmedi; soğuk şehirde kaldı.\n" +
                "Terzi kadının hasta oğlu bu gece rahat uyuyor.";
            if (menu) menu.ShowEnd(stats, cost);
            Debug.Log($"[MutluPrens] BÖLÜM TAMAMLANDI  süre={LevelTime:0.0}sn  donma={FreezeCount}  çarpma={HitCount}");
        }

        // ================================================================== Kontrol noktası ve başarısızlık

        public void ReachCheckpoint(Checkpoint cp)
        {
            if (Phase != GamePhase.Deliver && Phase != GamePhase.Return) return;
            if (cp == _lastCheckpoint) return;
            _lastCheckpoint = cp;
            cp.SetLit(true);
            _respawnPosition = cp.spawnPoint.position;
            _respawnRotation = Phase == GamePhase.Return
                ? cp.spawnPoint.rotation * Quaternion.Euler(0f, 180f, 0f)
                : cp.spawnPoint.rotation;
            warmth.Refill();
            int total = checkpoints != null && checkpoints.Length > 0 ? checkpoints.Length : 5;
            hud.ShowToast($"Fener {cp.index}/{total} yandı  ·  ısındın", 2.2f);
            Debug.Log($"[MutluPrens] Kontrol noktası {cp.index} ({Phase})");
        }

        private void OnHitHazard(Hazard hazard)
        {
            HitCount++;
            warmth.ApplyHit(hazard.warmthDamage);
            chaseCamera.Shake(0.35f, 0.25f);
            hud.ShowAlert($"{hazard.label} çarptı  ·  ısı azaldı", 1.4f);
        }

        private void OnFrozen()
        {
            if (IsBusy || Phase == GamePhase.Ending) return;
            FreezeCount++;
            StartCoroutine(RespawnRoutine("Kırlangıç dondu  ·  son yanan fenere dönülüyor"));
        }

        private IEnumerator RespawnRoutine(string message)
        {
            if (IsBusy) yield break;
            IsBusy = true;
            Debug.Log($"[MutluPrens] Yeniden doğma: {(message ?? "oyuncu isteği")}  konum={swallow.transform.position}  ısı={warmth.Current:0}");
            swallow.InputEnabled = false;
            warmth.Active = false;
            swallowVisual.Fanning = false;
            if (message != null)
            {
                hud.ShowAlert(message, 2.2f);
                yield return new WaitForSecondsRealtime(0.8f);
            }
            yield return hud.Fade(1f, fadeTime);
            _zones.Clear();
            swallow.Teleport(_respawnPosition, _respawnRotation);
            warmth.Refill();
            chaseCamera.SnapToTarget();
            Respawned?.Invoke();
            yield return new WaitForSecondsRealtime(0.25f);
            yield return hud.Fade(0f, fadeTime);
            swallow.InputEnabled = true;
            warmth.Active = true;
            IsBusy = false;
        }

        // ================================================================== Arayüz yardımcıları

        private void UpdateGuidance()
        {
            Vector3 pos = swallow.transform.position;
            string label;
            float distance;
            Vector3 steer;

            switch (Phase)
            {
                case GamePhase.Listen:
                case GamePhase.TakeRuby:
                    label = Phase == GamePhase.Listen ? "Mutlu Prens" : "Kılıçtaki yakut";
                    steer = prince.rubyPoint.position;
                    distance = Vector3.Distance(pos, steer);
                    break;
                case GamePhase.Deliver:
                    label = "Terzi kadının evi";
                    distance = routeGuide.RemainingDistance(pos, true);
                    steer = routeGuide.SteerPoint(pos, true, 14f);
                    if (distance < 14f) steer = house.approachPoint.position;
                    float progress = routeGuide.Progress01(pos);
                    prince.SetSight(Mathf.Min(prince.Sight, Mathf.Lerp(0.8f, sightWhileCarrying, progress)));
                    hud.SetCost(true, prince.Sight, false, false);
                    break;
                case GamePhase.Fan:
                    label = "Terzi kadının penceresi";
                    steer = house.approachPoint.position;
                    distance = Vector3.Distance(pos, steer);
                    break;
                case GamePhase.Return:
                    label = "Mutlu Prens";
                    distance = routeGuide.RemainingDistance(pos, false);
                    steer = routeGuide.SteerPoint(pos, false, 14f);
                    if (distance < 14f) steer = prince.approachPoint.position;
                    break;
                default:
                    hud.SetCompass(false, "", 0f, 0f);
                    return;
            }

            Vector3 camFwd = _camera != null ? _camera.transform.forward : swallow.transform.forward;
            camFwd.y = 0f;
            Vector3 dir = steer - pos;
            dir.y = 0f;
            float angle = dir.sqrMagnitude > 0.01f && camFwd.sqrMagnitude > 0.01f
                ? Vector3.SignedAngle(camFwd, dir, Vector3.up)
                : 0f;
            hud.SetCompass(true, label, distance, angle);
        }

        private void UpdateColdFeedback()
        {
            float n = warmth.Normalized;
            float frost = Mathf.InverseLerp(0.6f, 0.05f, n);
            if (MigrationPostponed) frost = Mathf.Max(frost, 0.12f);
            hud.SetFrost(frost);
            hud.SetWarmth(n, warmth.CapNormalized, warmth.IsInCold, warmth.IsWarming, swallow.AboveCeiling,
                swallow.WindStrength > 9f && !swallow.AboveCeiling);
        }

        // ================================================================== Menü komutları

        private void SetPaused(bool paused)
        {
            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            swallow.InputEnabled = !paused && !IsBusy;
            if (menu) menu.ShowPause(paused);
            hud.SetHudVisible(!paused);
        }

        public void Resume()
        {
            if (IsPaused) SetPaused(false);
        }

        public void RespawnFromMenu()
        {
            SetPaused(false);
            StartCoroutine(RespawnRoutine(null));
        }

        /// <summary>Sahneyi yeniden yükler. skipMenu=true ise doğrudan oyun başlar ("Tekrar Oyna").</summary>
        public void RestartLevel(bool skipMenu)
        {
            SkipMenuOnce = skipMenu;
            Time.timeScale = 1f;
            var scene = SceneManager.GetActiveScene();
            if (scene.buildIndex >= 0) SceneManager.LoadScene(scene.buildIndex);
            else SceneManager.LoadScene(scene.name);
        }
    }
}
