namespace InternshipMpbile.Controls
{
    /// <summary>
    /// Sayfaların sağ alt köşesinde duran mesajlaşma butonu. Basıldığında
    /// Mesajlar ekranını açar; sayfaların kendi kodunda hiçbir şey gerekmez.
    /// </summary>
    public partial class MesajButonu : ContentView
    {
        // Arka arkaya basışta ekranın iki kez yığına eklenmesini önler.
        private bool aciliyor;

        public MesajButonu()
        {
            InitializeComponent();
        }

        private async void OnTapped(object? sender, TappedEventArgs e)
        {
            if (aciliyor)
                return;

            aciliyor = true;

            try
            {
                await Daire.ScaleTo(0.92, 60, Easing.CubicOut);
                await Daire.ScaleTo(1.0, 60, Easing.CubicIn);

                await Shell.Current.GoToAsync("mesajlar");
            }
            finally
            {
                aciliyor = false;
            }
        }
    }
}
