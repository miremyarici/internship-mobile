using InternshipMpbile.Localization;
using InternshipMpbile.Models;
using InternshipMpbile.Services;

namespace InternshipMpbile.Pages
{
    /// <summary>
    /// İki kişi arasındaki yazışma. Ekran açıkken 4 saniyede bir tabloya bakılır;
    /// karşı taraf başka bir uygulamadan yazdığında mesaj böyle düşer.
    /// </summary>
    [QueryProperty(nameof(Eposta), "eposta")]
    [QueryProperty(nameof(AdSoyad), "ad")]
    public partial class SohbetPage : ContentPage
    {
        private static readonly TimeSpan YoklamaAraligi = TimeSpan.FromSeconds(4);

        private IDispatcherTimer? _yoklama;

        // Aynı listeyi boşuna yeniden kurmamak için son durum burada tutulur.
        private int _sonMesajId = -1;
        private int _sonMesajSayisi;

        public SohbetPage()
        {
            InitializeComponent();
        }

        private string _eposta = string.Empty;
        private string _adSoyad = string.Empty;

        /// <summary>Konuşulan kişinin e-postası (Mesajlar ekranından gelir).</summary>
        public string Eposta
        {
            get => _eposta;
            set => _eposta = Uri.UnescapeDataString(value ?? string.Empty);
        }

        /// <summary>Başlıkta görünen ad; dizinde kayıtlı değilse e-postanın kendisi.</summary>
        public string AdSoyad
        {
            get => _adSoyad;
            set
            {
                _adSoyad = Uri.UnescapeDataString(value ?? string.Empty);
                Title = _adSoyad;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await MesajlariYukleAsync(ilkYukleme: true);

            _yoklama = Dispatcher.CreateTimer();
            _yoklama.Interval = YoklamaAraligi;
            _yoklama.Tick += async (_, _) => await MesajlariYukleAsync(ilkYukleme: false);
            _yoklama.Start();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            _yoklama?.Stop();
            _yoklama = null;
        }

        private async Task MesajlariYukleAsync(bool ilkYukleme)
        {
            if (Oturum.AktifKullanici is not { } ben || Eposta.Length == 0)
                return;

            GostergeyiAyarla(ilkYukleme);

            try
            {
                var mesajlar = await MesajService.SohbetiGetirAsync(ben.Eposta, Eposta);

                // Ekran açıkken gelen mesaj okunmuş sayılır.
                await MesajService.OkunduIsaretleAsync(ben.Eposta, Eposta);

                var sonId = mesajlar.Count > 0 ? mesajlar[^1].Id : -1;

                // Değişen bir şey yoksa listeyi yeniden kurmak kaydırmayı bozar.
                if (!ilkYukleme && sonId == _sonMesajId && mesajlar.Count == _sonMesajSayisi)
                    return;

                var yeniMesajVar = sonId != _sonMesajId;

                _sonMesajId = sonId;
                _sonMesajSayisi = mesajlar.Count;

                BindableLayout.SetItemsSource(MesajYigini, mesajlar);
                BosGorunum.IsVisible = mesajlar.Count == 0;

                if (yeniMesajVar)
                    EnAltaKaydir();
            }
            catch (Exception ex) when (ilkYukleme)
            {
                await DisplayAlert(Ceviri.Al("Hata"),
                    $"{Ceviri.Al("Mesajlar yüklenirken bir hata oluştu:")} {ex.Message}",
                    Ceviri.Al("Tamam"));
            }
            catch (Exception)
            {
                // Yoklama sırasındaki geçici hata ekranı bölmez; sonraki turda düzelir.
            }
            finally
            {
                GostergeyiAyarla(false);
            }
        }

        /// <summary>Yeni balon eklendikten sonra yerleşimin oturmasını bekleyip en alta iner.</summary>
        private void EnAltaKaydir() =>
            Dispatcher.Dispatch(async () =>
            {
                await Task.Delay(60);
                await MesajScroll.ScrollToAsync(0, Math.Max(0, MesajYigini.Height), true);
            });

        private void GostergeyiAyarla(bool gorunur)
        {
            YukleniyorIndicator.IsVisible = gorunur;
            YukleniyorIndicator.IsRunning = gorunur;
        }

        // ==================== Gönderme ====================

        private async void OnGonderTapped(object? sender, TappedEventArgs e) => await GonderAsync();

        private async void OnGonderClicked(object? sender, EventArgs e) => await GonderAsync();

        private async Task GonderAsync()
        {
            var metin = MesajAlani.Text?.Trim() ?? string.Empty;

            if (metin.Length == 0 || Oturum.AktifKullanici is not { } ben)
                return;

            // Alan hemen boşalır; gönderim başarısızsa metin geri konur.
            MesajAlani.Text = string.Empty;

            try
            {
                await MesajService.GonderAsync(ben.Eposta, Eposta, metin);
                await MesajlariYukleAsync(ilkYukleme: false);
            }
            catch (Exception ex)
            {
                MesajAlani.Text = metin;

                await DisplayAlert(Ceviri.Al("Hata"),
                    $"{Ceviri.Al("Mesaj gönderilemedi:")} {ex.Message}", Ceviri.Al("Tamam"));
            }
        }
    }
}
