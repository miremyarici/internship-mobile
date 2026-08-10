using System.Security.Cryptography;
using System.Text.RegularExpressions;
using InternshipMpbile.Models;
using Microsoft.Data.SqlClient;

namespace InternshipMpbile.Services
{
    /// <summary>Kullanici tablosu üzerindeki işlemler: kayıt, listeleme, giriş ve parola.</summary>
    public static partial class KullaniciService
    {
        /// <summary>Kullanıcı Ekleme ekranındaki rol listesi; tablodaki CHECK ile aynı olmalıdır.</summary>
        public static readonly string[] Roller = { "Kullanıcı", "Personel", "Admin" };

        private const string SelectQuery = @"
            SELECT Id, Isim, Soyisim, Eposta, Rol, GeciciSifre
            FROM Kullanici
            WHERE Aktif = 1
            ORDER BY Id DESC";

        // Sifre girilmez: yeni kullanıcı parolasını "Şifre almak istiyorum"
        // akışıyla alır. Rol dışındaki varsayılanlar tabloda tanımlıdır.
        private const string InsertQuery = @"
            INSERT INTO Kullanici (Isim, Soyisim, Eposta, Rol)
            VALUES (@Isim, @Soyisim, @Eposta, @Rol)";

        private const string EpostaVarMiQuery = @"
            SELECT COUNT(*) FROM Kullanici
            WHERE Eposta = @Eposta AND Aktif = 1";

        private const string SoftDeleteQuery = @"
            UPDATE Kullanici SET Aktif = 0 WHERE Id = @Id";

        // Parola özeti veritabanında karşılaştırılamaz (her kaydın tuzu farklıdır),
        // bu yüzden kayıt e-postayla çekilip doğrulama uygulamada yapılır.
        private const string GirisQuery = @"
            SELECT Id, Isim, Soyisim, Eposta, Rol, GeciciSifre, Sifre
            FROM Kullanici
            WHERE Eposta = @Eposta AND Aktif = 1";

        private const string SifreOzetiQuery = @"
            SELECT Sifre FROM Kullanici WHERE Id = @Id AND Aktif = 1";

        private const string GeciciSifreQuery = @"
            UPDATE Kullanici SET Sifre = @Sifre, GeciciSifre = 1
            WHERE Eposta = @Eposta AND Aktif = 1";

        private const string SifreDegistirQuery = @"
            UPDATE Kullanici SET Sifre = @YeniSifre, GeciciSifre = 0
            WHERE Id = @Id AND Aktif = 1";

