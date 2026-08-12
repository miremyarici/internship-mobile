using InternshipMpbile.Controls;
using InternshipMpbile.Localization;
using InternshipMpbile.Models;
using InternshipMpbile.Services;

namespace InternshipMpbile.Pages
{
    public partial class BasvuruFormuPage : ContentPage
    {
        private const int EnAzParolaUzunlugu = 6;

        // Sabit değil özellik: dil değiştirilebildiği için metin her kullanımda çevrilir.
        private static string EkranAdi => Ceviri.Al("Başvuru Formu");
        private static string ParolaEkraniAdi => Ceviri.Al("Parolayı Değiştir");
        private static string Tamam => Ceviri.Al("Tamam");

        public BasvuruFormuPage()
        {
            InitializeComponent();
        }

        // Geçici parolayla giriş yapan kullanıcıyı, formu kullanmadan önce parola
        // değiştirme pop-up'ı karşılar. Seçenekler de her girişte tazelenir; böylece
        // Referans Ekleme ekranında yazılan bir alt tip, buraya dönüldüğünde ilgili
        // açılır listede hazır olur.
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            ParolaDegistirOverlay.IsVisible = Oturum.AktifKullanici?.GeciciSifre == true;

            await SecenekleriYukleAsync();
        }

        private async Task SecenekleriYukleAsync()
        {
            // Başvuru durumu bir referans değil, sabit iş kuralıdır; tablodan beslenmez.
            BasvuruDurumuAlani.SecenekleriTazele(FormSecenekleri.BasvuruDurumlari);

            ILookup<string, string>? referanslar = null;

            try
            {
                referanslar = await ReferansService.AltTipleriTipeGoreGetirAsync();
            }
            catch (Exception ex)
            {
                // Referanslara ulaşılamazsa form varsayılan seçeneklerle çalışmayı sürdürür.
                await DisplayAlert(Ceviri.Al("Hata"),
                    $"{Ceviri.Al("Referanslar yüklenirken bir hata oluştu:")} {ex.Message}", Tamam);
            }

            Doldur(BasvuranBirimAlani, FormSecenekleri.BasvuranBirimler, ReferansTipleri.BasvuranBirim);
            Doldur(BasvuruYapilanProjeAlani, FormSecenekleri.BasvuruYapilanProjeler, ReferansTipleri.BasvuruYapilanProje);
            Doldur(BasvuruYapilanTurAlani, FormSecenekleri.BasvuruYapilanTurler, ReferansTipleri.BasvuruYapilanTur);
            Doldur(KatilimciTuruAlani, FormSecenekleri.KatilimciTurleri, ReferansTipleri.KatilimciTuru);
            Doldur(BasvuruDonemiAlani, FormSecenekleri.BasvuruDonemleri, ReferansTipleri.BasvuruDonemi);

            // Varsayılan seçeneklerin üstüne Referans tablosundakiler eklenir; Distinct
            // aynı değerin listede iki kez görünmesini önler.
            void Doldur(SeciciAlan alan, string[] varsayilanlar, string tip)
            {
                var eklenenler = referanslar?[tip] ?? Enumerable.Empty<string>();
                alan.SecenekleriTazele(varsayilanlar.Concat(eklenenler).Distinct().ToList());
            }
        }

