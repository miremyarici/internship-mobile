using InternshipMpbile.Models;
using InternshipMpbile.Services;

namespace InternshipMpbile.Pages
{
    public partial class ReferansEklemePage : ContentPage
    {
        public ReferansEklemePage()
        {
            InitializeComponent();
            ReferansTipiPicker.ItemsSource = ReferansService.Tipler;
        }

        // Tip seçildiğinde alt tip seçenekleri, başvuru formu tablosunda o sütuna
        // girilmiş olan değerlerden okunur.
        private async void OnReferansTipiChanged(object? sender, EventArgs e)
        {
            ReferansAltTipiPicker.ItemsSource = null;
            ReferansAltTipiPicker.SelectedItem = null;

            if (ReferansTipiPicker.SelectedItem is not string tip)
            {
                ReferansAltTipiPicker.IsEnabled = false;
                ReferansAltTipiPicker.Title = "Önce referans tipi seçiniz";
                return;
            }

            ReferansAltTipiPicker.IsEnabled = false;
            ReferansAltTipiPicker.Title = string.Empty;
            AltTipYukleniyorIndicator.IsVisible = true;
            AltTipYukleniyorIndicator.IsRunning = true;

            try
            {
                var altTipler = await ReferansService.AltTipleriGetirAsync(tip);

                ReferansAltTipiPicker.ItemsSource = altTipler;
                ReferansAltTipiPicker.IsEnabled = altTipler.Count > 0;
                ReferansAltTipiPicker.Title = altTipler.Count > 0
                    ? "Seçiniz"
                    : "Bu tip için kayıtlı değer yok";
            }
            catch (Exception ex)
            {
                ReferansAltTipiPicker.Title = "Seçiniz";
                await DisplayAlert("Hata", $"Alt tipler yüklenirken bir hata oluştu: {ex.Message}", "Tamam");
            }
            finally
            {
                AltTipYukleniyorIndicator.IsVisible = false;
                AltTipYukleniyorIndicator.IsRunning = false;
            }
        }

        private async void OnKaydetClicked(object? sender, EventArgs e)
        {
            if (ReferansTipiPicker.SelectedItem is not string tip ||
                ReferansAltTipiPicker.SelectedItem is not string altTip)
            {
                await DisplayAlert("Referans Ekleme", "Lütfen referans tipi ve alt tipini seçin.", "Tamam");
                return;
            }

            try
            {
                if (await ReferansService.VarMiAsync(tip, altTip))
                {
                    await DisplayAlert("Referans Ekleme", "Bu referans zaten kayıtlı.", "Tamam");
                    return;
                }

                await ReferansService.KaydetAsync(new Referans { Type = tip, Subtype = altTip });
                await DisplayAlert("Referans Ekleme", "Referans başarıyla kaydedildi.", "Tamam");
                FormuTemizle();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Kayıt sırasında bir hata oluştu: {ex.Message}", "Tamam");
            }
        }

        private void FormuTemizle()
        {
            // Tip sıfırlanınca OnReferansTipiChanged alt tip alanını da temizler.
            ReferansTipiPicker.SelectedItem = null;
        }
    }
}
