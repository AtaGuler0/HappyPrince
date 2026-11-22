using System.Collections.Generic;
using MutluPrens.Core;
using MutluPrens.Player;
using MutluPrens.World;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MutluPrens.EditorTools
{
    /// <summary>Oyuncu, kamera, rota, kontrol noktaları, rüzgâr/sıcaklık bölgeleri ve diyalog tetikleyicileri.</summary>
    public static partial class MutluPrensLevelBuilder
    {
        // Rota (gidiş): oyuncunun ve yol ışıklarının izlediği güvenli uçuş hattı.
        private static readonly Vector3[] RoutePoints =
        {
            // Meydan -> Katedral Caddesi (çanların altından, ~6 m)
            new(1.2f, 14.6f, -2.4f), new(7, 12, 2), new(0, 10, 22), new(0, 7, 38), new(0, 6, 48), new(0, 6, 62),
            new(0, 6, 76), new(0, 6, 90), new(0, 6, 98), new(0, 6, 106), new(0, 6, 118), new(0, 7, 136), new(0, 6, 156),
            // Saray Bulvarı (fener dizilerinin üstünden/arasından, sallanan fenerlerin altından)
            new(3, 6, 165), new(14, 7, 168), new(24, 11, 168), new(34, 13, 168), new(43, 11, 168), new(52, 9.5f, 168),
            new(60, 8, 167), new(66, 5, 168), new(74, 4, 168), new(83, 4, 168), new(92, 4, 168), new(102, 5, 168),
            new(112, 6, 169), new(119, 7, 176),
            // Rıhtım (alçak halatın üstünden, yüksek halatın altından)
            new(120, 8.5f, 182), new(120, 8.5f, 190), new(120, 6.5f, 196), new(120, 4.5f, 202), new(120, 6.5f, 214),
            new(120, 8.5f, 226), new(120, 6.5f, 232), new(120, 4.5f, 238), new(120, 6.5f, 249), new(120, 8.5f, 260),
            new(120, 6.5f, 266), new(120, 4.5f, 272), new(120, 7, 280), new(126, 8, 287),
            // Köprü ve Eski Çarşı (terazilerin kirişinin üstünden)
            new(140, 8, 288), new(160, 8, 288), new(178, 8, 288), new(192, 9, 288), new(199, 14, 288), new(206, 18, 288),
            new(213, 14, 288), new(220, 12, 288), new(226, 15, 288), new(232, 18, 288), new(239, 14, 288), new(248, 9, 288),
            // Yoksul mahalle (çamaşır iplerinin üstünden ve altından) -> pencere
            new(258, 7, 288), new(270, 9.5f, 288), new(277, 7, 288), new(284, 4.5f, 288), new(292, 3.8f, 288),
            new(296, 3.6f, 288), new(300.5f, 3.2f, 288),
        };

        // =====================================================================================
        // Rüzgâr ve sıcaklık bölgeleri
        // =====================================================================================

        private static void BuildWeather(Transform root)
        {
            // Nehirden rıhtıma doğru esen kesik kesik soğuk rüzgâr (köprüyü de kapsar)
            WindZone(root, "Nehir_Ruzgari", new Vector3(147f, 12f, 242f), new Vector3(62f, 24f, 116f), Vector3.left,
                2f, 11f, true, 3.4f, 1.4f, 0f, -2.5f);

            // Sarayın sıcak pencereleri
            BoxTrigger("Saray_Pencerelerinin_Sicakligi", root, new Vector3(64f, 8f, 172.5f), new Vector3(80f, 16f, 3f))
                .AddComponent<WarmthArea>().ratePerSecond = 10f;

            // Güney yolu: sürüye doğru giden yolda Kırlangıç'ı geri iten rüzgâr (tereddüt anı)
            WindZone(root, "Guney_Yolu_Geri_Iten_Ruzgar", new Vector3(0, 12, -68), new Vector3(12, 24, 40), Vector3.forward,
                26f, 0f, false, 1f, 1f, 0f, -4f);
        }

        private static GameObject WindZone(Transform parent, string name, Vector3 center, Vector3 size, Vector3 dir,
            float baseStrength, float gust, bool gusty, float interval, float duration, float offset, float cold)
        {
            var go = BoxTrigger(name, parent, center, size);
            var w = go.AddComponent<WindArea>();
            w.direction = dir;
            w.baseStrength = baseStrength;
            w.gustStrength = gust;
            w.gusty = gusty;
            w.gustInterval = interval;
            w.gustDuration = duration;
            w.timeOffset = offset;
            w.coldRate = cold;

            var psGo = new GameObject("Ruzgar_Cizgileri");
            psGo.transform.SetParent(go.transform, false);
            psGo.transform.position = center;
            var ps = psGo.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 1.4f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.14f);
            main.startColor = new Color(0.85f, 0.92f, 1f, 0.45f);
            main.maxParticles = 1500;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = Mathf.Min(500f, size.x * size.z * 0.06f);
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = size;
            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;
            float speed = gusty ? 16f : 14f;
            vel.x = new ParticleSystem.MinMaxCurve(dir.x * speed * 0.8f, dir.x * speed * 1.2f);
            vel.y = new ParticleSystem.MinMaxCurve(-0.5f, 0.2f);
            vel.z = new ParticleSystem.MinMaxCurve(dir.z * speed * 0.8f, dir.z * speed * 1.2f);
            var psr = psGo.GetComponent<ParticleSystemRenderer>();
            psr.renderMode = ParticleSystemRenderMode.Stretch;
            psr.velocityScale = 0.12f;
            psr.lengthScale = 2f;
            psr.sharedMaterial = ParticleMat("Ruzgar_Parcacik", new Color(0.9f, 0.95f, 1f, 0.5f));
            w.streaks = ps;
            return go;
        }

        // =====================================================================================
        // Kontrol noktaları (yol fenerleri)
        // =====================================================================================

        private static Checkpoint[] BuildCheckpoints(Transform root)
        {
            var post = Mat("Demir", new Color(0.12f, 0.12f, 0.14f), 0.4f, 0.6f);
            var lamp = Mat("Kontrol_Feneri", new Color(0.3f, 0.3f, 0.35f), 0.4f, 0f, new Color(0.15f, 0.25f, 0.45f));
            return new[]
            {
                MakeCheckpoint(root, 1, new Vector3(0, 0, 98), true, new Vector3(5.2f, 0, 98), post, lamp),
                MakeCheckpoint(root, 2, new Vector3(16, 0, 168), false, new Vector3(16, 0, 163f), post, lamp),
                MakeCheckpoint(root, 3, new Vector3(120, 0, 182), true, new Vector3(114.8f, 0, 182), post, lamp),
                MakeCheckpoint(root, 4, new Vector3(192, 0, 288), false, new Vector3(192, 0, 283f), post, lamp),
                MakeCheckpoint(root, 5, new Vector3(258, 0, 288), false, new Vector3(258, 0, 283f), post, lamp),
            };
        }

        private static Checkpoint MakeCheckpoint(Transform parent, int index, Vector3 center, bool alongZ, Vector3 lanternBase,
            Material post, Material lamp)
        {
            var size = alongZ ? new Vector3(12, 24, 2.5f) : new Vector3(2.5f, 24, 12);
            var go = BoxTrigger($"Fener_{index}", parent, center + Vector3.up * 12f, size);
            var cp = go.AddComponent<Checkpoint>();
            cp.index = index;
            cp.spawnPoint = Marker("Dogma_Noktasi", go.transform, center + Vector3.up * 6f, alongZ ? 0f : 90f);
            var lantern = new GameObject("Fener").transform;
            lantern.SetParent(go.transform, false);
            lantern.position = lanternBase;
            Box("Direk", lantern, lanternBase + Vector3.up * 3f, new Vector3(0.35f, 6f, 0.35f), post);
            cp.lampRenderer = Box("Fener_Cami", lantern, lanternBase + Vector3.up * 6.6f, new Vector3(0.9f, 1.2f, 0.9f), lamp).GetComponent<Renderer>();
            cp.lampLight = AddPointLight(lantern, "Fener_Isigi", lanternBase + Vector3.up * 6.6f, new Color(0.5f, 0.65f, 1f), 12f, 1.5f);
            SphereTrigger("Fener_Sicakligi", lantern, lanternBase + Vector3.up * 6f, 4.5f).AddComponent<WarmthArea>().ratePerSecond = 18f;
            return cp;
        }

        // =====================================================================================
        // Rota, yol ışıkları, hedef ışık sütunu
        // =====================================================================================

        private static RouteGuide BuildRoute(Transform root)
        {
            var wpRoot = Group("Rota_Noktalari", root);
            var wps = new Transform[RoutePoints.Length];
            for (int i = 0; i < RoutePoints.Length; i++) wps[i] = Marker($"RN_{i:00}", wpRoot, RoutePoints[i], 0f);

            var outMat = Mat("YolIsigi_Gidis", new Color(0.4f, 0.3f, 0.1f), 0.2f, 0f, new Color(3f, 2.2f, 0.6f));
            var retMat = Mat("YolIsigi_Donus", new Color(0.1f, 0.2f, 0.4f), 0.2f, 0f, new Color(0.6f, 1.4f, 3f));

            var lightsRoot = Group("Yol_Isiklari", root);
            var orbs = new List<Renderer>();
            float total = 0f;
            var cum = new float[RoutePoints.Length];
            for (int i = 1; i < RoutePoints.Length; i++) cum[i] = total += Vector3.Distance(RoutePoints[i - 1], RoutePoints[i]);
            for (float s = 8f; s < total - 3f; s += 4.5f)
            {
                int seg = 0;
                while (seg < RoutePoints.Length - 2 && cum[seg + 1] < s) seg++;
                float tt = (s - cum[seg]) / Mathf.Max(0.001f, cum[seg + 1] - cum[seg]);
                var p = Vector3.Lerp(RoutePoints[seg], RoutePoints[seg + 1], tt);
                orbs.Add(Prim(PrimitiveType.Sphere, "Isik", lightsRoot, p, Vector3.one * 0.24f, outMat, false).GetComponent<Renderer>());
            }

            var beaconRoot = new GameObject("Hedef_Isik_Sutunu").transform;
            beaconRoot.SetParent(root, false);
            var beamMat = TransparentMat("Hedef_Sutunu", new Color(1f, 0.82f, 0.45f, 0.18f));
            Prim(PrimitiveType.Cylinder, "Sutun", beaconRoot, new Vector3(0, 30, 0), new Vector3(1.2f, 30f, 1.2f), beamMat, false);
            AddPointLight(beaconRoot, "Sutun_Isigi", new Vector3(0, 4, 0), new Color(1f, 0.85f, 0.5f), 10f, 2f);

            var guide = root.gameObject.AddComponent<RouteGuide>();
            guide.waypoints = wps;
            guide.guideLightsRoot = lightsRoot.gameObject;
            guide.guideLights = orbs.ToArray();
            guide.outboundMaterial = outMat;
            guide.returnMaterial = retMat;
            guide.beacon = beaconRoot;
            return guide;
        }

        // =====================================================================================
        // Diyalog tetikleyicileri
        // =====================================================================================

        private static void BuildDialogueTriggers(Transform root)
        {
            Dialogue(root, "Katedral", new Vector3(0, 12, 38), new Vector3(12, 24, 2.5f), "cathedral", GamePhase.Deliver);
            Dialogue(root, "Saray_Balkonu", new Vector3(26, 12, 168), new Vector3(2.5f, 24, 12), "palace", GamePhase.Deliver);
            Dialogue(root, "Rihtim_Gemiler", new Vector3(120, 12, 177), new Vector3(12, 24, 2.5f), "river", GamePhase.Deliver);
            Dialogue(root, "Eski_Carsi", new Vector3(186, 12, 288), new Vector3(2.5f, 24, 12), "market", GamePhase.Deliver);
            Dialogue(root, "Yoksul_Mahalle", new Vector3(264, 12, 288), new Vector3(2.5f, 24, 12), "poor_quarter", GamePhase.Deliver);
            var south = Dialogue(root, "Guney_Tereddut", new Vector3(0, 12, -40), new Vector3(12, 24, 3f), "south", GamePhase.Listen, GamePhase.TakeRuby);
            south.repeatable = true;
            south.cooldown = 12f;
            var after = Dialogue(root, "Guney_Sonrasi", new Vector3(0, 12, -42), new Vector3(12, 24, 3f), "south_after", GamePhase.Deliver, GamePhase.Return);
            after.repeatable = true;
            after.cooldown = 12f;
        }

        private static DialogueTrigger Dialogue(Transform parent, string name, Vector3 center, Vector3 size, string id, params GamePhase[] phases)
        {
            var d = BoxTrigger(name, parent, center, size).AddComponent<DialogueTrigger>();
            d.lineSetId = id;
            d.phases = phases;
            return d;
        }

        // =====================================================================================
        // Kırlangıç, kameralar, kar, sürü
        // =====================================================================================

        private static SwallowController BuildSwallow(out SwallowWarmth warmth, out SwallowVisual visual)
        {
            var body = Mat("Kirlangic_Govde", new Color(0.12f, 0.16f, 0.32f), 0.5f);
            var belly = Mat("Kirlangic_Gogus", new Color(0.9f, 0.88f, 0.85f), 0.3f);
            var throat = Mat("Kirlangic_Gerdan", new Color(0.6f, 0.22f, 0.12f), 0.3f);
            var beak = Mat("Siyah", new Color(0.05f, 0.05f, 0.06f), 0.3f);
            var ruby = Mat("Yakut", new Color(0.7f, 0.02f, 0.05f), 0.9f, 0f, new Color(3f, 0.1f, 0.15f));

            var go = new GameObject("Kirlangic_Oyuncu");
            go.layer = 2; // Ignore Raycast: kamera çarpışma testi kuşu görmezden gelsin
            go.transform.position = new Vector3(0, 14, -16);
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 1f;
            rb.useGravity = false;
            go.AddComponent<SphereCollider>().radius = 0.45f;

            var model = new GameObject("Model").transform;
            model.SetParent(go.transform, false);
            var p = go.transform.position;
            var bodies = new List<Renderer>();
            GameObject Part(PrimitiveType t, string n, Transform par, Vector3 local, Vector3 scale, Material m, float yaw = 0f)
            {
                var o = Prim(t, n, par, p + local, scale, m, false);
                o.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
                o.layer = 2;
                return o;
            }
            bodies.Add(Part(PrimitiveType.Sphere, "Govde", model, Vector3.zero, new Vector3(0.5f, 0.42f, 1f), body).GetComponent<Renderer>());
            Part(PrimitiveType.Sphere, "Gogus", model, new Vector3(0, -0.08f, 0.12f), new Vector3(0.38f, 0.3f, 0.6f), belly);
            bodies.Add(Part(PrimitiveType.Sphere, "Bas", model, new Vector3(0, 0.1f, 0.5f), Vector3.one * 0.36f, body).GetComponent<Renderer>());
            Part(PrimitiveType.Sphere, "Gerdan", model, new Vector3(0, 0.0f, 0.62f), Vector3.one * 0.2f, throat);
            Part(PrimitiveType.Cube, "Gaga", model, new Vector3(0, 0.08f, 0.72f), new Vector3(0.06f, 0.06f, 0.16f), beak);
            bodies.Add(Part(PrimitiveType.Cube, "Kuyruk_Sol", model, new Vector3(-0.09f, 0f, -0.62f), new Vector3(0.06f, 0.03f, 0.55f), body, -14f).GetComponent<Renderer>());
            bodies.Add(Part(PrimitiveType.Cube, "Kuyruk_Sag", model, new Vector3(0.09f, 0f, -0.62f), new Vector3(0.06f, 0.03f, 0.55f), body, 14f).GetComponent<Renderer>());

            var wingL = new GameObject("Kanat_Pivot_Sol").transform;
            wingL.SetParent(model, false);
            wingL.localPosition = new Vector3(-0.2f, 0.08f, 0.05f);
            var wingR = new GameObject("Kanat_Pivot_Sag").transform;
            wingR.SetParent(model, false);
            wingR.localPosition = new Vector3(0.2f, 0.08f, 0.05f);
            var wl = Part(PrimitiveType.Cube, "Kanat_Sol", wingL, Vector3.zero, new Vector3(1.1f, 0.04f, 0.42f), body);
            wl.transform.localPosition = new Vector3(-0.55f, 0f, 0f);
            var wr = Part(PrimitiveType.Cube, "Kanat_Sag", wingR, Vector3.zero, new Vector3(1.1f, 0.04f, 0.42f), body);
            wr.transform.localPosition = new Vector3(0.55f, 0f, 0f);
            bodies.Add(wl.GetComponent<Renderer>());
            bodies.Add(wr.GetComponent<Renderer>());

            var carried = Part(PrimitiveType.Sphere, "Tasinan_Yakut", model, new Vector3(0, -0.35f, 0.15f), Vector3.one * 0.22f, ruby);
            AddPointLight(carried.transform, "Yakut_Isigi", p + new Vector3(0, -0.4f, 0.15f), new Color(1f, 0.15f, 0.2f), 3.5f, 2f);

            var controller = go.AddComponent<SwallowController>();
            warmth = go.AddComponent<SwallowWarmth>();
            visual = go.AddComponent<SwallowVisual>();
            visual.controller = controller;
            visual.warmth = warmth;
            visual.modelRoot = model;
            visual.leftWing = wingL;
            visual.rightWing = wingR;
            visual.bodyRenderers = bodies.ToArray();
            visual.carriedRuby = carried;
            return controller;
        }

        private static Camera BuildCamera(Transform target, out MenuCamera menuCam)
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = go.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = RenderSettings.fogColor;
            cam.fieldOfView = 62f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 450f;
            go.AddComponent<AudioListener>();
            cam.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            var chase = go.AddComponent<ChaseCamera>();
            chase.target = target;
            menuCam = go.AddComponent<MenuCamera>();
            // Sahne görünümü/ilk kare için menü kamerasının bakış açısı
            go.transform.position = new Vector3(Mathf.Sin(200f * Mathf.Deg2Rad) * 24f, 19f, Mathf.Cos(200f * Mathf.Deg2Rad) * 24f);
            go.transform.LookAt(new Vector3(0f, 15.5f, 0f));
            return cam;
        }

        private static SnowController BuildSnow(Transform parent, Transform follow)
        {
            var go = new GameObject("Kar_Yagisi");
            go.transform.SetParent(parent, false);
            go.transform.position = follow.position + new Vector3(0, 14, 0);
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 8f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.14f);
            main.startColor = new Color(1f, 1f, 1f, 0.85f);
            main.maxParticles = 8000;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.prewarm = true;
            var emission = ps.emission;
            emission.rateOverTime = 500f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(55f, 1f, 55f);
            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;
            vel.x = new ParticleSystem.MinMaxCurve(-0.6f, 0.6f);
            vel.y = new ParticleSystem.MinMaxCurve(-2.6f, -1.6f);
            vel.z = new ParticleSystem.MinMaxCurve(-0.9f, 0.3f);
            var psr = go.GetComponent<ParticleSystemRenderer>();
            psr.renderMode = ParticleSystemRenderMode.Billboard;
            psr.sharedMaterial = ParticleMat("Kar_Parcacik", Color.white);
            var snow = go.AddComponent<SnowController>();
            snow.follow = follow;
            snow.snow = ps;
            return snow;
        }

        private static FlockOfSwallows BuildFlock(Transform parent)
        {
            var mat = Mat("Suru_Kusu", new Color(0.08f, 0.09f, 0.12f), 0.3f);
            var root = new GameObject("Kirlangic_Surusu_Guneye_Gidiyor");
            root.transform.SetParent(parent, false);
            root.transform.position = new Vector3(0, 32, -96);
            for (int i = 0; i < 10; i++)
            {
                var b = new GameObject($"Kus_{i}").transform;
                b.SetParent(root.transform, false);
                b.localPosition = new Vector3(i, 0, 0);
                Box("Govde", b, b.position, new Vector3(0.35f, 0.18f, 0.8f), mat, false).transform.localPosition = Vector3.zero;
                Box("Kanatlar", b, b.position, new Vector3(1.8f, 0.05f, 0.35f), mat, false).transform.localPosition = new Vector3(0, 0.05f, 0.05f);
            }
            return root.AddComponent<FlockOfSwallows>();
        }
    }
}
