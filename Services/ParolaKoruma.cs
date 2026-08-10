using System.Security.Cryptography;

namespace InternshipMpbile.Services
{
    /// <summary>
    /// Parolalar veritabanında düz metin olarak tutulmaz. Her parola için
    /// rastgele bir tuz üretilir, PBKDF2 ile bir özet türetilir ve sonuç
    /// "yineleme.tuz.özet" biçiminde tek bir metin olarak saklanır.
    /// Özetten parolaya dönüş yoktur; giriş sırasında girilen parola aynı tuzla
    /// yeniden özetlenip saklanan özetle karşılaştırılır.
    /// </summary>
    public static class ParolaKoruma
    {
        private const int Yineleme = 100_000;
        private const int TuzUzunlugu = 16;
        private const int OzetUzunlugu = 32;

        private static readonly HashAlgorithmName Algoritma = HashAlgorithmName.SHA256;

        public static string Hashle(string parola)
        {
            var tuz = RandomNumberGenerator.GetBytes(TuzUzunlugu);
            var ozet = Turet(parola, tuz, Yineleme, OzetUzunlugu);

            return $"{Yineleme}.{Convert.ToBase64String(tuz)}.{Convert.ToBase64String(ozet)}";
        }

        /// <summary>
        /// Henüz parola almamış (Sifre sütunu NULL olan) kullanıcı ile bozuk
        /// biçimdeki kayıtlar için de false döner.
        /// </summary>
        public static bool Dogrula(string parola, string? saklanan)
        {
            if (string.IsNullOrWhiteSpace(saklanan))
                return false;

            var parcalar = saklanan.Split('.');

            if (parcalar.Length != 3 || !int.TryParse(parcalar[0], out var yineleme) || yineleme <= 0)
                return false;

            byte[] tuz;
            byte[] beklenenOzet;

            try
            {
                tuz = Convert.FromBase64String(parcalar[1]);
                beklenenOzet = Convert.FromBase64String(parcalar[2]);
            }
            catch (FormatException)
            {
                return false;
            }

            if (tuz.Length == 0 || beklenenOzet.Length == 0)
                return false;

            var ozet = Turet(parola, tuz, yineleme, beklenenOzet.Length);

            // Karşılaştırma süresi parolaya göre değişmesin diye sabit süreli.
            return CryptographicOperations.FixedTimeEquals(ozet, beklenenOzet);
        }

        private static byte[] Turet(string parola, byte[] tuz, int yineleme, int uzunluk) =>
            Rfc2898DeriveBytes.Pbkdf2(parola, tuz, yineleme, Algoritma, uzunluk);
    }
}
