using System.Globalization;

namespace InternshipMpbile.Localization
{
    /// <summary>
    /// Veritabanından gelen bir değeri listede çevrilmiş göstermek için:
    /// <c>Text="{Binding Rol, Converter={StaticResource Ceviri}}"</c>
    ///
    /// Yalnızca görüntüyü etkiler; modeldeki ve veritabanındaki değer Türkçe kalır.
    /// Karşılığı olmayan değerler (kullanıcının yazdığı alt tipler, proje adları)
    /// olduğu gibi görünür.
    /// </summary>
    public class CeviriConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is string metin ? Ceviri.Al(metin) : value;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException("Çeviri tek yönlüdür; ekrandan veriye geri yazılmaz.");
    }
}
