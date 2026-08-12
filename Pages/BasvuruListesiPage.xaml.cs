using InternshipMpbile.Controls;
using InternshipMpbile.Localization;
using InternshipMpbile.Models;
using InternshipMpbile.Services;

namespace InternshipMpbile.Pages
{
    public partial class BasvuruListesiPage : ContentPage
    {
        // Sabit değil özellik: dil değiştirilebildiği için metin her kullanımda çevrilir.
        private static string EkranAdi => Ceviri.Al("Başvuru Listesi");

        // Filtreleme bellekteki bu liste üzerinde yapılır; her filtre denemesinde
        // veritabanına yeniden gidilmez.
        private List<Basvuru> _tumBasvurular = new();

        public BasvuruListesiPage()
        {
            InitializeComponent();
        }

        // Sayfaya her girildiğinde liste tazelenir; formdan yeni kayıt
        // eklendiğinde menüden geri dönünce güncel veri görünür.
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await BasvurulariYukleAsync();
        }

        private async void OnRefreshing(object? sender, EventArgs e)
        {
            await BasvurulariYukleAsync();
            BasvuruRefreshView.IsRefreshing = false;
        }

        private async Task BasvurulariYukleAsync()
        {
            // Aşağı çekerek yenilemede RefreshView kendi göstergesini gösterir,
            // ilk yüklemede ise ortadaki gösterge devreye girer.
            var ilkYukleme = !BasvuruRefreshView.IsRefreshing;
            GostergeyiAyarla(ilkYukleme);

            try
            {
                _tumBasvurular = await BasvuruService.ListeleAsync();

                FiltreSecenekleriniTazele();
                Filtrele();
            }
            catch (Exception ex)
            {
                await DisplayAlert(Ceviri.Al("Hata"),
                    $"{Ceviri.Al("Başvurular yüklenirken bir hata oluştu:")} {ex.Message}", Ceviri.Al("Tamam"));
            }
            finally
            {
                GostergeyiAyarla(false);
            }
        }

        private void GostergeyiAyarla(bool gorunur)
        {
            YukleniyorIndicator.IsVisible = gorunur;
            YukleniyorIndicator.IsRunning = gorunur;
        }

        // ==================== Filtreleme ====================

        private void OnFiltreBasligiTapped(object? sender, TappedEventArgs e)
        {
            FiltrePaneli.IsVisible = !FiltrePaneli.IsVisible;
            FiltreOku.Rotation = FiltrePaneli.IsVisible ? 180 : 0;
        }

        /// <summary>
        /// Açılır listeler yalnızca listede gerçekten geçen değerlerle doldurulur;
        /// böylece hiçbir kaydı getirmeyecek bir filtre seçilemez.
        /// </summary>
        private void FiltreSecenekleriniTazele()
        {
            BasvuranBirimFiltresi.SecenekleriTazele(Secenekler(basvuru => basvuru.BasvuranBirim));
            BasvuruYapilanProjeFiltresi.SecenekleriTazele(Secenekler(basvuru => basvuru.BasvuruYapilanProje));
            BasvuruYapilanTurFiltresi.SecenekleriTazele(Secenekler(basvuru => basvuru.BasvuruYapilanTur));
            KatilimciTuruFiltresi.SecenekleriTazele(Secenekler(basvuru => basvuru.KatilimciTuru));
            BasvuruDonemiFiltresi.SecenekleriTazele(Secenekler(basvuru => basvuru.BasvuruDonemi));
        }

        private List<string> Secenekler(Func<Basvuru, string> alan) =>
            _tumBasvurular.Select(alan)
                .Where(deger => !string.IsNullOrWhiteSpace(deger))
                .Distinct()
                .OrderBy(deger => deger)
                .ToList();

        private async void OnFiltreleClicked(object? sender, EventArgs e)
        {
            var hibeMetni = HibeTutariFiltresi.Deger?.Trim();

            if (!string.IsNullOrEmpty(hibeMetni) && !decimal.TryParse(hibeMetni, out _))
            {
                await DisplayAlert(EkranAdi, Ceviri.Al("Hibe tutarı için geçerli bir sayı giriniz."), Ceviri.Al("Tamam"));
                return;
            }

            Filtrele();
        }

        private void OnTemizleClicked(object? sender, EventArgs e)
        {
            BasvuranBirimFiltresi.Temizle();
            BasvuruYapilanProjeFiltresi.Temizle();
            BasvuruYapilanTurFiltresi.Temizle();
            KatilimciTuruFiltresi.Temizle();
            BasvuruDonemiFiltresi.Temizle();
            BasvuruTarihiFiltresi.Temizle();
            DurumTarihiFiltresi.Temizle();
            HibeTutariFiltresi.Temizle();

            Filtrele();
        }

        /// <summary>
        /// Boş bırakılan filtreler dikkate alınmaz; doldurulmuş olanların hepsini
        /// birden sağlayan başvurular listelenir.
        /// </summary>
        private void Filtrele()
        {
            IEnumerable<Basvuru> sonuc = _tumBasvurular;

            sonuc = Suz(sonuc, BasvuranBirimFiltresi, basvuru => basvuru.BasvuranBirim);
            sonuc = Suz(sonuc, BasvuruYapilanProjeFiltresi, basvuru => basvuru.BasvuruYapilanProje);
            sonuc = Suz(sonuc, BasvuruYapilanTurFiltresi, basvuru => basvuru.BasvuruYapilanTur);
            sonuc = Suz(sonuc, KatilimciTuruFiltresi, basvuru => basvuru.KatilimciTuru);
            sonuc = Suz(sonuc, BasvuruDonemiFiltresi, basvuru => basvuru.BasvuruDonemi);

            if (BasvuruTarihiFiltresi.Secildi)
                sonuc = sonuc.Where(basvuru => basvuru.BasvuruTarihi.Date == BasvuruTarihiFiltresi.Tarih.Date);

            if (DurumTarihiFiltresi.Secildi)
                sonuc = sonuc.Where(basvuru => basvuru.DurumTarihi.Date == DurumTarihiFiltresi.Tarih.Date);

            if (decimal.TryParse(HibeTutariFiltresi.Deger?.Trim(), out var hibeTutari))
                sonuc = sonuc.Where(basvuru => basvuru.HibeTutari == hibeTutari);

            var listelenecek = sonuc.ToList();

            BasvuruCollectionView.ItemsSource = listelenecek;
            BosGorunumuAyarla(listelenecek.Count < _tumBasvurular.Count);
        }

        private static IEnumerable<Basvuru> Suz(
            IEnumerable<Basvuru> kaynak, SeciciAlan filtre, Func<Basvuru, string> alan) =>
            filtre.SecilenDeger is { } secim ? kaynak.Where(basvuru => alan(basvuru) == secim) : kaynak;

        // Liste filtre yüzünden boşaldıysa "henüz kayıt yok" demek yanıltıcı olur.
        private void BosGorunumuAyarla(bool filtreElemis)
        {
            BosBaslikLabel.Text = Ceviri.Al(filtreElemis ? "Filtreye uyan başvuru yok" : "Henüz başvuru yok");
            BosMetinLabel.Text = Ceviri.Al(filtreElemis
                ? "Filtreyi değiştirebilir ya da Temizle ile tüm başvuruları görebilirsiniz."
                : "Başvuru Formu ekranından yeni bir kayıt oluşturabilirsiniz.");
        }
    }
}
