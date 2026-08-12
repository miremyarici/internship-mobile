using InternshipMpbile.Localization;

namespace InternshipMpbile.Controls
{
    /// <summary>
    /// Başlık + açılır liste (dropdown) alanı. Seçenekler kod tarafından
    /// <see cref="SecenekleriTazele"/> ile verilir. Kutuda görünen metni Picker'ın
    /// kendisi değil, üstüne çizilen Label gösterir.
    /// </summary>
    public partial class SeciciAlan : ContentView
    {
        /// <summary>
        /// Listedeki bir seçenek. Ekranda çevrilmiş metni görünür ama
        /// <see cref="Deger"/> her zaman Türkçe kalır; kaydedilen ve sorgulanan
        /// değer bu olduğu için dil değişse de veritabanındaki karşılık kaymaz.
        /// </summary>
        private sealed class Secenek(string deger)
        {
            public string Deger { get; } = deger;

            // Picker, liste öğelerini bu metinle çizer.
            public override string ToString() => Ceviri.Al(Deger);
        }

        private List<string> _degerler = new();

        public SeciciAlan()
        {
            InitializeComponent();
            GorunenDegeriTazele();
        }

        public string Baslik
        {
            get => BaslikLabel.Text;
            set => BaslikLabel.Text = value;
        }

        /// <summary>Hiçbir seçim yokken kutuda görünen metin.</summary>
        public string YerTutucu
        {
            get => DegerPicker.Title;
            set
            {
                DegerPicker.Title = value;
                GorunenDegeriTazele();
            }
        }

        /// <summary>Seçilen seçeneğin Türkçe değeri; ekranda görünen çeviri değil.</summary>
        public string? SecilenDeger
        {
            get => (DegerPicker.SelectedItem as Secenek)?.Deger;
            set
            {
                DegerPicker.SelectedItem = value is null ? null : SecenekBul(value);
                GorunenDegeriTazele();
            }
        }

        public bool Bos => DegerPicker.SelectedItem is null;

        public void Temizle() => SecilenDeger = null;

        /// <summary>
        /// Seçenekleri Türkçe değerleriyle alır; listede çevrilmiş halleri görünür.
        /// Picker'a yeni bir kaynak atamak seçimi düşürdüğü için, liste içerik olarak
        /// aynıysa kaynağa hiç dokunulmaz; gerçekten değiştiyse seçim yeni listede de
        /// duruyorsa geri yazılır.
        /// </summary>
        public void SecenekleriTazele(IList<string> degerler)
        {
            if (_degerler.SequenceEqual(degerler))
                return;

            var oncekiSecim = SecilenDeger;

            _degerler = degerler.ToList();
            DegerPicker.ItemsSource = _degerler.Select(deger => new Secenek(deger)).ToList();

            SecilenDeger = oncekiSecim is not null && _degerler.Contains(oncekiSecim) ? oncekiSecim : null;
        }

        private Secenek? SecenekBul(string deger) =>
            (DegerPicker.ItemsSource as IEnumerable<Secenek>)?.FirstOrDefault(secenek => secenek.Deger == deger);

        private void OnPickerSecimiDegisti(object? sender, EventArgs e) => GorunenDegeriTazele();

        /// <summary>
        /// Seçim varsa çevrilmiş değeri koyu renkle, yoksa yer tutucuyu soluk renkle
        /// yazar. Picker'ın metni saydam olduğu için ekranda görünen tek metin budur.
        /// </summary>
        private void GorunenDegeriTazele()
        {
            var secim = SecilenDeger;

            GorunenDegerLabel.Text = secim is null ? DegerPicker.Title : Ceviri.Al(secim);
            GorunenDegerLabel.TextColor = Renk(secim is null ? "FormPlaceholder" : "FormFieldText");
        }

        private static Color Renk(string kaynakAdi) =>
            Application.Current?.Resources.TryGetValue(kaynakAdi, out var deger) == true && deger is Color renk
                ? renk
                : Colors.Black;
    }
}
