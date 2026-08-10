namespace InternshipMpbile.Controls
{
    /// <summary>
    /// Başlık + parola kutusundan oluşan form alanı. Karakterler nokta olarak
    /// gösterilir; alanın sonundaki göz ikonu basılı tutulduğu sürece yazılan
    /// metin okunabilir hale gelir.
    /// </summary>
    public partial class ParolaAlani : ContentView
    {
        public ParolaAlani()
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

        public bool Bos => string.IsNullOrWhiteSpace(DegerEntry.Text);

        public void Temizle()
        {
            DegerEntry.Text = string.Empty;
            ParolayiGizle();
        }

        private void OnGozBasildi(object? sender, EventArgs e)
        {
            DegerEntry.IsPassword = false;
            KapaliGozIkonu.IsVisible = false;
            AcikGozIkonu.IsVisible = true;
        }

        // Parmak butondan kaydığında Released gelmeyebildiği için Clicked de
        // aynı yere bağlıdır; ikisi de alanı yeniden gizlemekle yetinir.
        private void OnGozBirakildi(object? sender, EventArgs e) => ParolayiGizle();

        private void ParolayiGizle()
        {
            DegerEntry.IsPassword = true;
            KapaliGozIkonu.IsVisible = true;
            AcikGozIkonu.IsVisible = false;
        }
    }
}
