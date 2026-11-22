using System.Collections.Generic;
using MutluPrens.World;
using UnityEngine;

namespace MutluPrens.EditorTools
{
    /// <summary>Masaldaki yerler ve onlara özgü engeller.</summary>
    public static partial class MutluPrensLevelBuilder
    {
        // =====================================================================================
        // Mutlu Prens heykeli (meydan)
        // =====================================================================================

        private static PrinceStatue BuildPrince(Transform parent, out Transform startPoint)
        {
            var stone = Mat("Tas", new Color(0.5f, 0.5f, 0.52f), 0.1f);
            var gold = Mat("Altin_Yaldiz", new Color(0.85f, 0.65f, 0.2f), 0.65f, 0.75f);
            var ruby = Mat("Yakut", new Color(0.7f, 0.02f, 0.05f), 0.9f, 0f, new Color(3f, 0.1f, 0.15f));
            var eye = Mat("Prens_Gozu", new Color(0.8f, 0.05f, 0.08f), 0.9f, 0f, new Color(4f, 0.25f, 0.3f));
            var dark = Mat("Siyah", new Color(0.05f, 0.05f, 0.06f), 0.3f);

            var root = new GameObject("MutluPrens_Heykeli");
            root.transform.SetParent(parent, false);
            var t = root.transform;

            Box("Basamak", t, new Vector3(0, 0.25f, 0), new Vector3(9, 0.5f, 9), stone).isStatic = true;
            Box("Kaide", t, new Vector3(0, 1.5f, 0), new Vector3(7, 2, 7), stone).isStatic = true;
            Box("Sutun", t, new Vector3(0, 7, 0), new Vector3(2.6f, 10, 2.6f), stone).isStatic = true;
            Box("Sutun_Basligi", t, new Vector3(0, 12.25f, 0), new Vector3(3.4f, 0.5f, 3.4f), stone).isStatic = true;

            var statue = new GameObject("Prens_Figuru").transform;
            statue.SetParent(t, false);
            var goldParts = new List<Renderer>
            {
                Box("Etek", statue, new Vector3(0, 13.6f, 0), new Vector3(1.4f, 2.2f, 0.9f), gold).GetComponent<Renderer>(),
                Box("Govde", statue, new Vector3(0, 15.4f, 0), new Vector3(1.7f, 1.6f, 1f), gold).GetComponent<Renderer>(),
                Box("Pelerin", statue, new Vector3(0, 15.1f, 0.4f), new Vector3(2.1f, 2.2f, 0.3f), gold).GetComponent<Renderer>(),
                Prim(PrimitiveType.Sphere, "Bas", statue, new Vector3(0, 16.65f, 0), Vector3.one * 0.95f, gold, true).GetComponent<Renderer>(),
                Box("Tac", statue, new Vector3(0, 17.25f, 0), new Vector3(0.7f, 0.3f, 0.7f), gold).GetComponent<Renderer>(),
                Box("Sol_Kol", statue, new Vector3(-1.05f, 15.0f, -0.1f), new Vector3(0.35f, 1.4f, 0.35f), gold).GetComponent<Renderer>(),
                Box("Sag_Kol", statue, new Vector3(1.05f, 15.0f, -0.25f), new Vector3(0.35f, 1.4f, 0.35f), gold).GetComponent<Renderer>(),
            };
            var eyeL = Prim(PrimitiveType.Sphere, "Goz_Sol", statue, new Vector3(-0.2f, 16.72f, -0.43f), Vector3.one * 0.17f, eye, false);
            var eyeR = Prim(PrimitiveType.Sphere, "Goz_Sag", statue, new Vector3(0.2f, 16.72f, -0.43f), Vector3.one * 0.17f, eye, false);
            var eyeLight = AddPointLight(statue, "Goz_Isigi", new Vector3(0, 16.7f, -0.9f), new Color(1f, 0.2f, 0.2f), 3f, 1.5f);
            var tears = new[]
            {
                MakeTears(statue, "Gozyasi_Sol", new Vector3(-0.2f, 16.6f, -0.5f)),
                MakeTears(statue, "Gozyasi_Sag", new Vector3(0.2f, 16.6f, -0.5f)),
            };

            var sword = new GameObject("Kilic").transform;
            sword.SetParent(statue, false);
            Box("Kilic_Agzi", sword, new Vector3(1.15f, 13.25f, -0.5f), new Vector3(0.12f, 1.8f, 0.05f), gold);
            Box("Kilic_Siperi", sword, new Vector3(1.15f, 14.2f, -0.5f), new Vector3(0.7f, 0.12f, 0.12f), gold);
            Box("Kilic_Kabzasi", sword, new Vector3(1.15f, 14.45f, -0.5f), new Vector3(0.1f, 0.4f, 0.1f), dark);
            var rubyPos = new Vector3(1.15f, 14.85f, -0.5f);
            var rubyGo = Prim(PrimitiveType.Sphere, "Kabzadaki_Yakut", sword, rubyPos, Vector3.one * 0.34f, ruby, false);
            var rubyLight = AddPointLight(rubyGo.transform, "Yakut_Isigi", rubyPos + new Vector3(0, 0, -0.4f), new Color(1f, 0.15f, 0.2f), 4f, 2.5f);
            var socket = Prim(PrimitiveType.Sphere, "Bos_Yuva", sword, rubyPos, Vector3.one * 0.2f, dark, false);

            var spot = new GameObject("Heykel_Spotu").AddComponent<Light>();
            spot.transform.SetParent(t, false);
            spot.transform.position = new Vector3(0, 4, -13);
            spot.transform.LookAt(new Vector3(0, 15, 0));
            spot.type = LightType.Spot;
            spot.range = 32f;
            spot.spotAngle = 38f;
            spot.intensity = 30f;
            spot.color = new Color(1f, 0.92f, 0.78f);

            // Meydan süsleri: banklar ve meydanı çevreleyen alçak taş bordür
            var bench = Mat("Ahsap", new Color(0.35f, 0.24f, 0.15f));
            for (int i = 0; i < 6; i++)
            {
                float a = (i * 60f + 30f) * Mathf.Deg2Rad;
                var p = new Vector3(Mathf.Sin(a) * 14f, 0.45f, Mathf.Cos(a) * 14f);
                var b = Box("Bank", t, p, new Vector3(3f, 0.9f, 0.8f), bench);
                b.transform.rotation = Quaternion.LookRotation(-p.normalized);
            }

            var rubyPoint = Marker("Yakut_Noktasi", t, rubyPos, 0f);
            var approach = Marker("Yaklasma_Noktasi", t, new Vector3(1.2f, 14.6f, -2.4f), 0f);
            SphereTrigger("Etkilesim_Yakut", t, rubyPos, 3.2f).AddComponent<InteractionZone>().kind = InteractionKind.Ruby;
            SphereTrigger("Etkilesim_PrenseDonus", t, new Vector3(0, 14.5f, 0), 7f).AddComponent<InteractionZone>().kind = InteractionKind.Prince;
            startPoint = Marker("Baslangic_Noktasi", parent, new Vector3(0, 14f, -16f), 0f);

            var ps = root.AddComponent<PrinceStatue>();
            ps.eyeRenderers = new[] { eyeL.GetComponent<Renderer>(), eyeR.GetComponent<Renderer>() };
            ps.eyeLight = eyeLight;
            ps.swordRuby = rubyGo;
            ps.rubyLight = rubyLight;
            ps.emptySocket = socket;
            ps.goldRenderers = goldParts.ToArray();
            ps.tears = tears;
            ps.rubyPoint = rubyPoint;
            ps.approachPoint = approach;
            return ps;
        }

