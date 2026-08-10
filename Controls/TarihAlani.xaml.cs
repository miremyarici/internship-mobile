namespace InternshipMpbile.Controls
{
    /// <summary>
    /// Başlık + tarih seçici alan. Kullanıcı bir tarih seçene kadar DatePicker'ın
    /// metni saydam tutulur ve üstünde "gg.aa.yyyy" yer tutucusu gösterilir.
    /// </summary>
    public partial class TarihAlani : ContentView
    {
        public TarihAlani()
        {
            InitializeComponent();
        }

        public string Baslik
        {
            get => BaslikLabel.Text;
            set => BaslikLabel.Text = value;
        }

        public DateTime Tarih => DegerPicker.Date;

        public void Temizle()
        {
            DegerPicker.Date = DateTime.Today;
            DegerPicker.TextColor = Colors.Transparent;
            YerTutucuLabel.IsVisible = true;
        }

        private void OnTarihSecildi(object? sender, DateChangedEventArgs e)
        {
            YerTutucuLabel.IsVisible = false;
            DegerPicker.TextColor = AlanMetniRengi();
        }

        private static Color AlanMetniRengi() =>
            Application.Current?.Resources.TryGetValue("FormFieldText", out var renk) == true && renk is Color color
                ? color
                : Colors.Black;
    }
}
