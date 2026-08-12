using InternshipMpbile.Models;
using Microsoft.Data.SqlClient;

namespace InternshipMpbile.Services
{
    /// <summary>Referans tablosu üzerindeki işlemler ve alt tip seçeneklerinin kaynağı.</summary>
    public static class ReferansService
    {
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

        private const string AltTiplerQuery = @"
            SELECT Type, Subtype
            FROM Referans
            WHERE [Delete] = 0
            ORDER BY Subtype";

        /// <summary>
        /// Kayıtlı bütün referansları alt tip adlarına indirgeyip tipe göre gruplar.
        /// Başvuru Formu'ndaki beş açılır liste için tip başına ayrı sorgu açmak
        /// yerine tablo bir kez okunur; arama da sözlük üzerinden yapılır.
        /// </summary>
        public static async Task<ILookup<string, string>> AltTipleriTipeGoreGetirAsync()
        {
            var kayitlar = new List<(string Tip, string AltTip)>();

            using var connection = await Database.AcikBaglantiAsync();
            using var command = new SqlCommand(AltTiplerQuery, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                kayitlar.Add((reader.GetString(0), reader.GetString(1)));

            return kayitlar.ToLookup(kayit => kayit.Tip, kayit => kayit.AltTip);
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
