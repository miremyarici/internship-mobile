using InternshipMpbile.Services;

namespace InternshipMpbile.Pages
{
    public partial class BasvuruListesiPage : ContentPage
    {
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
            if (ilkYukleme)
            {
                YukleniyorIndicator.IsVisible = true;
                YukleniyorIndicator.IsRunning = true;
            }

            try
            {
                BasvuruCollectionView.ItemsSource = await BasvuruService.ListeleAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", $"Başvurular yüklenirken bir hata oluştu: {ex.Message}", "Tamam");
            }
            finally
            {
                YukleniyorIndicator.IsVisible = false;
                YukleniyorIndicator.IsRunning = false;
            }
        }
    }
}