        private static ParticleSystem MakeTears(Transform parent, string name, Vector3 pos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 1.6f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.1f);
            main.startColor = new Color(0.75f, 0.88f, 1f, 0.9f);
            main.gravityModifier = 0.8f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 40;
            var emission = ps.emission;
            emission.rateOverTime = 4f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.02f;
            var psr = go.GetComponent<ParticleSystemRenderer>();
            psr.sharedMaterial = ParticleMat("Kar_Parcacik", Color.white);
            return ps;
        }

        // =====================================================================================
        // Katedral: beyaz mermer melekler, vitraylar, çan kulesi ve caddeyi aşan çanlı kemerler
        // =====================================================================================

        private static void BuildCathedral(Transform root)
        {
            var stone = Mat("Katedral_Tasi", new Color(0.6f, 0.58f, 0.56f), 0.15f);
            var roof = Mat("Arduvaz_Cati", new Color(0.2f, 0.23f, 0.29f), 0.35f);
            var marble = Mat("Beyaz_Mermer", new Color(0.93f, 0.93f, 0.91f), 0.65f);
            var dark = Mat("Siyah", new Color(0.05f, 0.05f, 0.06f), 0.3f);
            Material[] glass =
            {
                Mat("Vitray_Mavi", new Color(0.1f, 0.15f, 0.4f), 0.8f, 0f, new Color(0.25f, 0.45f, 1.6f)),
                Mat("Vitray_Kirmizi", new Color(0.4f, 0.08f, 0.08f), 0.8f, 0f, new Color(1.5f, 0.25f, 0.2f)),
                Mat("Vitray_Altin", new Color(0.4f, 0.3f, 0.08f), 0.8f, 0f, new Color(1.6f, 1.1f, 0.3f)),
            };
            var bronze = Mat("Tehlike_Bronz_Can", new Color(0.55f, 0.33f, 0.14f), 0.6f, 0.7f, new Color(0.45f, 0.12f, 0.04f));

            var body = Group("Katedral_Binasi", root);
            Box("Nef", body, new Vector3(-16f, 14f, 96f), new Vector3(20f, 28f, 108f), stone).isStatic = true;
            var r1 = Box("Cati_Bati", body, new Vector3(-19.6f, 31f, 96f), new Vector3(12.5f, 0.7f, 108f), roof);
            r1.transform.rotation = Quaternion.Euler(0f, 0f, 32f);
            var r2 = Box("Cati_Dogu", body, new Vector3(-12.4f, 31f, 96f), new Vector3(12.5f, 0.7f, 108f), roof);
            r2.transform.rotation = Quaternion.Euler(0f, 0f, -32f);

            // Çan kulesi ve sivri külah
            var tower = Group("Can_Kulesi", root);
            Box("Kule", tower, new Vector3(-16f, 22f, 64f), new Vector3(12f, 44f, 12f), stone).isStatic = true;
            Box("Kule_Silmesi", tower, new Vector3(-16f, 44.5f, 64f), new Vector3(13f, 1f, 13f), stone);
            for (int s = 0; s < 4; s++)
            {
                var dir = Quaternion.Euler(0f, s * 90f, 0f) * Vector3.forward;
                var c = new Vector3(-16f, 39f, 64f) + dir * 6.05f;
                var size = Mathf.Abs(dir.x) > 0.5f ? new Vector3(0.15f, 5f, 3f) : new Vector3(3f, 5f, 0.15f);
                Box("Kule_Penceresi", tower, c, size, dark, false);
            }
            var k1 = Box("Kulah_1", tower, new Vector3(-16f, 48f, 64f), new Vector3(8f, 6f, 8f), roof);
            k1.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
            Box("Kulah_2", tower, new Vector3(-16f, 54f, 64f), new Vector3(5f, 6f, 5f), roof);
            var k3 = Box("Kulah_3", tower, new Vector3(-16f, 61f, 64f), new Vector3(2.4f, 8f, 2.4f), roof);
            k3.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
            Box("Hac_Dikey", tower, new Vector3(-16f, 66.4f, 64f), new Vector3(0.3f, 2.6f, 0.3f), marble);
            Box("Hac_Yatay", tower, new Vector3(-16f, 66.8f, 64f), new Vector3(1.5f, 0.3f, 0.3f), marble);

            // Caddeye bakan cephe: payandalar, vitraylar, gül pencere, meleklerin durduğu çıkıntılar
            var facade = Group("Cephe", root);
            int gi = 0;
            for (float z = 48f; z <= 144f; z += 12f)
            {
                Box("Payanda", facade, new Vector3(-5.75f, 11f, z), new Vector3(0.5f, 22f, 1.2f), stone);
                if (z + 6f < 144f)
                {
                    Box("Vitray", facade, new Vector3(-5.95f, 9f, z + 6f), new Vector3(0.12f, 8f, 2.4f), glass[gi % 3], false);
                    gi++;
                }
                if (((int)z / 12) % 2 == 0 && z + 6f < 144f)
                {
                    Box("Melek_Cikintisi", facade, new Vector3(-5.6f, 15.2f, z + 6f), new Vector3(1.6f, 0.4f, 2.6f), stone);
                    MakeAngel(facade, new Vector3(-5.4f, 15.4f, z + 6f), 90f, marble);
                }
            }
            var rose = Prim(PrimitiveType.Cylinder, "Gul_Pencere", facade, new Vector3(-5.95f, 21f, 96f), new Vector3(7f, 0.08f, 7f), glass[0], false);
            rose.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            var roseIn = Prim(PrimitiveType.Cylinder, "Gul_Pencere_Ic", facade, new Vector3(-5.9f, 21f, 96f), new Vector3(3.2f, 0.08f, 3.2f), glass[1], false);
            roseIn.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            AddPointLight(facade, "Vitray_Isigi", new Vector3(-3f, 10f, 96f), new Color(0.6f, 0.6f, 1f), 16f, 2f);

            // Caddeyi aşan kemerler ve sallanan çanlar
            var arches = Group("Canli_Kemerler", root);
            for (int i = 0; i < ArchZ.Length; i++) MakeBellArch(arches, ArchZ[i], i / 3f, stone, marble, bronze);
        }

