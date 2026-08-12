namespace InternshipMpbile.Localization
{
    /// <summary>
    /// XAML'de metinleri çevirmek için: <c>Text="{yerel:Ceviri Başvuru Formu}"</c>
    ///
    /// Değer, sayfa kurulurken bir kez hesaplanır. Dil değiştiğinde arayüz baştan
    /// kurulduğu için (bkz. App.DiliDegistir) bu yeterlidir ve her metin için
    /// bağlama (binding) kurmaya gerek kalmaz.
    /// </summary>
    [ContentProperty(nameof(Metin))]
    [AcceptEmptyServiceProvider]
    public class CeviriExtension : IMarkupExtension<string>
    {
        public string Metin { get; set; } = string.Empty;

        public string ProvideValue(IServiceProvider serviceProvider) => Ceviri.Al(Metin);

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
    }
}
