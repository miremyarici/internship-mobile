using InternshipMpbile.Models;
using InternshipMpbile.Services;

namespace InternshipMpbile.Pages
{
    public partial class ReferansListesiPage : ContentPage
    {
        // Pop-up açıkken silinmesi onaylanacak kayıt.
        private Referans? _silinecekReferans;

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
            if (ilkYukleme)
            {
                YukleniyorIndicator.IsVisible = true;
                YukleniyorIndicator.IsRunning = true;
            }

            try
            {
                ReferansCollectionView.ItemsSource = await ReferansService.ListeleAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Referanslar yüklenirken bir hata oluştu: {ex.Message}", "Tamam");
            }
            finally
            {
                YukleniyorIndicator.IsVisible = false;
                YukleniyorIndicator.IsRunning = false;
            }
        }

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
            SilmeOnayiOverlay.IsVisible = false;
            _silinecekReferans = null;

            if (referans is null)
                return;

            try
            {
                await ReferansService.SilAsync(referans.Id);
                await ReferanslariYukleAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Silme sırasında bir hata oluştu: {ex.Message}", "Tamam");
            }
        }

        // Hayır: hiçbir şey değişmez, kullanıcı listeye geri döner.
        private void OnHayirClicked(object? sender, EventArgs e)
        {
            SilmeOnayiOverlay.IsVisible = false;
            _silinecekReferans = null;
        }
    }
}
