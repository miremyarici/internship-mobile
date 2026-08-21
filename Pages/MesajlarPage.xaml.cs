using InternshipMpbile.Localization;
using InternshipMpbile.Models;
using InternshipMpbile.Services;

namespace InternshipMpbile.Pages
{
    /// <summary>
    /// Sohbet listesi. Arama kutusu iki işi birden yapar: var olan sohbetleri
    /// süzer ve mesajlaşma dizininde henüz konuşmadığın kişileri bulur — arkadaşını
    /// bulup ilk mesajı göndermenin yolu budur.
    /// </summary>
    public partial class MesajlarPage : ContentPage
    {
        private static string Tamam => Ceviri.Al("Tamam");

        // Arama, yüklenmiş listenin üzerinde yapılır; her tuşta veritabanına gidilmez.
        private List<SohbetOzeti> _sohbetler = new();

        // Kullanıcı yazmayı bırakınca dizinde arama yapmak için: her tuşta bir
        // sorgu açmak yerine son tuştan 350 ms sonra tek sorgu çalışır.
        private CancellationTokenSource? _aramaIptali;

        public MesajlarPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await SohbetleriYukleAsync();
        }

        private async void OnRefreshing(object? sender, EventArgs e)
        {
            await SohbetleriYukleAsync();
            MesajRefreshView.IsRefreshing = false;
        }

        private async Task SohbetleriYukleAsync()
        {
            if (Oturum.AktifKullanici is not { } ben)
                return;

            var ilkYukleme = !MesajRefreshView.IsRefreshing;
            GostergeyiAyarla(ilkYukleme);

            try
            {
                _sohbetler = await MesajService.SohbetleriListeleAsync(ben.Eposta);
                ListeyiGoster();
            }
            catch (Exception ex)
            {
                await DisplayAlert(Ceviri.Al("Hata"),
                    $"{Ceviri.Al("Mesajlar yüklenirken bir hata oluştu:")} {ex.Message}", Tamam);
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

        // ==================== Arama ====================

        private async void OnAramaDegisti(object? sender, TextChangedEventArgs e)
        {
            ListeyiGoster();

            // Önceki beklemeyi iptal et: yalnızca son yazılan metin sorgulanır.
            _aramaIptali?.Cancel();
            _aramaIptali = new CancellationTokenSource();

            var iptal = _aramaIptali.Token;
            var arama = (e.NewTextValue ?? string.Empty).Trim();

            if (arama.Length == 0 || Oturum.AktifKullanici is not { } ben)
            {
                KisileriGoster(new List<Kisi>());
                return;
            }

            try
            {
                await Task.Delay(350, iptal);

                var kisiler = await MesajService.KisileriAraAsync(ben.Eposta, arama);

                if (iptal.IsCancellationRequested)
                    return;

                // Zaten sohbet edilen kişiler yukarıdaki listede var, tekrar gösterilmez.
                var sohbetEdilenler = _sohbetler.Select(s => s.Eposta).ToHashSet();

                KisileriGoster(kisiler.Where(k => !sohbetEdilenler.Contains(k.Eposta)).ToList());
            }
            catch (OperationCanceledException)
            {
                // Kullanıcı yazmaya devam etti; bu arama artık geçersiz.
            }
            catch (Exception)
            {
                // Dizin okunamadıysa ekranı hata ile bölmeye değmez; liste boş kalır.
                KisileriGoster(new List<Kisi>());
            }
        }

        /// <summary>Aramaya uyan sohbetleri gösterir ve boş durumu ayarlar.</summary>
        private void ListeyiGoster()
        {
            var arama = AramaAlani.Text?.Trim() ?? string.Empty;

            var gorunenler = arama.Length == 0
                ? _sohbetler
                : _sohbetler.Where(s =>
                        s.AdSoyad.Contains(arama, StringComparison.CurrentCultureIgnoreCase) ||
                        s.Eposta.Contains(arama, StringComparison.OrdinalIgnoreCase) ||
                        s.SonMesaj.Contains(arama, StringComparison.CurrentCultureIgnoreCase))
                    .ToList();

            BindableLayout.SetItemsSource(SohbetListesi, gorunenler);

            BosGorunum.IsVisible = gorunenler.Count == 0 && !KisilerBasligi.IsVisible;

            BosBaslikLabel.Text = arama.Length == 0
                ? Ceviri.Al("Henüz mesaj yok")
                : Ceviri.Al("Sonuç bulunamadı");
        }

        private void KisileriGoster(List<Kisi> kisiler)
        {
            BindableLayout.SetItemsSource(KisiListesi, kisiler);

            KisilerBasligi.IsVisible = kisiler.Count > 0;

            if (kisiler.Count > 0)
                BosGorunum.IsVisible = false;
        }

        // ==================== Sohbete geçiş ====================

        private async void OnSohbetTapped(object? sender, TappedEventArgs e)
        {
            if ((sender as VisualElement)?.BindingContext is SohbetOzeti sohbet)
                await SohbetiAcAsync(sohbet.Eposta, sohbet.AdSoyad);
        }

        private async void OnKisiTapped(object? sender, TappedEventArgs e)
        {
            if ((sender as VisualElement)?.BindingContext is Kisi kisi)
                await SohbetiAcAsync(kisi.Eposta, kisi.AdSoyad);
        }

        private static Task SohbetiAcAsync(string eposta, string adSoyad) =>
            Shell.Current.GoToAsync(
                $"sohbet?eposta={Uri.EscapeDataString(eposta)}&ad={Uri.EscapeDataString(adSoyad)}");
    }
}
