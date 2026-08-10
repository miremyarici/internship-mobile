namespace InternshipMpbile.Models
{
    /// <summary>
    /// Referans tablosundaki bir kayıt. Tablodaki Delete sütunu yalnızca soft
    /// delete için kullanılır ve sorgularda süzüldüğü için modele taşınmaz.
    /// </summary>
    public class Referans
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Subtype { get; set; } = string.Empty;
    }
}
