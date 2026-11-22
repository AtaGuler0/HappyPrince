using System.Collections.Generic;
using MutluPrens.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MutluPrens.EditorTools
{
    /// <summary>
    /// "Mutlu Prens > Bölümü Yeniden Oluştur": gri kutu şehir bölümünü sıfırdan kurar ve
    /// Assets/_MutluPrens/Scenes/MutluPrens_Bolum1.unity olarak kaydeder. Ölçüler metre; +Z kuzey, +X doğu.
    ///
    /// Rota masaldaki sırayı izler:
    ///   Meydan ve Prens'in sütunu (0,0)
    ///   -> Katedral Caddesi (x=0, z 28..168): mermer melekli katedral, çanlı kemerler
    ///   -> Saray Bulvarı (z=168, x 0..120): balkonlu saray, fener dizileri
    ///   -> Rıhtım (x=120, z 168..288): direklerine fener asılı gemiler, halatlar, nehir rüzgârı
    ///   -> Köprü ve Eski Çarşı (z=288, x 120..256): bakır teraziler, tenteler
    ///   -> Yoksul Mahalle ve terzi kadının evi (~300, 288)
    ///
    /// Dosya parçaları: bu dosya (kurulum, ışık, şehir ızgarası, yardımcılar), .Landmarks (yapılar ve engeller),
    /// .Gameplay (oyuncu, rota, bölgeler), .UI (arayüz ve menüler).
    /// </summary>
    public static partial class MutluPrensLevelBuilder
    {
        public const string RootFolder = "Assets/_MutluPrens";
        public const string ScenePath = RootFolder + "/Scenes/MutluPrens_Bolum1.unity";
        private const string MatFolder = RootFolder + "/Materials";
        private const string SettingsFolder = RootFolder + "/Settings";
        private const string UIFolder = RootFolder + "/UI";
        private const string FontFolder = RootFolder + "/Fonts";

        // ---- Şehir planı ----
        private const float Cell = 4f;
        private const float StreetHalfWidth = 6f;
        private const float BuildingBand = 16f;
        private const float SquareRadius = 26f;

        private static readonly Vector2[] Route =
        {
            new(0, 0), new(0, 28), new(0, 168), new(120, 168), new(120, 288), new(200, 288), new(296, 288),
        };
        private static readonly Vector2[] SouthRoad = { new(0, 0), new(0, -84) };

        // Nehir: rıhtımın doğusunda kuzey-güney akar (x 126..178, z 150..310).
        private static readonly Vector2 RiverCenter = new(152, 230);
        private static readonly Vector2 RiverHalf = new(26, 80);

        // Özel yapıların ayrıldığı hücreler (hücre merkezi bu dikdörtgenin içindeyse genel bina konmaz).
        private static readonly Rect CathedralCells = Rect.MinMaxRect(-25, 43, -7, 149);
        private static readonly Rect PalaceCells = Rect.MinMaxRect(23, 175, 105, 205);
        private static readonly Rect HouseCells = Rect.MinMaxRect(301, 277, 319, 299);

        // Çanlı kemerlerin z konumları (Katedral Caddesi)
        private static readonly float[] ArchZ = { 56f, 84f, 112f };

        private static Dictionary<string, Material> _mats;
        private static System.Random _rng;

        // =====================================================================================
        // Menü
        // =====================================================================================

        [MenuItem("Mutlu Prens/Bölümü Yeniden Oluştur", priority = 0)]
        private static void MenuBuild()
        {
            if (EditorUtility.DisplayDialog("Mutlu Prens",
                    "Bölüm sahnesi (MutluPrens_Bolum1.unity) sıfırdan yeniden oluşturulacak.\nSahnede elle yaptığın değişiklikler kaybolur. Devam edilsin mi?",
                    "Oluştur", "Vazgeç"))
                Build(true);
        }

        [MenuItem("Mutlu Prens/Bölüm Sahnesini Aç", priority = 1)]
        private static void MenuOpen()
        {
            if (!System.IO.File.Exists(ScenePath)) { Build(true); return; }
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(ScenePath);
        }

        // =====================================================================================
        // Ana kurulum
        // =====================================================================================

        public static bool Build(bool askToSave)
        {
            if (askToSave && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;

            EnsureFolder(RootFolder, "Scenes");
            EnsureFolder(RootFolder, "Materials");
            EnsureFolder(RootFolder, "Settings");
            EnsureFolder(RootFolder, "UI");

            _rng = new System.Random(1888); // her kurulumda aynı şehir
            _mats = new Dictionary<string, Material>();
            var sprites = BuildUISprites();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var management = new GameObject("_Yonetim");
            var lighting = Group("Isiklandirma");
            var city = Group("Sehir");
            var square = Group("Meydan_MutluPrens");
            var cathedral = Group("Katedral_ve_Canli_Kemerler");
            var palace = Group("Saray_ve_Balkon");
            var river = Group("Nehir_Rihtim_Gemiler");
            var market = Group("Eski_Carsi");
            var poor = Group("Yoksul_Mahalle");
            var houseRoot = Group("Terzi_Kadinin_Evi");
            var weather = Group("Ruzgar_ve_Sicaklik_Bolgeleri");
            var checkpointsRoot = Group("Kontrol_Noktalari_Fenerler");
            var routeRoot = Group("Rota_ve_Yonlendirme");
            var dialogueRoot = Group("Diyalog_Tetikleyicileri");
            var atmosphere = Group("Atmosfer");

            BuildLighting(lighting);
            BuildCity(city);
            var prince = BuildPrince(square, out var startPoint);
            BuildCathedral(cathedral);
            BuildPalace(palace);
            BuildRiver(river);
            BuildMarket(market);
            BuildPoorQuarter(poor);
            var house = BuildHouse(houseRoot);
            BuildWeather(weather);
            var checkpoints = BuildCheckpoints(checkpointsRoot);
            var guide = BuildRoute(routeRoot);
            BuildDialogueTriggers(dialogueRoot);

            var swallow = BuildSwallow(out var warmth, out var visual);
            var cam = BuildCamera(swallow.transform, out var menuCam);
            var snow = BuildSnow(atmosphere, cam.transform);
            var flock = BuildFlock(atmosphere);

            var gm = management.AddComponent<GameManager>();
            var ui = BuildUI(sprites, gm, out var menu);

            gm.swallow = swallow;
            gm.warmth = warmth;
            gm.swallowVisual = visual;
            gm.chaseCamera = cam.GetComponent<MutluPrens.Player.ChaseCamera>();
            gm.menuCamera = menuCam;
            gm.prince = prince;
            gm.house = house;
            gm.routeGuide = guide;
            gm.hud = ui;
            gm.menu = menu;
            gm.snow = snow;
            gm.flock = flock;
            gm.startPoint = startPoint;
            gm.checkpoints = checkpoints;
            management.AddComponent<ScreenshotTool>().hud = ui;

            foreach (var m in _mats.Values) EditorUtility.SetDirty(m);
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings();
            Debug.Log("[MutluPrens] Bölüm sahnesi oluşturuldu: " + ScenePath);
            return true;
        }

        // =====================================================================================
        // Işıklandırma ve son işleme
        // =====================================================================================

        private static void BuildLighting(Transform parent)
        {
            var fogColor = new Color(0.085f, 0.1f, 0.16f);
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.2f, 0.23f, 0.33f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 35f;
            RenderSettings.fogEndDistance = 190f;
            RenderSettings.fogColor = fogColor;

            var lightingSettingsPath = SettingsFolder + "/MutluPrens_Lighting.lighting";
            var ls = AssetDatabase.LoadAssetAtPath<LightingSettings>(lightingSettingsPath);
            if (ls == null)
            {
                ls = new LightingSettings { name = "MutluPrens_Lighting" };
                AssetDatabase.CreateAsset(ls, lightingSettingsPath);
            }
            ls.bakedGI = false;
            ls.realtimeGI = false;
            Lightmapping.lightingSettings = ls;

            var moon = new GameObject("Ay_Isigi");
            moon.transform.SetParent(parent, false);
            moon.transform.rotation = Quaternion.Euler(38f, 150f, 0f);
            var l = moon.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = new Color(0.55f, 0.65f, 0.95f);
            l.intensity = 0.55f;
            l.shadows = LightShadows.Soft;
            l.shadowStrength = 0.6f;

            var profilePath = SettingsFolder + "/MutluPrens_PostProcess.asset";
            AssetDatabase.DeleteAsset(profilePath);
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, profilePath);
            var bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(0.9f);
            bloom.threshold.Override(1.0f);
            bloom.scatter.Override(0.65f);
            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.32f);
            var tonemap = profile.Add<Tonemapping>(true);
            tonemap.mode.Override(TonemappingMode.Neutral);
            foreach (var c in profile.components)
            {
                c.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(c, profile);
            }
            EditorUtility.SetDirty(profile);

            var volumeGo = new GameObject("Global_Volume");
            volumeGo.transform.SetParent(parent, false);
            var volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = profile;
        }

        // =====================================================================================
        // Şehir ızgarası (zemin, genel binalar, pencereler, sokak lambaları, sınırlar)
        // =====================================================================================

        private static float OpenSdf(Vector2 c)
        {
            float d = DistToPolyline(c, Route) - StreetHalfWidth;
            d = Mathf.Min(d, DistToPolyline(c, SouthRoad) - StreetHalfWidth);
            d = Mathf.Min(d, c.magnitude - SquareRadius);
            Vector2 q = new(Mathf.Abs(c.x - RiverCenter.x) - RiverHalf.x, Mathf.Abs(c.y - RiverCenter.y) - RiverHalf.y);
            float river = Vector2.Max(q, Vector2.zero).magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0f);
            return Mathf.Min(d, river);
        }

        private static float DistToPolyline(Vector2 p, Vector2[] line)
        {
            float best = float.MaxValue;
            for (int i = 0; i < line.Length - 1; i++)
            {
                Vector2 a = line[i], ab = line[i + 1] - a;
                float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
                best = Mathf.Min(best, Vector2.Distance(p, a + ab * t));
            }
            return best;
        }

        /// <summary>Semt: 0 katedral, 1 saray, 2 rıhtım, 3 çarşı, 4 yoksul mahalle.</summary>
        private static int District(Vector2 c)
        {
            if (c.x > 250f) return 4;
            if (c.y > 262f && c.x > 178f) return 3;
            if (c.x > 100f) return 2;
            if (c.y > 150f) return 1;
            return 0;
        }

        private static void BuildCity(Transform city)
        {
            var groundMat = Mat("Zemin_KarliParkeTasi", new Color(0.42f, 0.45f, 0.52f), 0.15f);
            Box("Zemin", city, new Vector3(140f, -0.5f, 116f), new Vector3(430f, 1f, 470f), groundMat).isStatic = true;

            var bounds = Group("Dunya_Sinirlari", city);
            InvisibleWall(bounds, new Vector3(140f, 20f, -116f), new Vector3(430f, 40f, 2f));
            InvisibleWall(bounds, new Vector3(140f, 20f, 348f), new Vector3(430f, 40f, 2f));
            InvisibleWall(bounds, new Vector3(-68f, 20f, 116f), new Vector3(2f, 40f, 470f));
            InvisibleWall(bounds, new Vector3(348f, 20f, 116f), new Vector3(2f, 40f, 470f));
            InvisibleWall(bounds, new Vector3(140f, 41f, 116f), new Vector3(430f, 2f, 470f));

            var buildings = Group("Binalar", city);
            var windowsRoot = Group("Pencereler", city);
            Material[][] palettes =
            {
                new[] { Mat("Bina_Gri", new Color(0.36f, 0.38f, 0.42f)), Mat("Bina_Mavi", new Color(0.3f, 0.33f, 0.41f)), Mat("Bina_Acik", new Color(0.46f, 0.46f, 0.49f)) },
                new[] { Mat("Bina_Krem", new Color(0.56f, 0.52f, 0.46f)), Mat("Bina_Kumtasi", new Color(0.46f, 0.42f, 0.38f)), Mat("Bina_Acik", new Color(0.46f, 0.46f, 0.49f)) },
                new[] { Mat("Bina_Tugla", new Color(0.38f, 0.26f, 0.22f)), Mat("Bina_Ambar", new Color(0.3f, 0.27f, 0.25f)), Mat("Bina_Gri", new Color(0.36f, 0.38f, 0.42f)) },
                new[] { Mat("Bina_Hardal", new Color(0.52f, 0.4f, 0.24f)), Mat("Bina_Kiremit", new Color(0.48f, 0.3f, 0.22f)), Mat("Bina_Kumtasi", new Color(0.46f, 0.42f, 0.38f)) },
                new[] { Mat("Bina_Ambar", new Color(0.3f, 0.27f, 0.25f)), Mat("Bina_Gri", new Color(0.36f, 0.38f, 0.42f)) },
            };
            Material[] low =
            {
                Mat("Ev_Kerpic", new Color(0.46f, 0.38f, 0.32f)),
                Mat("Ev_Ahsap", new Color(0.36f, 0.31f, 0.27f)),
            };
            float[] litChance = { 0.45f, 0.7f, 0.4f, 0.6f, 0.35f };
            var litWindow = Mat("Pencere_Isikli", new Color(0.25f, 0.18f, 0.06f), 0.4f, 0f, new Color(2.2f, 1.45f, 0.55f));
            var darkWindow = Mat("Pencere_Karanlik", new Color(0.07f, 0.09f, 0.13f), 0.7f);

            const int ix0 = -16, ix1 = 86, iz0 = -28, iz1 = 86;
            int nx = ix1 - ix0 + 1, nz = iz1 - iz0 + 1;
            var kind = new byte[nx, nz]; // 0 boş, 1 açık, 2 yüksek bina, 3 alçak ev, 4 özel yapı
            for (int i = 0; i < nx; i++)
            for (int j = 0; j < nz; j++)
            {
                var c = new Vector2((ix0 + i) * Cell, (iz0 + j) * Cell);
                if (HouseCells.Contains(c) || CathedralCells.Contains(c) || PalaceCells.Contains(c)) { kind[i, j] = 4; continue; }
                float d = OpenSdf(c);
                if (d < 0f) kind[i, j] = 1;
                else if (d < BuildingBand) kind[i, j] = (byte)(c.x > 250f && d < 8f ? 3 : 2);
            }

            int count = 0;
            for (int j = 0; j < nz; j++)
            {
                int i = 0;
                while (i < nx)
                {
                    byte k = kind[i, j];
                    if (k != 2 && k != 3) { i++; continue; }
                    int maxRun = 1 + _rng.Next(3);
                    int start = i;
                    while (i < nx && kind[i, j] == k && i - start < maxRun) i++;
                    int len = i - start;

                    float cx = ((ix0 + start) + (len - 1) * 0.5f) * Cell;
                    float cz = (iz0 + j) * Cell;
                    int district = District(new Vector2(cx, cz));
                    float h = k == 2 ? 26f + (float)_rng.NextDouble() * 10f : 5f + (float)_rng.NextDouble() * 5f;
                    var pal = palettes[district];
                    var mat = k == 2 ? pal[_rng.Next(pal.Length)] : low[_rng.Next(low.Length)];
                    var b = Box($"Bina_{count++:000}", buildings, new Vector3(cx, h * 0.5f, cz), new Vector3(len * Cell, h, Cell), mat);
                    b.isStatic = true;

                    if (k == 3 && _rng.NextDouble() < 0.5)
                        Box("Baca", buildings, new Vector3(cx + 1f, h + 1f, cz), new Vector3(0.8f, 2f, 0.8f), mat).isStatic = true;

                    float lit = litChance[district];
                    for (int s = start; s < start + len; s++)
                    {
                        float wx = (ix0 + s) * Cell;
                        TryWindow(kind, s, j + 1, nx, nz, new Vector3(wx, 0, cz + Cell * 0.5f + 0.05f), 0f, h, windowsRoot, litWindow, darkWindow, lit);
                        TryWindow(kind, s, j - 1, nx, nz, new Vector3(wx, 0, cz - Cell * 0.5f - 0.05f), 0f, h, windowsRoot, litWindow, darkWindow, lit);
                    }
                    TryWindow(kind, start - 1, j, nx, nz, new Vector3((ix0 + start) * Cell - Cell * 0.5f - 0.05f, 0, cz), 90f, h, windowsRoot, litWindow, darkWindow, lit);
                    TryWindow(kind, start + len, j, nx, nz, new Vector3((ix0 + start + len - 1) * Cell + Cell * 0.5f + 0.05f, 0, cz), 90f, h, windowsRoot, litWindow, darkWindow, lit);
                }
            }

            BuildLamps(Group("Sokak_Lambalari", city));
        }

        private static void TryWindow(byte[,] kind, int i, int j, int nx, int nz, Vector3 facePos, float yaw, float h,
            Transform parent, Material lit, Material dark, float litChance)
        {
            if (i < 0 || j < 0 || i >= nx || j >= nz || kind[i, j] != 1) return;
            int n = h > 12f ? 2 : 1;
            for (int w = 0; w < n; w++)
            {
                if (_rng.NextDouble() > 0.55) continue;
                float y = h > 12f ? 4f + (float)_rng.NextDouble() * (h - 8f) : 2.2f + (float)_rng.NextDouble() * 1.5f;
                var mat = _rng.NextDouble() < litChance ? lit : dark;
                var size = yaw == 0f ? new Vector3(1.3f, 1.7f, 0.1f) : new Vector3(0.1f, 1.7f, 1.3f);
                float off = ((float)_rng.NextDouble() - 0.5f) * 1.6f;
                var p = facePos + new Vector3(yaw == 0f ? off : 0f, y, yaw == 0f ? 0f : off);
                Box("Pencere", parent, p, size, mat, false).isStatic = true;
            }
        }

        private static bool LampKeepOut(Vector2 p)
        {
            if (p.magnitude < SquareRadius + 4f) return true;
            foreach (var z in ArchZ) if (Mathf.Abs(p.x) < 8f && Mathf.Abs(p.y - z) < 5f) return true;
            if (p.y > 160f && p.y < 176f && p.x > 26f && p.x < 100f) return true; // saray bulvarı (kendi fenerleri var)
            if (p.x > 124f && p.x < 182f) return true;                            // rıhtımın nehir tarafı ve köprü
            if (p.y > 280f && p.x > 194f && p.x < 250f) return true;              // çarşı tenteleri
            return false;
        }

        private static void BuildLamps(Transform parent)
        {
            var post = Mat("Demir", new Color(0.12f, 0.12f, 0.14f), 0.4f, 0.6f);
            var lamp = Mat("Sokak_Lambasi", new Color(0.4f, 0.3f, 0.15f), 0.3f, 0f, new Color(2.6f, 1.7f, 0.75f));
            int n = 0;
            for (int seg = 0; seg < Route.Length - 1; seg++)
            {
                Vector2 a = Route[seg], b = Route[seg + 1];
                Vector2 dir = (b - a).normalized, normal = new(-dir.y, dir.x);
                float len = Vector2.Distance(a, b);
                for (float s = 12f; s < len - 4f; s += 24f)
                {
                    Vector2 p = a + dir * s + normal * ((n % 2 == 0) ? 5.3f : -5.3f);
                    n++;
                    if (LampKeepOut(p)) continue;
                    var root = new GameObject($"Lamba_{n:00}");
                    root.transform.SetParent(parent, false);
                    root.transform.position = new Vector3(p.x, 0f, p.y);
                    Box("Direk", root.transform, root.transform.position + Vector3.up * 2.6f, new Vector3(0.25f, 5.2f, 0.25f), post);
                    Prim(PrimitiveType.Sphere, "Fener", root.transform, root.transform.position + Vector3.up * 5.4f, Vector3.one * 0.6f, lamp, false);
                    AddPointLight(root.transform, "Isik", root.transform.position + Vector3.up * 5f, new Color(1f, 0.75f, 0.45f), 13f, 3f);
                }
            }
        }

        // =====================================================================================
        // Genel yardımcılar
        // =====================================================================================

        private static Transform Group(string name, Transform parent = null)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static GameObject Box(string name, Transform parent, Vector3 center, Vector3 size, Material mat, bool collider = true)
            => Prim(PrimitiveType.Cube, name, parent, center, size, mat, collider);

        private static GameObject Prim(PrimitiveType type, string name, Transform parent, Vector3 center, Vector3 scale, Material mat, bool collider)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = center;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        /// <summary>İki dünya noktası arasında ince kiriş/halat (ebeveyn ölçeği 1 kabul edilir).</summary>
        private static GameObject Beam(string name, Transform parent, Vector3 a, Vector3 b, float thickness, Material mat, bool collider = true)
        {
            var go = Box(name, parent, (a + b) * 0.5f, Vector3.one, mat, collider);
            go.transform.rotation = Quaternion.LookRotation(b - a);
            go.transform.localScale = new Vector3(thickness, thickness, Vector3.Distance(a, b));
            return go;
        }

        private static void InvisibleWall(Transform parent, Vector3 center, Vector3 size)
        {
            var go = new GameObject("Gorunmez_Duvar");
            go.transform.SetParent(parent, false);
            go.transform.position = center;
            go.AddComponent<BoxCollider>().size = size;
            go.isStatic = true;
        }

        private static GameObject BoxTrigger(string name, Transform parent, Vector3 center, Vector3 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = center;
            var c = go.AddComponent<BoxCollider>();
            c.size = size;
            c.isTrigger = true;
            return go;
        }

        private static GameObject SphereTrigger(string name, Transform parent, Vector3 center, float radius)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = center;
            var c = go.AddComponent<SphereCollider>();
            c.radius = radius;
            c.isTrigger = true;
            return go;
        }

        private static Transform Marker(string name, Transform parent, Vector3 pos, float yaw)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, yaw, 0f));
            return go.transform;
        }

        private static Light AddPointLight(Transform parent, string name, Vector3 pos, Color color, float range, float intensity)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = color;
            l.range = range;
            l.intensity = intensity;
            l.shadows = LightShadows.None;
            return l;
        }

        private static Rigidbody Kinematic(GameObject go)
        {
            var rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            return rb;
        }

        private static Shader LitShader
        {
            get
            {
                var s = Shader.Find("Universal Render Pipeline/Lit");
                return s != null ? s : Shader.Find("Standard");
            }
        }

        private static Material Mat(string name, Color color, float smoothness = 0.2f, float metallic = 0f, Color? emission = null)
        {
            if (_mats.TryGetValue(name, out var cached)) return cached;
            string path = $"{MatFolder}/{name}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(LitShader);
                AssetDatabase.CreateAsset(m, path);
            }
            else if (m.shader != LitShader) m.shader = LitShader;

            m.SetColor("_BaseColor", color);
            m.color = color;
            m.SetFloat("_Smoothness", smoothness);
            m.SetFloat("_Metallic", metallic);
            if (emission.HasValue)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", emission.Value);
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            else
            {
                m.DisableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", Color.black);
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }
            _mats[name] = m;
            return m;
        }

        private static Material TransparentMat(string name, Color color)
        {
            if (_mats.TryGetValue(name, out var cached)) return cached;
            string path = $"{MatFolder}/{name}.mat";
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(shader);
                AssetDatabase.CreateAsset(m, path);
            }
            m.shader = shader;
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 2f);
            m.SetOverrideTag("RenderType", "Transparent");
            m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", (float)BlendMode.One);
            m.SetFloat("_ZWrite", 0f);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = (int)RenderQueue.Transparent;
            m.SetColor("_BaseColor", color);
            _mats[name] = m;
            return m;
        }

        private static Material ParticleMat(string name, Color color)
        {
            if (_mats.TryGetValue(name, out var cached)) return cached;
            string path = $"{MatFolder}/{name}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                var src = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/ParticlesUnlit.mat");
                m = src != null ? new Material(src) : new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
                AssetDatabase.CreateAsset(m, path);
            }
            m.SetColor("_BaseColor", color);
            _mats[name] = m;
            return m;
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder(parent))
            {
                var p = System.IO.Path.GetDirectoryName(parent)?.Replace('\\', '/');
                AssetDatabase.CreateFolder(p, System.IO.Path.GetFileName(parent));
            }
            if (!AssetDatabase.IsValidFolder(parent + "/" + child)) AssetDatabase.CreateFolder(parent, child);
        }

        private static void AddSceneToBuildSettings()
        {
            var list = new List<EditorBuildSettingsScene> { new(ScenePath, true) };
            foreach (var s in EditorBuildSettings.scenes)
                if (s.path != ScenePath) list.Add(new EditorBuildSettingsScene(s.path, false));
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
