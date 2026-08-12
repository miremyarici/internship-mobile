namespace InternshipMpbile.Services
{
    /// <summary>
    /// Karanlık mod tercihi. Dilin aksine bu tercih cihazda saklanır: uygulama
    /// kapatılıp açıldığında kullanıcı en son hangi modu seçtiyse onunla başlar.
    ///
    /// Tercih işletim sisteminin kendi açık/koyu ayarından bağımsızdır; uygulama
    /// yalnızca menüdeki anahtara bakar.
    /// </summary>
    public static class Tema
    {
        private const string Anahtar = "karanlik_mod";

        public static bool KaranlikMi
        {
            get => Preferences.Default.Get(Anahtar, false);
            set
            {
                Preferences.Default.Set(Anahtar, value);
                Uygula(value);
            }
        }

        /// <summary>Uygulama açılışında, arayüz kurulmadan önce kullanılacak tema.</summary>
        public static AppTheme Baslangic => KaranlikMi ? AppTheme.Dark : AppTheme.Light;

        /// <summary>
        /// Sayfalardaki her renk AppThemeBinding ile Light/Dark ikilisine bağlı
        /// olduğu için, tema burada değiştiğinde MAUI ekrandaki tüm elemanları
        /// kendisi günceller; tek bir sayfa bile yeniden kurulmaz.
        /// </summary>
        private static void Uygula(bool karanlik)
        {
            if (Application.Current is { } uygulama)
                uygulama.UserAppTheme = karanlik ? AppTheme.Dark : AppTheme.Light;
        }
    }
}
