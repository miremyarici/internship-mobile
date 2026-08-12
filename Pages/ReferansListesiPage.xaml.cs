using InternshipMpbile.Localization;
using InternshipMpbile.Models;
using InternshipMpbile.Services;

namespace InternshipMpbile.Pages
{
    public partial class ReferansListesiPage : ContentPage
    {
        // Pop-up açıkken silinmesi onaylanacak kayıt.
        private Referans? _silinecekReferans;

        // Filtreleme bellekteki bu liste üzerinde yapılır; her filtre denemesinde
        // veritabanına yeniden gidilmez.
        private List<Referans> _tumReferanslar = new();

        public ReferansListesiPage()
        {
            InitializeComponent();
        }

        // Sayfaya her girildiğinde liste tazelenir; ekleme ekranından yeni kayıt
        // eklendiğinde menüden geri dönünce güncel veri görünür.
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await ReferanslariYukleAsync();
        }

        private async void OnRefreshing(object? sender, EventArgs e)
        {
            await ReferanslariYukleAsync();
            ReferansRefreshView.IsRefreshing = false;
        }

        private async Task ReferanslariYukleAsync()
        {
            // Aşağı çekerek yenilemede RefreshView kendi göstergesini gösterir,
            // ilk yüklemede ise ortadaki gösterge devreye girer.
            var ilkYukleme = !ReferansRefreshView.IsRefreshing;
            GostergeyiAyarla(ilkYukleme);

            try
            {
                _tumReferanslar = await ReferansService.ListeleAsync();

                FiltreSecenekleriniTazele();
                Filtrele();
            }
            catch (Exception ex)
            {
                await DisplayAlert(Ceviri.Al("Hata"),
                    $"{Ceviri.Al("Referanslar yüklenirken bir hata oluştu:")} {ex.Message}", Ceviri.Al("Tamam"));
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

        /// <summary>
        /// Açılır liste yalnızca listede gerçekten geçen tiplerle doldurulur;
        /// böylece hiçbir kaydı getirmeyecek bir filtre seçilemez.
        /// </summary>
        private void FiltreSecenekleriniTazele() =>
            ReferansTipiFiltresi.SecenekleriTazele(
                _tumReferanslar.Select(referans => referans.Type)
                    .Where(tip => !string.IsNullOrWhiteSpace(tip))
                    .Distinct()
                    .OrderBy(tip => tip)
                    .ToList());

        private void OnFiltreleClicked(object? sender, EventArgs e) => Filtrele();

        private void OnTemizleClicked(object? sender, EventArgs e)
        {
            ReferansTipiFiltresi.Temizle();
            Filtrele();
        }

        private void Filtrele()
        {
            var listelenecek = ReferansTipiFiltresi.SecilenDeger is { } tip
                ? _tumReferanslar.Where(referans => referans.Type == tip).ToList()
                : _tumReferanslar;

            ReferansCollectionView.ItemsSource = listelenecek;
            BosGorunumuAyarla(listelenecek.Count < _tumReferanslar.Count);
        }

        // Liste filtre yüzünden boşaldıysa "henüz kayıt yok" demek yanıltıcı olur.
        private void BosGorunumuAyarla(bool filtreElemis)
        {
            BosBaslikLabel.Text = Ceviri.Al(filtreElemis ? "Filtreye uyan referans yok" : "Henüz referans yok");
            BosMetinLabel.Text = Ceviri.Al(filtreElemis
                ? "Filtreyi değiştirebilir ya da Temizle ile tüm referansları görebilirsiniz."
                : "Referans Ekleme ekranından yeni bir kayıt oluşturabilirsiniz.");
        }

        // ==================== Silme ====================

        private void OnSilTapped(object? sender, TappedEventArgs e)
        {
            if (sender is not Element element || element.BindingContext is not Referans referans)
                return;

            _silinecekReferans = referans;
            SilmeOnayiOverlay.IsVisible = true;
        }

        // Evet: veritabanındaki Delete sütunu 1 yapılır (soft delete) ve kayıt
        // listeden düşer, çünkü liste yalnızca Delete = 0 olanları getirir.
        private async void OnEvetClicked(object? sender, EventArgs e)
        {
            var referans = _silinecekReferans;
            OnayiKapat();

            if (referans is null)
                return;

            try
            {
                await ReferansService.SilAsync(referans.Id);
                await ReferanslariYukleAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert(Ceviri.Al("Hata"),
                    $"{Ceviri.Al("Silme sırasında bir hata oluştu:")} {ex.Message}", Ceviri.Al("Tamam"));
            }
        }

        // Hayır: hiçbir şey değişmez, kullanıcı listeye geri döner.
        private void OnHayirClicked(object? sender, EventArgs e) => OnayiKapat();

        private void OnayiKapat()
        {
            SilmeOnayiOverlay.IsVisible = false;
            _silinecekReferans = null;
        }
    }
}