        private async void OnKaydetClicked(object? sender, EventArgs e)
        {
            if (!ZorunluAlanlarDolu())
            {
                await DisplayAlert(EkranAdi, Ceviri.Al("Lütfen tüm zorunlu alanları doldurun."), Tamam);
                return;
            }

            if (!HibeTutariniCoz(out var hibeTutari))
            {
                await DisplayAlert(EkranAdi, Ceviri.Al("Hibe tutarı 0'dan büyük bir sayı olmalıdır."), Tamam);
                return;
            }

            if (DurumTarihiAlani.Tarih > BasvuruTarihiAlani.Tarih)
            {
                await DisplayAlert(EkranAdi,
                    Ceviri.Al("Durum tarihi, başvuru tarihinden ileri bir tarih olamaz."), Tamam);
                return;
            }

            var projeAdi = ProjeAdiAlani.Deger!.Trim();

            KaydetButonu.IsEnabled = false;

            try
            {
                if (await BasvuruService.ProjeAdiVarMiAsync(projeAdi))
                {
                    await DisplayAlert(EkranAdi,
                        Ceviri.Al("Bu proje adı daha önce kullanılmış. Lütfen farklı bir proje adı giriniz."), Tamam);
                    return;
                }

                await BasvuruService.KaydetAsync(FormdanBasvuruOlustur(projeAdi, hibeTutari));
                await DisplayAlert(EkranAdi, Ceviri.Al("Başvuru başarıyla kaydedildi."), Tamam);
                FormuTemizle();
            }
            catch (Exception ex)
            {
                await DisplayAlert(Ceviri.Al("Hata"),
                    $"{Ceviri.Al("Kayıt sırasında bir hata oluştu:")} {ex.Message}", Tamam);
            }
            finally
            {
                KaydetButonu.IsEnabled = true;
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

        /// <summary>
        /// Hibe tutarı zorunlu değildir: alan boşsa tutar null kalır ve geçerli sayılır.
        /// Doluysa sayıya çevrilebilmeli ve 0'dan büyük olmalıdır.
        /// </summary>
        private bool HibeTutariniCoz(out decimal? tutar)
        {
            tutar = null;

            var metin = HibeTutariAlani.Deger?.Trim();

            if (string.IsNullOrEmpty(metin))
                return true;

            if (!decimal.TryParse(metin, out var deger) || deger <= 0)
                return false;

            tutar = deger;
            return true;
        }

        private Basvuru FormdanBasvuruOlustur(string projeAdi, decimal? hibeTutari) => new()
        {
            ProjeAdi = projeAdi,
            BasvuranBirim = BasvuranBirimAlani.SecilenDeger!,
            BasvuruYapilanProje = BasvuruYapilanProjeAlani.SecilenDeger!,
            BasvuruYapilanTur = BasvuruYapilanTurAlani.SecilenDeger!,
            KatilimciTuru = KatilimciTuruAlani.SecilenDeger!,
            BasvuruDonemi = BasvuruDonemiAlani.SecilenDeger!,
            BasvuruTarihi = BasvuruTarihiAlani.Tarih,
            BasvuruDurumu = BasvuruDurumuAlani.SecilenDeger!,
            DurumTarihi = DurumTarihiAlani.Tarih,
            HibeTutari = hibeTutari
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
                await DisplayAlert(ParolaEkraniAdi, Ceviri.Al("Lütfen tüm alanları doldurun."), Tamam);
                return;
            }

            if (yeniParola != yeniParolaTekrar)
            {
                await DisplayAlert(ParolaEkraniAdi, Ceviri.Al("Yeni parolalar birbiriyle aynı değil."), Tamam);
                return;
            }

            if (yeniParola.Length < EnAzParolaUzunlugu)
            {
                await DisplayAlert(ParolaEkraniAdi,
                    Ceviri.Al("Yeni parola en az {0} karakter olmalıdır.", EnAzParolaUzunlugu), Tamam);
                return;
            }

            if (yeniParola == eskiParola)
            {
                await DisplayAlert(ParolaEkraniAdi,
                    Ceviri.Al("Yeni parola geçici parolanızdan farklı olmalıdır."), Tamam);
                return;
            }

            ParolaKaydetButonu.IsEnabled = false;

            try
            {
                if (!await KullaniciService.SifreDegistirAsync(kullanici.Id, eskiParola, yeniParola))
                {
                    await DisplayAlert(ParolaEkraniAdi, Ceviri.Al("Eski parolanız hatalı."), Tamam);
                    return;
                }

                kullanici.GeciciSifre = false;
                ParolaDegistirOverlay.IsVisible = false;
                ParolaAlanlariniTemizle();

                await BildirimGosterAsync(Ceviri.Al("Parolanız başarıyla değiştirildi."));
            }
            catch (Exception ex)
            {
                await DisplayAlert(Ceviri.Al("Hata"),
                    $"{Ceviri.Al("Parola değiştirilirken bir hata oluştu:")} {ex.Message}", Tamam);
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
