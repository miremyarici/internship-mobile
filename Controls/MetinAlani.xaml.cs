namespace InternshipMpbile.Controls
{
    /// <summary>
    /// Başlık + tek satırlık metin kutusundan oluşan form alanı.
    /// </summary>
    public partial class MetinAlani : ContentView
    {
        public MetinAlani()
        {
            InitializeComponent();
        }

        public string Baslik
        {
            get => BaslikLabel.Text;
            set => BaslikLabel.Text = value;
        }

        public string? Deger
        {
            get => DegerEntry.Text;
            set => DegerEntry.Text = value;
        }

        public Keyboard Klavye
        {
            get => DegerEntry.Keyboard;
            set => DegerEntry.Keyboard = value;
        }

        public bool Bos => string.IsNullOrWhiteSpace(DegerEntry.Text);

        public void Temizle() => DegerEntry.Text = string.Empty;
    }
}