        /// <summary>Yalnızca silinmemiş kullanıcılar, en son eklenen en üstte.</summary>
        public static async Task<List<Kullanici>> ListeleAsync()
        {
            var kullanicilar = new List<Kullanici>();

            using var connection = await Database.AcikBaglantiAsync();
            using var command = new SqlCommand(SelectQuery, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                kullanicilar.Add(Oku(reader));

            return kullanicilar;
        }

        public static async Task KaydetAsync(Kullanici kullanici)
        {
            using var connection = await Database.AcikBaglantiAsync();
            using var command = new SqlCommand(InsertQuery, connection);

            command.Parameters.AddWithValue("@Isim", kullanici.Isim);
            command.Parameters.AddWithValue("@Soyisim", kullanici.Soyisim);
            command.Parameters.AddWithValue("@Eposta", kullanici.Eposta);
            command.Parameters.AddWithValue("@Rol", kullanici.Rol);

            await command.ExecuteNonQueryAsync();
        }

        /// <summary>Aynı e-posta ile duran (silinmemiş) bir kullanıcı var mı?</summary>
        public static async Task<bool> EpostaVarMiAsync(string eposta)
        {
            using var connection = await Database.AcikBaglantiAsync();
            using var command = new SqlCommand(EpostaVarMiQuery, connection);

            command.Parameters.AddWithValue("@Eposta", eposta);

            return (int)(await command.ExecuteScalarAsync() ?? 0) > 0;
        }

        /// <summary>Soft delete: kayıt tabloda kalır, Aktif sütunu 0 yapılır.</summary>
        public static async Task SilAsync(int id)
        {
            using var connection = await Database.AcikBaglantiAsync();
            using var command = new SqlCommand(SoftDeleteQuery, connection);

            command.Parameters.AddWithValue("@Id", id);

            await command.ExecuteNonQueryAsync();
        }

        /// <summary>Bilgiler doğruysa kullanıcıyı, değilse null döndürür.</summary>
        public static async Task<Kullanici?> GirisYapAsync(string eposta, string sifre)
        {
            using var connection = await Database.AcikBaglantiAsync();
            using var command = new SqlCommand(GirisQuery, connection);

            command.Parameters.AddWithValue("@Eposta", eposta);

            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            var saklananOzet = reader.IsDBNull(6) ? null : reader.GetString(6);

            return ParolaKoruma.Dogrula(sifre, saklananOzet) ? Oku(reader) : null;
        }

        /// <summary>
        /// Kullanıcıya 6 haneli geçici parola üretip e-posta ile gönderir ve
        /// ancak gönderim başarılıysa özetini veritabanına yazar; böylece
        /// ulaşmayan bir parola yüzünden kullanıcı hesabından olmaz.
        /// </summary>
        public static async Task GeciciSifreGonderAsync(string eposta, string adSoyad)
        {
            var geciciSifre = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

            await EpostaService.GeciciParolaGonderAsync(eposta, adSoyad, geciciSifre);

            using var connection = await Database.AcikBaglantiAsync();
            using var command = new SqlCommand(GeciciSifreQuery, connection);

            command.Parameters.AddWithValue("@Sifre", ParolaKoruma.Hashle(geciciSifre));
            command.Parameters.AddWithValue("@Eposta", eposta);

            await command.ExecuteNonQueryAsync();
        }

        /// <summary>Geçici parola bayrağını da kaldırır. Eski parola yanlışsa false döner.</summary>
        public static async Task<bool> SifreDegistirAsync(int id, string eskiSifre, string yeniSifre)
        {
            using var connection = await Database.AcikBaglantiAsync();

            string? saklananOzet;

            using (var okuCommand = new SqlCommand(SifreOzetiQuery, connection))
            {
                okuCommand.Parameters.AddWithValue("@Id", id);
                saklananOzet = await okuCommand.ExecuteScalarAsync() as string;
            }

            if (!ParolaKoruma.Dogrula(eskiSifre, saklananOzet))
                return false;

            using var guncelleCommand = new SqlCommand(SifreDegistirQuery, connection);

            guncelleCommand.Parameters.AddWithValue("@Id", id);
            guncelleCommand.Parameters.AddWithValue("@YeniSifre", ParolaKoruma.Hashle(yeniSifre));

            return await guncelleCommand.ExecuteNonQueryAsync() > 0;
        }

        /// <summary>Kullanıcının adını e-postasından bulur; kayıt yoksa null döner.</summary>
        public static async Task<string?> AdSoyadGetirAsync(string eposta)
        {
            using var connection = await Database.AcikBaglantiAsync();
            using var command = new SqlCommand(
                "SELECT Isim + ' ' + Soyisim FROM Kullanici WHERE Eposta = @Eposta AND Aktif = 1",
                connection);

            command.Parameters.AddWithValue("@Eposta", eposta);

            return await command.ExecuteScalarAsync() as string;
        }

        public static bool EpostaGecerliMi(string eposta) => EpostaKalibi().IsMatch(eposta);

        [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
        private static partial Regex EpostaKalibi();

        private static Kullanici Oku(SqlDataReader reader) => new()
        {
            Id = reader.GetInt32(0),
            Isim = reader.GetString(1),
            Soyisim = reader.GetString(2),
            Eposta = reader.GetString(3),
            Rol = reader.GetString(4),
            GeciciSifre = reader.GetBoolean(5)
        };
    }
}
