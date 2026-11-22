# Mutlu Prens — Kırlangıç'ın Bir Gecesi (vize prototipi, v0.2)

Oscar Wilde'ın "Mutlu Prens" (1888) masalının "yakut ve terzi kadın" bölümünden uyarlanmış, 3–5 dakikada bitirilebilen, tek bölümlük oynanabilir Unity prototipi. Görseller gri kutu (primitive) şekillerdir.

- **Unity:** 6000.4.0f1 (Unity 6.4), URP, yeni Input System
- **Bölüm sahnesi:** `Assets/_MutluPrens/Scenes/MutluPrens_Bolum1.unity` (Build Settings'te ilk sırada)

## Nasıl oynanır

1. Projeyi Unity Hub'dan aç ve `MutluPrens_Bolum1` sahnesini aç (veya menüden **Mutlu Prens > Bölüm Sahnesini Aç**).
2. Play'e bas. Ana menü açılır (kamera Prens'in sütunu etrafında döner). **Oyuna Başla**'yı seç.

Ana menü: Oyuna Başla · Nasıl Oynanır · Masal Hakkında · Ayarlar (ses düzeyi, görüntü kalitesi, tam ekran, kamera sarsıntısı) · Çıkış. Menülerde fare veya ↑/↓ + Enter, geri için Esc kullanılır.

| Tuş | İşlev |
|---|---|
| W / S (↑ / ↓) | ileri uç / yavaşla |
| A / D (← / →) | sola / sağa dön |
| Space | yüksel |
| Ctrl veya C | alçal |
| Shift | hızlı uç (daha çok üşütür) |
| E | etkileşim; çocuğu serinletirken **basılı tut** |
| Esc | duraklatma menüsü |
| R | son yanan fenere dön |
| Enter / Tab | altyazıyı geç |
| F1 | kontroller panelini aç/kapat |
| H | arayüzü gizle (temiz ekran görüntüsü için) |
| F12 | ekran görüntüsü, proje klasöründe `Screenshots/` |

## Bölümün akışı (masaldaki sırayla)

1. **Prens'i dinle:** Kırlangıç sütunun önünde başlar. Bulutsuz gökten üstüne düşen damlalar Prens'in gözyaşlarıdır. Prens yardım ister; Kırlangıç Mısır'da, Nil kıyısında bekleyen arkadaşlarını anar. Oyuncu güney yolundan sürüye uçmaya kalkarsa ters rüzgâr ve Prens'in yalvarışı onu geri çağırır (tereddüt).
2. **Yakutu al:** Kılıcın kabzasındaki yakuta yaklaşıp **E**. Sürü güneye uçup gider.
3. **Yakutu götür:** Altın ışık noktaları ve üstteki pusula yolu gösterir.
   - **Katedral Caddesi:** beyaz mermer melekli katedral ve çan kulesi; caddeyi aşan kemerlerde sallanan çanlar.
   - **Saray Bulvarı:** dans sesleri gelen saray, pencerelerde dans eden gölgeler. Balkondaki genç kız balo elbisesine çarkıfelek işletmektedir; o elbiseyi işleyen terzi kadındır. Bulvarda fener dizileri ve sallanan fener sıraları vardır, sarayın pencereleri ısıtır.
   - **Rıhtım ve nehir:** direklerine fener asılmış gemiler. Rıhtıma uzanan halatlar dönüşümlü olarak alçakta ve yüksekte durur; nehirden esen soğuk rüzgâr kuşu rıhtıma iter.
   - **Köprü ve Eski Çarşı:** tenteler, tezgâhlar ve sıcak mangallar; sokağın üstünde sallanan dev bakır teraziler.
   - **Yoksul Mahalle:** çamaşır ipleri ve mum ışığı yanan pencere. Çocuk yatağında kıvranmakta, annesi masanın başında uyuyakalmıştır.
4. **Teslim:** Yakut, kadının yüksüğünün yanına, işlenmekte olan saten elbisenin yanına bırakılır (**E**).
5. **Çocuğu serinlet:** **E basılı tutulunca** Kırlangıç kanatlarını hızla çırpar; çocuğun soluk mavi rengi ısınır ve çocuk uykuya dalar.
6. **Prens'e dön:** Mavi ışıklar meydana geri götürür. Bitişte kamera Prens'in kararmış gözlerine bakar.

**Yardımın bedeli oyunda görünür:**
- Yakut kılıçtan kaybolur.
- Prens'in kırmızı gözlerinin ışığı yakut uzaklaştıkça söner ve teslimde tamamen kararır. Heykelin altın yaldızı donuklaşır (sağ üstteki "Yardımın Bedeli" göstergesi).
- Çocuk serinletildikten sonra göç ertelenir: Kırlangıç'ın azami ısısı %70'e düşer, daha hızlı üşür, kar yoğunlaşır, ekran kenarları buzlanır ve kuşun rengi buz mavisine kayar.

