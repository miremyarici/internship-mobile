using InternshipMpbile.Localization;
using InternshipMpbile.Models;
using InternshipMpbile.Services;

namespace InternshipMpbile.Pages
{
    public partial class KullanicilarPage : ContentPage
    {
        // Sabit değil özellik: dil değiştirilebildiği için metin her kullanımda çevrilir.
        private static string EkranAdi => Ceviri.Al("Kullanıcı Ekleme");
        private static string Tamam => Ceviri.Al("Tamam");

        // Onay pop-up'ları açıkken işlem yapılacak kayıtlar.
        private Kullanici? _kaydedilecekKullanici;
        private Kullanici? _silinecekKullanici;

        public KullanicilarPage()
        {
            InitializeComponent();
            RolAlani.SecenekleriTazele(KullaniciService.Roller);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await KullanicilariYukleAsync();
        }

        private async void OnRefreshing(object? sender, EventArgs e)
        {
            await KullanicilariYukleAsync();
            KullaniciRefreshView.IsRefreshing = false;
        }

        private async Task KullanicilariYukleAsync()
        {
            // Aşağı çekerek yenilemede RefreshView kendi göstergesini gösterir,
            // ilk yüklemede ise ortadaki gösterge devreye girer.
            var ilkYukleme = !KullaniciRefreshView.IsRefreshing;
            GostergeyiAyarla(ilkYukleme);

            try
            {
                var kullanicilar = await KullaniciService.ListeleAsync();

                BindableLayout.SetItemsSource(KullaniciListesi, kullanicilar);
                BosListeGorunumu.IsVisible = kullanicilar.Count == 0;
            }
            catch (Exception ex)
            {
                await DisplayAlert(Ceviri.Al("Hata"),
                    $"{Ceviri.Al("Kullanıcılar yüklenirken bir hata oluştu:")} {ex.Message}", Tamam);
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

        // ==================== Kullanıcı ekleme ====================

        private async void OnKaydetClicked(object? sender, EventArgs e)
        {
            var isim = IsimAlani.Deger?.Trim() ?? string.Empty;
            var soyisim = SoyisimAlani.Deger?.Trim() ?? string.Empty;
            var eposta = EpostaAlani.Deger?.Trim() ?? string.Empty;

            if (isim.Length == 0 || soyisim.Length == 0 || eposta.Length == 0 ||
                RolAlani.SecilenDeger is not string rol)
            {
                await DisplayAlert(EkranAdi, Ceviri.Al("Lütfen tüm alanları doldurun."), Tamam);
                return;
            }

            if (!KullaniciService.EpostaGecerliMi(eposta))
            {
                await DisplayAlert(EkranAdi, Ceviri.Al("Lütfen geçerli bir e-posta adresi giriniz."), Tamam);
                return;
            }

            try
            {
                if (await KullaniciService.EpostaVarMiAsync(eposta))
                {
                    await DisplayAlert(EkranAdi, Ceviri.Al("Bu e-posta ile kayıtlı bir kullanıcı zaten var."), Tamam);
                    return;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert(Ceviri.Al("Hata"),
                    $"{Ceviri.Al("Kayıt sırasında bir hata oluştu:")} {ex.Message}", Tamam);
                return;
            }

            _kaydedilecekKullanici = new Kullanici
            {
                Isim = isim,
                Soyisim = soyisim,
                Eposta = eposta,
                Rol = rol
            };

            OnayOzetiniDoldur(_kaydedilecekKullanici);
            KayitOnayiOverlay.IsVisible = true;
        }

        private void OnayOzetiniDoldur(Kullanici kullanici)
        {
            OnayIsimLabel.Text = kullanici.Isim;
            OnaySoyisimLabel.Text = kullanici.Soyisim;
            OnayEpostaLabel.Text = kullanici.Eposta;
            OnayRolLabel.Text = Ceviri.Al(kullanici.Rol);
        }

        private async void OnKayitEvetClicked(object? sender, EventArgs e)
        {
            var kullanici = _kaydedilecekKullanici;
            KayitOnayiniKapat();

            if (kullanici is null)
                return;

            try
            {
                await KullaniciService.KaydetAsync(kullanici);

                FormuTemizle();
                await KullanicilariYukleAsync();
                await BildirimGosterAsync(Ceviri.Al("Kullanıcı başarıyla eklendi."));
            }
            catch (Exception ex)
            {
                await DisplayAlert(Ceviri.Al("Hata"),
                    $"{Ceviri.Al("Kayıt sırasında bir hata oluştu:")} {ex.Message}", Tamam);
            }
        }

        private void OnKayitHayirClicked(object? sender, EventArgs e) => KayitOnayiniKapat();

        private void KayitOnayiniKapat()
        {
            KayitOnayiOverlay.IsVisible = false;
            _kaydedilecekKullanici = null;
        }

        private void FormuTemizle()
        {
            IsimAlani.Temizle();
            SoyisimAlani.Temizle();
            EpostaAlani.Temizle();
            RolAlani.Temizle();
        }

        // ==================== Silme ====================

        private void OnSilTapped(object? sender, TappedEventArgs e)
        {
            if (sender is not Element element || element.BindingContext is not Kullanici kullanici)
                return;

            _silinecekKullanici = kullanici;
            SilmeOnayiOverlay.IsVisible = true;
        }

        // Evet: veritabanındaki Aktif sütunu 0 yapılır (soft delete) ve kayıt
        // listeden düşer, çünkü liste yalnızca Aktif = 1 olanları getirir.
        private async void OnSilmeEvetClicked(object? sender, EventArgs e)
        {
            var kullanici = _silinecekKullanici;
            SilmeOnayiniKapat();

            if (kullanici is null)
                return;

            try
            {
                await KullaniciService.SilAsync(kullanici.Id);
                await KullanicilariYukleAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert(Ceviri.Al("Hata"),
                    $"{Ceviri.Al("Silme sırasında bir hata oluştu:")} {ex.Message}", Tamam);
            }
        }

        private void OnSilmeHayirClicked(object? sender, EventArgs e) => SilmeOnayiniKapat();

        private void SilmeOnayiniKapat()
        {
            SilmeOnayiOverlay.IsVisible = false;
            _silinecekKullanici = null;
        }

        // ==================== Bildirim ====================

        /// <summary>Sağ üstte beliren, 1,5 saniye ekranda kalan yeşil bildirim.</summary>
        private async Task BildirimGosterAsync(string mesaj)
        {
            BildirimLabel.Text = mesaj;
            BildirimToast.Opacity = 0;
            BildirimToast.IsVisible = true;

            await BildirimToast.FadeTo(1, 150);
            await Task.Delay(1500);
            await BildirimToast.FadeTo(0, 200);

            BildirimToast.IsVisible = false;
        }
    }
}
