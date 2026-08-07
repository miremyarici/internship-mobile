using InternshipMpbile.Models;
using Microsoft.Data.SqlClient;

namespace InternshipMpbile.Services
{
    public static class ReferansService
    {
        // Referans tipi -> BasvuruFormu tablosundaki karşılık gelen sütun adı.
        // Sütun adları sorguya parametre olarak gönderilemediği için alt tip
        // sorgusuna yalnızca bu sözlükteki sabit değerler yazılır; kullanıcıdan
        // gelen metin hiçbir zaman doğrudan sorguya girmez.
        private static readonly Dictionary<string, string> TipSutunlari = new()
        {
            ["Başvuran Birim"] = "BasvuranBirim",
            ["Başvuru Yapılan Proje"] = "BasvuruYapilanProje",
            ["Başvuru Yapılan Tür"] = "BasvuruYapilanTur",
            ["Katılımcı Türü"] = "KatilimciTuru",
            ["Başvuru Dönemi"] = "BasvuruDonemi"
        };

        public static List<string> Tipler => TipSutunlari.Keys.ToList();

        // Seçilen referans tipine karşılık gelen sütunda başvuru formu üzerinden
        // girilmiş olan farklı değerleri getirir.
        public static async Task<List<string>> AltTipleriGetirAsync(string tip)
        {
            if (!TipSutunlari.TryGetValue(tip, out var sutun))
                return new List<string>();

            var altTipler = new List<string>();

            using var connection = new SqlConnection(BasvuruService.connectionStr);
            await connection.OpenAsync();

            var query = $@"SELECT DISTINCT {sutun}
                           FROM BasvuruFormu
                           WHERE {sutun} IS NOT NULL AND LTRIM(RTRIM({sutun})) <> ''
                           ORDER BY {sutun}";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                altTipler.Add(reader.GetString(0));

            return altTipler;
        }

        // Aynı tip/alt tip ikilisi silinmemiş bir kayıt olarak duruyor mu?
        public static async Task<bool> VarMiAsync(string tip, string altTip)
        {
            using var connection = new SqlConnection(BasvuruService.connectionStr);
            await connection.OpenAsync();

            const string query = @"SELECT COUNT(*) FROM Referans
                                   WHERE Type = @Type AND Subtype = @Subtype AND [Delete] = 0";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Type", tip);
            command.Parameters.AddWithValue("@Subtype", altTip);

            return (int)(await command.ExecuteScalarAsync() ?? 0) > 0;
        }

        // Delete sütunu tabloda DEFAULT 0 olduğu için insert'te belirtilmez.
        public static async Task KaydetAsync(Referans referans)
        {
            using var connection = new SqlConnection(BasvuruService.connectionStr);
            await connection.OpenAsync();

            const string query = @"INSERT INTO Referans (Type, Subtype)
                                   VALUES (@Type, @Subtype)";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Type", referans.Type);
            command.Parameters.AddWithValue("@Subtype", referans.Subtype);

            await command.ExecuteNonQueryAsync();
        }

        // Yalnızca silinmemiş kayıtlar, en son eklenen en üstte olacak şekilde.
        public static async Task<List<Referans>> ListeleAsync()
        {
            var referanslar = new List<Referans>();

            using var connection = new SqlConnection(BasvuruService.connectionStr);
            await connection.OpenAsync();

            const string query = @"SELECT Id, Type, Subtype
                                   FROM Referans
                                   WHERE [Delete] = 0
                                   ORDER BY Id DESC";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                referanslar.Add(new Referans
                {
                    Id = reader.GetInt32(0),
                    Type = reader.GetString(1),
                    Subtype = reader.GetString(2)
                });
            }

            return referanslar;
        }

        // Soft delete: kayıt tabloda kalır, Delete sütunu 1 yapılır.
        public static async Task SilAsync(int id)
        {
            using var connection = new SqlConnection(BasvuruService.connectionStr);
            await connection.OpenAsync();

            const string query = @"UPDATE Referans SET [Delete] = 1 WHERE Id = @Id";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            await command.ExecuteNonQueryAsync();
        }
    }
}
