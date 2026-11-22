# Teslim PDF'i — İçerik

PDF, `Docs/Teslim/pdf_olustur.ps1` betiğiyle üretilir:

```
powershell -ExecutionPolicy Bypass -File Docs\Teslim\pdf_olustur.ps1
```

Betik çalışırken eksik bilgileri sorar: grup numarası, ders, öğretim elemanı, yapımcı, tarih, build / video / Git bağlantıları. Yanıtlar `Docs/Teslim/teslim_bilgileri.json` dosyasına kaydedilir. Bilgileri değiştirmek için `-Yeniden`, yer tutucularla önizleme için `-Onizleme` kullanılır.

## Sayfalar

1. **Kapak:** build, oynanış videosu ve Git deposu bağlantıları; ders, öğretim elemanı, grup, ekip (Ata Güler, Utku Kaya), yapımcı, tarih; ana menü görüntüsü.
2. **Oynanış ve mekanikler:** kontroller, 5 adımlık görev akışı, uçuş ve engeller, ısı sistemi, yönlendirme ve arayüz.
3–6. **Oyun içi ekran görüntüleri:** her sayfada iki görüntü ve mekaniği anlatan alt yazı.

Görseller `Docs/Teslim/gorseller/` klasöründen alınır (`oyun_01_ana_menu.png` … `oyun_09_bolum_sonu.png`). Bir görüntüyü değiştirmek için aynı adla yenisini koyup betiği tekrar çalıştırmak yeterli.

## Teslim kuralları

- Ekip başına tek PDF; yalnızca yapımcı gönderir.
- Build, oynanış videosu (1–2 dk) ve Git deposu bağlantıları PDF'in ilk sayfasında yer alır ve herkese açık olmalıdır.
- Dosya adı: `Vize_MutluPrens_GrupX.pdf`
- Alıcı: ziyad.guliyev@nisantasi.edu.tr
- Konu: `Mutlu Prens Vize – Grup X`
