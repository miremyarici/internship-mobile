using InternshipMpbile.Localization;
using InternshipMpbile.Models;
using InternshipMpbile.Services;

namespace InternshipMpbile.Pages
{
    public partial class ReferansEklemePage : ContentPage
    {
        // Sabit değil özellik: dil değiştirilebildiği için metin her kullanımda çevrilir.
        private static string EkranAdi => Ceviri.Al("Referans Ekleme");
        private static string Tamam => Ceviri.Al("Tamam");

        public ReferansEklemePage()
        {
            InitializeComponent();
            ReferansTipiAlani.SecenekleriTazele(ReferansTipleri.Tumu);
        }

        private async void OnKaydetClicked(object? sender, EventArgs e)
        {
            if (ReferansTipiAlani.SecilenDeger is not string tip || ReferansAltTipiAlani.Bos)
            {
                await DisplayAlert(EkranAdi, Ceviri.Al("Lütfen referans tipini seçip alt tipini yazınız."), Tamam);
                return;
            }

            var altTip = ReferansAltTipiAlani.Deger!.Trim();

            KaydetButonu.IsEnabled = false;

            try
            {
                if (await ReferansService.VarMiAsync(tip, altTip))
                {
                    await DisplayAlert(EkranAdi, Ceviri.Al("Bu referans zaten kayıtlı."), Tamam);
                    return;
                }

                await ReferansService.KaydetAsync(new Referans { Type = tip, Subtype = altTip });
                await DisplayAlert(EkranAdi, Ceviri.Al("Referans başarıyla kaydedildi."), Tamam);

                ReferansTipiAlani.Temizle();
                ReferansAltTipiAlani.Temizle();
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
    }
}
