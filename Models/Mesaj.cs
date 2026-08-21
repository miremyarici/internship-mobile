using System.Globalization;
using InternshipMpbile.Localization;

namespace InternshipMpbile.Models
{
    /// <summary>Mesaj durumları; veritabanındaki Durum sütununun karşılığı.</summary>
    public static class MesajDurumu
    {
        public const byte Gonderildi = 0;
        public const byte Iletildi = 1;
        public const byte Okundu = 2;
    }

    /// <summary>
    /// Tek bir mesaj. Metin veritabanında şifreli durur; bu sınıftaki Metin
    /// her zaman çözülmüş (okunabilir) halidir.
    /// </summary>
    public class Mesaj
    {
        public int Id { get; set; }
        public Guid IstemciAnahtari { get; set; }
        public string GonderenEposta { get; set; } = string.Empty;
        public string AliciEposta { get; set; } = string.Empty;
        public string Metin { get; set; } = string.Empty;

        /// <summary>Her zaman UTC saklanır, ekranda yerel saate çevrilir.</summary>
        public DateTime GonderimZamani { get; set; }

        public byte Durum { get; set; }

        /// <summary>Mesajı bu cihazdaki kullanıcı mı gönderdi? (balonun sağda durması)</summary>
        public bool Benden { get; set; }

        public bool Karsidan => !Benden;

        public string SaatMetni => Zaman.Saat(GonderimZamani);

        // Yalnızca kendi gönderdiğim mesajın tiki görünür; üçünden biri açıktır.
        public bool TekTik => Benden && Durum == MesajDurumu.Gonderildi;
        public bool CiftTikGri => Benden && Durum == MesajDurumu.Iletildi;
        public bool CiftTikMavi => Benden && Durum == MesajDurumu.Okundu;
    }

    /// <summary>
    /// Mesajlar ekranındaki bir satır: bir kişiyle olan sohbetin son durumu.
    /// </summary>
    public class SohbetOzeti
    {
        public string Eposta { get; set; } = string.Empty;
        public string AdSoyad { get; set; } = string.Empty;
        public string SonMesaj { get; set; } = string.Empty;
        public DateTime SonZaman { get; set; }
        public byte SonDurum { get; set; }
        public bool SonMesajBenden { get; set; }
        public int OkunmamisSayisi { get; set; }

        /// <summary>Profil fotoğrafı olmadığı için avatar dairesinde baş harfler gösterilir.</summary>
        public string BasHarfler => Zaman.BasHarfler(AdSoyad);

        public string ZamanMetni => Zaman.ListeMetni(SonZaman);

        public bool OkunmamisVar => OkunmamisSayisi > 0;
        public string OkunmamisMetni => OkunmamisSayisi > 99 ? "99+" : OkunmamisSayisi.ToString();

        public bool TekTik => SonMesajBenden && SonDurum == MesajDurumu.Gonderildi;
        public bool CiftTikGri => SonMesajBenden && SonDurum == MesajDurumu.Iletildi;
        public bool CiftTikMavi => SonMesajBenden && SonDurum == MesajDurumu.Okundu;
    }

    /// <summary>Mesajlaşma dizinindeki bir kişi (kiminle konuşabilirim).</summary>
    public class Kisi
    {
        public string Eposta { get; set; } = string.Empty;
        public string AdSoyad { get; set; } = string.Empty;

        public string BasHarfler => Zaman.BasHarfler(AdSoyad);
    }

    /// <summary>Listede ve balonlarda görünen tarih/saat metinleri tek yerden üretilir.</summary>
    internal static class Zaman
    {
        public static string Saat(DateTime utc) => utc.ToLocalTime().ToString("HH:mm");

        /// <summary>Bugünse saat, dünse "Dün", bu haftaysa gün adı, daha eskiyse tarih.</summary>
        public static string ListeMetni(DateTime utc)
        {
            var yerel = utc.ToLocalTime();
            var bugun = DateTime.Now.Date;

            if (yerel.Date == bugun)
                return yerel.ToString("HH:mm");

            if (yerel.Date == bugun.AddDays(-1))
                return Ceviri.Al("Dün");

            if ((bugun - yerel.Date).TotalDays < 7)
                return yerel.ToString("ddd", Kultur());

            return yerel.ToString("dd.MM.yyyy");
        }

        public static string BasHarfler(string adSoyad)
        {
            var parcalar = adSoyad.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parcalar.Length == 0)
                return "?";

            var ilk = char.ToUpperInvariant(parcalar[0][0]);

            return parcalar.Length == 1
                ? ilk.ToString()
                : $"{ilk}{char.ToUpperInvariant(parcalar[^1][0])}";
        }

        private static CultureInfo Kultur() =>
            CultureInfo.GetCultureInfo(Ceviri.AktifDil == Dil.Ingilizce ? "en-US" : "tr-TR");
    }
}
