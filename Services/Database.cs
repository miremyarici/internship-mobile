using Microsoft.Data.SqlClient;

namespace InternshipMpbile.Services
{
    /// <summary>
    /// Veritabanı bağlantısının tek sahibi. Servisler bağlantı dizesini bilmek
    /// zorunda kalmadan buradan açılmış bir bağlantı ister.
    /// </summary>
    internal static class Database
    {
        // Geliştirme makinesi düşük bellekle (6 GB) çalışıyor; Visual Studio, Android
        // araçları ve SQL Server aynı anda açıkken Windows sık sık belleği sıkıştırıp
        // sayfalamaya düşüyor. Ölçümde bağlantı açmanın tek başına 29 saniye sürdüğü
        // görüldü; bu yüzden varsayılan süreler (bağlantı için 15 sn, komut için 30 sn)
        // yetmiyor. İkisi de burada, tek yerden, bolca tutulur.
        private const string ZamanAsimiAyarlari = "Connect Timeout=60;Command Timeout=90;";

        // Android emulator'de Windows kimlik doğrulaması kullanılamadığı için host
        // makineye 10.0.2.2 üzerinden, sabit 1433 portu ve SQL Authentication ile bağlanılır.
        private const string AndroidConnectionString =
            "Server=10.0.2.2,1433;Database=InternshipMobile;User Id=internapp;Password=irem2004;TrustServerCertificate=True;" + ZamanAsimiAyarlari;

        // Windows'ta uygulama, makinenin kendi kimliğiyle (Trusted_Connection) bağlanır.
        // Adres "LAPTOP-XXX\SQLEXPRESS" gibi adlandırılmış instance yerine doğrudan
        // port ile yazılır: instance adını porta çevirme işini SQL Server Browser
        // servisi yapar, o servis kapalıyken bağlantı el sıkışma aşamasında asılıp
        // zaman aşımına düşer. Instance zaten sabit 1433 portunu dinlediği için
        // porta bağlanmak hem Browser bağımlılığını kaldırır hem de bağlantı
        // dizesini makine adından bağımsız kılar.
        private const string WindowsConnectionString =
            "Server=localhost,1433;Database=InternshipMobile;Trusted_Connection=True;TrustServerCertificate=True;" + ZamanAsimiAyarlari;

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