        private static void MakeAngel(Transform parent, Vector3 basePos, float yaw, Material marble)
        {
            var a = new GameObject("Mermer_Melek").transform;
            a.SetParent(parent, false);
            a.SetPositionAndRotation(basePos, Quaternion.Euler(0f, yaw, 0f));
            Vector3 P(float x, float y, float z) => basePos + a.rotation * new Vector3(x, y, z);
            Prim(PrimitiveType.Capsule, "Govde", a, P(0f, 1f, 0f), new Vector3(0.7f, 1f, 0.7f), marble, false);
            Prim(PrimitiveType.Sphere, "Bas", a, P(0f, 2.3f, 0f), Vector3.one * 0.45f, marble, false);
            var wl = Box("Kanat_Sol", a, P(-0.35f, 1.55f, -0.3f), new Vector3(0.12f, 1.7f, 0.9f), marble, false);
            wl.transform.rotation = a.rotation * Quaternion.Euler(0f, 25f, 20f);
            var wr = Box("Kanat_Sag", a, P(0.35f, 1.55f, -0.3f), new Vector3(0.12f, 1.7f, 0.9f), marble, false);
            wr.transform.rotation = a.rotation * Quaternion.Euler(0f, -25f, -20f);
        }

        private static void MakeBellArch(Transform parent, float z, float phase, Material stone, Material marble, Material bronze)
        {
            var arch = new GameObject($"Kemer_z{z:0}").transform;
            arch.SetParent(parent, false);
            Box("Ayak_Bati", arch, new Vector3(-5.4f, 7.5f, z), new Vector3(1.2f, 15f, 2.2f), stone).isStatic = true;
            Box("Ayak_Dogu", arch, new Vector3(5.4f, 7.5f, z), new Vector3(1.2f, 15f, 2.2f), stone).isStatic = true;
            Box("Lento", arch, new Vector3(0f, 16.5f, z), new Vector3(12f, 3f, 2.2f), stone).isStatic = true;
            Box("Silme", arch, new Vector3(0f, 18.3f, z), new Vector3(12.6f, 0.6f, 2.8f), stone).isStatic = true;
            MakeAngel(arch, new Vector3(-3f, 18.6f, z), 180f, marble);
            MakeAngel(arch, new Vector3(3f, 18.6f, z), 180f, marble);

            var bell = new GameObject("Sallanan_Can");
            bell.transform.SetParent(arch, false);
            bell.transform.position = new Vector3(0f, 15f, z);
            Kinematic(bell);
            var p = bell.AddComponent<Pendulum>();
            p.localAxis = Vector3.forward;
            p.amplitude = 40f;
            p.period = 2.8f;
            p.phase = phase;
            var hz = bell.AddComponent<Hazard>();
            hz.label = "Katedral çanı";
            var bt = bell.transform;
            Box("Boyunduruk", bt, new Vector3(0f, 14f, z), new Vector3(0.3f, 2f, 0.3f), bronze);
            Prim(PrimitiveType.Cylinder, "Can_Govdesi", bt, new Vector3(0f, 11.8f, z), new Vector3(2.2f, 0.9f, 2.2f), bronze, true);
            Prim(PrimitiveType.Cylinder, "Can_Agzi", bt, new Vector3(0f, 10.9f, z), new Vector3(2.7f, 0.12f, 2.7f), bronze, true);
            Prim(PrimitiveType.Sphere, "Tokmak", bt, new Vector3(0f, 10.5f, z), Vector3.one * 0.5f, bronze, false);
        }

        // =====================================================================================
        // Saray: balkondaki genç kız, dans eden gölgeler, fener dizileri
        // =====================================================================================

