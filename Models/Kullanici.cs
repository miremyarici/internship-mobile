namespace InternshipMpbile.Models
{
    /// <summary>
    /// Kullanici tablosundaki bir kayıt. Aktif sütunu yalnızca soft delete için
    /// kullanılır ve sorgularda süzüldüğü için modele taşınmaz; parola da hiçbir
    /// zaman listeye okunmaz, ekranda sabit bir maske gösterilir.
    /// </summary>
    public class Kullanici
    {
        public int Id { get; set; }
        public string Isim { get; set; } = string.Empty;
        public string Soyisim { get; set; } = string.Empty;
        public string Eposta { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;

        /// <summary>Parola e-posta ile gönderilen geçici parola mı?</summary>
        public bool GeciciSifre { get; set; }

        /// <summary>Hamburger menüde ve onay ekranlarında gösterilen ad.</summary>
        public string TamAd => $"{Isim} {Soyisim}";

        /// <summary>Listede parola sütununda görünen sabit maske.</summary>
        public string SifreMaskesi => "••••••";
    }
}