**Başarısızlık:** Isı sıfıra inerse Kırlangıç donar ve son yanan fenerden (yolda 5 fener) devam eder. Kırmızımsı/bakır engellere çarpmak ısı kaybettirir. Duraklatma menüsünden son fenere dönülebilir, bölüm yeniden başlatılabilir veya ana menüye dönülebilir.

## Arayüz

Arayüz ekranın köşelerine yaslanır ve kutu kullanmaz:
- **Sol üst:** görev ve 5 adım noktası
- **Üst orta:** pusula oku, hedef ve mesafe
- **Sol alt:** ince ısı çubuğu ve göç durumu
- **Sağ üst:** yardımın bedeli
- **Alt:** altyazı bandı ve etkileşim ipucu (E tuşu ve basılı tutma çubuğu)

Kontroller paneli F1 ile açılır. Yazı tipi **Inter**'dir (SIL Open Font License 1.1); dosyası ve lisansı `Assets/_MutluPrens/Fonts/` klasöründe.

## Klasör yapısı

```
Assets/_MutluPrens/
  Scenes/      MutluPrens_Bolum1.unity
  Scripts/
    Core/      GameManager (akış), StoryText (tüm metinler), ScreenshotTool, AutopilotDriver (yalnızca editör testi)
    Player/    SwallowController (uçuş), SwallowWarmth (ısı), SwallowVisual, ChaseCamera, MenuCamera, FlightInput
    World/     PrinceStatue, SeamstressHouse, RouteGuide, Checkpoint, WindArea, WarmthArea, Hazard,
               Rotator / Pendulum / Oscillator (hareketli öğeler), InteractionZone, DialogueTrigger,
               FlockOfSwallows, SnowController
    UI/        HUDController, MenuController, MenuButton
    Editor/    MutluPrensLevelBuilder (+ .Landmarks, .Gameplay, .UI) sahneyi kodla kurar; MutluPrensEditorAutomation
  Materials/   gri kutu materyalleri (Türkçe adlı)
  UI/          kodla üretilen arayüz sprite'ları (yuvarlak köşe, gradyan, ok, süsleme)
  Fonts/       Inter (OFL) ve lisansı
  Settings/    aydınlatma ve son işleme (bloom, vinyet) ayarları
Docs/          teslim PDF'i için içerik taslağı ve taslak ekran görüntüleri
```

Sahne hiyerarşisi de yerlere göre gruplanmıştır: `_Yonetim`, `Isiklandirma`, `Sehir`, `Meydan_MutluPrens`, `Katedral_ve_Canli_Kemerler`, `Saray_ve_Balkon`, `Nehir_Rihtim_Gemiler`, `Eski_Carsi`, `Yoksul_Mahalle`, `Terzi_Kadinin_Evi`, `Ruzgar_ve_Sicaklik_Bolgeleri`, `Kontrol_Noktalari_Fenerler`, `Rota_ve_Yonlendirme`, `Diyalog_Tetikleyicileri`, `Atmosfer`, `Kirlangic_Oyuncu`, `Main Camera`, `UI_Canvas`, `EventSystem`.

## Editör menüsü (Mutlu Prens)

- **Bölümü Yeniden Oluştur:** sahneyi sıfırdan, her seferinde aynı şekilde kurar. Sahnede elle yapılan değişiklikler silinir.
- **Bölüm Sahnesini Aç**
- **Otomatik Oynanış Testi (Play):** bir test pilotu, oyuncuyla aynı uçuş girdilerini kullanarak bölümü baştan sona uçar. Sonuç Console'a `[MP-AUTO]` önekiyle yazılır; taslak ekran görüntüleri `Docs/Taslak_Ekran_Goruntuleri/` klasörüne kaydedilir.

- **Oynanış Videosu Kaydet (Play):** test pilotunun uçuşunu 1920x1080, 30 fps MP4 olarak `Docs/Teslim/MutluPrens_Oynanis_Videosu.mp4` dosyasına kaydeder (yaklaşık 2 dk; replikler kısaltılır, dönüş yolu 2x hızlıdır).
- **Windows Build Al:** `Builds/Windows/MutluPrens.exe` üretir.
- **Teslim > 1) / 2) Editör görünümü:** PDF'teki editör ekran görüntüleri için standart pencere düzenine geçip sahneyi veya proje klasörlerini hazırlar. **3)** önceki pencere düzenini geri yükler.

Not: Test çalışırken betik değiştirmeyin; Play sırasında yeniden derleme testi bozar.

## Teslim dosyaları (`Docs/Teslim/`)

- `pdf_olustur.ps1`: kapak bilgilerini ve bağlantıları betiğin başına yazıp çalıştırınca `Vize_MutluPrens_Grup<X>.pdf` üretilir (Microsoft Edge ile).
- `gorseller/`: PDF'e giren oyun içi ve editör ekran görüntüleri (`editor_01_sahne.png`, `editor_02_proje.png` adlarıyla eklenir).
- `E-posta_Taslagi.txt`: teslim e-postasının taslağı.
- Video ve build zip'i depoya eklenmez; herkese açık bir bağlantıyla paylaşılır.