        private static void BuildPalace(Transform root)
        {
            var cream = Mat("Saray_Tasi", new Color(0.8f, 0.74f, 0.64f), 0.25f);
            var roof = Mat("Saray_Kubbe", new Color(0.32f, 0.4f, 0.46f), 0.5f, 0.3f);
            var goldTrim = Mat("Saray_Yaldiz", new Color(0.8f, 0.6f, 0.25f), 0.7f, 0.8f, new Color(0.35f, 0.22f, 0.05f));
            var window = Mat("Saray_Penceresi", new Color(0.5f, 0.35f, 0.12f), 0.5f, 0f, new Color(3.2f, 2.1f, 0.9f));
            var shadow = Mat("Dans_Golgesi", new Color(0.04f, 0.03f, 0.03f), 0.1f);
            var banner = Mat("Saray_Flamasi", new Color(0.55f, 0.08f, 0.12f), 0.3f);
            var dress = Mat("Balo_Elbisesi", new Color(0.95f, 0.82f, 0.86f), 0.4f);
            var suit = Mat("Delikanli", new Color(0.12f, 0.16f, 0.32f), 0.3f);

            var body = Group("Saray_Binasi", root);
            Box("Ana_Bina", body, new Vector3(64f, 13.5f, 190f), new Vector3(84f, 27f, 32f), cream).isStatic = true;
            Box("Orta_Kosk", body, new Vector3(64f, 16f, 182f), new Vector3(20f, 32f, 14f), cream).isStatic = true;
            Box("Korniz", body, new Vector3(64f, 27.3f, 174.6f), new Vector3(84.4f, 0.8f, 1.4f), goldTrim);
            Prim(PrimitiveType.Sphere, "Kubbe", body, new Vector3(64f, 32f, 186f), new Vector3(15f, 11f, 15f), roof, false);
            Prim(PrimitiveType.Cylinder, "Kubbe_Feneri", body, new Vector3(64f, 38.5f, 186f), new Vector3(2f, 1.5f, 2f), goldTrim, false);

            // Sütunlar ve iki sıra ışıklı pencere
            var facade = Group("Saray_Cephesi", root);
            for (float x = 27f; x <= 101f; x += 6f)
            {
                if (Mathf.Abs(x - 64f) < 6f) continue;
                Prim(PrimitiveType.Cylinder, "Sutun", facade, new Vector3(x, 6f, 173.6f), new Vector3(0.9f, 6f, 0.9f), cream, true);
                Box("Pencere_Alt", facade, new Vector3(x + 3f, 5f, 173.9f), new Vector3(2f, 3.6f, 0.15f), window, false);
                Box("Pencere_Ust", facade, new Vector3(x + 3f, 14f, 173.9f), new Vector3(2f, 3.6f, 0.15f), window, false);
                Box("Pencere_Cati", facade, new Vector3(x + 3f, 21.5f, 173.9f), new Vector3(1.6f, 2.4f, 0.15f), window, false);
            }
            AddPointLight(facade, "Saray_Isigi_1", new Vector3(40f, 6f, 171f), new Color(1f, 0.78f, 0.45f), 16f, 3f);
            AddPointLight(facade, "Saray_Isigi_2", new Vector3(88f, 6f, 171f), new Color(1f, 0.78f, 0.45f), 16f, 3f);
            Box("Flama_Bati", facade, new Vector3(40f, 21f, 173.8f), new Vector3(2.2f, 6f, 0.1f), banner, false);
            Box("Flama_Dogu", facade, new Vector3(88f, 21f, 173.8f), new Vector3(2.2f, 6f, 0.1f), banner, false);

            // Pencerelerde dans eden çiftlerin gölgeleri
            var dancers = Group("Dans_Eden_Golgeler", root);
            float[] dx = { 36f, 54f, 78f, 96f };
            for (int i = 0; i < dx.Length; i++)
            {
                var d = Box("Dans_Eden_Cift", dancers, new Vector3(dx[i], 4.6f, 173.8f), new Vector3(0.9f, 1.8f, 0.05f), shadow, false);
                var o = d.AddComponent<Oscillator>();
                o.offset = new Vector3(0.45f, 0f, 0f);
                o.period = 1.6f + i * 0.2f;
                o.phase = i * 0.25f;
            }

            // Balkon: genç kız ve sevdiği delikanlı
            var balcony = Group("Balkon", root);
            Box("Balkon_Zemini", balcony, new Vector3(64f, 8f, 172.6f), new Vector3(8f, 0.4f, 2.8f), cream);
            Box("Korkuluk_On", balcony, new Vector3(64f, 8.75f, 171.3f), new Vector3(8f, 1.1f, 0.15f), goldTrim);
            Box("Korkuluk_Bati", balcony, new Vector3(60.05f, 8.75f, 172.6f), new Vector3(0.15f, 1.1f, 2.8f), goldTrim);
            Box("Korkuluk_Dogu", balcony, new Vector3(67.95f, 8.75f, 172.6f), new Vector3(0.15f, 1.1f, 2.8f), goldTrim);
            Box("Balkon_Kapisi", balcony, new Vector3(64f, 10.2f, 173.9f), new Vector3(3f, 4f, 0.15f), window, false);
            Prim(PrimitiveType.Capsule, "Genc_Kiz", balcony, new Vector3(63.2f, 9.1f, 172.8f), new Vector3(0.75f, 0.9f, 0.75f), dress, false);
            Prim(PrimitiveType.Sphere, "Genc_Kiz_Bas", balcony, new Vector3(63.2f, 10.3f, 172.8f), Vector3.one * 0.4f, dress, false);
            Prim(PrimitiveType.Capsule, "Delikanli", balcony, new Vector3(64.8f, 9.2f, 172.9f), new Vector3(0.6f, 1f, 0.6f), suit, false);
            Prim(PrimitiveType.Sphere, "Delikanli_Bas", balcony, new Vector3(64.8f, 10.5f, 172.9f), Vector3.one * 0.4f, suit, false);
            AddPointLight(balcony, "Balkon_Isigi", new Vector3(64f, 11f, 171f), new Color(1f, 0.8f, 0.55f), 9f, 2.5f);

            // Bulvar boyunca fener dizileri (kırmızımsı = tehlike)
            var lanterns = Group("Saray_Fener_Dizileri", root);
            Garland(lanterns, 34f, 9f);
            Garland(lanterns, 52f, 5f);
            Garland(lanterns, 52f, 14f);
            LanternStrings(lanterns, 74f, 0f);
            LanternStrings(lanterns, 92f, 0.5f);
        }

        private static Material[] FestiveLanternMats() => new[]
        {
            Mat("Fener_Kirmizi", new Color(0.7f, 0.1f, 0.08f), 0.4f, 0f, new Color(2.2f, 0.35f, 0.2f)),
            Mat("Fener_Altin", new Color(0.7f, 0.5f, 0.1f), 0.4f, 0f, new Color(2.4f, 1.5f, 0.35f)),
            Mat("Fener_Turuncu", new Color(0.8f, 0.35f, 0.08f), 0.4f, 0f, new Color(2.4f, 0.9f, 0.2f)),
        };

