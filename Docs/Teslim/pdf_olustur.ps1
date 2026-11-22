# Mutlu Prens vize teslim PDF'ini üretir: kapak (bağlantılar), oynanış mekanikleri, oyun içi ekran görüntüleri.
#
# Kullanım (proje klasöründe):
#   powershell -ExecutionPolicy Bypass -File Docs\Teslim\pdf_olustur.ps1
# Eksik bilgiler (grup no, ders, bağlantılar...) çalışırken sorulur ve teslim_bilgileri.json dosyasına kaydedilir;
# sonraki çalıştırmalarda yeniden sorulmaz. Bilgileri baştan girmek için:  -Yeniden
# Yer tutucularla önizleme üretmek için (soru sormadan):  -Onizleme
#
# Çıktı: Docs\Teslim\Vize_MutluPrens_Grup<NO>.pdf  (Microsoft Edge ile HTML'den yazdırılır)

param([switch]$Yeniden, [switch]$Onizleme)

$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$infoPath = Join-Path $here "teslim_bilgileri.json"

$Uyeler = "Ata Güler<br>Utku Kaya"

# Sorulacak bilgiler: anahtar, soru, önizleme yer tutucusu
$fields = @(
  @("Grup",         "Grup numarası (dosya adında ve e-posta konusunda kullanılır, ör. 3)", "X"),
  @("Ders",         "Ders adı ve kodu",                                                    "[Ders adı ve kodu]"),
  @("OgretimUyesi", "Öğretim elemanı",                                                     "[Öğretim elemanı]"),
  @("Yapimci",      "Yapımcı (PDF'i gönderen kişi): Ata Güler mi, Utku Kaya mı?",          "[Yapımcı]"),
  @("Tarih",        "Teslim tarihi (ör. 25.11.2026)",                                     "[Teslim tarihi]"),
  @("BuildLink",    "Build bağlantısı (herkese açık)",                                     "[Build bağlantısı – herkese açık]"),
  @("VideoLink",    "Oynanış videosu bağlantısı (herkese açık)",                           "[Oynanış videosu bağlantısı – herkese açık]"),
  @("GitLink",      "Git deposu bağlantısı (herkese açık)",                                "[Git deposu bağlantısı – herkese açık]")
)

$info = @{}
if ((Test-Path $infoPath) -and -not $Yeniden) {
  (Get-Content $infoPath -Raw -Encoding UTF8 | ConvertFrom-Json).PSObject.Properties | ForEach-Object { $info[$_.Name] = $_.Value }
}
foreach ($f in $fields) {
  $key = $f[0]
  if ($Onizleme) { if (-not $info[$key]) { $info[$key] = $f[2] }; continue }
  while (-not $info[$key] -or "$($info[$key])".Trim() -eq "") {
    $info[$key] = (Read-Host $f[1]).Trim()
  }
}
if (-not $Onizleme) {
  [IO.File]::WriteAllText($infoPath, ($info | ConvertTo-Json), (New-Object System.Text.UTF8Encoding $false))
}

function ConvertTo-HtmlText([string]$s) { [System.Net.WebUtility]::HtmlEncode($s) }
function Format-Link([string]$s) { if ($s -match '^https?://') { "<a href='$(ConvertTo-HtmlText $s)'>$(ConvertTo-HtmlText $s)</a>" } else { ConvertTo-HtmlText $s } }

