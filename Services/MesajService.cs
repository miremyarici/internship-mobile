using InternshipMpbile.Models;
using Microsoft.Data.SqlClient;

namespace InternshipMpbile.Services
{
    /// <summary>
    /// Mesaj ve MesajKullanici tabloları üzerindeki işlemler.
    ///
    /// Metinler tabloya <see cref="MesajKoruma"/> ile şifreli yazılır; gönderen,
    /// alıcı, zaman ve durum sütunları düz kalır çünkü sıralama, okunmamış sayacı
    /// ve sohbet gruplaması bu sütunlarla yapılır.
    ///
    /// Tablolar hangi SQL Server örneğindeyse mesajlaşma orada buluşur: aynı
    /// örneği iki uygulama da gösterirse iki uygulama haberleşir. Bunun için
    /// yalnızca <see cref="Database"/> içindeki adres değişir, buradaki kod değişmez.
    /// </summary>
    public static class MesajService
    {
        // Sohbet listesi: her kişiyle olan en son mesaj + o kişiden gelen
        // okunmamış sayısı. ROW_NUMBER ile kişi başına tek satır bırakılır.
        private const string SohbetlerQuery = @"
            WITH Sohbet AS (
                SELECT
                    CASE WHEN GonderenEposta = @Ben THEN AliciEposta ELSE GonderenEposta END AS Karsi,
                    Id, GonderenEposta, MetinSifreli, GonderimZamani, Durum
                FROM Mesaj
                WHERE GonderenEposta = @Ben OR AliciEposta = @Ben
            ),
            Son AS (
                SELECT *, ROW_NUMBER() OVER (PARTITION BY Karsi ORDER BY Id DESC) AS Sira
                FROM Sohbet
            )
            SELECT s.Karsi,
                   ISNULL(k.Isim + ' ' + k.Soyisim, s.Karsi) AS AdSoyad,
                   s.MetinSifreli,
                   s.GonderimZamani,
                   s.Durum,
                   CASE WHEN s.GonderenEposta = @Ben THEN 1 ELSE 0 END AS Benden,
                   (SELECT COUNT(*) FROM Mesaj m
                     WHERE m.AliciEposta = @Ben AND m.GonderenEposta = s.Karsi
                       AND m.Durum < 2) AS Okunmamis
            FROM Son s
            LEFT JOIN MesajKullanici k ON k.Eposta = s.Karsi
            WHERE s.Sira = 1
            ORDER BY s.GonderimZamani DESC";

        private const string SohbetQuery = @"
            SELECT Id, IstemciAnahtari, GonderenEposta, AliciEposta,
                   MetinSifreli, GonderimZamani, Durum
            FROM Mesaj
            WHERE (GonderenEposta = @Ben AND AliciEposta = @Karsi)
               OR (GonderenEposta = @Karsi AND AliciEposta = @Ben)
            ORDER BY Id";

        private const string GonderQuery = @"
            INSERT INTO Mesaj
                (IstemciAnahtari, GonderenEposta, AliciEposta, MetinSifreli, MetinOzeti, GonderimZamani, Durum)
            VALUES
                (@Anahtar, @Gonderen, @Alici, @Sifreli, @Ozet, @Zaman, 0)";

        // Karşı taraftan gelmiş, henüz okunmamış mesajlar okundu yapılır.
        private const string OkunduQuery = @"
            UPDATE Mesaj SET Durum = 2
            WHERE AliciEposta = @Ben AND GonderenEposta = @Karsi AND Durum < 2";

        // Bana gelen mesajlar en az ""iletildi"" olur; uygulama listeyi her
        // tazelediğinde karşı tarafın tiki tek tikten çift griye döner.
        private const string IletildiQuery = @"
            UPDATE Mesaj SET Durum = 1
            WHERE AliciEposta = @Ben AND Durum = 0";

        private const string DizinQuery = @"
            SELECT Eposta, Isim + ' ' + Soyisim AS AdSoyad
            FROM MesajKullanici
            WHERE Eposta <> @Ben
              AND (@Ara = '' OR Isim + ' ' + Soyisim LIKE @Kalip OR Eposta LIKE @Kalip)
            ORDER BY Isim, Soyisim";

        // Giriş yapan kullanıcı dizine yazılır; böylece karşı uygulama onu bulabilir.
        private const string DizineYazQuery = @"
            MERGE MesajKullanici AS hedef
            USING (SELECT @Eposta AS Eposta) AS kaynak ON hedef.Eposta = kaynak.Eposta
            WHEN MATCHED THEN
                UPDATE SET Isim = @Isim, Soyisim = @Soyisim, SonGorulme = SYSUTCDATETIME()
            WHEN NOT MATCHED THEN
                INSERT (Eposta, Isim, Soyisim, Uygulama, SonGorulme)
                VALUES (@Eposta, @Isim, @Soyisim, @Uygulama, SYSUTCDATETIME());";

        /// <summary>Bu uygulamanın dizindeki adı; karşı taraf kaydın kimden geldiğini böyle ayırır.</summary>
        private const string UygulamaAdi = "irem";

        /// <summary>İki sistem arasında ortak anahtar e-postadır; her yerde aynı biçimde yazılır.</summary>
        public static string Normalize(string eposta) => eposta.Trim().ToLowerInvariant();

