using InternshipMpbile.Models;

namespace InternshipMpbile.Services
{
    /// <summary>
    /// Uygulama açıkken giriş yapmış kullanıcıyı tutar. Çıkış yapıldığında veya
    /// uygulama kapandığında temizlenir; kalıcı bir "beni hatırla" saklamaz.
    /// </summary>
    public static class Oturum
    {
        public static Kullanici? AktifKullanici { get; private set; }

        public static void Baslat(Kullanici kullanici) => AktifKullanici = kullanici;

        public static void Bitir() => AktifKullanici = null;
    }
}