# ---------------- Görseller ----------------
$imgSrc = Join-Path $here "gorseller"
$imgOut = Join-Path $here "_pdf_gorseller"
New-Item -ItemType Directory -Force $imgOut | Out-Null
Add-Type -AssemblyName System.Drawing
function Convert-Image([string]$src, [string]$dst, [int]$maxW) {
  $img = [System.Drawing.Image]::FromFile($src)
  try {
    $w = [Math]::Min($maxW, $img.Width); $h = [int]($img.Height * $w / $img.Width)
    $bmp = New-Object System.Drawing.Bitmap $w, $h
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.DrawImage($img, 0, 0, $w, $h); $g.Dispose()
    $codec = [System.Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object { $_.MimeType -eq "image/jpeg" }
    $ep = New-Object System.Drawing.Imaging.EncoderParameters 1
    $ep.Param[0] = New-Object System.Drawing.Imaging.EncoderParameter ([System.Drawing.Imaging.Encoder]::Quality), 84L
    $bmp.Save($dst, $codec, $ep); $bmp.Dispose()
  } finally { $img.Dispose() }
}

$oyun = @(
  @("oyun_01_ana_menu.png",         "Ana menü: Oyuna Başla, Nasıl Oynanır, Masal Hakkında, Ayarlar ve Çıkış. Menüler fare veya klavye (↑ ↓ ve Enter) ile kullanılır."),
  @("oyun_02_prens_ile_konusma.png","Başlangıç: Kırlangıç, Prens'in sütununa yaklaşınca konuşma altyazıyla akar. Sağ altta kontroller, sol altta ısı çubuğu görünür."),
  @("oyun_03_katedral.png",         "Katedral Caddesi: kemerlerde sallanan çanlar hareketli engeldir. Altın yol ışıkları güvenli uçuş hattını, üstteki pusula hedefi ve mesafeyi gösterir."),
  @("oyun_04_saray.png",            "Saray Bulvarı: fener dizileri farklı yüksekliklerde yolu keser; oyuncu yükselip alçalarak aralarından geçer. Saray pencereleri ısıtır."),
  @("oyun_05_rihtim.png",           "Rıhtım: gemi halatları dönüşümlü olarak alçakta ve yüksekte uzanır. Nehirden esen soğuk rüzgâr Kırlangıç'ı iter ve ısısını düşürür."),
  @("oyun_06_eski_carsi.png",       "Eski Çarşı: sallanan bakır teraziler en güvenli şekilde üstlerinden aşılır. Yoldaki fenerler kontrol noktasıdır; yanınca ısı dolar."),
  @("oyun_07_serinletme.png",       "Teslim: E ile yakut masaya bırakılır, ardından E basılı tutularak çocuk kanatlarla serinletilir. İlerleme çubuğu ipucunun altında dolar."),
  @("oyun_08_kararan_prens.png",    "Dönüş ve bedel: Prens'in gözleri kararmıştır. Göç ertelendiği için Kırlangıç'ın ısı sınırı düşer ve ekran kenarları buzlanır."),
  @("oyun_09_bolum_sonu.png",       "Bölüm sonu: süre, donma ve çarpma sayısı gösterilir. Tekrar Oyna veya Ana Menü seçilebilir.")
)

function Fig($item) {
  $src = Join-Path $imgSrc $item[0]
  if (-not (Test-Path $src)) { return "<figure class='missing'><div>Görsel bulunamadı: $($item[0])</div><figcaption>$(ConvertTo-HtmlText $item[1])</figcaption></figure>" }
  $dst = Join-Path $imgOut ([IO.Path]::ChangeExtension($item[0], ".jpg"))
  Convert-Image $src $dst 1600
  $rel = "_pdf_gorseller/" + [IO.Path]::GetFileName($dst)
  $cap = if ($item[1]) { "<figcaption>$(ConvertTo-HtmlText $item[1])</figcaption>" } else { "" }
  return "<figure><img src='$rel'>$cap</figure>"
}

$o = $oyun | ForEach-Object { Fig $_ }
$cover = Fig @("oyun_01_ana_menu.png", "")
$Grup = ConvertTo-HtmlText $info.Grup

$html = @"
<!doctype html><html lang="tr"><head><meta charset="utf-8"><title>Vize_MutluPrens_Grup$Grup</title>
<style>
@page { size: A4; margin: 16mm 16mm 18mm 16mm; }
* { box-sizing: border-box; }
body { font-family: "Segoe UI", Arial, sans-serif; color: #1d2230; font-size: 10.5pt; line-height: 1.45; margin: 0; }
h1 { font-size: 26pt; margin: 0 0 2mm 0; letter-spacing: -0.3pt; }
h2 { font-size: 15pt; margin: 0 0 4mm 0; padding-bottom: 2mm; border-bottom: 1.5pt solid #c9a24a; }
h3 { font-size: 11.5pt; margin: 4mm 0 1.5mm 0; }
.kicker { color: #a07a2a; font-weight: 600; letter-spacing: 1.2pt; font-size: 9pt; text-transform: uppercase; }
.sub { font-style: italic; color: #4b5a78; font-size: 13pt; margin-bottom: 6mm; }
.page { page-break-after: always; }
.page:last-child { page-break-after: auto; }
table.info { border-collapse: collapse; width: 100%; margin: 4mm 0 5mm 0; }
table.info td { padding: 1.6mm 2mm; border-bottom: 0.5pt solid #d9dde6; vertical-align: top; }
table.info td:first-child { width: 38mm; color: #59627a; font-weight: 600; }
.links { background: #f6f1e4; border: 1pt solid #d8c18a; border-radius: 3mm; padding: 4mm 5mm; margin-bottom: 5mm; }
.links b { display: inline-block; width: 36mm; }
.links div { margin: 1.2mm 0; word-break: break-all; }
.links a { color: #1d2230; }
figure { margin: 0 0 6mm 0; page-break-inside: avoid; }
figure img { width: 100%; border-radius: 1.5mm; border: 0.5pt solid #c8ccd6; display: block; }
figcaption { font-size: 9.5pt; color: #3b4256; margin-top: 1.8mm; }
figure.missing div { height: 60mm; border: 1pt dashed #b33; display:flex; align-items:center; justify-content:center; color:#b33; }
table.keys { border-collapse: collapse; margin: 1mm 0 2mm 0; }
table.keys td { padding: 0.9mm 3mm 0.9mm 0; vertical-align: top; }
table.keys td:first-child { font-weight: 600; white-space: nowrap; }
ul { margin: 1mm 0 2mm 5mm; padding: 0; } li { margin: 0.8mm 0; }
</style></head><body>

<section class="page">
  <div class="kicker">Vize teslimi · Oynanabilir prototip · Grup $Grup</div>
  <h1>Mutlu Prens</h1>
  <div class="sub">Kırlangıç'ın Bir Gecesi</div>
  <div class="links">
    <div><b>Build (Windows)</b> $(Format-Link $info.BuildLink)</div>
    <div><b>Oynanış videosu</b> $(Format-Link $info.VideoLink)</div>
    <div><b>Git deposu</b> $(Format-Link $info.GitLink)</div>
  </div>
  <table class="info">
    <tr><td>Ders</td><td>$(ConvertTo-HtmlText $info.Ders)</td></tr>
    <tr><td>Öğretim elemanı</td><td>$(ConvertTo-HtmlText $info.OgretimUyesi)</td></tr>
    <tr><td>Grup</td><td>$Grup</td></tr>
    <tr><td>Ekip üyeleri</td><td>$Uyeler</td></tr>
    <tr><td>Yapımcı</td><td>$(ConvertTo-HtmlText $info.Yapimci)</td></tr>
    <tr><td>Tarih</td><td>$(ConvertTo-HtmlText $info.Tarih)</td></tr>
  </table>
  $cover
</section>

<section class="page">
  <h2>Oynanış ve mekanikler</h2>
  <p>Oyuncu Kırlangıç'ı yönetir. Mutlu Prens'in kılıcındaki yakutu alır, şehrin üzerinden uçarak yoksul terzi kadının evine götürür, çocuğu serinletir ve Prens'e geri döner. Tek bölüm 3–5 dakikada tamamlanır.</p>

  <h3>Kontroller</h3>
  <table class="keys">
    <tr><td>W / S</td><td>ileri uç / yavaşla</td></tr>
    <tr><td>A / D</td><td>sola / sağa dön</td></tr>
    <tr><td>Space · Ctrl veya C</td><td>yüksel · alçal</td></tr>
    <tr><td>Shift</td><td>hızlı uç (daha çok üşütür)</td></tr>
    <tr><td>E</td><td>etkileşim; basılı tutunca kanat çırpma</td></tr>
    <tr><td>Esc · R · F1</td><td>duraklatma menüsü · son fenere dön · kontroller paneli</td></tr>
  </table>

  <h3>Görev akışı (5 adım)</h3>
  <ul>
    <li><b>Prens'i dinle:</b> sütuna yaklaşınca konuşma başlar. Oyuncu güneydeki sürüye uçmaya kalkarsa ters rüzgâr onu geri iter.</li>
    <li><b>Yakutu al:</b> kılıcın kabzasındaki yakuta yaklaşıp E'ye basılır.</li>
    <li><b>Yakutu götür:</b> katedral, saray, rıhtım, eski çarşı ve yoksul mahalleden geçen engelli rota.</li>
    <li><b>Çocuğu serinlet:</b> pencerede E basılı tutulur, ilerleme çubuğu dolar.</li>
    <li><b>Prens'e dön:</b> aynı rota geri uçulur; bitişte istatistik ekranı açılır.</li>
  </ul>

  <h3>Uçuş ve engeller</h3>
  <ul>
    <li>Serbest üç boyutlu uçuş. Çatıların üstüne çıkılmak istenirse ayaz Kırlangıç'ı aşağı iter; rota kısaltılamaz.</li>
    <li>Hareketli engeller: sallanan çanlar, fener dizileri ve sıraları, yalpalayan gemilerin halatları, bakır teraziler, çamaşır ipleri.</li>
    <li>Engele çarpınca Kırlangıç geri savrulur, kamera sarsılır ve ısı azalır.</li>
  </ul>

  <h3>Isı sistemi</h3>
  <ul>
    <li>Isı zamanla, soğuk rüzgârda, hızlı uçarken ve çarpınca düşer; fenerler, sıcak pencereler ve mangallar ısıtır.</li>
    <li>Isı biterse Kırlangıç donar ve son yanan fenerden (yolda 5 kontrol noktası) devam eder.</li>
    <li>Çocuk serinletildikten sonra göç ertelenir: azami ısı %70'e iner, soğuma hızlanır, ekran kenarları buzlanır.</li>
  </ul>

  <h3>Yönlendirme ve arayüz</h3>
  <ul>
    <li>Yol ışıkları gidişte altın, dönüşte mavidir; hedefin üstünde bir ışık sütunu yanar.</li>
    <li>Ekranda yalnızca görev cümlesi, pusula ve mesafe, ısı çubuğu ve gerektiğinde etkileşim ipucu ile altyazı bulunur.</li>
  </ul>
</section>

<section class="page">
  <h2>Oyun içi ekran görüntüleri</h2>
  $($o[1]) $($o[2])
</section>
<section class="page">
  $($o[3]) $($o[4])
</section>
<section class="page">
  $($o[5]) $($o[6])
</section>
<section class="page">
  $($o[7]) $($o[8])
</section>

</body></html>
"@

$htmlPath = Join-Path $here "teslim.html"
[IO.File]::WriteAllText($htmlPath, $html, (New-Object System.Text.UTF8Encoding $false))
$pdfPath = Join-Path $here ("Vize_MutluPrens_Grup{0}.pdf" -f $info.Grup)
$edge = @("C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe", "C:\Program Files\Microsoft\Edge\Application\msedge.exe") | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $edge) { throw "Microsoft Edge bulunamadı. teslim.html dosyasını tarayıcıda açıp PDF olarak yazdırabilirsiniz." }
$profileDir = Join-Path $env:TEMP "mp_pdf_edge_profile"
& $edge --headless=new --disable-gpu --no-first-run "--user-data-dir=$profileDir" --no-pdf-header-footer "--print-to-pdf=$pdfPath" ("file:///" + ($htmlPath -replace '\\','/')) | Out-Null
Start-Sleep -Seconds 2
if (Test-Path $pdfPath) { "PDF hazır: $pdfPath  ({0:0.0} MB)" -f ((Get-Item $pdfPath).Length / 1MB) } else { "PDF üretilemedi; teslim.html dosyasını tarayıcıda açıp PDF olarak yazdırın." }
