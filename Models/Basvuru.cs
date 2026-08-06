namespace InternshipMpbile.Models
{
    public class Basvuru
    {
        public int Id { get; set; }
        public string ProjeAdi { get; set; } = string.Empty;
        public string BasvuranBirim { get; set; } = string.Empty;
        public string BasvuruYapilanProje { get; set; } = string.Empty;
        public string BasvuruYapilanTur { get; set; } = string.Empty;
        public string KatilimciTuru { get; set; } = string.Empty;
        public string BasvuruDonemi { get; set; } = string.Empty;
        public DateTime BasvuruTarihi { get; set; }
        public string BasvuruDurumu { get; set; } = string.Empty;
        public DateTime DurumTarihi { get; set; }
        public decimal? HibeTutari { get; set; }

        // Listede doğrudan bağlanabilmek için hazırlanmış görüntüleme metinleri.
        public string BasvuruTarihiText => BasvuruTarihi.ToString("dd.MM.yyyy");
        public string DurumTarihiText => DurumTarihi.ToString("dd.MM.yyyy");
        public string HibeTutariText => HibeTutari.HasValue ? $"{HibeTutari.Value:N2} ₺" : "-";
    }
}