        public static async Task<List<SohbetOzeti>> SohbetleriListeleAsync(string benimEpostam)
        {
            var ben = Normalize(benimEpostam);
            var sohbetler = new List<SohbetOzeti>();

            using var connection = await Database.AcikBaglantiAsync();

            // Önce bana gelenler "iletildi" olur, sonra liste okunur.
            using (var iletildi = new SqlCommand(IletildiQuery, connection))
            {
                iletildi.Parameters.AddWithValue("@Ben", ben);
                await iletildi.ExecuteNonQueryAsync();
            }

            using var command = new SqlCommand(SohbetlerQuery, connection);
            command.Parameters.AddWithValue("@Ben", ben);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                sohbetler.Add(new SohbetOzeti
                {
                    Eposta = reader.GetString(0),
                    AdSoyad = reader.GetString(1),
                    SonMesaj = MesajKoruma.Coz(reader.GetString(2)),
                    SonZaman = reader.GetDateTime(3),
                    SonDurum = reader.GetByte(4),
                    SonMesajBenden = reader.GetInt32(5) == 1,
                    OkunmamisSayisi = reader.GetInt32(6)
                });
            }

            return sohbetler;
        }

        public static async Task<List<Mesaj>> SohbetiGetirAsync(string benimEpostam, string karsiEposta)
        {
            var ben = Normalize(benimEpostam);
            var mesajlar = new List<Mesaj>();

            using var connection = await Database.AcikBaglantiAsync();
            using var command = new SqlCommand(SohbetQuery, connection);

            command.Parameters.AddWithValue("@Ben", ben);
            command.Parameters.AddWithValue("@Karsi", Normalize(karsiEposta));

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var gonderen = reader.GetString(2);

                mesajlar.Add(new Mesaj
                {
                    Id = reader.GetInt32(0),
                    IstemciAnahtari = reader.GetGuid(1),
                    GonderenEposta = gonderen,
                    AliciEposta = reader.GetString(3),
                    Metin = MesajKoruma.Coz(reader.GetString(4)),
                    GonderimZamani = reader.GetDateTime(5),
                    Durum = reader.GetByte(6),
                    Benden = gonderen == ben
                });
            }

            return mesajlar;
        }

        public static async Task GonderAsync(string benimEpostam, string aliciEposta, string metin)
        {
            using var connection = await Database.AcikBaglantiAsync();
            using var command = new SqlCommand(GonderQuery, connection);

            command.Parameters.AddWithValue("@Anahtar", Guid.NewGuid());
            command.Parameters.AddWithValue("@Gonderen", Normalize(benimEpostam));
            command.Parameters.AddWithValue("@Alici", Normalize(aliciEposta));
            command.Parameters.AddWithValue("@Sifreli", MesajKoruma.Sifrele(metin));
            command.Parameters.AddWithValue("@Ozet", MesajKoruma.Ozet(metin));
            command.Parameters.AddWithValue("@Zaman", DateTime.UtcNow);

            await command.ExecuteNonQueryAsync();
        }

        /// <summary>Sohbet ekranı açıldığında karşı taraftan gelenleri okundu yapar.</summary>
        public static async Task OkunduIsaretleAsync(string benimEpostam, string karsiEposta)
        {
            using var connection = await Database.AcikBaglantiAsync();
            using var command = new SqlCommand(OkunduQuery, connection);

            command.Parameters.AddWithValue("@Ben", Normalize(benimEpostam));
            command.Parameters.AddWithValue("@Karsi", Normalize(karsiEposta));

            await command.ExecuteNonQueryAsync();
        }

        /// <summary>Yeni sohbet başlatmak için dizinde arama; boş metin herkesi getirir.</summary>
        public static async Task<List<Kisi>> KisileriAraAsync(string benimEpostam, string arama)
        {
            var kisiler = new List<Kisi>();
            var temiz = arama.Trim();

            using var connection = await Database.AcikBaglantiAsync();
            using var command = new SqlCommand(DizinQuery, connection);

            command.Parameters.AddWithValue("@Ben", Normalize(benimEpostam));
            command.Parameters.AddWithValue("@Ara", temiz);
            command.Parameters.AddWithValue("@Kalip", $"%{temiz}%");

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                kisiler.Add(new Kisi { Eposta = reader.GetString(0), AdSoyad = reader.GetString(1) });

            return kisiler;
        }

        /// <summary>
        /// Giriş yapan kullanıcıyı mesajlaşma dizinine yazar. Karşı uygulamanın
        /// seni bulabilmesi buna bağlıdır.
        /// </summary>
        public static async Task DizineYazAsync(Kullanici kullanici)
        {
            using var connection = await Database.AcikBaglantiAsync();
            using var command = new SqlCommand(DizineYazQuery, connection);

            command.Parameters.AddWithValue("@Eposta", Normalize(kullanici.Eposta));
            command.Parameters.AddWithValue("@Isim", kullanici.Isim);
            command.Parameters.AddWithValue("@Soyisim", kullanici.Soyisim);
            command.Parameters.AddWithValue("@Uygulama", UygulamaAdi);

            await command.ExecuteNonQueryAsync();
        }
    }
}
