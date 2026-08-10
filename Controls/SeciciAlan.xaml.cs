using System.Collections;

namespace InternshipMpbile.Controls
{
    /// <summary>
    /// Başlık + açılır liste (dropdown) alanı. Seçenekler kod tarafından
    /// <see cref="Secenekler"/> ile verilir.
    /// </summary>
    public partial class SeciciAlan : ContentView
    {
        /// <summary>Kullanıcı listeden farklı bir seçenek seçtiğinde tetiklenir.</summary>
        public event EventHandler? SecimDegisti;

        public SeciciAlan()
        {
            InitializeComponent();
        }

        public string Baslik
        {
            get => BaslikLabel.Text;
            set => BaslikLabel.Text = value;
        }

        /// <summary>Hiçbir seçim yokken listede görünen metin.</summary>
        public string YerTutucu
        {
            get => DegerPicker.Title;
            set => DegerPicker.Title = value;
        }

        public IList? Secenekler
        {
            get => DegerPicker.ItemsSource;
            set => DegerPicker.ItemsSource = value;
        }

        public string? SecilenDeger
        {
            get => DegerPicker.SelectedItem as string;
            set => DegerPicker.SelectedItem = value;
        }

        /// <summary>Alan kullanıcı tarafından açılabilir mi (örn. önkoşul seçilmeden kapalı tutulur).</summary>
        public bool Secilebilir
        {
            get => DegerPicker.IsEnabled;
            set => DegerPicker.IsEnabled = value;
        }

        /// <summary>Seçenekler veritabanından gelirken alanın içinde gösterge döndürür.</summary>
        public bool Yukleniyor
        {
            get => YukleniyorIndicator.IsRunning;
            set
            {
                YukleniyorIndicator.IsVisible = value;
                YukleniyorIndicator.IsRunning = value;
            }
        }

        public bool Bos => DegerPicker.SelectedItem is null;

        public void Temizle() => DegerPicker.SelectedItem = null;

        private void OnPickerSecimiDegisti(object? sender, EventArgs e) =>
            SecimDegisti?.Invoke(this, EventArgs.Empty);
    }
}
