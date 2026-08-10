using InternshipMpbile.Models;
using Microsoft.Data.SqlClient;

namespace InternshipMpbile.Services
{
    /// <summary>Referans tablosu üzerindeki işlemler ve alt tip seçeneklerinin kaynağı.</summary>
    public static class ReferansService
    {
        /// <summary>
        /// Referans tipi -> BasvuruFormu tablosundaki karşılık gelen sütun adı.
        /// Sütun adları sorguya parametre olarak gönderilemediği için alt tip
        /// sorgusuna yalnızca bu sözlükteki sabit değerler yazılır; kullanıcıdan
        /// gelen metin hiçbir zaman doğrudan sorguya girmez.
        /// </summary>
        private static readonly Dictionary<string, string> TipSutunlari = new()
        {
            ["Başvuran Birim"] = "BasvuranBirim",
            ["Başvuru Yapılan Proje"] = "BasvuruYapilanProje",
            ["Başvuru Yapılan Tür"] = "BasvuruYapilanTur",
            ["Katılımcı Türü"] = "KatilimciTuru",
            ["Başvuru Dönemi"] = "BasvuruDonemi"
        };

        private const string VarMiQuery = @"
            SELECT COUNT(*) FROM Referans
            WHERE Type = @Type AND Subtype = @Subtype AND [Delete] = 0";

        private const string InsertQuery = @"
            INSERT INTO Referans (Type, Subtype)
            VALUES (@Type, @Subtype)";

        private const string SelectQuery = @"
            SELECT Id, Type, Subtype
            FROM Referans
            WHERE [Delete] = 0
            ORDER BY Id DESC";

        private const string SoftDeleteQuery = @"
            UPDATE Referans SET [Delete] = 1 WHERE Id = @Id";

        /// <summary>Referans Ekleme ekranındaki tip listesi.</summary>
        public static List<string> Tipler => TipSutunlari.Keys.ToList();

        /// <summary>
        /// Seçilen referans tipine karşılık gelen sütunda, başvuru formu üzerinden
        /// girilmiş olan farklı değerleri getirir.
        /// </summary>
        public static async Task<List<string>> AltTipleriGetirAsync(string tip)
        {
            if (!TipSutunlari.TryGetValue(tip, out var sutun))
                return new List<string>();

            var altTipler = new List<string>();

            var query = $@"
                SELECT DISTINCT {sutun}
                FROM BasvuruFormu
                WHERE {sutun} IS NOT NULL AND LTRIM(RTRIM({sutun})) <> ''
                ORDER BY {sutun}";

            using var connection = await Database.AcikBaglantiAsync();
            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                altTipler.Add(reader.GetString(0));

            return altTipler;
        }

        /// <summary>Aynı tip/alt tip ikilisi silinmemiş bir kayıt olarak duruyor mu?</summary>
        public static async Task<bool> VarMiAsync(string tip, string altTip)
        {
            using var connection = await Database.AcikBaglantiAsync();
            using var command = new SqlCommand(VarMiQuery, connection);

            command.Parameters.AddWithValue("@Type", tip);
            command.Parameters.AddWithValue("@Subtype", altTip);

            return (int)(await command.ExecuteScalarAsync() ?? 0) > 0;
        }

        /// <summary>Delete sütunu tabloda DEFAULT 0 olduğu için insert'te belirtilmez.</summary>
        public static async Task KaydetAsync(Referans referans)
        {
            using var connection = await Database.AcikBaglantiAsync();
            using var command = new SqlCommand(InsertQuery, connection);

            command.Parameters.AddWithValue("@Type", referans.Type);
            command.Parameters.AddWithValue("@Subtype", referans.Subtype);

            await command.ExecuteNonQueryAsync();
        }

        /// <summary>Yalnızca silinmemiş kayıtlar, en son eklenen en üstte olacak şekilde.</summary>
        public static async Task<List<Referans>> ListeleAsync()
        {
            var referanslar = new List<Referans>();

            using var connection = await Database.AcikBaglantiAsync();
            using var command = new SqlCommand(SelectQuery, connection);
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

        /// <summary>Soft delete: kayıt tabloda kalır, Delete sütunu 1 yapılır.</summary>
        public static async Task SilAsync(int id)
        {
            using var connection = await Database.AcikBaglantiAsync();
            using var command = new SqlCommand(SoftDeleteQuery, connection);

            command.Parameters.AddWithValue("@Id", id);

            await command.ExecuteNonQueryAsync();
        }
    }
}
