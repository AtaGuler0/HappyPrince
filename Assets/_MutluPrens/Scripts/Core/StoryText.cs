namespace MutluPrens.Core
{
    /// <summary>Tek bir altyazı satırı (konuşan, metin, ekranda kalma süresi).</summary>
    public readonly struct DialogueLine
    {
        public readonly string Speaker;
        public readonly string Text;
        public readonly float Duration;

        public DialogueLine(string speaker, string text, float duration)
        {
            Speaker = speaker;
            Text = text;
            Duration = duration;
        }
    }

    /// <summary>
    /// Oyundaki tüm metinler tek yerde. Olaylar ve rota Oscar Wilde'ın "Mutlu Prens" (1888) masalını izler:
    /// katedral kulesinin mermer melekleri, saray balkonundaki genç kız, direklerine fener asılı gemiler,
    /// bakır terazili eski çarşı, yüksüğün yanına bırakılan yakut ve çocuğu kanatlarıyla serinleten Kırlangıç.
    /// Replikler bu prototip için Türkçe olarak yeniden yazılmıştır.
    /// </summary>
    public static class StoryText
    {
        public const string Prince = "Mutlu Prens";
        public const string Swallow = "Kırlangıç";
        public const string Hint = "İpucu";
        public const string Girl = "Saraydaki genç kız";
        public const string Boy = "Hasta çocuk";

        public static readonly DialogueLine[] PrinceRequest =
        {
            new(Swallow, "Gökyüzünde tek bir bulut yok ama üstüme damlalar düşüyor... Ağlıyor musun, Prens?", 4.5f),
            new(Prince, "Buradan, bu yüksek sütunun tepesinden şehrin bütün kederini görüyorum.", 4.5f),
            new(Prince, "Kırlangıç, küçük Kırlangıç... Uzakta, dar bir sokakta yoksul bir terzi kadın yaşıyor.", 4.5f),
            new(Prince, "Oğlu ateşler içinde yatıyor; annesinin ona nehir suyundan başka verecek bir şeyi yok.", 4.5f),
            new(Swallow, "Ama arkadaşlarım beni Mısır'da bekliyor. Şimdi Nil'in üzerinde, nilüferlerin arasında uçuyorlar...", 4.5f),
            new(Prince, "Bir gece kal ve habercim ol. Kılıcımın kabzasındaki yakutu ona götür.", 4.5f),
        };

        public static readonly DialogueLine[] Accept =
        {
            new(Swallow, "Burası çok soğuk... Ama bir gece kalıp senin habercin olacağım.", 3.5f),
            new(Prince, "Teşekkür ederim, küçük Kırlangıç.", 3f),
        };

        public static readonly DialogueLine[] SouthHesitation =
        {
            new(Swallow, "Sürüm güneye uzaklaşıyor... Hâlâ onlara yetişebilirim.", 3.5f),
            new(Swallow, "Ama Prens'in yüzündeki hüzün aklımdan çıkmıyor.", 3.5f),
            new(Prince, "Kırlangıç, küçük Kırlangıç... Yalnızca bir gece.", 3f),
        };

        public static readonly DialogueLine[] SouthAfter =
        {
            new(Swallow, "Sürüm çoktan gitti. Prens'i yalnız bırakamam.", 3.5f),
        };

        public static readonly DialogueLine[] Cathedral =
        {
            new(Swallow, "Katedral kulesinin beyaz mermer melekleri... Çanlar gece rüzgârında sallanıyor.", 4.5f),
            new(Hint, "Çanların altından geç ya da çan öbür yana sallanınca yanından süzül.", 4f),
        };

        public static readonly DialogueLine[] Palace =
        {
            new(Swallow, "Saraydan müzik ve dans sesleri geliyor.", 3f),
            new(Girl, "Umarım balo elbisem zamanında yetişir. Üzerine çarkıfelek çiçekleri işletiyorum...", 4.5f),
            new(Girl, "...ama terziler öyle tembel ki!", 2.5f),
            new(Swallow, "O elbiseyi işleyen, yakutu götürdüğüm kadın olmalı.", 3.5f),
            new(Hint, "Asılı fener dizilerinin altından alçaktan uç. Sarayın pencereleri seni ısıtır.", 4f),
        };

        public static readonly DialogueLine[] River =
        {
            new(Swallow, "Nehirdeki gemilerin direklerine fenerler asılmış.", 3.5f),
            new(Hint, "Halatlar alçakta ve yüksekte dönüşümlü uzanıyor. Nehirden esen rüzgâr seni rıhtıma iter.", 4.5f),
        };

        public static readonly DialogueLine[] Market =
        {
            new(Swallow, "Eski çarşıda tüccarlar bakır terazilerde para tartıyor.", 3.5f),
            new(Hint, "Dev terazilerin kefelerine çarpma; üstlerinden aşmak en güvenlisi.", 4f),
        };

        public static readonly DialogueLine[] PoorQuarter =
        {
            new(Swallow, "İşte orada: mum ışığı yanan küçük pencere.", 3.5f),
            new(Swallow, "Çocuk yatağında kıvranıyor; annesi yorgunluktan uyuyakalmış.", 4f),
        };

        public static readonly DialogueLine[] RubyPlaced =
        {
            new(Swallow, "Yakutu masanın üstüne, kadının yüksüğünün yanına bıraktım.", 3.5f),
            new(Hint, "Şimdi E'yi basılı tutarak kanatlarınla çocuğun alnını serinlet.", 4f),
        };

        public static readonly DialogueLine[] Fanned =
        {
            new(Boy, "Ne kadar serinledim... İyileşiyor olmalıyım.", 3.5f),
            new(Swallow, "Çocuk derin bir uykuya daldı. Sürüm ise çoktan gitti; artık bu soğuk şehirde kalacağım.", 5f),
            new(Hint, "Göç ertelendi: daha çabuk üşüyeceksin. Prens'e geri dön.", 4f),
        };

        public static readonly DialogueLine[] Ending =
        {
            new(Swallow, "Ne garip... Hava bu kadar soğuk ama içim ısınıyor.", 4f),
            new(Prince, "Çünkü iyi bir şey yaptın, küçük Kırlangıç.", 3.5f),
            new(Prince, "Gözlerimin ışığı söndü... ama bu gece şehirde bir çocuk rahat uyuyor.", 4.5f),
        };

        public static DialogueLine[] Get(string id)
        {
            switch (id)
            {
                case "prince_request": return PrinceRequest;
                case "accept": return Accept;
                case "south": return SouthHesitation;
                case "south_after": return SouthAfter;
                case "cathedral": return Cathedral;
                case "palace": return Palace;
                case "river": return River;
                case "market": return Market;
                case "poor_quarter": return PoorQuarter;
                case "ruby_placed": return RubyPlaced;
                case "fanned": return Fanned;
                case "ending": return Ending;
                default: return System.Array.Empty<DialogueLine>();
            }
        }

        // ---- Görev metinleri ----
        public static readonly string[] Steps =
        {
            "Prens'i dinle",
            "Yakutu kılıçtan al",
            "Yakutu terzi kadına götür",
            "Çocuğu kanatlarınla serinlet",
            "Prens'e geri dön",
        };

        public const string ObjListen = "Mutlu Prens'in sütununa yaklaş ve onu dinle.";
        public const string ObjTakeRuby = "Kılıcın kabzasındaki yakutu al.";
        public const string ObjDeliver = "Yakutu terzi kadının evine götür. Altın ışıkları izle.";
        public const string ObjFan = "E'yi basılı tutarak çocuğu kanatlarınla serinlet.";
        public const string ObjReturn = "Mutlu Prens'e geri dön. Mavi ışıkları izle.";
        public const string ObjEnding = "Bölüm tamamlandı.";

        // ---- Menü metinleri ----
        public const string MenuKicker = "OSCAR WILDE'IN MASALINDAN";
        public const string TitleName = "Mutlu Prens";
        public const string TitleSub = "Kırlangıç'ın Bir Gecesi";

        public const string HowToPlay =
            "<b>Amaç</b>\n" +
            "Prens'in kılıcındaki yakutu al, şehrin üzerinden uçarak terzi kadının evine götür, " +
            "çocuğu serinlet ve Prens'e geri dön.\n\n" +
            "<b>Kontroller</b>\n" +
            "W / S   ileri uç / yavaşla\n" +
            "A / D   sola / sağa dön\n" +
            "Space   yüksel        Ctrl veya C   alçal\n" +
            "Shift   hızlı uç (daha çok üşütür)\n" +
            "E   etkileşim (basılı tut: kanat çırp)\n" +
            "Esc   duraklat        R   son fenere dön\n" +
            "F1   kontroller paneli\n\n" +
            "<b>Isı</b>\n" +
            "Soğuk rüzgârlar ve çarpmalar ısını düşürür. Fenerler ve sıcak pencereler ısıtır. " +
            "Isın biterse Kırlangıç donar ve son yaktığın fenerden devam edersin.";

        public const string AboutStory =
            "Şehrin yukarısında, yüksek bir sütunun üstünde Mutlu Prens'in heykeli durur. " +
            "Üstü altın yaldızla kaplıdır, gözleri parlak taşlardır, kılıcının kabzasında kocaman bir yakut parlar.\n\n" +
            "Bir sonbahar gecesi, güneye göç eden sürüsünden geri kalmış bir kırlangıç heykelin ayaklarının dibine konar. " +
            "Prens ondan bir iyilik ister...\n\n" +
            "<i>Bu prototip, Oscar Wilde'ın 1888'de yayımlanan \"Mutlu Prens\" masalının yakut bölümünden uyarlanmıştır. " +
            "Gri kutu grafiklerle hazırlanmış bir vize prototipidir.</i>";

        public const string Footer = "↑ ↓  seç     Enter  onayla     Esc  geri";
        public const string Version = "Vize prototipi  ·  v0.2";
    }
}
