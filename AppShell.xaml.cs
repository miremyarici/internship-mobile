using InternshipMpbile.Localization;
using InternshipMpbile.Services;

namespace InternshipMpbile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Menüde yer almayan, koddan açılan ekranlar.
            Routing.RegisterRoute("mesajlar", typeof(Pages.MesajlarPage));
            Routing.RegisterRoute("sohbet", typeof(Pages.SohbetPage));

            // Shell yalnızca giriş yapıldıktan sonra oluşturulduğu için aktif
            // kullanıcı bu noktada bellidir.
            KullaniciAdiLabel.Text = Oturum.AktifKullanici?.TamAd ?? string.Empty;

            AktifDiliGoster();
            AktifTemayiGoster();
        }

        // ==================== Karanlık mod ====================

        /// <summary>
        /// Anahtarı saklanan tercihle eşler. Toggled olayını geçici olarak ayırmak,
        /// IsToggled atamasının OnKaranlikModDegisti'yi tetikleyip tercihi zaten
        /// olduğu değerle yeniden yazmasını (gereksiz bir diske yazma) önler.
        /// </summary>
        private void AktifTemayiGoster()
        {
            KaranlikModAnahtari.Toggled -= OnKaranlikModDegisti;
            KaranlikModAnahtari.IsToggled = Tema.KaranlikMi;
            KaranlikModAnahtari.Toggled += OnKaranlikModDegisti;
        }

        // Tercihi yazmak ve temayı uygulamak Tema'nın işi; burada tek satır kalır.
        private void OnKaranlikModDegisti(object? sender, ToggledEventArgs e) =>
            Tema.KaranlikMi = e.Value;

        // ==================== Dil seçimi ====================

        /// <summary>
        /// Seçili dilin bayrağını ve adını gösterir. Ad, o anki dilde yazılır:
        /// Türkçeyken "Türkçe", İngilizceyken "English".
        /// </summary>
        private void AktifDiliGoster()
        {
            var ingilizce = Ceviri.AktifDil == Dil.Ingilizce;

            AktifBayrak.Source = ingilizce ? "bayrak_en.png" : "bayrak_tr.png";
            AktifDilLabel.Text = Ceviri.Al(ingilizce ? "İngilizce" : "Türkçe");
        }

        private void OnDilTapped(object? sender, TappedEventArgs e)
        {
            DilSecenekleri.IsVisible = !DilSecenekleri.IsVisible;
            DilOku.Rotation = DilSecenekleri.IsVisible ? 180 : 0;
        }

        private void OnTurkceSecildi(object? sender, TappedEventArgs e) => DiliSec(Dil.Turkce);

        private void OnIngilizceSecildi(object? sender, TappedEventArgs e) => DiliSec(Dil.Ingilizce);

        private void DiliSec(Dil dil)
        {
            DilSecenekleri.IsVisible = false;
            DilOku.Rotation = 0;

            if (Ceviri.AktifDil == dil)
                return;

            FlyoutIsPresented = false;

            // Arayüz, menü kapanma animasyonu bittikten sonra yeni dille kurulur.
            Dispatcher.Dispatch(() => App.DiliDegistir(dil));
        }

        // ==================== Çıkış ====================

        private void OnCikisYapTapped(object? sender, TappedEventArgs e)
        {
            FlyoutIsPresented = false;

            // Kök sayfa, menü kapanma animasyonu bittikten sonra değiştirilir.
            Dispatcher.Dispatch(App.GirisEkraninaGec);
        }
    }
}
