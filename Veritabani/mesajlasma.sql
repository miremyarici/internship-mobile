-- Mesajlaşma tabloları. SSMS'te InternshipMobile veritabanını seçip çalıştırın.
--
-- İki uygulama (senin ve arkadaşının) bu iki tabloyu AYNI SQL Server örneğinde
-- paylaşır; buluşma noktası burasıdır. Host olan makinede bir kez çalıştırmak
-- yeterlidir, diğer uygulama sadece bağlantı adresini bu makineye çevirir.

IF OBJECT_ID('dbo.Mesaj', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Mesaj
    (
        Id              INT IDENTITY(1,1)   NOT NULL,
        -- İstemcide üretilir: ağ koparsa yeniden gönderim çift kayıt oluşturmaz.
        IstemciAnahtari UNIQUEIDENTIFIER    NOT NULL,
        -- İki sistemin ortak kimliği e-postadır, Id değil (Id'ler örtüşmez).
        GonderenEposta  NVARCHAR(200)       NOT NULL,
        AliciEposta     NVARCHAR(200)       NOT NULL,
        -- AES-256-GCM ile şifrelenmiş metin: base64(rastgele + şifreli + etiket).
        MetinSifreli    NVARCHAR(MAX)       NOT NULL,
        -- Bütünlük özeti (SHA-256): kaydın elle değiştirilmediğini gösterir.
        MetinOzeti      CHAR(64)            NOT NULL,
        -- Her zaman UTC; ekranda yerel saate çevrilir.
        GonderimZamani  DATETIME2(3)        NOT NULL CONSTRAINT DF_Mesaj_Zaman DEFAULT SYSUTCDATETIME(),
        -- 0 Gönderildi, 1 İletildi, 2 Okundu
        Durum           TINYINT             NOT NULL CONSTRAINT DF_Mesaj_Durum DEFAULT 0,

        CONSTRAINT PK_Mesaj PRIMARY KEY (Id),
        CONSTRAINT UQ_Mesaj_IstemciAnahtari UNIQUE (IstemciAnahtari),
        CONSTRAINT CK_Mesaj_Durum CHECK (Durum IN (0, 1, 2))
    );

    CREATE INDEX IX_Mesaj_Alici  ON dbo.Mesaj (AliciEposta, Durum);
    CREATE INDEX IX_Mesaj_Sohbet ON dbo.Mesaj (GonderenEposta, AliciEposta, Id);
END;
GO

IF OBJECT_ID('dbo.MesajKullanici', 'U') IS NULL
BEGIN
    -- Mesajlaşma dizini: "arkadaşını bul" araması bu tabloda çalışır.
    -- Her uygulama giriş yapan kendi kullanıcısını buraya yazar.
    CREATE TABLE dbo.MesajKullanici
    (
        Eposta      NVARCHAR(200)   NOT NULL,
        Isim        NVARCHAR(100)   NOT NULL,
        Soyisim     NVARCHAR(100)   NOT NULL,
        -- Kaydın hangi uygulamadan geldiği ('irem' / 'arkadas').
        Uygulama    NVARCHAR(50)    NOT NULL,
        SonGorulme  DATETIME2(3)    NOT NULL CONSTRAINT DF_MesajKullanici_SonGorulme DEFAULT SYSUTCDATETIME(),

        CONSTRAINT PK_MesajKullanici PRIMARY KEY (Eposta)
    );
END;
GO

-- Kontrol: mesajların şifreli durduğunu görmek için
-- SELECT Id, GonderenEposta, AliciEposta, MetinSifreli, MetinOzeti, Durum FROM dbo.Mesaj ORDER BY Id DESC;
