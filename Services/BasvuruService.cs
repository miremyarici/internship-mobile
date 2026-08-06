using InternshipMpbile.Models;
using Microsoft.Data.SqlClient;

namespace InternshipMpbile.Services
{
    public static class BasvuruService
    {
        // Windows'ta çalışan makinenin kendi kimliğiyle (Trusted_Connection) bağlanır.
        // Android emulator'de Windows kimlik doğrulaması kullanılamaz; host makineye
        // 10.0.2.2 üzerinden, sabit 1433 portu ve SQL Authentication ile bağlanılır.
        public static string connectionStr = DeviceInfo.Platform == DevicePlatform.Android
            ? "Server=10.0.2.2,1433;Database=InternshipMobile;User Id=internapp;Password=irem2004;TrustServerCertificate=True;"
            : "Server=LAPTOP-NNNR9RGP\\SQLEXPRESS;Database=InternshipMobile;Trusted_Connection=True;TrustServerCertificate=True;";

        public static async Task KaydetAsync(Basvuru basvuru)
        {
            using var connection = new SqlConnection(connectionStr);
            await connection.OpenAsync();

            const string query = @"INSERT INTO BasvuruFormu
                (ProjeAdi, BasvuranBirim, BasvuruYapilanProje, BasvuruYapilanTur, KatilimciTuru, BasvuruDonemi, BasvuruTarihi, BasvuruDurumu, DurumTarihi, HibeTutari)
                VALUES
                (@ProjeAdi, @BasvuranBirim, @BasvuruYapilanProje, @BasvuruYapilanTur, @KatilimciTuru, @BasvuruDonemi, @BasvuruTarihi, @BasvuruDurumu, @DurumTarihi, @HibeTutari)";

            using var command = new SqlCommand(query, connection);
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

        // En son kaydedilen başvuru en üstte olacak şekilde tüm kayıtları getirir.
        public static async Task<List<Basvuru>> ListeleAsync()
        {
            var basvurular = new List<Basvuru>();

            using var connection = new SqlConnection(connectionStr);
            await connection.OpenAsync();

            const string query = @"SELECT Id, ProjeAdi, BasvuranBirim, BasvuruYapilanProje, BasvuruYapilanTur,
                                          KatilimciTuru, BasvuruDonemi, BasvuruTarihi, BasvuruDurumu, DurumTarihi, HibeTutari
                                   FROM BasvuruFormu
                                   ORDER BY Id DESC";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                basvurular.Add(new Basvuru
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
                });
            }

            return basvurular;
        }
    }
}
