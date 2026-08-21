using System.Security.Cryptography;
using System.Text;

namespace InternshipMpbile.Services
{
    /// <summary>
    /// Mesaj metinleri veritabanına düz yazılmaz: AES-256-GCM ile şifrelenir ve
    /// "rastgele sayı + şifreli metin + doğrulama etiketi" tek bir base64 metni
    /// olarak saklanır. Parolalardan farkı, buranın geri döndürülebilir olmasıdır
    /// (bkz. <see cref="ParolaKoruma"/>): mesajın ekranda okunabilmesi gerekir.
    ///
    /// Anahtar iki uygulamada da birebir aynı olmalıdır; arkadaşının kodunda da
    /// aşağıdaki değer bulunmalı, yoksa gelen mesajı çözemez.
    /// </summary>
    public static class MesajKoruma
    {
        private const string AnahtarBase64 = "7MLKmLkPsFdRu7M6Wj671BOUyae3E3lnxI3RPEEPzT8=";

        private const int RastgeleUzunlugu = 12;  // AES-GCM için önerilen nonce boyu
        private const int EtiketUzunlugu = 16;

        private static readonly byte[] Anahtar = Convert.FromBase64String(AnahtarBase64);

        /// <summary>Çözülemeyen bir kayıt ekranda boş görünmesin diye bununla değiştirilir.</summary>
        public static string Cozulemedi { get; } = "🔒";

        public static string Sifrele(string metin)
        {
            var acik = Encoding.UTF8.GetBytes(metin);
            var rastgele = RandomNumberGenerator.GetBytes(RastgeleUzunlugu);
            var sifreli = new byte[acik.Length];
            var etiket = new byte[EtiketUzunlugu];

            using (var aes = new AesGcm(Anahtar, EtiketUzunlugu))
                aes.Encrypt(rastgele, acik, sifreli, etiket);

            var paket = new byte[RastgeleUzunlugu + sifreli.Length + EtiketUzunlugu];

            Buffer.BlockCopy(rastgele, 0, paket, 0, RastgeleUzunlugu);
            Buffer.BlockCopy(sifreli, 0, paket, RastgeleUzunlugu, sifreli.Length);
            Buffer.BlockCopy(etiket, 0, paket, RastgeleUzunlugu + sifreli.Length, EtiketUzunlugu);

            return Convert.ToBase64String(paket);
        }

        /// <summary>
        /// Anahtar tutmuyorsa, kayıt bozuksa ya da metin sonradan değiştirilmişse
        /// GCM doğrulaması patlar; kullanıcıya hata göstermek yerine kilit işareti
        /// döndürülür.
        /// </summary>
        public static string Coz(string? saklanan)
        {
            if (string.IsNullOrWhiteSpace(saklanan))
                return string.Empty;

            try
            {
                var paket = Convert.FromBase64String(saklanan);

                if (paket.Length <= RastgeleUzunlugu + EtiketUzunlugu)
                    return Cozulemedi;

                var sifreliUzunluk = paket.Length - RastgeleUzunlugu - EtiketUzunlugu;

                var rastgele = new byte[RastgeleUzunlugu];
                var sifreli = new byte[sifreliUzunluk];
                var etiket = new byte[EtiketUzunlugu];
                var acik = new byte[sifreliUzunluk];

                Buffer.BlockCopy(paket, 0, rastgele, 0, RastgeleUzunlugu);
                Buffer.BlockCopy(paket, RastgeleUzunlugu, sifreli, 0, sifreliUzunluk);
                Buffer.BlockCopy(paket, RastgeleUzunlugu + sifreliUzunluk, etiket, 0, EtiketUzunlugu);

                using (var aes = new AesGcm(Anahtar, EtiketUzunlugu))
                    aes.Decrypt(rastgele, sifreli, etiket, acik);

                return Encoding.UTF8.GetString(acik);
            }
            catch (Exception e) when (e is FormatException or CryptographicException or ArgumentException)
            {
                return Cozulemedi;
            }
        }

        /// <summary>
        /// Mesajın bütünlük özeti (SHA-256, onaltılık). Şifrelemenin yerine değil,
        /// yanına yazılır: kaydın sonradan elle değiştirilmediğini gösterir.
        /// </summary>
        public static string Ozet(string metin) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(metin))).ToLowerInvariant();
    }
}
