namespace InternshipMpbile.Services
{
    /// <summary>
    /// Başvuru Formu ekranındaki açılır listelerin geçici seçenekleri.
    /// Bu değerler ileride lookup tablolarından okunacak; o zamana kadar
    /// tek yerde toplu dursun diye buraya alındı.
    /// </summary>
    public static class FormSecenekleri
    {
        public static readonly string[] BasvuranBirimler =
        {
            "Bilgi İşlem",
            "İnsan Kaynakları",
            "Yatırım İşleri",
            "test"
        };

        public static readonly string[] BasvuruYapilanProjeler =
        {
            "Erasmus",
            "Merkezi",
            "Avrupa",
            "Diğer"
        };

        public static readonly string[] BasvuruYapilanTurler =
        {
            "Gençlik",
            "Yetişkin",
            "Spor",
            "Mesleki",
            "Dijital",
            "Diğer"
        };

        public static readonly string[] KatilimciTurleri =
        {
            "Koordinatör",
            "Ortak",
            "R2",
            "R3"
        };

        public static readonly string[] BasvuruDonemleri =
        {
            "R1"
        };

        public static readonly string[] BasvuruDurumlari =
        {
            "Kabul",
            "Red"
        };
    }
}
