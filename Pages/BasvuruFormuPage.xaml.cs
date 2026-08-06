using InternshipMpbile.Models;
using InternshipMpbile.Services;

namespace InternshipMpbile.Pages
{
    public partial class BasvuruFormuPage : ContentPage
    {
        public BasvuruFormuPage()
        {
            InitializeComponent();
            DoldurSecenekler();
        }

        // Geçici seçenekler - lookup tabloları oluşturulduğunda buradan kaldırılacak.
        private void DoldurSecenekler()
        {
            BasvuranBirimPicker.ItemsSource = new List<string>
            {
                "Bilgi İşlem",
                "İnsan Kaynakları",
                "Yatırım İşleri",
                "test"
            };

            BasvuruYapilanProjePicker.ItemsSource = new List<string>
            {
                "Erasmus",
                "Merkezi",
                "Avrupa",
                "Diğer"
            };

            BasvuruYapilanTurPicker.ItemsSource = new List<string>
            {
                "Gençlik",
                "Yetişkin",
                "Spor",
                "Mesleki",
                "Dijital",
                "Diğer"
            };

            KatilimciTuruPicker.ItemsSource = new List<string>
            {
                "Koordinatör",
                "Ortak",
                "R2",
                "R3"
            };

            BasvuruDonemiPicker.ItemsSource = new List<string>
            {
                "R1"
            };

            BasvuruDurumuPicker.ItemsSource = new List<string>
            {
                "Kabul",
                "Red"
            };
        }

        // Tarih seçilene kadar "gg.aa.yyyy" yer tutucusu görünür; seçildiğinde
        // yer tutucu gizlenip DatePicker metni görünür hale gelir.
        private void OnBasvuruTarihiSelected(object? sender, DateChangedEventArgs e)
        {
            BasvuruTarihiPlaceholder.IsVisible = false;
            BasvuruTarihiPicker.TextColor = GetFieldTextColor();
        }

        private void OnDurumTarihiSelected(object? sender, DateChangedEventArgs e)
        {
            DurumTarihiPlaceholder.IsVisible = false;
            DurumTarihiPicker.TextColor = GetFieldTextColor();
        }

        private static Color GetFieldTextColor() =>
            Application.Current?.Resources.TryGetValue("FormFieldText", out var color) == true && color is Color c
                ? c
                : Colors.Black;

        private async void OnKaydetClicked(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ProjeAdiEntry.Text) ||
                BasvuranBirimPicker.SelectedItem is null ||
                BasvuruYapilanProjePicker.SelectedItem is null ||
                BasvuruYapilanTurPicker.SelectedItem is null ||
                KatilimciTuruPicker.SelectedItem is null ||
                BasvuruDonemiPicker.SelectedItem is null ||
                BasvuruDurumuPicker.SelectedItem is null)
            {
                await DisplayAlert("Başvuru Formu", "Lütfen tüm zorunlu alanları doldurun.", "Tamam");
                return;
            }

            var basvuru = new Basvuru
            {
                ProjeAdi = ProjeAdiEntry.Text,
                BasvuranBirim = (string)BasvuranBirimPicker.SelectedItem,
                BasvuruYapilanProje = (string)BasvuruYapilanProjePicker.SelectedItem,
                BasvuruYapilanTur = (string)BasvuruYapilanTurPicker.SelectedItem,
                KatilimciTuru = (string)KatilimciTuruPicker.SelectedItem,
                BasvuruDonemi = (string)BasvuruDonemiPicker.SelectedItem,
                BasvuruTarihi = BasvuruTarihiPicker.Date,
                BasvuruDurumu = (string)BasvuruDurumuPicker.SelectedItem,
                DurumTarihi = DurumTarihiPicker.Date,
                HibeTutari = decimal.TryParse(HibeTutariEntry.Text, out var hibeTutari) ? hibeTutari : null
            };

            try
            {
                await BasvuruService.KaydetAsync(basvuru);
                await DisplayAlert("Başvuru Formu", "Başvuru başarıyla kaydedildi.", "Tamam");
                FormuTemizle();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Kayıt sırasında bir hata oluştu: {ex.Message}", "Tamam");
            }
        }

        // Kayıt sonrası alanları boşaltarak formu yeni başvuruya hazırlar.
        private void FormuTemizle()
        {
            ProjeAdiEntry.Text = string.Empty;
            HibeTutariEntry.Text = string.Empty;

            BasvuranBirimPicker.SelectedItem = null;
            BasvuruYapilanProjePicker.SelectedItem = null;
            BasvuruYapilanTurPicker.SelectedItem = null;
            KatilimciTuruPicker.SelectedItem = null;
            BasvuruDonemiPicker.SelectedItem = null;
            BasvuruDurumuPicker.SelectedItem = null;

            BasvuruTarihiPicker.Date = DateTime.Today;
            DurumTarihiPicker.Date = DateTime.Today;
            BasvuruTarihiPicker.TextColor = Colors.Transparent;
            DurumTarihiPicker.TextColor = Colors.Transparent;
            BasvuruTarihiPlaceholder.IsVisible = true;
            DurumTarihiPlaceholder.IsVisible = true;
        }
    }
}
