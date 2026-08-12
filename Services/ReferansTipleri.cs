namespace InternshipMpbile.Services
{
    /// <summary>
    /// Referans tipleri. Hem Referans Ekleme ekranındaki tip listesi hem de
    /// Başvuru Formu'ndaki hangi açılır listenin hangi referans tipinden
    /// beslendiği bu adlara bakar; iki taraf birbirinden kaymasın diye
    /// metinler tek yerde tutulur.
    /// </summary>
    public static class ReferansTipleri
    {
        public const string BasvuranBirim = "Başvuran Birim";
        public const string BasvuruYapilanProje = "Başvuru Yapılan Proje";
        public const string BasvuruYapilanTur = "Başvuru Yapılan Tür";
        public const string KatilimciTuru = "Katılımcı Türü";
        public const string BasvuruDonemi = "Başvuru Dönemi";

        /// <summary>Referans Ekleme ekranındaki tip açılır listesi.</summary>
        public static readonly string[] Tumu =
        {
            BasvuranBirim,
            BasvuruYapilanProje,
            BasvuruYapilanTur,
            KatilimciTuru,
            BasvuruDonemi
        };
    }
}
