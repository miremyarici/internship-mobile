using InternshipMpbile.Models;
using Microsoft.Data.SqlClient;

namespace InternshipMpbile.Services
{
    /// <summary>BasvuruFormu tablosu üzerindeki okuma/yazma işlemleri.</summary>
    public static class BasvuruService
    {
        private const string InsertQuery = @"
            INSERT INTO BasvuruFormu
                (ProjeAdi, BasvuranBirim, BasvuruYapilanProje, BasvuruYapilanTur, KatilimciTuru,
                 BasvuruDonemi, BasvuruTarihi, BasvuruDurumu, DurumTarihi, HibeTutari)
            VALUES
                (@ProjeAdi, @BasvuranBirim, @BasvuruYapilanProje, @BasvuruYapilanTur, @KatilimciTuru,
                 @BasvuruDonemi, @BasvuruTarihi, @BasvuruDurumu, @DurumTarihi, @HibeTutari)";

        private const string SelectQuery = @"
            SELECT Id, ProjeAdi, BasvuranBirim, BasvuruYapilanProje, BasvuruYapilanTur,
                   KatilimciTuru, BasvuruDonemi, BasvuruTarihi, BasvuruDurumu, DurumTarihi, HibeTutari
            FROM BasvuruFormu
            ORDER BY Id DESC";

        public static async Task KaydetAsync(Basvuru basvuru)
        {
            using var connection = await Database.AcikBaglantiAsync();
            using var command = new SqlCommand(InsertQuery, connection);

            command.Parameters.AddWithValue("@ProjeAdi", basvuru.ProjeAdi);
            command.Parameters.AddWithValue("@BasvuranBirim", basvuru.BasvuranBirim);
            command.Parameters.AddWithValue("@BasvuruYapilanProje", basvuru.BasvuruYapilanProje);
            command.Parameters.AddWithValue("@BasvuruYapilanTur", basvuru.BasvuruYapilanTur);
            command.Parameters.AddWithValue("@KatilimciTuru", basvuru.KatilimciTuru);
            command.Parameters.AddWithValue("@BasvuruDonemi", basvuru.BasvuruDonemi);
            command.Parameters.AddWithValue("@BasvuruTarihi", basvuru.BasvuruTarihi);
            command.Parameters.AddWithValue("@BasvuruDurumu", basvuru.BasvuruDurumu);
            command.Parameters.AddWithValue("@DurumTarihi", basvuru.DurumTarihi);
            command.Parameters.AddWithValue("@HibeTutari", (object?)basvuru.HibeTutari ?? DBNull.Value);

            await command.ExecuteNonQueryAsync();
        }

        /// <summary>En son kaydedilen başvuru en üstte olacak şekilde tüm kayıtları getirir.</summary>
        public static async Task<List<Basvuru>> ListeleAsync()
        {
            var basvurular = new List<Basvuru>();

            using var connection = await Database.AcikBaglantiAsync();
            using var command = new SqlCommand(SelectQuery, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                basvurular.Add(Oku(reader));

            return basvurular;
        }

        private static Basvuru Oku(SqlDataReader reader) => new()
        {
            Id = reader.GetInt32(0),
            ProjeAdi = reader.GetString(1),
            BasvuranBirim = reader.GetString(2),
            BasvuruYapilanProje = reader.GetString(3),
            BasvuruYapilanTur = reader.GetString(4),
            KatilimciTuru = reader.GetString(5),
            BasvuruDonemi = reader.GetString(6),
            BasvuruTarihi = reader.GetDateTime(7),
            BasvuruDurumu = reader.GetString(8),
            DurumTarihi = reader.GetDateTime(9),
            HibeTutari = reader.IsDBNull(10) ? null : reader.GetDecimal(10)
        };
    }
}
