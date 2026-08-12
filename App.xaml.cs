using InternshipMpbile.Localization;
using InternshipMpbile.Pages;
using InternshipMpbile.Services;

namespace InternshipMpbile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Tema işletim sisteminin ayarından değil kullanıcının menüdeki
            // seçiminden gelir; bu seçim cihazda saklandığı için ilk ekran
            // doğrudan doğru modla açılır ve bir anlık renk sıçraması olmaz.
            UserAppTheme = Tema.Baslangic;
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

        /// <summary>
        /// Dili değiştirir ve menülü ana ekranı baştan kurar. Metinler sayfa
        /// kurulurken çevrildiği için, dilin her yere işlemesinin yolu arayüzü
        /// yeniden oluşturmaktır. Oturum açık kaldığı için kullanıcı düşmez.
        /// </summary>
        public static void DiliDegistir(Dil dil)
        {
            Ceviri.AktifDil = dil;
            KokSayfayiDegistir(new AppShell());
        }

        private static void KokSayfayiDegistir(Page sayfa)
        {
            if (Current?.Windows.FirstOrDefault() is Window pencere)
                pencere.Page = sayfa;
        }
    }
}
