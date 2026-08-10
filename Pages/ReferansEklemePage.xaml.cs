using InternshipMpbile.Models;
using InternshipMpbile.Services;

namespace InternshipMpbile.Pages
{
    public partial class ReferansEklemePage : ContentPage
    {
        private const string EkranAdi = "Referans Ekleme";
        private const string TipSecilmediMesaji = "Önce referans tipi seçiniz";
        private const string AltTipYokMesaji = "Bu tip için kayıtlı değer yok";

        public ReferansEklemePage()
        {
            InitializeComponent();
            ReferansTipiAlani.Secenekler = ReferansService.Tipler;
        }

        // Tip seçildiğinde alt tip seçenekleri, başvuru formu tablosunda o sütuna
        // girilmiş olan değerlerden okunur.
        private async void OnReferansTipiDegisti(object? sender, EventArgs e)
        {
            AltTipAlaniniSifirla();

            if (ReferansTipiAlani.SecilenDeger is not string tip)
                return;

            ReferansAltTipiAlani.YerTutucu = string.Empty;
            ReferansAltTipiAlani.Yukleniyor = true;

            try
            {
                var altTipler = await ReferansService.AltTipleriGetirAsync(tip);

                ReferansAltTipiAlani.Secenekler = altTipler;
                ReferansAltTipiAlani.Secilebilir = altTipler.Count > 0;
                ReferansAltTipiAlani.YerTutucu = altTipler.Count > 0 ? "Seçiniz" : AltTipYokMesaji;
            }
            catch (Exception ex)
            {
                ReferansAltTipiAlani.YerTutucu = "Seçiniz";
                await DisplayAlert("Hata", $"Alt tipler yüklenirken bir hata oluştu: {ex.Message}", "Tamam");
            }
            finally
            {
                ReferansAltTipiAlani.Yukleniyor = false;
            }
        }

        private void AltTipAlaniniSifirla()
        {
            ReferansAltTipiAlani.Secenekler = null;
            ReferansAltTipiAlani.Temizle();
            ReferansAltTipiAlani.Secilebilir = false;
            ReferansAltTipiAlani.YerTutucu = TipSecilmediMesaji;
        }

        private async void OnKaydetClicked(object? sender, EventArgs e)
        {
            if (ReferansTipiAlani.SecilenDeger is not string tip ||
                ReferansAltTipiAlani.SecilenDeger is not string altTip)
            {
                await DisplayAlert(EkranAdi, "Lütfen referans tipi ve alt tipini seçin.", "Tamam");
                return;
            }

            try
            {
                if (await ReferansService.VarMiAsync(tip, altTip))
                {
                    await DisplayAlert(EkranAdi, "Bu referans zaten kayıtlı.", "Tamam");
                    return;
                }

                await ReferansService.KaydetAsync(new Referans { Type = tip, Subtype = altTip });
                await DisplayAlert(EkranAdi, "Referans başarıyla kaydedildi.", "Tamam");

                // Tip sıfırlanınca OnReferansTipiDegisti alt tip alanını da temizler.
                ReferansTipiAlani.Temizle();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Kayıt sırasında bir hata oluştu: {ex.Message}", "Tamam");
            }
        }
    }
}