        /// <summary>Bulvarı enlemesine geçen, hafif sarkık fener dizisi (sabit engel).</summary>
        private static void Garland(Transform parent, float x, float y)
        {
            var rope = Mat("Tehlike_Ip", new Color(0.55f, 0.12f, 0.08f), 0.3f, 0f, new Color(0.35f, 0.04f, 0.02f));
            var mats = FestiveLanternMats();
            var root = new GameObject($"Fener_Dizisi_x{x:0}_y{y:0}");
            root.transform.SetParent(parent, false);
            root.AddComponent<Hazard>().label = "Fener dizisi";
            var a = new Vector3(x, y + 0.6f, 162.1f);
            var m = new Vector3(x, y - 0.6f, 168f);
            var b = new Vector3(x, y + 0.6f, 173.9f);
            Beam("Ip_1", root.transform, a, m, 0.14f, rope);
            Beam("Ip_2", root.transform, m, b, 0.14f, rope);
            int n = 0;
            for (float z = 163.5f; z < 173f; z += 1.6f)
            {
                float t = Mathf.Abs(z - 168f) / 5.9f;
                float yy = Mathf.Lerp(y - 0.6f, y + 0.6f, t) - 0.55f;
                Prim(PrimitiveType.Sphere, "Fener", root.transform, new Vector3(x, yy, z), Vector3.one * 0.5f, mats[n++ % mats.Length], true);
            }
        }

        /// <summary>Yüksek bir telden sarkan, ileri geri sallanan dikey fener sıraları.</summary>
        private static void LanternStrings(Transform parent, float x, float phase)
        {
            var wire = Mat("Demir", new Color(0.12f, 0.12f, 0.14f), 0.4f, 0.6f);
            var mats = FestiveLanternMats();
            Box($"Tel_x{x:0}", parent, new Vector3(x, 20.2f, 168f), new Vector3(0.15f, 0.15f, 12.2f), wire).isStatic = true;
            float[] zs = { 163.5f, 168f, 172.5f };
            for (int i = 0; i < zs.Length; i++)
            {
                var root = new GameObject("Sallanan_Fener_Sirasi");
                root.transform.SetParent(parent, false);
                root.transform.position = new Vector3(x, 20f, zs[i]);
                Kinematic(root);
                var p = root.AddComponent<Pendulum>();
                p.localAxis = Vector3.forward;
                p.amplitude = 28f;
                p.period = 3f;
                p.phase = phase + i * 0.33f;
                root.AddComponent<Hazard>().label = "Asılı fenerler";
                Box("Ip", root.transform, new Vector3(x, 14.5f, zs[i]), new Vector3(0.08f, 11f, 0.08f), wire);
                for (int k = 0; k < 4; k++)
                    Box("Fener", root.transform, new Vector3(x, 17f - k * 2.5f, zs[i]), new Vector3(0.55f, 0.75f, 0.55f), mats[(i + k) % mats.Length]);
            }
        }

        // =====================================================================================
        // Nehir, rıhtım ve direklerine fener asılı gemiler
        // =====================================================================================

        private static void BuildRiver(Transform root)
        {
            var water = Mat("Nehir_Suyu", new Color(0.05f, 0.1f, 0.17f), 0.92f);
            var stone = Mat("Tas", new Color(0.5f, 0.5f, 0.52f), 0.1f);
            var iron = Mat("Demir", new Color(0.12f, 0.12f, 0.14f), 0.4f, 0.6f);

            Box("Nehir_Suyu", root, new Vector3(RiverCenter.x, 0.03f, RiverCenter.y), new Vector3(RiverHalf.x * 2f, 0.06f, RiverHalf.y * 2f), water, false);
            Box("Rihtim_Kenari", root, new Vector3(126.3f, 0.4f, 230f), new Vector3(0.6f, 0.8f, 160f), stone).isStatic = true;

            var bridge = Group("Kopru", root);
            Box("Kopru_Zemini", bridge, new Vector3(152f, 1.4f, 288f), new Vector3(56f, 0.4f, 8f), stone).isStatic = true;
            Box("Korkuluk_G", bridge, new Vector3(152f, 2.1f, 284.2f), new Vector3(56f, 1f, 0.25f), stone).isStatic = true;
            Box("Korkuluk_K", bridge, new Vector3(152f, 2.1f, 291.8f), new Vector3(56f, 1f, 0.25f), stone).isStatic = true;
            for (float x = 136f; x <= 168f; x += 16f) Box("Ayak", bridge, new Vector3(x, 0.6f, 288f), new Vector3(2f, 1.2f, 7f), stone).isStatic = true;
            AddPointLight(bridge, "Kopru_Feneri", new Vector3(152f, 5f, 285f), new Color(1f, 0.75f, 0.45f), 14f, 2.5f);

            var ships = Group("Gemiler", root);
            MakeShip(ships, new Vector3(156f, 0f, 196f), 0f);
            MakeShip(ships, new Vector3(160f, 0f, 232f), 0.4f);
            MakeShip(ships, new Vector3(154f, 0f, 266f), 0.7f);

            // Rıhtımın ortasında hamalların ısındığı mangal: uzun ve soğuk nehir geçişinde bir mola
            var coal = Mat("Mangal_Atesi", new Color(0.5f, 0.15f, 0.05f), 0.2f, 0f, new Color(3f, 0.9f, 0.2f));
            var copper = Mat("Bakir", new Color(0.72f, 0.42f, 0.25f), 0.7f, 0.85f);
            var brazier = Group("Rihtim_Mangali", root);
            Prim(PrimitiveType.Cylinder, "Mangal", brazier, new Vector3(116.2f, 0.6f, 244f), new Vector3(1.2f, 0.6f, 1.2f), copper, true);
            Prim(PrimitiveType.Sphere, "Kor", brazier, new Vector3(116.2f, 1.25f, 244f), new Vector3(1f, 0.45f, 1f), coal, false);
            AddPointLight(brazier, "Mangal_Isigi", new Vector3(117f, 3f, 244f), new Color(1f, 0.55f, 0.2f), 12f, 3.5f);
            SphereTrigger("Mangal_Sicakligi", brazier, new Vector3(118f, 5.5f, 244f), 5.5f).AddComponent<WarmthArea>().ratePerSecond = 22f;

            // Rıhtım duvarındaki halat babaları
            var bollards = Group("Halat_Babalari", root);
            float[] zs = { 190f, 202f, 226f, 238f, 260f, 272f };
            for (int i = 0; i < zs.Length; i++)
                Prim(PrimitiveType.Cylinder, "Baba", bollards, new Vector3(114.6f, i % 2 == 0 ? 1.5f : 8f, zs[i]), new Vector3(0.5f, 0.3f, 0.5f), iron, false)
                    .transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        }

