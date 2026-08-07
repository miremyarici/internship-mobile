namespace InternshipMpbile.Models
{
    public class Referans
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Subtype { get; set; } = string.Empty;

        // Soft delete bayrağı: 0 = aktif, 1 = silinmiş.
        public int Delete { get; set; }
    }
}
