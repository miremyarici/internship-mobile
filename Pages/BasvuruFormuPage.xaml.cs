using InternshipMpbile.Models;
using InternshipMpbile.Services;

namespace InternshipMpbile.Pages
{
    public partial class BasvuruFormuPage : ContentPage
    {
        private const string EkranAdi = "Başvuru Formu";
        private const string ParolaEkraniAdi = "Parolayı Değiştir";
        private const int EnAzParolaUzunlugu = 6;

        public BasvuruFormuPage()
        {
            InitializeComponent();
            SecenekleriYukle();
        }

        // Geçici parolayla giriş yapan kullanıcıyı, formu kullanmadan önce
        // parola değiştirme pop-up'ı karşılar.
        protected override void OnAppearing()
        {
            base.OnAppearing();
            ParolaDegistirOverlay.IsVisible = Oturum.AktifKullanici?.GeciciSifre == true;
        }

        private void SecenekleriYukle()
        {
            BasvuranBirimAlani.Secenekler = FormSecenekleri.BasvuranBirimler;
            BasvuruYapilanProjeAlani.Secenekler = FormSecenekleri.BasvuruYapilanProjeler;
            BasvuruYapilanTurAlani.Secenekler = FormSecenekleri.BasvuruYapilanTurler;
            KatilimciTuruAlani.Secenekler = FormSecenekleri.KatilimciTurleri;
            BasvuruDonemiAlani.Secenekler = FormSecenekleri.BasvuruDonemleri;
            BasvuruDurumuAlani.Secenekler = FormSecenekleri.BasvuruDurumlari;
        }

        private async void OnKaydetClicked(object? sender, EventArgs e)
        {
            if (!ZorunluAlanlarDolu())
            {
                await DisplayAlert(EkranAdi, "Lütfen tüm zorunlu alanları doldurun.", "Tamam");
                return;
            }

            try
            {
                await BasvuruService.KaydetAsync(FormdanBasvuruOlustur());
                await DisplayAlert(EkranAdi, "Başvuru başarıyla kaydedildi.", "Tamam");
                FormuTemizle();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Kayıt sırasında bir hata oluştu: {ex.Message}", "Tamam");
            }
        }

        // Hibe tutarı dışındaki tüm alanlar zorunludur.
        private bool ZorunluAlanlarDolu() =>
            !ProjeAdiAlani.Bos &&
            !BasvuranBirimAlani.Bos &&
            !BasvuruYapilanProjeAlani.Bos &&
            !BasvuruYapilanTurAlani.Bos &&
            !KatilimciTuruAlani.Bos &&
            !BasvuruDonemiAlani.Bos &&
            !BasvuruDurumuAlani.Bos;

        private Basvuru FormdanBasvuruOlustur() => new()
        {
            ProjeAdi = ProjeAdiAlani.Deger!,
            BasvuranBirim = BasvuranBirimAlani.SecilenDeger!,
            BasvuruYapilanProje = BasvuruYapilanProjeAlani.SecilenDeger!,
            BasvuruYapilanTur = BasvuruYapilanTurAlani.SecilenDeger!,
            KatilimciTuru = KatilimciTuruAlani.SecilenDeger!,
            BasvuruDonemi = BasvuruDonemiAlani.SecilenDeger!,
            BasvuruTarihi = BasvuruTarihiAlani.Tarih,
            BasvuruDurumu = BasvuruDurumuAlani.SecilenDeger!,
            DurumTarihi = DurumTarihiAlani.Tarih,
            HibeTutari = decimal.TryParse(HibeTutariAlani.Deger, out var hibeTutari) ? hibeTutari : null
        };

        // Kayıt sonrası alanları boşaltarak formu yeni başvuruya hazırlar.
        private void FormuTemizle()
        {
            ProjeAdiAlani.Temizle();
            HibeTutariAlani.Temizle();

            BasvuranBirimAlani.Temizle();
            BasvuruYapilanProjeAlani.Temizle();
            BasvuruYapilanTurAlani.Temizle();
            KatilimciTuruAlani.Temizle();
            BasvuruDonemiAlani.Temizle();
            BasvuruDurumuAlani.Temizle();

            BasvuruTarihiAlani.Temizle();
            DurumTarihiAlani.Temizle();
        }

        // ==================== Parola değiştirme ====================

        private async void OnParolaKaydetClicked(object? sender, EventArgs e)
        {
            if (Oturum.AktifKullanici is not { } kullanici)
                return;

            var eskiParola = EskiParolaAlani.Deger ?? string.Empty;
            var yeniParola = YeniParolaAlani.Deger ?? string.Empty;
            var yeniParolaTekrar = YeniParolaTekrarAlani.Deger ?? string.Empty;

            if (eskiParola.Length == 0 || yeniParola.Length == 0 || yeniParolaTekrar.Length == 0)
            {
                await DisplayAlert(ParolaEkraniAdi, "Lütfen tüm alanları doldurun.", "Tamam");
                return;
            }

            if (yeniParola != yeniParolaTekrar)
            {
                await DisplayAlert(ParolaEkraniAdi, "Yeni parolalar birbiriyle aynı değil.", "Tamam");
                return;
            }

            if (yeniParola.Length < EnAzParolaUzunlugu)
            {
                await DisplayAlert(ParolaEkraniAdi,
                    $"Yeni parola en az {EnAzParolaUzunlugu} karakter olmalıdır.", "Tamam");
                return;
            }

            if (yeniParola == eskiParola)
            {
                await DisplayAlert(ParolaEkraniAdi, "Yeni parola geçici parolanızdan farklı olmalıdır.", "Tamam");
                return;
            }

            ParolaKaydetButonu.IsEnabled = false;

            try
            {
                if (!await KullaniciService.SifreDegistirAsync(kullanici.Id, eskiParola, yeniParola))
                {
                    await DisplayAlert(ParolaEkraniAdi, "Eski parolanız hatalı.", "Tamam");
                    return;
                }

                kullanici.GeciciSifre = false;
                ParolaDegistirOverlay.IsVisible = false;
                ParolaAlanlariniTemizle();

                await BildirimGosterAsync("Parolanız başarıyla değiştirildi.");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Parola değiştirilirken bir hata oluştu: {ex.Message}", "Tamam");
            }
            finally
            {
                ParolaKaydetButonu.IsEnabled = true;
            }
        }

        private void ParolaAlanlariniTemizle()
        {
            EskiParolaAlani.Temizle();
            YeniParolaAlani.Temizle();
            YeniParolaTekrarAlani.Temizle();
        }

        /// <summary>Sağ üstte beliren, 1,5 saniye ekranda kalan yeşil bildirim.</summary>
        private async Task BildirimGosterAsync(string mesaj)
        {
            BildirimLabel.Text = mesaj;
            BildirimToast.Opacity = 0;
            BildirimToast.IsVisible = true;

            await BildirimToast.FadeTo(1, 150);
            await Task.Delay(1500);
            await BildirimToast.FadeTo(0, 200);

            BildirimToast.IsVisible = false;
        }
    }
}
