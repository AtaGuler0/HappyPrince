using System;
using System.IO;
using MutluPrens.Core;
using MutluPrens.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace MutluPrens.EditorTools
{
    /// <summary>Arayüz: kodla üretilen sprite'lar, oyun içi HUD, ana menü, duraklatma ve bölüm sonu ekranları.</summary>
    public static partial class MutluPrensLevelBuilder
    {
        private class UISprites
        {
            public Sprite Rounded, Circle, Arrow, GradientH, GradientV, Corner, Vignette, Ornament, Diamond;
        }

        // Renk paleti
        private static readonly Color Gold = new(1f, 0.83f, 0.52f);
        private static readonly Color Ice = new(0.66f, 0.85f, 1f);
        private static readonly Color Ink = new(0.94f, 0.95f, 0.98f);
        private static readonly Color Muted = new(0.62f, 0.67f, 0.78f);
        private static readonly Color Danger = new(1f, 0.55f, 0.5f);
        private static readonly Color Night = new(0.025f, 0.035f, 0.075f);

        private static Font _fRegular, _fSemi, _fItalic;

        // =====================================================================================
        // Sprite üretimi (Assets/_MutluPrens/UI/*.png)
        // =====================================================================================

        private static UISprites BuildUISprites()
        {
            var s = new UISprites
            {
                Rounded = SaveSprite("UI_YuvarlakKose", 64, 64, (x, y) =>
                {
                    const float r = 14f;
                    float dx = Mathf.Max(Mathf.Abs(x - 32f) - (32f - r), 0f);
                    float dy = Mathf.Max(Mathf.Abs(y - 32f) - (32f - r), 0f);
                    return Mathf.Clamp01(r - Mathf.Sqrt(dx * dx + dy * dy) + 0.5f);
                }, new Vector4(18, 18, 18, 18)),
                Circle = SaveSprite("UI_Daire", 64, 64, (x, y) =>
                    Mathf.Clamp01(31f - Vector2.Distance(new Vector2(x, y), new Vector2(32f, 32f)) + 0.5f), Vector4.zero),
                Arrow = SaveSprite("UI_Ok", 64, 64, (x, y) =>
                {
                    var p = new Vector2(x / 64f, y / 64f);
                    float d = Mathf.Min(SegDist(p, new Vector2(0.18f, 0.3f), new Vector2(0.5f, 0.74f)),
                                        SegDist(p, new Vector2(0.5f, 0.74f), new Vector2(0.82f, 0.3f)));
                    return Mathf.Clamp01((0.08f - d) * 64f + 0.5f);
                }, Vector4.zero),
                GradientH = SaveSprite("UI_Gradyan_Yatay", 256, 8, (x, y) => Mathf.Pow(1f - x / 256f, 1.6f), Vector4.zero),
                GradientV = SaveSprite("UI_Gradyan_Dikey", 8, 256, (x, y) => Mathf.Pow(1f - y / 256f, 1.5f), Vector4.zero),
                Corner = SaveSprite("UI_Kose_Golgesi", 256, 256, (x, y) =>
                    Mathf.Pow(1f - x / 256f, 1.5f) * Mathf.Pow(y / 256f, 1.5f), Vector4.zero),
                Vignette = SaveSprite("UI_Buz_Vinyeti", 256, 256, (x, y) =>
                {
                    float u = x / 128f - 1f, v = y / 128f - 1f;
                    float d = Mathf.Sqrt(u * u + v * v) / 1.414f;
                    float a = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.4f, 1f, d));
                    return a * (0.8f + Mathf.PerlinNoise(x * 0.08f, y * 0.08f) * 0.35f);
                }, Vector4.zero),
                Ornament = SaveSprite("UI_Susleme", 512, 24, (x, y) =>
                {
                    float fx = Mathf.Abs(x - 256f), fy = Mathf.Abs(y - 12f);
                    float line = Mathf.Clamp01(1.2f - fy) * Mathf.Pow(Mathf.Clamp01(1f - fx / 256f), 0.8f);
                    float diamond = Mathf.Clamp01(8f - (fx + fy) + 0.5f);
                    float dots = Mathf.Clamp01(2.5f - Vector2.Distance(new Vector2(fx, fy), new Vector2(22f, 0f)) + 0.5f);
                    return Mathf.Max(line, Mathf.Max(diamond, dots));
                }, Vector4.zero),
                Diamond = SaveSprite("UI_Elmas", 32, 32, (x, y) =>
                    Mathf.Clamp01(13f - (Mathf.Abs(x - 16f) + Mathf.Abs(y - 16f)) + 0.5f), Vector4.zero),
            };
            return s;
        }

        private static float SegDist(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            return Vector2.Distance(p, a + ab * t);
        }

        private static Sprite SaveSprite(string name, int w, int h, Func<float, float, float> alpha, Vector4 border)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float a = Mathf.Clamp01(alpha(x + 0.5f, y + 0.5f));
                px[y * w + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255f));
            }
            tex.SetPixels32(px);
            tex.Apply();
            var bytes = tex.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(tex);

            string path = $"{UIFolder}/{name}.png";
            File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.mipmapEnabled = false;
            imp.alphaIsTransparency = true;
            imp.wrapMode = TextureWrapMode.Clamp;
            imp.filterMode = FilterMode.Bilinear;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.spriteBorder = border;
            imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void LoadFonts()
        {
            var fallback = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _fRegular = AssetDatabase.LoadAssetAtPath<Font>(FontFolder + "/Inter-Regular.ttf") ?? fallback;
            _fSemi = AssetDatabase.LoadAssetAtPath<Font>(FontFolder + "/Inter-SemiBold.ttf") ?? _fRegular;
            _fItalic = AssetDatabase.LoadAssetAtPath<Font>(FontFolder + "/Inter-Italic.ttf") ?? _fRegular;
        }

        // =====================================================================================
        // Kanvas
        // =====================================================================================

        private static HUDController BuildUI(UISprites sp, GameManager gm, out MenuController menu)
        {
            LoadFonts();
            var go = new GameObject("UI_Canvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            var hud = go.AddComponent<HUDController>();
            menu = go.AddComponent<MenuController>();
            menu.game = gm;
            hud.regularFont = _fRegular;
            hud.italicFont = _fItalic;
            var t = go.transform;

            var frost = Img(Stretch("Buz_Vinyeti", t), new Color(0.85f, 0.94f, 1f, 0f));
            frost.sprite = sp.Vignette;
            hud.frostImage = frost;

            BuildHud(hud, sp, t);
            BuildMainMenu(menu, sp, t);
            BuildPauseMenu(menu, sp, t);
            BuildEndScreen(menu, sp, t);

            hud.fadeImage = Img(Stretch("Kararma", t), new Color(0f, 0f, 0f, 0f));
            foreach (var tr in go.GetComponentsInChildren<Transform>(true)) tr.gameObject.layer = 5; // UI katmanı

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            var module = es.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
            return hud;
        }

        // =====================================================================================
        // Oyun içi HUD: köşelere yaslanan, kutusuz öğeler
        // =====================================================================================

        private static void BuildHud(HUDController hud, UISprites sp, Transform canvas)
        {
            var root = Stretch("HUD", canvas);
            hud.hudGroup = root.gameObject.AddComponent<CanvasGroup>();
            hud.hudGroup.interactable = false;
            hud.hudGroup.blocksRaycasts = false;
            hud.hudGroup.alpha = 0f; // editörde (Play dışında) menünün üstüne binmesin; oyun başlayınca belirir
            var shade = new Color(Night.r, Night.g, Night.b, 0.6f);

            // ---- Sol üst: yalnızca görev cümlesi ----
            CornerShade("Gorev_Golgesi", root, sp.Corner, 0, new Vector2(760, 180), shade);
            var obj = UIRect("Gorev", root, new Vector2(0, 1), new Vector2(0, 1), new Vector2(44, -36), new Vector2(660, 70));
            hud.objectiveText = Txt(UIRect("Metin", obj, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, new Vector2(620, 70)),
                StoryText.ObjListen, _fRegular, 22, TextAnchor.UpperLeft, Ink);

            // ---- Üst orta: pusula ----
            var comp = UIRect("Pusula", root, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -24), new Vector2(520, 90));
            hud.compassRoot = comp.gameObject;
            var arrow = UIRect("Ok", comp, new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(0, -16), new Vector2(30, 30));
            Img(arrow, Gold).sprite = sp.Arrow;
            hud.arrow = arrow;
            hud.targetText = Txt(UIRect("Hedef", comp, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -36), new Vector2(520, 24)),
                "", _fSemi, 17, TextAnchor.UpperCenter, Ink);
            hud.distanceText = Txt(UIRect("Mesafe", comp, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -58), new Vector2(520, 20)),
                "", _fRegular, 14, TextAnchor.UpperCenter, Muted);

            // ---- Sol alt: ısı ----
            CornerShade("Isi_Golgesi", root, sp.Corner, 2, new Vector2(700, 210), shade);
            var warm = UIRect("Isi", root, Vector2.zero, Vector2.zero, new Vector2(44, 40), new Vector2(380, 92));
            Txt(UIRect("Etiket", warm, Vector2.zero, Vector2.zero, new Vector2(0, 66), new Vector2(120, 20)), "ISI", _fSemi, 14, TextAnchor.LowerLeft, Gold);
            hud.warmthValue = Txt(UIRect("Deger", warm, Vector2.zero, Vector2.zero, new Vector2(240, 66), new Vector2(60, 20)), "100", _fSemi, 14, TextAnchor.LowerRight, Ink);
            var bar = UIRect("Cubuk", warm, Vector2.zero, Vector2.zero, new Vector2(0, 54), new Vector2(300, 8));
            Sliced(Img(bar, new Color(1f, 1f, 1f, 0.14f)), sp.Rounded, 4f);
            var fill = Stretch("Dolu", bar);
            hud.warmthFillImage = Sliced(Img(fill, new Color(1f, 0.66f, 0.3f)), sp.Rounded, 4f);
            hud.warmthFill = fill;
            var cap = Stretch("Soguk_Siniri", bar);
            cap.anchorMin = new Vector2(1f, 0f);
            Sliced(Img(cap, new Color(0.55f, 0.75f, 1f, 0.3f)), sp.Rounded, 4f);
            hud.warmthCap = cap;
            hud.statusText = Txt(UIRect("Durum", warm, Vector2.zero, Vector2.zero, new Vector2(0, 24), new Vector2(420, 22)), "", _fItalic, 15, TextAnchor.LowerLeft, Ice);
            hud.migrationText = Txt(UIRect("Goc", warm, Vector2.zero, Vector2.zero, new Vector2(0, 2), new Vector2(420, 20)), "", _fRegular, 14, TextAnchor.LowerLeft, Muted);

            // ---- Alt: altyazı bandı ----
            var sub = UIRect("Altyazi", root, new Vector2(0.5f, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(1920, 240));
            sub.anchorMin = new Vector2(0f, 0f);
            sub.anchorMax = new Vector2(1f, 0f);
            sub.sizeDelta = new Vector2(0f, 240f);
            hud.subtitleGroup = sub.gameObject.AddComponent<CanvasGroup>();
            hud.subtitleGroup.alpha = 0f;
            Img(Stretch("Bant", sub), new Color(0f, 0f, 0f, 0.72f)).sprite = sp.GradientV;
            hud.speakerText = Txt(UIRect("Konusan", sub, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 130), new Vector2(1000, 22)), "", _fSemi, 15, TextAnchor.MiddleCenter, Gold);
            hud.subtitleText = Txt(UIRect("Metin", sub, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 56), new Vector2(1040, 68)), "", _fItalic, 25, TextAnchor.UpperCenter, Ink);

            // ---- Alt orta: etkileşim ipucu ----
            var prompt = UIRect("Etkilesim", root, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 240), new Vector2(560, 52));
            hud.promptGroup = prompt.gameObject.AddComponent<CanvasGroup>();
            hud.promptGroup.alpha = 0f;
            Sliced(Img(prompt, new Color(Night.r, Night.g, Night.b, 0.8f)), sp.Rounded, 1f);
            var key = UIRect("Tus", prompt, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(10, 0), new Vector2(34, 34));
            Sliced(Img(key, Gold), sp.Rounded, 2f);
            Txt(Stretch("E", key), "E", _fSemi, 18, TextAnchor.MiddleCenter, Night, false);
            hud.promptText = Txt(UIRect("Metin", prompt, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(58, 1), new Vector2(490, 40)), "", _fRegular, 19, TextAnchor.MiddleLeft, Ink);
            var prog = UIRect("Ilerleme", prompt, Vector2.zero, Vector2.zero, new Vector2(58, 7), new Vector2(476, 4));
            Sliced(Img(prog, new Color(1f, 1f, 1f, 0.15f)), sp.Rounded, 8f);
            var progFill = Stretch("Dolu", prog);
            Sliced(Img(progFill, Gold), sp.Rounded, 8f);
            hud.promptProgressRoot = prog.gameObject;
            hud.promptProgressFill = progFill;

            // ---- Orta: bildirim ve uyarı ----
            var toast = UIRect("Bildirim", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 200), new Vector2(900, 70));
            hud.toastGroup = toast.gameObject.AddComponent<CanvasGroup>();
            hud.toastGroup.alpha = 0f;
            hud.toastText = Txt(UIRect("Metin", toast, new Vector2(0.5f, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(900, 36)), "", _fSemi, 26, TextAnchor.MiddleCenter, Ink);
            Img(UIRect("Susleme", toast, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -42), new Vector2(300, 14)), new Color(Gold.r, Gold.g, Gold.b, 0.85f)).sprite = sp.Ornament;
            hud.alertText = Txt(UIRect("Uyari", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 128), new Vector2(900, 30)), "", _fSemi, 19, TextAnchor.MiddleCenter, Danger);

            // ---- Sağ alt: kontroller (oyunun başında görünür, yakut alınınca gizlenir; F1 ile açılır) ----
            var ctrl = UIRect("Kontroller_Paneli", root, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-44, 40), new Vector2(400, 262));
            Sliced(Img(ctrl, new Color(Night.r, Night.g, Night.b, 0.82f)), sp.Rounded, 1f);
            var ctrlText = Stretch("Metin", ctrl);
            ctrlText.offsetMin = new Vector2(24, 16);
            ctrlText.offsetMax = new Vector2(-20, -18);
            Txt(ctrlText,
                "<b>KONTROLLER</b>\n" +
                "<b>W / S</b>  ·  ileri / yavaşla\n" +
                "<b>A / D</b>  ·  sola / sağa dön\n" +
                "<b>Space</b>  ·  yüksel     <b>Ctrl / C</b>  ·  alçal\n" +
                "<b>Shift</b>  ·  hızlı uç\n" +
                "<b>E</b>  ·  etkileşim (basılı tut: kanat çırp)\n" +
                "<b>R</b>  ·  son fenere dön\n" +
                "<b>Esc</b>  ·  menü     <b>F1</b>  ·  bu paneli gizle",
                _fRegular, 16, TextAnchor.UpperLeft, Ink);
            hud.controlsPanel = ctrl.gameObject;
        }

        /// <summary>Ekran köşesine yaslanan yumuşak koyu gölge. corner: 0 sol üst, 1 sağ üst, 2 sol alt, 3 sağ alt.</summary>
        private static RectTransform CornerShade(string name, Transform parent, Sprite sprite, int corner, Vector2 size, Color color)
        {
            Vector2 anchor = corner switch { 0 => new Vector2(0, 1), 1 => new Vector2(1, 1), 2 => new Vector2(0, 0), _ => new Vector2(1, 0) };
            var rt = UIRect(name, parent, anchor, new Vector2(0, 1), Vector2.zero, size);
            rt.localScale = new Vector3(corner == 1 || corner == 3 ? -1f : 1f, corner >= 2 ? -1f : 1f, 1f);
            Img(rt, color).sprite = sprite;
            return rt;
        }

        // =====================================================================================
        // Ana menü
        // =====================================================================================

        private static void BuildMainMenu(MenuController menu, UISprites sp, Transform canvas)
        {
            var mm = Stretch("Ana_Menu", canvas);
            menu.mainMenuGroup = mm.gameObject.AddComponent<CanvasGroup>();

            var left = UIRect("Sol_Golge", mm, new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(1250, 0));
            left.anchorMin = new Vector2(0f, 0f);
            left.anchorMax = new Vector2(0f, 1f);
            left.sizeDelta = new Vector2(1250f, 0f);
            Img(left, new Color(Night.r, Night.g, Night.b, 0.93f)).sprite = sp.GradientH;
            var bottom = UIRect("Alt_Golge", mm, new Vector2(0.5f, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(0, 200));
            bottom.anchorMin = new Vector2(0f, 0f);
            bottom.anchorMax = new Vector2(1f, 0f);
            bottom.sizeDelta = new Vector2(0f, 200f);
            Img(bottom, new Color(0f, 0f, 0f, 0.55f)).sprite = sp.GradientV;

            var a = new Vector2(0, 0.5f);
            Txt(UIRect("Ust_Baslik", mm, a, a, new Vector2(152, 300), new Vector2(800, 24)), StoryText.MenuKicker, _fSemi, 16, TextAnchor.MiddleLeft, Gold);
            Txt(UIRect("Ad", mm, a, a, new Vector2(142, 222), new Vector2(1000, 130)), StoryText.TitleName, _fSemi, 112, TextAnchor.MiddleLeft, Ink);
            Txt(UIRect("Alt_Baslik", mm, a, a, new Vector2(152, 142), new Vector2(800, 44)), StoryText.TitleSub, _fItalic, 34, TextAnchor.MiddleLeft, Ice);
            Img(UIRect("Susleme", mm, a, a, new Vector2(152, 104), new Vector2(300, 14)), new Color(Gold.r, Gold.g, Gold.b, 0.8f)).sprite = sp.Ornament;

            var buttons = UIRect("Dugmeler", mm, a, a, new Vector2(118, -95), new Vector2(480, 320));
            menu.mainButtons = buttons.gameObject;
            var b0 = MenuBtn("Oyuna_Basla", buttons, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 0), new Vector2(460, 54), "Oyuna Başla", 30, menu.OnPlay, sp, out _);
            MenuBtn("Nasil_Oynanir", buttons, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, -62), new Vector2(460, 54), "Nasıl Oynanır", 30, menu.OnHowTo, sp, out _);
            MenuBtn("Masal_Hakkinda", buttons, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, -124), new Vector2(460, 54), "Masal Hakkında", 30, menu.OnAbout, sp, out _);
            MenuBtn("Ayarlar", buttons, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, -186), new Vector2(460, 54), "Ayarlar", 30, menu.OnSettings, sp, out _);
            MenuBtn("Cikis", buttons, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, -248), new Vector2(460, 54), "Çıkış", 30, menu.OnQuit, sp, out _);
            menu.mainFirst = b0;


            // Sağdaki ayrıntı kartı
            var card = UIRect("Detay_Karti", mm, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-110, 0), new Vector2(700, 650));
            Sliced(Img(card, new Color(Night.r, Night.g, Night.b, 0.9f)), sp.Rounded, 0.7f);
            menu.detailCard = card.gameObject;

            var how = DetailPanel("Nasil_Oynanir", card, "Nasıl Oynanır", sp);
            Txt(UIRect("Metin", how, new Vector2(0, 1), new Vector2(0, 1), new Vector2(50, -112), new Vector2(600, 440)), StoryText.HowToPlay, _fRegular, 18, TextAnchor.UpperLeft, Ink);
            menu.howToBack = MenuBtn("Geri", how, Vector2.zero, Vector2.zero, new Vector2(30, 30), new Vector2(260, 48), "Geri", 22, menu.OnBack, sp, out _);
            menu.howToPanel = how.gameObject;

            var about = DetailPanel("Masal_Hakkinda", card, "Masal Hakkında", sp);
            Txt(UIRect("Metin", about, new Vector2(0, 1), new Vector2(0, 1), new Vector2(50, -112), new Vector2(600, 440)), StoryText.AboutStory, _fRegular, 19, TextAnchor.UpperLeft, Ink);
            menu.aboutBack = MenuBtn("Geri", about, Vector2.zero, Vector2.zero, new Vector2(30, 30), new Vector2(260, 48), "Geri", 22, menu.OnBack, sp, out _);
            menu.aboutPanel = about.gameObject;

            var set = DetailPanel("Ayarlar", card, "Ayarlar", sp);
            var tl = new Vector2(0, 1);
            menu.settingsFirst = MenuBtn("Ses", set, tl, tl, new Vector2(30, -120), new Vector2(640, 50), "Ses düzeyi", 22, menu.OnVolume, sp, out menu.volumeLabel);
            MenuBtn("Kalite", set, tl, tl, new Vector2(30, -178), new Vector2(640, 50), "Görüntü kalitesi", 22, menu.OnQuality, sp, out menu.qualityLabel);
            MenuBtn("Tam_Ekran", set, tl, tl, new Vector2(30, -236), new Vector2(640, 50), "Tam ekran", 22, menu.OnFullscreen, sp, out menu.fullscreenLabel);
            MenuBtn("Sarsinti", set, tl, tl, new Vector2(30, -294), new Vector2(640, 50), "Kamera sarsıntısı", 22, menu.OnShake, sp, out menu.shakeLabel);
            Txt(UIRect("Not", set, tl, tl, new Vector2(50, -372), new Vector2(600, 60)),
                "Enter ile değeri değiştir. Bu prototipte henüz ses içeriği yok; ses ayarı ileride kullanılmak üzere hazır.",
                _fItalic, 15, TextAnchor.UpperLeft, Muted);
            MenuBtn("Geri", set, Vector2.zero, Vector2.zero, new Vector2(30, 30), new Vector2(260, 48), "Geri", 22, menu.OnBack, sp, out _);
            menu.settingsPanel = set.gameObject;

            how.gameObject.SetActive(false);
            about.gameObject.SetActive(false);
            set.gameObject.SetActive(false);
            card.gameObject.SetActive(false);
        }

        private static RectTransform DetailPanel(string name, Transform card, string title, UISprites sp)
        {
            var p = Stretch(name, card);
            Txt(UIRect("Baslik", p, new Vector2(0, 1), new Vector2(0, 1), new Vector2(50, -40), new Vector2(600, 42)), title, _fSemi, 32, TextAnchor.UpperLeft, Gold);
            Img(UIRect("Susleme", p, new Vector2(0, 1), new Vector2(0, 1), new Vector2(50, -88), new Vector2(240, 12)), new Color(Gold.r, Gold.g, Gold.b, 0.7f)).sprite = sp.Ornament;
            return p;
        }

        // =====================================================================================
        // Duraklatma ve bölüm sonu
        // =====================================================================================

        private static void BuildPauseMenu(MenuController menu, UISprites sp, Transform canvas)
        {
            var p = Stretch("Duraklatma", canvas);
            Img(p, new Color(0.01f, 0.02f, 0.05f, 0.55f));
            var c = new Vector2(0.5f, 0.5f);
            var card = UIRect("Kart", p, c, c, Vector2.zero, new Vector2(560, 480));
            Sliced(Img(card, new Color(Night.r, Night.g, Night.b, 0.92f)), sp.Rounded, 0.7f);
            var top = new Vector2(0.5f, 1);
            Txt(UIRect("Baslik", card, top, top, new Vector2(0, -40), new Vector2(500, 46)), "Duraklatıldı", _fSemi, 36, TextAnchor.UpperCenter, Ink);
            Img(UIRect("Susleme", card, top, top, new Vector2(0, -96), new Vector2(240, 12)), new Color(Gold.r, Gold.g, Gold.b, 0.75f)).sprite = sp.Ornament;
            menu.pauseFirst = MenuBtn("Devam", card, top, top, new Vector2(0, -128), new Vector2(440, 54), "Devam Et", 26, menu.OnResume, sp, out _);
            MenuBtn("Son_Fener", card, top, top, new Vector2(0, -190), new Vector2(440, 54), "Son Fenere Dön", 26, menu.OnCheckpoint, sp, out _);
            MenuBtn("Yeniden", card, top, top, new Vector2(0, -252), new Vector2(440, 54), "Bölümü Yeniden Başlat", 26, menu.OnRestart, sp, out _);
            MenuBtn("Ana_Menu", card, top, top, new Vector2(0, -314), new Vector2(440, 54), "Ana Menüye Dön", 26, menu.OnMainMenu, sp, out _);
            Txt(UIRect("Ipucu", card, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(500, 20)), "Esc  devam et", _fRegular, 14, TextAnchor.MiddleCenter, Muted);
            menu.pausePanel = p.gameObject;
            p.gameObject.SetActive(false);
        }

        private static void BuildEndScreen(MenuController menu, UISprites sp, Transform canvas)
        {
            var e = Stretch("Bolum_Sonu", canvas);
            menu.endGroup = e.gameObject.AddComponent<CanvasGroup>();
            Img(e, new Color(0.01f, 0.015f, 0.04f, 0.84f));
            var c = new Vector2(0.5f, 0.5f);
            Txt(UIRect("Ust_Baslik", e, c, c, new Vector2(0, 260), new Vector2(1000, 24)), "KIRLANGIÇ'IN BİR GECESİ", _fSemi, 16, TextAnchor.MiddleCenter, Gold);
            menu.endTitle = Txt(UIRect("Baslik", e, c, c, new Vector2(0, 190), new Vector2(1200, 90)), "Bölüm Tamamlandı", _fSemi, 72, TextAnchor.MiddleCenter, Ink);
            menu.endStats = Txt(UIRect("Istatistik", e, c, c, new Vector2(0, 120), new Vector2(1000, 26)), "", _fRegular, 19, TextAnchor.MiddleCenter, Muted);
            Img(UIRect("Susleme", e, c, c, new Vector2(0, 80), new Vector2(320, 14)), new Color(Gold.r, Gold.g, Gold.b, 0.85f)).sprite = sp.Ornament;
            Txt(UIRect("Bedel_Baslik", e, c, c, new Vector2(0, 36), new Vector2(800, 22)), "YARDIMIN BEDELİ", _fSemi, 15, TextAnchor.MiddleCenter, Gold);
            menu.endCost = Txt(UIRect("Bedel", e, c, c, new Vector2(0, -36), new Vector2(1100, 110)), "", _fItalic, 23, TextAnchor.UpperCenter, Ink);
            menu.endFirst = MenuBtn("Tekrar_Oyna", e, c, c, new Vector2(-170, -180), new Vector2(300, 54), "Tekrar Oyna", 26, menu.OnReplay, sp, out _, TextAnchor.MiddleCenter);
            MenuBtn("Ana_Menu", e, c, c, new Vector2(170, -180), new Vector2(300, 54), "Ana Menü", 26, menu.OnMainMenu, sp, out _, TextAnchor.MiddleCenter);
            e.gameObject.SetActive(false);
        }

        // =====================================================================================
        // UI yardımcıları
        // =====================================================================================

        private static Button MenuBtn(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size,
            string text, int fontSize, UnityAction action, UISprites sp, out Text label, TextAnchor align = TextAnchor.MiddleLeft)
        {
            var rt = UIRect(name, parent, anchor, pivot, pos, size);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0f);
            img.raycastTarget = true;
            var btn = rt.gameObject.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.targetGraphic = img;

            var lrt = Stretch("Yazi", rt);
            lrt.offsetMin = new Vector2(align == TextAnchor.MiddleCenter ? 24f : 34f, 0f);
            label = Txt(lrt, text, _fSemi, fontSize, align, new Color(0.86f, 0.88f, 0.94f, 0.85f));

            var mk = UIRect("Isaret", rt, new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(14, 0), new Vector2(12, 12));
            Img(mk, Gold).sprite = sp.Diamond;
            mk.gameObject.SetActive(false);

            var mb = rt.gameObject.AddComponent<MenuButton>();
            mb.label = label;
            mb.marker = mk.gameObject;
            UnityEventTools.AddPersistentListener(btn.onClick, action);
            return btn;
        }

        private static RectTransform UIRect(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        private static RectTransform Stretch(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return rt;
        }

        private static Image Img(RectTransform rt, Color c)
        {
            var i = rt.gameObject.AddComponent<Image>();
            i.color = c;
            i.raycastTarget = false;
            return i;
        }

        private static Image Sliced(Image img, Sprite sprite, float pixelsPerUnitMultiplier)
        {
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = pixelsPerUnitMultiplier;
            return img;
        }

        private static Text Txt(RectTransform rt, string text, Font font, int size, TextAnchor anchor, Color c, bool shadow = true)
        {
            var t = rt.gameObject.AddComponent<Text>();
            t.font = font;
            t.text = text;
            t.fontSize = size;
            t.fontStyle = FontStyle.Normal;
            t.alignment = anchor;
            t.color = c;
            t.supportRichText = true;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.lineSpacing = 1.12f;
            if (shadow)
            {
                var s = rt.gameObject.AddComponent<Shadow>();
                s.effectColor = new Color(0f, 0f, 0f, 0.6f);
                s.effectDistance = new Vector2(1.5f, -1.5f);
            }
            return t;
        }
    }
}
