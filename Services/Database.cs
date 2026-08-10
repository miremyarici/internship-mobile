using Microsoft.Data.SqlClient;

namespace InternshipMpbile.Services
{
    /// <summary>
    /// Veritabanı bağlantısının tek sahibi. Servisler bağlantı dizesini bilmek
    /// zorunda kalmadan buradan açılmış bir bağlantı ister.
    /// </summary>
    internal static class Database
    {
        // Android emulator'de Windows kimlik doğrulaması kullanılamadığı için host
        // makineye 10.0.2.2 üzerinden, sabit 1433 portu ve SQL Authentication ile bağlanılır.
        private const string AndroidConnectionString =
            "Server=10.0.2.2,1433;Database=InternshipMobile;User Id=internapp;Password=irem2004;TrustServerCertificate=True;";

        // Windows'ta uygulama, makinenin kendi kimliğiyle (Trusted_Connection) bağlanır.
        private const string WindowsConnectionString =
            "Server=LAPTOP-NNNR9RGP\\SQLEXPRESS;Database=InternshipMobile;Trusted_Connection=True;TrustServerCertificate=True;";

        private static string ConnectionString => DeviceInfo.Platform == DevicePlatform.Android
            ? AndroidConnectionString
            : WindowsConnectionString;

        /// <summary>Açılmış bir bağlantı döndürür; çağıran taraf using ile kapatmalıdır.</summary>
        public static async Task<SqlConnection> AcikBaglantiAsync()
        {
            var connection = new SqlConnection(ConnectionString);

            try
            {
                await connection.OpenAsync();
                return connection;
            }
            catch
            {
                // Açılamayan bağlantı havuzda asılı kalmasın.
                connection.Dispose();
                throw;
            }
        }
    }
}