        /// <summary>
        /// Demirli gemi: gövde, iki direk, seren ve direklere asılı fenerler. Direk tepelerinden rıhtıma
        /// uzanan halatlar tehlikelidir; biri alçak (rıhtımda ~4 m), diğeri yüksek (~9.6 m) bağlanır.
        /// Gemi hafifçe yalpalar, halatlar da onunla birlikte iner kalkar.
        /// </summary>
        private static void MakeShip(Transform parent, Vector3 c, float phase)
        {
            var hull = Mat("Gemi_Govdesi", new Color(0.22f, 0.15f, 0.1f), 0.3f);
            var deck = Mat("Ahsap", new Color(0.35f, 0.24f, 0.15f));
            var sail = Mat("Yelken", new Color(0.78f, 0.74f, 0.66f), 0.1f);
            var rope = Mat("Tehlike_Ip", new Color(0.55f, 0.12f, 0.08f), 0.3f, 0f, new Color(0.35f, 0.04f, 0.02f));
            var lantern = Mat("Gemi_Feneri", new Color(0.6f, 0.45f, 0.15f), 0.3f, 0f, new Color(3f, 2f, 0.7f));

            var root = new GameObject($"Gemi_z{c.z:0}");
            root.transform.SetParent(parent, false);
            root.transform.position = c;
            Kinematic(root);
            var p = root.AddComponent<Pendulum>();
            p.localAxis = Vector3.forward;
            p.amplitude = 2.5f;
            p.period = 5f;
            p.phase = phase;
            var t = root.transform;

            Box("Govde", t, c + new Vector3(0f, 1.5f, 0f), new Vector3(7f, 3f, 22f), hull);
            var bow = Box("Pruva", t, c + new Vector3(0f, 1.5f, 11f), new Vector3(4.9f, 3f, 4.9f), hull);
            bow.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
            Box("Guverte", t, c + new Vector3(0f, 3.05f, 0f), new Vector3(6.6f, 0.1f, 21f), deck, false);
            Box("Kamara", t, c + new Vector3(0f, 4f, -7f), new Vector3(4f, 2f, 5f), deck);

            for (int m = 0; m < 2; m++)
            {
                float mz = m == 0 ? -6f : 6f;
                var mastBase = c + new Vector3(0f, 3f, mz);
                Prim(PrimitiveType.Cylinder, "Direk", t, mastBase + new Vector3(0f, 9f, 0f), new Vector3(0.5f, 9f, 0.5f), deck, true);
                Box("Seren_Alt", t, mastBase + new Vector3(0f, 7f, 0f), new Vector3(9f, 0.3f, 0.3f), deck);
                Box("Seren_Ust", t, mastBase + new Vector3(0f, 13f, 0f), new Vector3(7f, 0.3f, 0.3f), deck);
                Box("Sarili_Yelken", t, mastBase + new Vector3(0f, 7.45f, 0f), new Vector3(8.4f, 0.6f, 0.5f), sail, false);
                foreach (float sx in new[] { -4.3f, 4.3f })
                    Prim(PrimitiveType.Sphere, "Direk_Feneri", t, mastBase + new Vector3(sx, 6.3f, 0f), Vector3.one * 0.5f, lantern, false);

                // Rıhtıma uzanan halat: kıç direği alçağa, baş direği yükseğe bağlanır.
                var top = mastBase + new Vector3(0f, 17.5f, 0f);
                var anchor = new Vector3(114.6f, m == 0 ? 1.5f : 8f, c.z + mz);
                var r = Beam(m == 0 ? "Halat_Alcak" : "Halat_Yuksek", t, top, anchor, 0.18f, rope);
                r.AddComponent<Hazard>().label = "Gemi halatı";
            }
            AddPointLight(t, "Gemi_Isigi", c + new Vector3(-4.3f, 9f, 0f), new Color(1f, 0.75f, 0.4f), 14f, 3f);
        }

        // =====================================================================================
        // Eski çarşı: tenteler, tezgâhlar, bakır teraziler
        // =====================================================================================

        private static void BuildMarket(Transform root)
        {
            Material[] awnings =
            {
                Mat("Tente_Kirmizi", new Color(0.55f, 0.15f, 0.12f), 0.2f),
                Mat("Tente_Krem", new Color(0.75f, 0.68f, 0.52f), 0.2f),
                Mat("Tente_Yesil", new Color(0.2f, 0.35f, 0.25f), 0.2f),
            };
            var wood = Mat("Ahsap", new Color(0.35f, 0.24f, 0.15f));
            var copperDecor = Mat("Bakir", new Color(0.72f, 0.42f, 0.25f), 0.7f, 0.85f);
            var coal = Mat("Mangal_Atesi", new Color(0.5f, 0.15f, 0.05f), 0.2f, 0f, new Color(3f, 0.9f, 0.2f));

            var stalls = Group("Tenteler_ve_Tezgahlar", root);
            int n = 0;
            for (float x = 198f; x <= 248f; x += 8f)
            {
                bool south = n % 2 == 0;
                float wallZ = south ? 282f : 294f, sign = south ? 1f : -1f;
                var aw = Box("Tente", stalls, new Vector3(x, 4.6f, wallZ + sign * 1.6f), new Vector3(6f, 0.2f, 3.2f), awnings[n % awnings.Length]);
                aw.transform.rotation = Quaternion.Euler(sign * -14f, 0f, 0f);
                Box("Tezgah", stalls, new Vector3(x, 0.5f, wallZ + sign * 1.4f), new Vector3(4f, 1f, 1.8f), wood);
                Prim(PrimitiveType.Cylinder, "Bakir_Tava", stalls, new Vector3(x - 1f, 1.08f, wallZ + sign * 1.4f), new Vector3(0.8f, 0.05f, 0.8f), copperDecor, false);
                Prim(PrimitiveType.Cylinder, "Kucuk_Terazi", stalls, new Vector3(x + 1f, 1.3f, wallZ + sign * 1.4f), new Vector3(0.1f, 0.3f, 0.1f), copperDecor, false);
                n++;
            }

            var braziers = Group("Mangallar", root);
            foreach (float x in new[] { 214f, 238f })
            {
                Prim(PrimitiveType.Cylinder, "Mangal", braziers, new Vector3(x, 0.6f, 285.2f), new Vector3(1f, 0.6f, 1f), copperDecor, true);
                Prim(PrimitiveType.Sphere, "Kor", braziers, new Vector3(x, 1.2f, 285.2f), new Vector3(0.9f, 0.4f, 0.9f), coal, false);
                AddPointLight(braziers, "Mangal_Isigi", new Vector3(x, 2.5f, 285.5f), new Color(1f, 0.55f, 0.2f), 10f, 3f);
                SphereTrigger("Mangal_Sicakligi", braziers, new Vector3(x, 4f, 286f), 4.5f).AddComponent<WarmthArea>().ratePerSecond = 18f;
            }

            var scales = Group("Bakir_Teraziler", root);
            CopperScale(scales, 206f, 0f);
            CopperScale(scales, 232f, 0.5f);
        }

