#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using MutluPrens.Player;
using UnityEngine;

namespace MutluPrens.Core
{
    /// <summary>
    /// YALNIZCA EDİTÖR: Bölümün baştan sona tamamlanabildiğini doğrulamak için otomatik test pilotu.
    /// "Mutlu Prens > Otomatik Oynanış Testi" menüsüyle başlar; Kırlangıç'ı oyuncuyla aynı uçuş girdileriyle
    /// rota noktaları üzerinden uçurur, sonucu Console'a "[MP-AUTO]" önekiyle yazar ve
    /// Docs/Taslak_Ekran_Goruntuleri klasörüne taslak ekran görüntüleri kaydeder.
    /// </summary>
    public class AutopilotDriver : MonoBehaviour, IFlightInputSource
    {
        public const string FlagPath = "Temp/MP_autopilot_run";
        private const float Timeout = 600f;

        private GameManager _gm;
        private ScreenshotTool _shots;
        private readonly List<Vector3> _path = new();
        private readonly HashSet<string> _taken = new();
        private int _index;
        private float _elapsed;
        private float _stuckTimer;
        private Vector3 _lastPos;
        private bool _finished;
        private float _menuTimer;
        private bool _started;
        private float _endTimer;
        private float _traceTimer;

        private void Start()
        {
            Application.runInBackground = true; // editör odakta değilken de test sürsün
            _gm = GetComponent<GameManager>();
            _shots = GetComponent<ScreenshotTool>();
            if (_shots == null) _shots = gameObject.AddComponent<ScreenshotTool>();
            _gm.swallow.InputSource = this;
            _gm.PhaseChanged += OnPhaseChanged;
            _gm.Respawned += () => { _index = NearestIndex(); Log($"Yeniden doğdu, rota indeksi {_index}"); };
            if (GameplayVideoRecorder.Requested) _video = gameObject.AddComponent<GameplayVideoRecorder>();
            Log(_video != null ? "Otomatik test başladı (video kaydı)." : "Otomatik test başladı.");
        }

        private static void Log(string msg) => Debug.Log("[MP-AUTO] " + msg);

        // Video kaydında: replikler okunabilir sürede geçilir, dönüş yolu 2x hızlanır (video 2 dakika civarında kalsın).
        private GameplayVideoRecorder _video;
        private float _skipTimer;

        private void VideoPacing()
        {
            if (_video == null) return;
            if (_gm.hud.IsDialogueBusy)
            {
                _skipTimer += Time.deltaTime / Mathf.Max(0.01f, Time.timeScale); // videodaki saniye
                if (_skipTimer > 2.7f) { _gm.hud.SkipLine(); _skipTimer = 0f; }
            }
            else _skipTimer = 0f;
            Time.timeScale = _gm.Phase == GamePhase.Return ? 2f : 1f;
        }

        private void Shot(string name)
        {
            if (_video != null) return;
            if (_shots == null || _taken.Contains(name)) return;
            _taken.Add(name);
            _shots.folderName = "Docs/Taslak_Ekran_Goruntuleri";
            _shots.Capture(name);
        }

        private void Update()
        {
            if (_finished) return;
            _elapsed += Time.deltaTime;

            if (!_started)
            {
                _menuTimer += Time.deltaTime;
                if (_menuTimer > 3f) Shot("01_ana_menu");
                if (_menuTimer > 4f)
                {
                    _started = true;
                    _gm.StartLevel();
                }
                return;
            }

            VideoPacing();
            var p = _gm.swallow.transform.position;
            switch (_gm.Phase)
            {
                case GamePhase.Listen:
                    if (_elapsed > 18f) Shot("02_prens_ile_konusma");
                    break;
                case GamePhase.Deliver:
                    if (p.x < 5f && p.z > 76f && p.z < 82f) Shot("03_katedral_canlari");
                    if (p.x > 40f && p.x < 46f && p.z < 175f) Shot("04_saray_balkonu");
                    if (p.x > 64f && p.x < 70f && p.z < 175f) Shot("05_saray_fenerleri");
                    if (p.x < 125f && p.z > 216f && p.z < 222f) Shot("06_rihtim_gemiler");
                    if (p.x > 146f && p.x < 152f && p.z > 280f) Shot("07_kopru_bora");
                    if (p.x > 196f && p.x < 202f) Shot("08_eski_carsi_terazi");
                    if (p.x > 286f) Shot("09_terzi_kadinin_evi");
                    break;
                case GamePhase.Fan:
                    if (_gm.FanProgress > 0.6f) Shot("10_cocugu_serinletme");
                    break;
                case GamePhase.Return:
                    if (p.x > 270f && p.x < 285f) Shot("11_teslim_sonrasi");
                    if (p.x < 5f && p.z < 30f) Shot("12_kararan_prens");
                    break;
                case GamePhase.Ending:
                    _endTimer += Time.deltaTime;
                    if (_video != null && _endTimer > 12.5f) Finish(true, "Bölüm tamamlandı (video)");
                    if (_endTimer > 1.5f) Shot("13_bolum_sonu_konusma");
                    if (_endTimer > 16f) Shot("14_bolum_sonu_ekrani");
                    if (_endTimer > 17.5f) Finish(true, "Bölüm tamamlandı");
                    break;
            }

            _traceTimer += Time.deltaTime;
            if (_traceTimer > 10f)
            {
                _traceTimer = 0f;
                Log($"İz: t={_elapsed:0} aşama={_gm.Phase} konum={p} hedef[{_index}]={Target()} ısı={_gm.warmth.Current:0} çarpma={_gm.HitCount} timeScale={Time.timeScale}");
            }

            if ((p - _lastPos).sqrMagnitude > 1f) { _lastPos = p; _stuckTimer = 0f; }
            else if (!_gm.IsBusy && _gm.Phase != GamePhase.Listen && _gm.Phase != GamePhase.Fan && _gm.Phase != GamePhase.Ending)
            {
                _stuckTimer += Time.deltaTime;
                if (_stuckTimer > 12f) { Log($"UYARI: 12 sn ilerleme yok. Konum {p}, hedef {Target()}"); _stuckTimer = 0f; }
            }

            if (_elapsed > Timeout) Finish(false, "Zaman aşımı");
        }

