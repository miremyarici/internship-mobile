using InternshipMpbile.Pages;
using InternshipMpbile.Services;

namespace InternshipMpbile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        // Uygulama giriş ekranıyla açılır; menülü ana ekrana ancak giriş
        // yapıldıktan sonra geçilir.
        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new GirisPage());
        }

        /// <summary>Giriş başarılı olduğunda menülü ana ekrana geçer.</summary>
        public static void AnaEkranaGec() => KokSayfayiDegistir(new AppShell());

        /// <summary>Oturumu kapatıp giriş ekranına döner.</summary>
        public static void GirisEkraninaGec()
        {
            Oturum.Bitir();
            KokSayfayiDegistir(new GirisPage());
        }

        private static void KokSayfayiDegistir(Page sayfa)
        {
            if (Current?.Windows.FirstOrDefault() is Window pencere)
                pencere.Page = sayfa;
        }
    }
}