        /// <summary>
        /// Çarşının üstüne asılmış dev bakır terazi: kiriş sokak boyunca sabit, terazi kolu ve kefeler
        /// sallanır. En güvenli yol kirişin üstünden geçmektir.
        /// </summary>
        private static void CopperScale(Transform parent, float x, float phase)
        {
            var iron = Mat("Demir", new Color(0.12f, 0.12f, 0.14f), 0.4f, 0.6f);
            var copper = Mat("Tehlike_Bakir", new Color(0.72f, 0.38f, 0.2f), 0.7f, 0.8f, new Color(0.45f, 0.12f, 0.04f));
            var coin = Mat("Altin_Sikke", new Color(0.85f, 0.65f, 0.2f), 0.8f, 0.9f, new Color(0.4f, 0.28f, 0.05f));

            Box($"Terazi_Kirisi_x{x:0}", parent, new Vector3(x, 15.4f, 288f), new Vector3(0.4f, 0.4f, 12.2f), iron).isStatic = true;
            var root = new GameObject("Sallanan_Bakir_Terazi");
            root.transform.SetParent(parent, false);
            root.transform.position = new Vector3(x, 15f, 288f);
            Kinematic(root);
            var p = root.AddComponent<Pendulum>();
            p.localAxis = Vector3.right;
            p.amplitude = 20f;
            p.period = 3.6f;
            p.phase = phase;
            root.AddComponent<Hazard>().label = "Bakır terazi";
            var t = root.transform;
            Box("Terazi_Kolu", t, new Vector3(x, 14.4f, 288f), new Vector3(0.35f, 0.35f, 10f), copper);
            Box("Ibre", t, new Vector3(x, 13f, 288f), new Vector3(0.15f, 2.8f, 0.15f), copper);
            foreach (float dz in new[] { -4.8f, 4.8f })
            {
                Box("Zincir", t, new Vector3(x, 12.9f, 288f + dz), new Vector3(0.06f, 3f, 0.06f), iron);
                Prim(PrimitiveType.Cylinder, "Kefe", t, new Vector3(x, 11.3f, 288f + dz), new Vector3(2.8f, 0.15f, 2.8f), copper, true);
                Prim(PrimitiveType.Cylinder, "Sikkeler", t, new Vector3(x, 11.5f, 288f + dz), new Vector3(1.2f, 0.12f, 1.2f), coin, false);
            }
        }

        // =====================================================================================
        // Yoksul mahalle: çamaşır ipleri
        // =====================================================================================

        private static void BuildPoorQuarter(Transform root)
        {
            var rope = Mat("Tehlike_Ip", new Color(0.55f, 0.12f, 0.08f), 0.3f, 0f, new Color(0.35f, 0.04f, 0.02f));
            var cloth = Mat("Yamali_Camasir", new Color(0.62f, 0.3f, 0.2f), 0.2f, 0f, new Color(0.25f, 0.06f, 0.03f));
            Laundry(root, 270f, 6f, rope, cloth);
            Laundry(root, 284f, 9.5f, rope, cloth);
        }

        private static void Laundry(Transform parent, float x, float y, Material rope, Material cloth)
        {
            var root = new GameObject($"Camasir_Ipi_x{x:0}_y{y:0.#}");
            root.transform.SetParent(parent, false);
            root.AddComponent<Hazard>().label = "Çamaşır ipi";
            Box("Ip", root.transform, new Vector3(x, y, 288f), new Vector3(0.15f, 0.15f, 12.2f), rope);
            foreach (var dz in new[] { -4.5f, -1.5f, 1.5f, 4.5f })
                Box("Camasir", root.transform, new Vector3(x, y - 1f, 288f + dz), new Vector3(0.1f, 1.8f, 1.6f), cloth);
        }

        // =====================================================================================
        // Terzi kadının evi
        // =====================================================================================

