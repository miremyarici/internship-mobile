using InternshipMpbile.Services;

namespace InternshipMpbile.Pages
{
    public partial class GirisPage : ContentPage
    {
        private const string EkranAdi = "Giriş Yap";

        public GirisPage()
        {
            InitializeComponent();
        }

        private async void OnGirisYapClicked(object? sender, EventArgs e)
        {
            var eposta = EpostaAlani.Deger?.Trim() ?? string.Empty;
            var parola = ParolaAlani.Deger ?? string.Empty;

            if (eposta.Length == 0 || parola.Length == 0)
            {
                await DisplayAlert(EkranAdi, "Lütfen e-posta ve parolanızı giriniz.", "Tamam");
                return;
            }

            GirisButonu.IsEnabled = false;

            try
            {
                var kullanici = await KullaniciService.GirisYapAsync(eposta, parola);

                if (kullanici is null)
                {
                    await DisplayAlert(EkranAdi, "E-posta veya parola hatalı.", "Tamam");
                    return;
                }

                Oturum.Baslat(kullanici);
                ParolaAlani.Temizle();

                // Giriş başarılı: pencerenin kök sayfası menülü ana ekranla değişir.
                App.AnaEkranaGec();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Giriş sırasında bir hata oluştu: {ex.Message}", "Tamam");
            }
            finally
            {
                GirisButonu.IsEnabled = true;
            }
        }

        private void OnSifreAlTapped(object? sender, TappedEventArgs e)
        {
            // Giriş ekranına yazılmış adres varsa pop-up'a hazır gelsin.
            SifreAlmaEpostaAlani.Deger = EpostaAlani.Deger;
            SifreAlmaOverlay.IsVisible = true;
        }

        private async void OnSifreGonderClicked(object? sender, EventArgs e)
        {
            var eposta = SifreAlmaEpostaAlani.Deger?.Trim() ?? string.Empty;

            if (!KullaniciService.EpostaGecerliMi(eposta))
            {
                await DisplayAlert("Şifre Alma", "Lütfen geçerli bir e-posta adresi giriniz.", "Tamam");
                return;
            }

            GondericiyiAyarla(true);

            try
            {
                var adSoyad = await KullaniciService.AdSoyadGetirAsync(eposta);

                if (adSoyad is null)
                {
                    await DisplayAlert("Şifre Alma", "Bu e-posta ile kayıtlı bir kullanıcı bulunamadı.", "Tamam");
                    return;
                }

                await KullaniciService.GeciciSifreGonderAsync(eposta, adSoyad);

                SifreAlmayiKapat();
                await DisplayAlert("Şifre Alma",
                    "Geçici parolanız e-posta adresinize gönderildi. Giriş yaptıktan sonra yeni parolanızı belirleyebilirsiniz.",
                    "Tamam");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Geçici parola gönderilemedi: {ex.Message}", "Tamam");
            }
            finally
            {
                GondericiyiAyarla(false);
            }
        }

        private void OnSifreAlmaVazgecClicked(object? sender, EventArgs e) => SifreAlmayiKapat();

        private void SifreAlmayiKapat()
        {
            SifreAlmaOverlay.IsVisible = false;
            SifreAlmaEpostaAlani.Temizle();
        }

        private void GondericiyiAyarla(bool gonderiliyor)
        {
            GonderiliyorIndicator.IsVisible = gonderiliyor;
            GonderiliyorIndicator.IsRunning = gonderiliyor;
            GonderButonu.IsEnabled = !gonderiliyor;
        }
    }
}