        private void OnPhaseChanged(GamePhase phase)
        {
            Log($"Aşama -> {phase}  t={_elapsed:0.0}s  donma={_gm.FreezeCount} çarpma={_gm.HitCount} ısı={_gm.warmth.Current:0}");
            BuildPath();
        }

        private void BuildPath()
        {
            _path.Clear();
            var rg = _gm.routeGuide;
            switch (_gm.Phase)
            {
                case GamePhase.Listen:
                case GamePhase.TakeRuby:
                    _path.Add(_gm.prince.approachPoint.position);
                    break;
                case GamePhase.Deliver:
                    for (int i = 1; i < rg.Count; i++) _path.Add(rg.GetPoint(i));
                    _path.Add(_gm.house.approachPoint.position);
                    break;
                case GamePhase.Fan:
                    _path.Add(_gm.house.approachPoint.position);
                    break;
                case GamePhase.Return:
                    for (int i = rg.Count - 2; i >= 0; i--) _path.Add(rg.GetPoint(i));
                    _path.Add(_gm.prince.approachPoint.position);
                    break;
            }
            _index = 0;
        }

        private int NearestIndex()
        {
            if (_path.Count == 0) return 0;
            var pos = _gm.swallow.transform.position;
            int best = 0;
            float bestD = float.MaxValue;
            for (int i = 0; i < _path.Count; i++)
            {
                float d = (pos - _path[i]).sqrMagnitude;
                if (d < bestD) { bestD = d; best = i; }
            }
            return Mathf.Min(best + 1, _path.Count - 1);
        }

        private Vector3 Target() => _path.Count == 0 ? _gm.swallow.transform.position : _path[Mathf.Clamp(_index, 0, _path.Count - 1)];

        public FlightInput ReadFlight()
        {
            var input = new FlightInput();
            if (_path.Count == 0) return input;

            var s = _gm.swallow;
            var pos = s.transform.position;
            var target = Target();
            Vector3 to = target - pos;

            bool last = _index >= _path.Count - 1;
            if (!last && to.magnitude < 3.5f) { _index++; target = Target(); to = target - pos; }
            Vector3 flat = new(to.x, 0f, to.z);

            float desiredYaw = Mathf.Atan2(flat.x, flat.z) * Mathf.Rad2Deg;
            if (last && flat.magnitude < 1.5f && _gm.Phase == GamePhase.Fan) desiredYaw = 90f; // pencereye, çocuğa dön
            float delta = Mathf.DeltaAngle(s.Yaw, desiredYaw);
            input.Turn = Mathf.Clamp(delta / 25f, -1f, 1f);
            float hd = flat.magnitude;
            input.Throttle = Mathf.Abs(delta) < 50f ? Mathf.Clamp(hd / 5f, 0.25f, 1f) : 0.15f;
            if (last && hd < 1.2f) input.Throttle = 0f;
            input.Vertical = Mathf.Clamp(to.y / 1.5f, -1f, 1f);
            return input;
        }

        public bool InteractPressedThisFrame() => _gm != null && _gm.CanInteractNow;
        public bool InteractHeld() => _gm != null && _gm.CanFanNow;

        private void Finish(bool success, string reason)
        {
            if (_finished) return;
            _finished = true;
            Log($"SONUÇ: {(success ? "BAŞARILI" : "BAŞARISIZ")} ({reason})  süre={_gm.LevelTime:0.0}s  donma={_gm.FreezeCount}  çarpma={_gm.HitCount}  aşama={_gm.Phase}");
            try { File.Delete(FlagPath); } catch { /* yoksay */ }
            if (_video != null)
            {
                _video.StopRecording();
                try { File.Delete(GameplayVideoRecorder.FlagPath); } catch { /* yoksay */ }
            }
            Time.timeScale = 1f;
            Invoke(nameof(StopPlay), 1f);
        }

        private void StopPlay() => UnityEditor.EditorApplication.isPlaying = false;
    }
}
#endif