        private static SeamstressHouse BuildHouse(Transform root)
        {
            var wall = Mat("TerziEvi_Duvar", new Color(0.5f, 0.42f, 0.34f));
            var wood = Mat("Ahsap", new Color(0.35f, 0.24f, 0.15f));
            var floor = Mat("TerziEvi_Taban", new Color(0.3f, 0.22f, 0.15f));
            var woman = Mat("Terzi_Kadin", new Color(0.45f, 0.35f, 0.48f));
            var child = Mat("Hasta_Cocuk", new Color(0.55f, 0.65f, 0.8f));
            var blanket = Mat("Battaniye", new Color(0.55f, 0.3f, 0.25f));
            var candle = Mat("Mum", new Color(0.9f, 0.85f, 0.7f), 0.2f, 0f, new Color(4f, 2.5f, 0.8f));
            var ruby = Mat("Yakut", new Color(0.7f, 0.02f, 0.05f), 0.9f, 0f, new Color(3f, 0.1f, 0.15f));
            var metal = Mat("Demir", new Color(0.12f, 0.12f, 0.14f), 0.4f, 0.6f);
            var satin = Mat("Saten_Elbise", new Color(0.92f, 0.85f, 0.9f), 0.6f);
            var passion = Mat("Carkifelek_Nakisi", new Color(0.5f, 0.2f, 0.6f), 0.3f);

            var t = root;
            var shell = Group("Duvarlar", t);
            Box("Cephe_Sol", shell, new Vector3(303.2f, 3.5f, 283f), new Vector3(0.4f, 7f, 4.4f), wall);
            Box("Cephe_Sag", shell, new Vector3(303.2f, 3.5f, 293f), new Vector3(0.4f, 7f, 4.4f), wall);
            Box("Cephe_Alt", shell, new Vector3(303.2f, 0.8f, 288f), new Vector3(0.4f, 1.6f, 6f), wall);
            Box("Cephe_Ust", shell, new Vector3(303.2f, 5.8f, 288f), new Vector3(0.4f, 2.4f, 6f), wall);
            Box("Pencere_Esigi", shell, new Vector3(302.9f, 1.65f, 288f), new Vector3(0.7f, 0.15f, 6.4f), wood);
            Box("Pencere_Kanadi_Sol", shell, new Vector3(302.4f, 3.1f, 284.6f), new Vector3(0.1f, 3f, 1.6f), wood, false)
                .transform.rotation = Quaternion.Euler(0f, -35f, 0f);
            Box("Pencere_Kanadi_Sag", shell, new Vector3(302.4f, 3.1f, 291.4f), new Vector3(0.1f, 3f, 1.6f), wood, false)
                .transform.rotation = Quaternion.Euler(0f, 35f, 0f);
            Box("Arka_Duvar", shell, new Vector3(315f, 3.5f, 288f), new Vector3(0.4f, 7f, 14.4f), wall);
            Box("Yan_Duvar_G", shell, new Vector3(309f, 3.5f, 281f), new Vector3(12f, 7f, 0.4f), wall);
            Box("Yan_Duvar_K", shell, new Vector3(309f, 3.5f, 295f), new Vector3(12f, 7f, 0.4f), wall);
            Box("Cati", shell, new Vector3(309f, 7.2f, 288f), new Vector3(12.8f, 0.4f, 14.8f), wood);
            Box("Cati_Sirti", shell, new Vector3(309f, 8.2f, 288f), new Vector3(8f, 1.6f, 14.8f), wood);
            Box("Taban", shell, new Vector3(309f, 0.05f, 288f), new Vector3(12f, 0.1f, 14f), floor);

            var interior = Group("Ic_Mekan", t);
            Box("Masa", interior, new Vector3(306.5f, 0.95f, 286f), new Vector3(1.6f, 0.1f, 1.2f), wood);
            Box("Masa_Ayagi", interior, new Vector3(306.5f, 0.45f, 286f), new Vector3(1.1f, 0.9f, 0.7f), wood);
            Box("Mum_Govdesi", interior, new Vector3(306.95f, 1.2f, 285.6f), new Vector3(0.12f, 0.4f, 0.12f), candle, false);
            var candleLight = AddPointLight(interior, "Mum_Isigi", new Vector3(306.95f, 1.8f, 285.6f), new Color(1f, 0.7f, 0.35f), 8f, 3f);
            Prim(PrimitiveType.Cylinder, "Yuksuk", interior, new Vector3(306.1f, 1.06f, 286.35f), new Vector3(0.12f, 0.06f, 0.12f), metal, false);
            var tableRuby = Prim(PrimitiveType.Sphere, "Masadaki_Yakut", interior, new Vector3(306.4f, 1.17f, 286.35f), Vector3.one * 0.26f, ruby, false);
            AddPointLight(tableRuby.transform, "Yakut_Isigi", new Vector3(306.4f, 1.6f, 286.35f), new Color(1f, 0.2f, 0.25f), 4f, 2f);
            // Masada işlenmekte olan saten balo elbisesi ve çarkıfelek nakışları
            Box("Saten_Elbise", interior, new Vector3(306.7f, 1.03f, 285.8f), new Vector3(1.2f, 0.05f, 0.7f), satin, false);
            Box("Carkifelek_1", interior, new Vector3(306.9f, 1.07f, 285.7f), new Vector3(0.15f, 0.02f, 0.15f), passion, false);
            Box("Carkifelek_2", interior, new Vector3(306.5f, 1.07f, 285.95f), new Vector3(0.15f, 0.02f, 0.15f), passion, false);

            // Yorgunluktan masanın başında uyuyakalmış anne
            var sleep = Quaternion.Euler(22f, -41f, 0f);
            var womanPos = new Vector3(307.9f, 1.0f, 284.4f);
            var womanGo = Prim(PrimitiveType.Capsule, "Terzi_Kadin_Uyuyor", interior, womanPos, new Vector3(0.7f, 1f, 0.7f), woman, false);
            womanGo.transform.rotation = sleep;
            Prim(PrimitiveType.Sphere, "Terzi_Kadin_Bas", interior, womanPos + sleep * new Vector3(0f, 1.2f, 0.1f), Vector3.one * 0.45f, woman, false);
            Box("Sandalye", interior, new Vector3(308.4f, 0.45f, 284.2f), new Vector3(0.7f, 0.9f, 0.7f), wood);

            Box("Yatak", interior, new Vector3(311.6f, 0.4f, 291.6f), new Vector3(3.2f, 0.8f, 1.8f), wood);
            Box("Battaniye", interior, new Vector3(311.9f, 0.86f, 291.6f), new Vector3(2.4f, 0.14f, 1.6f), blanket, false);
            var childGo = Prim(PrimitiveType.Capsule, "Hasta_Cocuk", interior, new Vector3(311.4f, 1.1f, 291.6f), new Vector3(0.45f, 0.8f, 0.45f), child, false);
            childGo.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            var roomGlow = AddPointLight(interior, "Oda_Sicakligi", new Vector3(309f, 3.5f, 288f), new Color(1f, 0.65f, 0.35f), 11f, 0f);
            AddPointLight(t, "Pencere_Isigi", new Vector3(302f, 3.2f, 288f), new Color(1f, 0.72f, 0.4f), 9f, 3.5f);

            var approach = Marker("Teslim_Noktasi", t, new Vector3(300.5f, 3.2f, 288f), 90f);
            var returnSpawn = Marker("Donus_Baslangici", t, new Vector3(295f, 5f, 288f), 270f);
            BoxTrigger("Etkilesim_Pencere", t, new Vector3(300.8f, 3.2f, 288f), new Vector3(5f, 3.8f, 7f)).AddComponent<InteractionZone>().kind = InteractionKind.Delivery;
            BoxTrigger("Isinma_Pencere_Onu", t, new Vector3(300f, 3.5f, 288f), new Vector3(6f, 6f, 8f)).AddComponent<WarmthArea>().ratePerSecond = 15f;

            var house = t.gameObject.AddComponent<SeamstressHouse>();
            house.approachPoint = approach;
            house.returnSpawnPoint = returnSpawn;
            house.rubyOnTable = tableRuby;
            house.childRenderer = childGo.GetComponent<Renderer>();
            house.candleLight = candleLight;
            house.roomGlow = roomGlow;
            return house;
        }
    }
}
