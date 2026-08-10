using InternshipMpbile.Services;

namespace InternshipMpbile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Shell yalnızca giriş yapıldıktan sonra oluşturulduğu için aktif
            // kullanıcı bu noktada bellidir.
            KullaniciAdiLabel.Text = Oturum.AktifKullanici?.TamAd ?? string.Empty;
        }

        private void OnCikisYapTapped(object? sender, TappedEventArgs e)
        {
            FlyoutIsPresented = false;

            // Kök sayfa, menü kapanma animasyonu bittikten sonra değiştirilir.
            Dispatcher.Dispatch(App.GirisEkraninaGec);
        }
    }
}
