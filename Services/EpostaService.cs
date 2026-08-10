using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace InternshipMpbile.Services
{
    /// <summary>
    /// Geçici parola e-postasını Gmail API üzerinden (HTTPS/443), miremyarici@gmail.com
    /// adresinden gönderir. Kurumsal ağlarda genelde kapatılan ham SMTP (port 587)
    /// yerine, normal web trafiğiyle aynı HTTPS portu kullanıldığı için bakanlık gibi
    /// kısıtlı ağlarda da çalışması beklenir.
    ///
    /// Kullanmadan önce aşağıdaki üç sabiti doldurun; bunlar bir kerelik yapılan
    /// kurulumdan elde edilir:
    ///   1) console.cloud.google.com'da miremyarici@gmail.com ile bir proje açın.
    ///   2) "APIs & Services > Library" içinden Gmail API'yi etkinleştirin.
    ///   3) "APIs & Services > OAuth consent screen": External, Test users kısmına
    ///      miremyarici@gmail.com'u ekleyin.
    ///   4) "APIs & Services > Credentials > Create Credentials > OAuth client ID",
    ///      Application type: Desktop app. Çıkan ClientId / ClientSecret'ı aşağıya yazın.
    ///   5) scratch klasöründeki GmailTokenAl aracını çalıştırıp tarayıcıdan izin verin;
    ///      araç bir RefreshToken üretir, onu da aşağıya yazın.
    /// </summary>
    public static class EpostaService
    {
        private const string ClientId = "689711944994-utanm0doc4utf733omlmdgvu0423eods.apps.googleusercontent.com";
        private const string ClientSecret = "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";
        private const string RefreshToken = "1//03Ww7qfuHOXRwCgYIARAAGAMSNwF-L9Ir2Kmqy5iry6CUnrgCR6XL2cdyaMrsCP6UB9i3uYji-Qwd0nmOupiK4T5roLy_yQ4uZbg";

        private const string GondericiAdresi = "miremyarici@gmail.com";
        private const string GondericiAdi = "GSB Başvuru Sistemi";

        private static readonly HttpClient Http = new();

        /// <summary>Üç sabit doldurulmadan gönderim denenmez.</summary>
        public static bool Yapilandirildi =>
            !ClientId.StartsWith("DOLDURUN", StringComparison.Ordinal) &&
            !ClientSecret.StartsWith("DOLDURUN", StringComparison.Ordinal) &&
            !RefreshToken.StartsWith("DOLDURUN", StringComparison.Ordinal);

        public static async Task GeciciParolaGonderAsync(string alici, string adSoyad, string geciciParola)
        {
            if (!Yapilandirildi)
                throw new InvalidOperationException(
                    "E-posta gönderimi yapılandırılmamış. Services/EpostaService.cs dosyasındaki Gmail API bilgilerini doldurun.");

            var erisimJetonu = await ErisimJetonuAlAsync();
            var mime = MimeOlustur(alici, adSoyad, geciciParola);

            using var istek = new HttpRequestMessage(HttpMethod.Post,
                "https://gmail.googleapis.com/gmail/v1/users/me/messages/send")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { raw = Base64UrlEncode(mime) }),
                    Encoding.UTF8, "application/json")
            };
            istek.Headers.Authorization = new AuthenticationHeaderValue("Bearer", erisimJetonu);

            using var yanit = await Http.SendAsync(istek);

            if (!yanit.IsSuccessStatusCode)
            {
                var detay = await yanit.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Gmail API mail gönderemedi ({(int)yanit.StatusCode}): {detay}");
            }
        }

        /// <summary>RefreshToken hiç süresi dolmayan bir izindir; her gönderimde ondan kısa ömürlü bir erişim jetonu türetilir.</summary>
        private static async Task<string> ErisimJetonuAlAsync()
        {
            var istek = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = ClientId,
                ["client_secret"] = ClientSecret,
                ["refresh_token"] = RefreshToken,
                ["grant_type"] = "refresh_token"
            });

            using var yanit = await Http.PostAsync("https://oauth2.googleapis.com/token", istek);
            var govde = await yanit.Content.ReadAsStringAsync();

            if (!yanit.IsSuccessStatusCode)
                throw new InvalidOperationException($"Erişim jetonu alınamadı: {govde}");

            using var belge = JsonDocument.Parse(govde);
            return belge.RootElement.GetProperty("access_token").GetString()!;
        }

        private static string MimeOlustur(string alici, string adSoyad, string geciciParola)
        {
            var govde =
                $"Sayın {adSoyad},{Environment.NewLine}{Environment.NewLine}" +
                $"Geçici parolanız: {geciciParola}{Environment.NewLine}{Environment.NewLine}" +
                "Bu parola ile giriş yaptığınızda sizden yeni bir parola belirlemeniz istenecektir." +
                $"{Environment.NewLine}{Environment.NewLine}GSB Başvuru Sistemi";

            // Konu satırı Türkçe karakter içerdiği için RFC 2047 (encoded-word) ile kodlanır.
            var konu = "=?UTF-8?B?" + Convert.ToBase64String(Encoding.UTF8.GetBytes("Geçici Parolanız")) + "?=";

            return
                $"From: {GondericiAdi} <{GondericiAdresi}>\r\n" +
                $"To: {alici}\r\n" +
                $"Subject: {konu}\r\n" +
                "MIME-Version: 1.0\r\n" +
                "Content-Type: text/plain; charset=UTF-8\r\n" +
                "\r\n" +
                govde;
        }

        /// <summary>Gmail API "raw" alanı standart base64 değil, URL-güvenli (base64url) bekler.</summary>
        private static string Base64UrlEncode(string metin) =>
            Convert.ToBase64String(Encoding.UTF8.GetBytes(metin))
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");
    }
}
